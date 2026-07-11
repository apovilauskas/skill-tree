namespace skill_tree.Entities;

public class SkillLog
{
    public int Id { get; set; }
    public int SkillId { get; set; }
    public Skill? Skill { get; set; }
    public double Amount { get; set; } 
    public string Note { get; set; } = string.Empty;
    public DateTime Date { get; set; } =  DateTime.UtcNow;
}