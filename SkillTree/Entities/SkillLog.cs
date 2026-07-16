namespace skill_tree.Entities;

public class SkillLog
{
    public int Id { get; set; }
    public int SkillId { get; set; }
    public Skill? Skill { get; set; }
    public double Amount { get; set; } = 0;
    public string? Note { get; set; }
    public DateTime Date { get; set; } =  DateTime.UtcNow;
}