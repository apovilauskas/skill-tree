namespace skill_tree.Entities;

public class UserSkillProgress
{
    public int Id {get; set;}
    public string UserId { get; set; }
    public int SkillId { get; set; }
    public Skill Skill { get; set; }
    public SkillStatus SkillStatus { get; set; } 
}