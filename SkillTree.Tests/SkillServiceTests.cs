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
}