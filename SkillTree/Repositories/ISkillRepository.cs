using skill_tree.Entities;

namespace skill_tree.Repositories;

public interface ISkillRepository
{
    public Task<IEnumerable<Skill>> GetAllAsync();
    public Task AddAsync(Skill skill);
    public Task<bool> AddPrerequisitesAsync(SkillPrerequisite skillPrerequisite);
    public Task<bool> ExistsAsync(int id);

}