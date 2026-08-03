using skill_tree.Common;
using skill_tree.DTOs;
using skill_tree.Entities;
using skill_tree.Repositories;
using skill_tree.SkillMappingExtensions;

namespace skill_tree.Services;

public class SkillService : ISkillService
{
    private readonly ISkillRepository _repository;
    private readonly ICurrentUserService  _currentUserService;

    public SkillService(ISkillRepository repository, ICurrentUserService  currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }
    
    public async Task<CanStartResult> CanStartAsync(int skillId)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null) throw new UnauthorizedAccessException();

        var skill = await _repository.GetSkillAsync(skillId);
        if (skill == null) return CanStartResult.SkillNotFound;

        var completedSkillIds = await _repository.GetCompletedSkillsIds(userId);

        bool allPrerequisitesCompleted = skill.Prerequisites
            .All(p => completedSkillIds.Contains(p.PrerequisiteId));

        return allPrerequisitesCompleted ? CanStartResult.Available : CanStartResult.LockedByPrerequisites;
    }
    
    private async Task<Dictionary<int, List<int>>> BuildGraphAsync()
    {
        return await _repository.GetSkillPrerequisiteGraphAsync();
    }
    
    private bool IsValidPrerequisite(int skillId, int prerequisiteId, Dictionary<int, List<int>> graph)
    {
        if (!graph.TryGetValue(prerequisiteId, out var prereq))
        {
            return true;
        }
        if(prereq.Count < 1) return true;
        foreach (int pId in prereq)
        {
            if (pId == skillId) return false;
            if (!IsValidPrerequisite(skillId, pId, graph)) return false;
        }
        return true;
    }

    public async Task<IEnumerable<SkillResponseDto>> GetAllSkillsAsync()
    {
        var id = _currentUserService.GetUserId();
        if (id == null) throw new UnauthorizedAccessException();

        var skills = (await _repository.GetAllSkillsWithPrerequisitesAsync()).ToList();
        var progressBySkill = await _repository.GetAllUserSkillProgressesAsync(id);
        var statusMap = progressBySkill.ToDictionary(kv => kv.Key, kv => kv.Value.SkillStatus);
        var skillIds = skills.Select(s => s.Id).ToList();
        var logsBySkill = await _repository.GetLogsBySkillIdsAsync(id, skillIds);

        return skills.Select(skill =>
        {
            progressBySkill.TryGetValue(skill.Id, out var progress);
            var logs = logsBySkill.GetValueOrDefault(skill.Id, new List<SkillLog>());
            var startedAt = progress?.StartedAt ?? skill.CreatedAt;
            var progressValue = Progress(skill.Target, logs, startedAt);
            return skill.ToDto(progress, progressValue, statusMap);
        });
    }

    public async Task<SkillResponseDto> CreateSkillAsync(CreateSkillDto skill)
    {
        var entity = skill.ToEntity();
        await _repository.AddAsync(entity);
        return entity.ToDto(userProgress: null, progressValue: 0.0, prerequisiteStatuses: new Dictionary<int, SkillStatus>());
    }

    public async Task<CreatePrerequisiteResult> CreatePrerequisiteAsync(int skillId, PrerequisiteIdDto prerequisiteId)
    { 
        var prereqId = prerequisiteId.Id;
        if (!await _repository.ExistsAsync(skillId) || !await _repository.ExistsAsync(prereqId))
        {
            return CreatePrerequisiteResult.SkillNotFound;
        }

        if (skillId == prereqId) return CreatePrerequisiteResult.CircularDependencyDetected;
        var prerequisiteGraph = await BuildGraphAsync();
        if (!IsValidPrerequisite(skillId, prereqId, prerequisiteGraph)) return CreatePrerequisiteResult.CircularDependencyDetected;
        
        var skillPrerequisite = new SkillPrerequisite()
        {
            SkillId = skillId,
            PrerequisiteId = prereqId
        };
        await _repository.AddPrerequisitesAsync(skillPrerequisite);
        return CreatePrerequisiteResult.Success;
    }
    
    public async Task<IEnumerable<SkillLogResponseDto>?> GetSkillLogsAsync(int skillId)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null) throw new UnauthorizedAccessException();

        if (!await _repository.ExistsAsync(skillId)) return null;

        var logs = await _repository.GetLogsAsync(skillId, userId);
        return logs.Select(s => s.ToDto());
    }
    
    public async Task<bool> CreateSkillLogAsync(int skillId, CreateSkillLogDto skillLog)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null) throw new UnauthorizedAccessException();
    
        if (!await _repository.ExistsAsync(skillId)) return false;
    
        var entity = skillLog.ToEntity();
        entity.SkillId = skillId;
        await _repository.AddLogAsync(entity, userId);
    
        await RefreshStatus(userId, skillId);
        return true;
    }

    private async Task<UserSkillProgress?> GetUserSkillProgressAsync(string userId, int skillId)
    {
        return await _repository.GetUserSkillProgressAsync(userId, skillId);
    }

    public async Task<bool> DeleteSkillAsync(int skillId)
    {
        return await _repository.RemoveSkillAsync(skillId);
    }

    public async Task<bool> EditSkillAsync(int skillId, EditSkillDto editSkillDto)
    {
        return await _repository.EditSkillAsync(skillId, editSkillDto.name, editSkillDto.description, editSkillDto.metric);
    }

    public async Task<bool> DeletePrerequisiteAsync(int skillPrerequisiteId)
    {
        return await _repository.RemovePrerequisiteAsync(skillPrerequisiteId);
    }
    
    private double Progress(double target, List<SkillLog> logs, DateTime createdAt)
    {
        if (logs == null || logs.Count == 0 || target <= 0)
            return 0.0;

        var practicedDays = logs
            .Select(sl => sl.Date.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        double totalAmount = logs.Sum(sl => sl.Amount);
        double daysPracticed = practicedDays.Count;
        double daysSinceUnlock = Math.Max(1, (DateTime.UtcNow.Date - createdAt.Date).Days);
        
        int streak = 0; // must include today or yesterday, then count consecutive days backwards
        if (practicedDays.Count > 0)
        {
            var today = DateTime.UtcNow.Date;
            var mostRecent = practicedDays[0];

            if (mostRecent == today || mostRecent == today.AddDays(-1))
            {
                streak = 1;
                for (int i = 0; i < practicedDays.Count - 1; i++)
                {
                    if (practicedDays[i] == practicedDays[i + 1].AddDays(1))
                        streak++;
                    else
                        break;
                }
            }
        }
        
        double consistencyMultiplier = 0.8 + 0.4 * (0.5 * daysPracticed / daysSinceUnlock + 0.5 * Math.Min(1, streak / 30.0));
        return Math.Min(100.0, totalAmount / target * consistencyMultiplier * 100.0); 
    }
    
    private async Task<SkillStatus> RefreshStatus(string userId, int skillId)
    {
        var us = await GetUserSkillProgressAsync(userId, skillId);
        if (us == null) us = await _repository.AddUserSkillProgressAsync(userId, skillId);

        var skill = await _repository.GetSkillAsync(skillId);
        if (skill == null) return SkillStatus.Locked;

        var target = skill.Target;
        var startedAt = us.StartedAt ?? throw new InvalidOperationException("StartedAt missing in SkillService");
        var logs = await _repository.GetLogsAsync(skillId, userId);
        var progress = Progress(target, logs.ToList(), startedAt);

        var newStatus = us.SkillStatus;
        if (progress >= 100 && us.SkillStatus != SkillStatus.Completed)
        {
            newStatus = SkillStatus.Completed;
        }
        else if (progress > 0 && us.SkillStatus == SkillStatus.Locked)
        {
            newStatus = SkillStatus.InProgress;
        }

        if (newStatus != us.SkillStatus)
        {
            await _repository.UpdateAsync(skillId, userId, newStatus);
        }

        return newStatus;
    }
    
    public async Task<IEnumerable<CompletedSkillResponseDto>> GetCompletedSkillsAsync()
    {
        var id = _currentUserService.GetUserId();
        if (id == null) throw new UnauthorizedAccessException();
        var progresses = await _repository.GetCompletedSortedRecentSkillsAsync(id);
        return progresses.Select(p => p.Skill.ToCompletedDto(p.CompletedAt ?? DateTime.UtcNow));
    }
    
    public async Task<IEnumerable<UnlockedSkillResponseDto>> GetUnlockedSkillsAsync()
    {
        var id = _currentUserService.GetUserId();
        if (id == null) throw new UnauthorizedAccessException();

        var progresses = (await _repository.GetUnlockedSkillsAsync(id)).ToList();
        var skillIds = progresses.Select(p => p.SkillId).ToList();
        var logsBySkill = await _repository.GetLogsBySkillIdsAsync(id, skillIds);

        return progresses.Select(p =>
        {
            var logs = logsBySkill.GetValueOrDefault(p.SkillId, new List<SkillLog>());
            var startedAt = p.StartedAt ?? throw new InvalidOperationException("StartedAt missing on existing progress row");
            var progressValue = Progress(p.Skill.Target, logs, startedAt);
            return p.Skill.ToUnlockedDto(progressValue);
        });
    }

    public async Task<IEnumerable<UnlockedSkillResponseDto>> GetRecommendationsAsync()
    {
        var id = _currentUserService.GetUserId();
        if (id == null) throw new UnauthorizedAccessException();

        var recommendations = (await _repository.GetRecommendedSkills(id)).ToList();
        var skillIds = recommendations.Select(r => r.Skill.Id).ToList();
        var logsBySkill = await _repository.GetLogsBySkillIdsAsync(id, skillIds);

        return recommendations
            .OrderByDescending(RankingFormula)
            .Select(r =>
            {
                var logs = logsBySkill.GetValueOrDefault(r.Skill.Id, new List<SkillLog>());
                var startedAt = r.StartedAt ?? throw new InvalidOperationException("StartedAt missing on existing progress row");
                var progressValue = Progress(r.Skill.Target, logs, startedAt);
                return r.Skill.ToUnlockedDto(progressValue);
            })
            .Take(3);
    }

    private double RankingFormula(SkillRecommendation skill)
    {
        double total = 0;
        
        // up to 15 pts for each inactive day
        if (skill.LastLog == null) total += 15;
        else{
            var days = (DateTime.UtcNow - skill.LastLog.Value).TotalDays;
            total += Math.Min(days, 15);
        }
        
        // +10 for each skill this prerequisite unlocks
        total += skill.UnlockCount * 10;
        
        return total;
    }
}