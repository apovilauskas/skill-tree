using skill_tree.Entities;

namespace skill_tree.DTOs;

public class SkillResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillStatus Status { get; set; } = SkillStatus.Locked;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string Metric {get; set;} = string.Empty;
    public double Target { get; set; } = 100;
    public List<PrerequisiteInfoResponseDto> PrerequisitesInfo { get; set; }
}