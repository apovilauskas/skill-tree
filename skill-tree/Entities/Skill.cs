namespace skill_tree.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillStatus Status { get; set; } = SkillStatus.NotStarted;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<SkillPrerequisite> Prerequisites { get; set; } = new();
}