using skill_tree.Common;
using skill_tree.DTOs;
using skill_tree.Entities;

namespace skill_tree.Repositories;

public interface ISkillRepository
{
    public Task<IEnumerable<Skill>> GetAllSkillsWithPrerequisitesAsync();
    public Task AddAsync(Skill skill);
    public Task AddPrerequisitesAsync(SkillPrerequisite skillPrerequisite);
    public Task<bool> ExistsAsync(int id);
    public Task<IEnumerable<SkillLog>> GetLogsAsync(int logId, string userId);
    public Task<Dictionary<int, List<SkillLog>>> GetLogsBySkillIdsAsync(string userId, IEnumerable<int> skillIds);
    public Task AddLogAsync(SkillLog skillLog, string userId);
    public Task<Skill?> GetSkillAsync(int skillId);
    public Task<IEnumerable<UserSkillProgress>> GetCompletedSortedRecentSkillsAsync(string userId);
    public Task<IEnumerable<UserSkillProgress>> GetUnlockedSkillsAsync(string userId);
    public Task<Dictionary<int, List<int>>> GetSkillPrerequisiteGraphAsync();
    public Task<IEnumerable<SkillRecommendation>> GetRecommendedSkills(string userId);
    public Task UpdateAsync(int skillId, string userId, SkillStatus newStatus);
    public Task<UserSkillProgress?> GetUserSkillProgressAsync(string userId, int skillId);
    public Task<UserSkillProgress> AddUserSkillProgressAsync(string userId, int skillId);
    public Task<IEnumerable<int>> GetCompletedSkillsIds(string userId);
    public Task<Dictionary<int, UserSkillProgress>> GetAllUserSkillProgressesAsync(string userId);
}