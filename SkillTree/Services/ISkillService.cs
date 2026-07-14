using skill_tree.Entities;

namespace skill_tree.Services;

public interface ISkillService
{
    public double Progress(Skill skill);
    public bool CanStart(int skillId);
}