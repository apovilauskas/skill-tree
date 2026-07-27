namespace skill_tree.DTOs;

public class SkillLogResponseDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Note { get; set; }
    public double Amount { get; set; }
}