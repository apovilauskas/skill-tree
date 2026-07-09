namespace skill_tree.Entities;

public class SkillPrerequisite
{
    public int Id { get; set; }
    
    public int SkillId { get; set; }
    public Skill? Skill { get; set; }
    public int PrerequisiteId { get; set; }
    public Skill? Prerequisite { get; set; }
}