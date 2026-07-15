using skill_tree.Entities;
using skill_tree.Repositories;

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

    public async Task<IEnumerable<Skill>> GetAllSkillsAsync()
    {
       return await _repository.GetAllAsync();
    }

    public async Task CreateSkillAsync(Skill skill)
    {
        await _repository.AddAsync(skill);
    }

    public async Task<bool> CreatePrerequisitesAsync(int skillId, int prerequisiteId)
    {
        if (!await _repository.ExistsAsync(skillId) || !await _repository.ExistsAsync(prerequisiteId))
        {
            return false;
        }
        var skillPrerequisite = new SkillPrerequisite()
        {
            SkillId = skillId,
            PrerequisiteId = prerequisiteId
        };
        await _repository.AddPrerequisitesAsync(skillPrerequisite);
        return true;
    }

    public async Task<IEnumerable<SkillLog>> GetSkillLogsAsync(int skillId)
    {
        if(!await _repository.ExistsAsync(skillId)) return null;
        return await _repository.GetLogsAsync(skillId);
    }

    public async Task<bool> CreateSkillLogAsync(SkillLog skillLog)
    {
        if (! await _repository.ExistsAsync(skillLog.SkillId))
        {
            return false;
        }
        await _repository.AddLogAsync(skillLog);
        return true;
    }
    
    public async Task<bool> CanStart(int skillId)
    {
        return await _repository.CanStart(skillId);
    }
}