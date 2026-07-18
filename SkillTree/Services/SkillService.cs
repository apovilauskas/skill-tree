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
    
    public async Task<SkillStatus> CanStart(int skillId)
    {
        var skill = await _repository.GetSkillAsync(skillId);
        if(skill == null) return SkillStatus.NotFound;
        if (skill.Prerequisites.Any(sp => sp.Prerequisite.Status != SkillStatus.Completed)) return SkillStatus.Locked;
        return SkillStatus.InProgress;
    }

    private async Task<Dictionary<int, List<int>>> BuildGraphAsync()
    {
        var graph = new Dictionary<int, List<int>>();
        var skills = await _repository.GetAllSkillsWithPrerequisitesAsync();
        foreach (var skill in skills)
        {
            graph[skill.Id] = skill.Prerequisites
                .Select(p => p.PrerequisiteId)
                .ToList();
        }
        return graph;
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

    public async Task<bool> CreatePrerequisiteAsync(int skillId, PrerequisiteIdDto prerequisiteId)
    { 
        var prereqId = prerequisiteId.Id;
        if (!await _repository.ExistsAsync(skillId) || !await _repository.ExistsAsync(prereqId))
        {
            return false;
        }

        if (skillId == prereqId) return false;

        var prerequisiteGraph = await BuildGraphAsync();
        if (!IsValidPrerequisite(skillId, prereqId, prerequisiteGraph)) return false;
        
        var skillPrerequisite = new SkillPrerequisite()
        {
            SkillId = skillId,
            PrerequisiteId = prereqId
        };
        await _repository.AddPrerequisitesAsync(skillPrerequisite);
        return true;
    }

    public async Task<IEnumerable<SkillLogResponseDto>> GetSkillLogsAsync(int skillId)
    {
        if(!await _repository.ExistsAsync(skillId)) return null;
        var a = await _repository.GetLogsAsync(skillId);
        return a.Select(s => s.ToDto());
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
    
}