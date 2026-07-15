namespace skill_tree.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillStatus Status { get; set; } = SkillStatus.Locked;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<SkillPrerequisite> Prerequisites { get; set; } = new();
    public List<SkillLog> SkillLogs { get; set; } = new();
    public string Metric {get; set;} = string.Empty; //A label like "Matches", "Hours"
    public int Target { get; set; } = 100;
}