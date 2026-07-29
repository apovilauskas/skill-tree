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
        var skill = await _repository.GetSkillAsync(skillId);
        if(skill == null) return CanStartResult.SkillNotFound;
        if (skill.Prerequisites.Any(sp => sp.Prerequisite.Status != SkillStatus.Completed)) return CanStartResult.LockedByPrerequisites;
        return CanStartResult.Available;
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
        var skills = await _repository.GetAllAsync();
        return skills.Select(s => s.ToDto());
    }

    public async Task<SkillResponseDto> CreateSkillAsync(CreateSkillDto skill)
    {
        var entity = skill.ToEntity();
        await _repository.AddAsync(entity);
        return entity.ToDto();
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

    public async Task<IEnumerable<SkillLogResponseDto>> GetSkillLogsAsync(int skillId)
    {
        if(!await _repository.ExistsAsync(skillId)) return null;
        var sk = await _repository.GetLogsAsync();
        return sk.Select(s => s.ToDto());
    }
    
    public async Task<bool> CreateSkillLogAsync(int skillId, CreateSkillLogDto skillLog)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null) throw new UnauthorizedAccessException();
        
        if(!await _repository.ExistsAsync(skillId)) return false;
        
        var entity = skillLog.ToEntity();
        entity.SkillId = skillId;
        await _repository.AddLogAsync(entity, userId);
        
        await _repository.UpdateAsync(skillId, userId, await RefreshStatus(userId, skillId));
        return true;
    }

    public async Task<UserSkillProgress?> GetUserSkillProgressAsync(string userId, int skillId)
    {
        return await _repository.GetUserSkillProgressAsync(userId, skillId);
    }
    
    private double Progress(double target, List<SkillLog> logs, DateTime createdAt)
    {
        if (logs.Count == 0) return 0.0;
        if (target <= 0) return 0.0;
        
        double totalAmount = logs.Sum(h => h.Amount);
        var practicedLogsList = logs.Select(s => s.Date).Distinct().OrderByDescending(d => d.Date).ToList();
        double daysPracticed = practicedLogsList.Count;
        double daysSinceUnlock = (DateTime.UtcNow - createdAt).Days;
        if (daysSinceUnlock < 1) daysSinceUnlock = 1;
        
        int streak = 0;
        if(daysPracticed > 0)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday =  today.AddDays(-1);
            var mostRecentLog = practicedLogsList[0];
            if (today == mostRecentLog.Date || yesterday == mostRecentLog.Date)
            {
                streak = 1;
                for (int i = 0; i < practicedLogsList.Count-1; i++)
                {
                    if (practicedLogsList[i] == practicedLogsList[i + 1].AddDays(1)) streak++;
                    else break;
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
        var target = skill.Target;
        var startedAt = us.StartedAt ?? throw new InvalidOperationException("Started at error in SkillService");
        var logs = await _repository.GetLogsAsync(skillId, userId);
        var progress = Progress(target, logs.ToList(), startedAt);

        if (progress >= 100 && us.SkillStatus != SkillStatus.Completed) return SkillStatus.Completed;
        if (progress > 0 && us.SkillStatus == SkillStatus.Locked) return SkillStatus.InProgress;
        return SkillStatus.Locked;
    }

    public async Task<IEnumerable<CompletedSkillResponseDto>> GetCompletedSkillsAsync()
    {
        var id = _currentUserService.GetUserId();
        if (id == null) throw new UnauthorizedAccessException();
        var skills = await _repository.GetCompletedSortedRecentSkillsAsync(id);
        return skills.Select(s => s.ToCompletedDto());
    }
    
    public async Task<IEnumerable<UnlockedSkillResponseDto>> GetUnlockedSkillsAsync()
    {
        var id = _currentUserService.GetUserId();
        if (id == null) throw new UnauthorizedAccessException();
        var skills = await _repository.GetUnlockedSkillsAsync(id);
        return skills.Select(s => s.ToUnlockedDto());
    }

    public async Task<IEnumerable<UnlockedSkillResponseDto>> GetRecommendationsAsync()
    {
        var id = _currentUserService.GetUserId();
        if (id == null) throw new UnauthorizedAccessException();
        var skills = await _repository.GetRecommendedSkills(id);

        return skills
            .OrderByDescending(Formula) 
            .Select(s => s.Skill.ToUnlockedDto())
            .Take(3);
    }

    private double Formula(SkillRecommendation skill)
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