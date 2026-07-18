using Moq;
using skill_tree.DTOs;
using skill_tree.Entities;
using skill_tree.Repositories;
using skill_tree.Services;

namespace skill_tree.tests;

public class SkillServiceTests
{
    private readonly Mock<ISkillRepository> _repository;
    private readonly ISkillService _service;

    public SkillServiceTests()
    {
        _repository = new Mock<ISkillRepository>();
        _service = new SkillService(_repository.Object);
    }

    private Skill CreateSkill(int id, SkillStatus status = SkillStatus.Locked, List<SkillPrerequisite>? prerequisites = null)
    {
        return new Skill
        {
            Id = id,
            Status = status,
            Prerequisites = prerequisites ?? new List<SkillPrerequisite>()
        };
    }

    private SkillPrerequisite CreatePrerequisite(int skillId, int prerequisiteId, Skill? prerequisiteSkill = null)
    {
        return new SkillPrerequisite
        {
            SkillId = skillId,
            PrerequisiteId = prerequisiteId,
            Prerequisite = prerequisiteSkill ?? new Skill { Id = prerequisiteId }
        };
    }

    private PrerequisiteIdDto CreatePrerequisiteDto(int prerequisiteId)
    {
        return new PrerequisiteIdDto { Id = prerequisiteId };
    }

    [Fact]
    public async Task CanStart_WithNonExistingSkill_ShouldReturnNotFound()
    {
        _repository.Setup(r => r.GetSkillAsync(-5)).ReturnsAsync((Skill?)null);
        var result = await _service.CanStart(-5);
        Assert.Equal(SkillStatus.NotFound, result);
    }

    [Fact]
    public async Task CanStart_WithIncompletePrerequisites_ShouldReturnLocked()
    {
        var prereqSkill = CreateSkill(1, SkillStatus.Locked);
        var skill = CreateSkill(10, SkillStatus.Locked, new List<SkillPrerequisite>
        {
            CreatePrerequisite(10, 1, prereqSkill)
        });

        _repository.Setup(r => r.GetSkillAsync(10)).ReturnsAsync(skill);

        var result = await _service.CanStart(10);
        Assert.Equal(SkillStatus.Locked, result);
    }

    [Fact]
    public async Task CanStart_WithCompletePrerequisites_ShouldReturnInProgress()
    {
        var prereqSkill = CreateSkill(5, SkillStatus.Completed);
        var skill = CreateSkill(20, SkillStatus.Locked, new List<SkillPrerequisite>
        {
            CreatePrerequisite(20, 5, prereqSkill)
        });

        _repository.Setup(r => r.GetSkillAsync(20)).ReturnsAsync(skill);

        var result = await _service.CanStart(20);
        Assert.Equal(SkillStatus.InProgress, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_WithBadSkillId_ShouldReturnFalse()
    {
        _repository.Setup(r => r.ExistsAsync(999))
                   .ReturnsAsync(false);

        var dto = CreatePrerequisiteDto(1);
        var result = await _service.CreatePrerequisiteAsync(999, dto);
        Assert.False(result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_WithBadPrerequisiteId_ShouldReturnFalse()
    {
        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        var dto = CreatePrerequisiteDto(999);
        var result = await _service.CreatePrerequisiteAsync(10, dto);
        Assert.False(result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_WithSameSkillInSecondLayer_ShouldReturnFalse()
    {
        // 10 -> 20 -> 10
        var skill10 = CreateSkill(10);
        var skill20 = CreateSkill(20, SkillStatus.Locked, new List<SkillPrerequisite>
        {
            CreatePrerequisite(20, 10, skill10)
        });

        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(20)).ReturnsAsync(true);
        _repository.Setup(r => r.GetAllSkillsWithPrerequisitesAsync()).ReturnsAsync(new List<Skill> { skill10, skill20 });

        var dto = CreatePrerequisiteDto(20);
        var result = await _service.CreatePrerequisiteAsync(10, dto);
        Assert.False(result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_WithSameSkillInThirdLayer_ShouldReturnFalse()
    {
        // Cycle: 10 -> 20 -> 30 -> 10
        var skill10 = CreateSkill(10);
        var skill30 = CreateSkill(30, SkillStatus.Locked, new List<SkillPrerequisite>
        {
            CreatePrerequisite(30, 10, skill10)
        });
        var skill20 = CreateSkill(20, SkillStatus.Locked, new List<SkillPrerequisite>
        {
            CreatePrerequisite(20, 30, skill30)
        });

        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(20)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(30)).ReturnsAsync(true);
        _repository.Setup(r => r.GetAllSkillsWithPrerequisitesAsync())
                   .ReturnsAsync(new List<Skill> { skill10, skill20, skill30 });

        var dto = CreatePrerequisiteDto(20);
        var result = await _service.CreatePrerequisiteAsync(10, dto);
        Assert.False(result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_WithUniquePrerequisites_ShouldReturnTrue()
    {
        var skill10 = CreateSkill(10);
        var skill50 = CreateSkill(50);

        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(50)).ReturnsAsync(true);
        _repository.Setup(r => r.GetAllSkillsWithPrerequisitesAsync())
                   .ReturnsAsync(new List<Skill> { skill10, skill50 });

        var dto = CreatePrerequisiteDto(50);
        var result = await _service.CreatePrerequisiteAsync(10, dto);
        Assert.True(result);
    }
}