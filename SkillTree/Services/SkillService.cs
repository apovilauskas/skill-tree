using skill_tree.Common;
using skill_tree.DTOs;
using skill_tree.Entities;
using skill_tree.Repositories;
using skill_tree.SkillMappingExtensions;

namespace skill_tree.Services;

public class SkillService : ISkillService
{
    private readonly ISkillRepository _repository;

    public SkillService(ISkillRepository repository)
    {
        _repository = repository;
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
        var sk = await _repository.GetLogsAsync(skillId);
        return sk.Select(s => s.ToDto());
    }

    public async Task<bool> CreateSkillLogAsync(int skillId, CreateSkillLogDto skillLog)
    {
        var entity = skillLog.ToEntity();
        entity.SkillId = skillId;
        if (!await _repository.ExistsAsync(skillId))
        {
            return false;
        }
        await _repository.AddLogAsync(entity);
        return true;
    }

    public async Task<IEnumerable<CompletedSkillResponseDto>> GetCompletedSkillsAsync()
    {
        var skills = await _repository.GetCompletedSortedRecentSkillsAsync();
        return skills.Select(s => s.ToCompletedDto());
    }
    
    public async Task<IEnumerable<UnlockedSkillResponseDto>> GetUnlockedSkillsAsync()
    {
        var skills = await _repository.GetUnlockedSkillsAsync();
        return skills.Select(s => s.ToUnlockedDto());
    }

    public async Task<IEnumerable<UnlockedSkillResponseDto>> GetRecommendations()
    {
        var skills = await _repository.GetRecommendedSkills();

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