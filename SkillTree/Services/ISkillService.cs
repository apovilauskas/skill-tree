using skill_tree.Entities;

namespace skill_tree.Services;

public interface ISkillService
{
    public double Progress(Skill skill);
    public bool CanStart(int skillId);
    public Task<IEnumerable<Skill>> GetAllSkillsAsync();
    public Task CreateSkillAsync(Skill skill);
    public Task<bool> CreatePrerequisitesAsync(int skillId, int prerequisiteId);
}