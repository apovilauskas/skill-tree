using skill_tree.DTOs;
using skill_tree.Entities;

namespace skill_tree.SkillMappingExtensions;

public static class SkillMappingExtensions
{
    public static Skill ToEntity(this CreateSkillDto dto)
    {
        var entity = new Skill
        {
            Name = dto.Name,
            Description = dto.Description,
            Metric = dto.Metric,
            Target = dto.Target?? 100
        };
        return entity;
    }

    public static SkillResponseDto ToDto(this Skill entity)
    {
        List<PrerequisiteInfoResponseDto> infos = entity.Prerequisites.Select(s=> s.ToDto()).ToList();
        
        var dto = new SkillResponseDto()
        {
            Name = entity.Name,
            Description = entity.Description,
            Metric = entity.Metric,
            Target = entity.Target,
            Status =  entity.Status,
            Id =  entity.Id,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt,
            Progress = entity.Progress(),
            PrerequisitesInfo =  infos
        };
        return dto;
    }

    public static SkillLog ToEntity(this CreateSkillLogDto dto)
    {
        var entity = new SkillLog
        {
            Amount = dto.Amount,
            Note = dto.Note
        };
        return entity;
    }

    public static SkillLogResponseDto ToDto(this SkillLog entity)
    {
        var dto = new SkillLogResponseDto
        {
            Id =  entity.Id,
            Amount = entity.Amount,
            Note =  entity.Note,
            CreatedAt = entity.Date
        };
        return dto;
    }

    public static PrerequisiteInfoResponseDto ToDto(this SkillPrerequisite entity)
    {
        var dto = new PrerequisiteInfoResponseDto
        {
            Id =  entity.PrerequisiteId,
            Description = entity.Prerequisite.Description,
            Name = entity.Prerequisite.Name,
            Status = entity.Prerequisite.Status
        };
        return dto;
    }
}