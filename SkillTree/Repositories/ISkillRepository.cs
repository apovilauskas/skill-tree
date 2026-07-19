using skill_tree.DTOs;
using skill_tree.Entities;

namespace skill_tree.Repositories;

public interface ISkillRepository
{
    public Task<IEnumerable<Skill>> GetAllAsync();
    public Task<IEnumerable<Skill>> GetAllSkillsWithPrerequisitesAsync();
    public Task AddAsync(Skill skill);
    public Task AddPrerequisitesAsync(SkillPrerequisite skillPrerequisite);
    public Task<bool> ExistsAsync(int id);
    public Task<IEnumerable<SkillLog>> GetLogsAsync(int id);
    public Task AddLogAsync(SkillLog skillLog);
    public Task<Skill?> GetSkillAsync(int skillId);
    public Task<IEnumerable<Skill>> GetCompletedSortedRecentSkillsAsync();
    public Task<IEnumerable<Skill>> GetUnlockedSkillsAsync();
    Task<Dictionary<int, List<int>>> GetSkillPrerequisiteGraphAsync();
}