using skill_tree.Entities;

namespace skill_tree.Common;

public class SkillRecommendation
{
    public Skill Skill { get; set; }
    public DateTime? LastLog {get; set;} // the last log for that user for that skill
    public int UnlockCount  {get; set;}
    public DateTime? StartedAt { get; set; }
}