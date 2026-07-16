namespace skill_tree.DTOs;

public class PrerequisiteInfoResponseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public int Id { get; set; }
}