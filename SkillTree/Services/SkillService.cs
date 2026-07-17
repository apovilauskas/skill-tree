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

    public double Progress(Skill skill)
    {
        if (skill.SkillLogs.Count == 0) return 0.0;
        if (skill.Target <= 0) return 0.0;
        
        //v is matches/chapters/attempts/hours and T is target amount.
        double v = skill.SkillLogs.Sum(h => h.Amount);
        double t = skill.Target;
        
        //d1 is total days practiced, d2 is total days since beginning to unlock skill
        var practicedDays = skill.SkillLogs.Select(s => s.Date).Distinct().OrderByDescending(d => d.Date).ToList();
        double d1 = practicedDays.Count;
        double d2 = (DateTime.UtcNow - skill.CreatedAt).Days;
        if (d2 < 1) d2 = 1;
        
        //s is current streak
        int s = 0;
        if(d1 > 0)
        {
            var today = DateTime.UtcNow.Date;
            var yesterday =  today.AddDays(-1);
            var mostRecentLog = practicedDays[0];
            if (today == mostRecentLog.Date || yesterday == mostRecentLog.Date)
            {
                s = 1;
                for (int i = 0; i < practicedDays.Count-1; i++)
                {
                    if (practicedDays[i] == practicedDays[i + 1].AddDays(1)) s++;
                    else break;
                } 
            }
        }
        
        //Consistency multiplier c, rewards for consistency and streak
        double c = 0.8 + 0.4 * (0.5 * d1 / d2 + 0.5 * Math.Min(1, s / 30.0));
        
        return Math.Min(100.0, v / t * c * 100.0); 
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