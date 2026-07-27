using skill_tree.Common;
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
    public Task<IEnumerable<SkillLog>> GetLogsAsync(int logId, string userId);
    public Task AddLogAsync(SkillLog skillLog, string userId);
    public Task<Skill?> GetSkillAsync(int skillId);
    public Task<IEnumerable<Skill>> GetCompletedSortedRecentSkillsAsync(string userId);
    public Task<IEnumerable<Skill>> GetUnlockedSkillsAsync(string userId);
    public Task<Dictionary<int, List<int>>> GetSkillPrerequisiteGraphAsync();
    public Task<IEnumerable<SkillRecommendation>> GetRecommendedSkills(string userId);
    public Task UpdateAsync(Skill skill, string userId);
    public Task SaveChangesAsync();
    
}