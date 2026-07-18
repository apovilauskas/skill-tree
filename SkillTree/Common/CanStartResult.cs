namespace skill_tree.Common;

public enum CanStartResult
{
    Available, 
    LockedByPrerequisites,
    SkillNotFound
}