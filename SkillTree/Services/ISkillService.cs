using skill_tree.Common;
using skill_tree.DTOs;

namespace skill_tree.Services;

public interface ISkillService
{
    public Task<CanStartResult> CanStartAsync(int skillId);
    public Task<IEnumerable<SkillResponseDto>> GetAllSkillsAsync();
    public Task<SkillResponseDto> CreateSkillAsync(CreateSkillDto skill);
    public Task<CreatePrerequisiteResult> CreatePrerequisiteAsync(int skillId, PrerequisiteIdDto prerequisiteId);
    public Task<IEnumerable<SkillLogResponseDto>> GetSkillLogsAsync(int skillId);
    public Task<bool> CreateSkillLogAsync(int skillId, CreateSkillLogDto skillLog);
    public Task<IEnumerable<UnlockedSkillResponseDto>> GetUnlockedSkillsAsync();
}