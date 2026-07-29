namespace skill_tree.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<SkillPrerequisite> Prerequisites { get; set; } = new();
    public string Metric {get; set;} = string.Empty; //A label like "Matches", "Hours"
    public double Target { get; set; } = 100;
}