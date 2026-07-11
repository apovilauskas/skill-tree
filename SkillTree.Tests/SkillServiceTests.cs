using skill_tree.Entities;
using skill_tree.Service;

namespace skill_tree.tests;

public class SkillServiceTests
{
    [Fact]
    public void Progress_WithZeroLogs_ShouldReturnZero()
    {
        Skill testSkill = new Skill
        {
            SkillLogs = new List<SkillLog>(),
            Metric = "hour",
            Target = 40
        };
        
        SkillTreeService skillTreeService = new SkillTreeService();
        double progress = skillTreeService.Progress(testSkill);
        Assert.Equal(0, progress);
    }

    private Skill CreateSkillWithStreak(int daysInARow)
    {
        Skill skill = new Skill
        {
            SkillLogs = new List<SkillLog>(),
            CreatedAt =  DateTime.UtcNow.AddDays(-daysInARow),
            Metric = "attempt",
            Target = 90
        };

        for (int i = 0; i < daysInARow; i++)
        {
            SkillLog skillLog = new SkillLog
            {
                Id = 0,
                SkillId = 0,
                Skill = null,
                Amount = 1.5,
                Date = DateTime.UtcNow.Date.AddDays(-i),
            };
            skill.SkillLogs.Add(skillLog);
        }

        return skill;
    }

    [Fact]
    public void Progress_With30DayStreak_ShouldCalculateMaxConsistency()
    {
        Skill skill = CreateSkillWithStreak(30);
        SkillTreeService skillTreeService = new SkillTreeService();
        double progress = skillTreeService.Progress(skill);
        Assert.Equal(60, progress, 2);
    }
    
    
}