using skill_tree.Entities;

namespace skill_tree.Common;

public class SkillRecommendation
{
    public Skill Skill { get; set; }
    public DateTime? LastLog {get; set;}
    public int UnlockCount  {get; set;}
}