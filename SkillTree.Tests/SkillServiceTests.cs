using skill_tree.Entities;
using skill_tree.Services;

namespace skill_tree.tests;

public class SkillServiceTests
{
    private readonly SkillTreeService _skillTreeService = new SkillTreeService();
    
    [Fact]
    public void Progress_WithZeroLogs_ShouldReturnZero()
    {
        Skill testSkill = new Skill
        {
            SkillLogs = new List<SkillLog>(),
            Metric = "hour",
            Target = 40
        };
        
        double progress = _skillTreeService.Progress(testSkill);
        Assert.Equal(0, progress);
    }

    [Fact]
    public void Progress_With30DayStreak_ShouldCalculateMaxConsistency()
    {
        Skill skill = CreateSkillWithStreak(30, 1, 1.5);
        double progress = _skillTreeService.Progress(skill);
        Assert.Equal(60, progress, 2);
    }
    
    [Fact]
    public void Progress_With30DaysStreakWithMultipleLogsDaily_ShouldCalculateMaxConsistency()
    {
        Skill skill = CreateSkillWithStreak(30, 3, 0.5);
        double progress = _skillTreeService.Progress(skill);
        Assert.Equal(60, progress, 2);
    }
    
    private Skill CreateSkillWithStreak(int daysInARow, int logsPerDay, double amountPerLog)
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
            for (int j = 0; j < logsPerDay; j++)
            {
                SkillLog skillLog = new SkillLog
                {
                    Id = 0, 
                    SkillId = 0,
                    Skill = null,
                    Amount = amountPerLog,
                    Date = DateTime.UtcNow.Date.AddDays(-i),
                }; 
                skill.SkillLogs.Add(skillLog);
            }
        }
        return skill;
    }
    
}