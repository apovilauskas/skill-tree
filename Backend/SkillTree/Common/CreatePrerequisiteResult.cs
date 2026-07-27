namespace skill_tree.Common;

public enum CreatePrerequisiteResult
{
    Success, 
    SkillNotFound, 
    CircularDependencyDetected
}