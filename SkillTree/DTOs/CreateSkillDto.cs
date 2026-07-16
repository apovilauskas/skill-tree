namespace skill_tree.DTOs;

public class CreateSkillDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Metric { get; set; } = string.Empty;
    public int? Target  { get; set; }
}