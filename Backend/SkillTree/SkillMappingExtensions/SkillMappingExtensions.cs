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
            Target = dto.Target ?? 100
        };
        return entity;
    }

    public static SkillResponseDto ToDto(this Skill entity, UserSkillProgress? userProgress, double progressValue, IReadOnlyDictionary<int, SkillStatus> prerequisiteStatuses)
    {
        List<PrerequisiteInfoResponseDto> infos = entity.Prerequisites
            .Select(p => p.ToDto(prerequisiteStatuses.GetValueOrDefault(p.PrerequisiteId, SkillStatus.Locked)))
            .ToList();
        var dto = new SkillResponseDto
        {
            Name = entity.Name,
            Description = entity.Description,
            Metric = entity.Metric,
            Target = entity.Target,
            Status = userProgress?.SkillStatus ?? SkillStatus.Locked,
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            CompletedAt = userProgress?.CompletedAt,
            Progress = progressValue,
            PrerequisitesInfo = infos
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
            Id = entity.Id,
            Amount = entity.Amount,
            Note = entity.Note,
            CreatedAt = entity.Date
        };
        return dto;
    }

    public static PrerequisiteInfoResponseDto ToDto(this SkillPrerequisite entity, SkillStatus prerequisiteStatus)
    {
        var dto = new PrerequisiteInfoResponseDto
        {
            Id = entity.PrerequisiteId,
            Description = entity.Prerequisite.Description,
            Name = entity.Prerequisite.Name,
            Status = prerequisiteStatus
        };
        return dto;
    }

    public static UnlockedSkillResponseDto ToUnlockedDto(this Skill entity, double progressValue)
    {
        var dto = new UnlockedSkillResponseDto
        {
            Name = entity.Name,
            Description = entity.Description,
            Metric = entity.Metric,
            Target = entity.Target,
            Id = entity.Id,
            Progress = progressValue,
        };
        return dto;
    }

    public static CompletedSkillResponseDto ToCompletedDto(this Skill entity, DateTime completedAt)
    {
        var dto = new CompletedSkillResponseDto
        {
            Name = entity.Name,
            Description = entity.Description,
            Metric = entity.Metric,
            Target = entity.Target,
            Id = entity.Id,
            CompletedAt = completedAt,
        };
        return dto;
    }
}