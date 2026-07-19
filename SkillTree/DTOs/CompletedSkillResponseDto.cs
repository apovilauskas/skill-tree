namespace skill_tree.DTOs;

public class CompletedSkillResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Metric {get; set;} = string.Empty;
    public double Target { get; set; } = 100;
    public DateTime CompletedAt { get; set; }
}