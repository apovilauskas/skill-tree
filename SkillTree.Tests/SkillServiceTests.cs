using Moq;
using skill_tree.Common;
using skill_tree.DTOs;
using skill_tree.Entities;
using skill_tree.Repositories;
using skill_tree.Services;

namespace skill_tree.Tests;

public class SkillServiceTests
{
    private readonly Mock<ISkillRepository> _repository;
    private readonly ISkillService _service;

    public SkillServiceTests()
    {
        _repository = new Mock<ISkillRepository>();
        _service = new SkillService(_repository.Object);
    }

    private Skill CreateSkill(int id, SkillStatus status = SkillStatus.Locked, 
        List<SkillPrerequisite>? prerequisites = null)
    {
        return new Skill
        {
            Id = id,
            Status = status,
            Prerequisites = prerequisites ?? new List<SkillPrerequisite>()
        };
    }

    private SkillPrerequisite CreatePrerequisite(int skillId, int prerequisiteId, 
        Skill? prerequisiteSkill = null)
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
    public async Task CanStart_NonExistingSkill_ShouldReturnNotFound()
    {
        _repository.Setup(r => r.GetSkillAsync(-5)).ReturnsAsync((Skill?)null);

        var result = await _service.CanStartAsync(-5);

        Assert.Equal(CanStartResult.SkillNotFound, result);
    }

    [Fact]
    public async Task CanStart_IncompletePrerequisites_ShouldReturnLocked()
    {
        var prereqSkill = CreateSkill(1, SkillStatus.Locked);
        var skill = CreateSkill(10, SkillStatus.Locked, new List<SkillPrerequisite>
        {
            CreatePrerequisite(10, 1, prereqSkill)
        });

        _repository.Setup(r => r.GetSkillAsync(10)).ReturnsAsync(skill);

        var result = await _service.CanStartAsync(10);

        Assert.Equal(CanStartResult.LockedByPrerequisites, result);
    }

    [Fact]
    public async Task CanStart_CompletePrerequisites_ShouldReturnAvailable()
    {
        var prereqSkill = CreateSkill(5, SkillStatus.Completed);
        var skill = CreateSkill(20, SkillStatus.Locked, new List<SkillPrerequisite>
        {
            CreatePrerequisite(20, 5, prereqSkill)
        });

        _repository.Setup(r => r.GetSkillAsync(20)).ReturnsAsync(skill);

        var result = await _service.CanStartAsync(20);

        Assert.Equal(CanStartResult.Available, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_BadSkillId_ShouldReturnFalse()
    {
        _repository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        var dto = CreatePrerequisiteDto(1);
        var result = await _service.CreatePrerequisiteAsync(999, dto);

        Assert.Equal(CreatePrerequisiteResult.SkillNotFound, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_BadPrerequisiteId_ShouldReturnFalse()
    {
        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        var dto = CreatePrerequisiteDto(999);
        var result = await _service.CreatePrerequisiteAsync(10, dto);

        Assert.Equal(CreatePrerequisiteResult.SkillNotFound, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_SameSkillInSecondLayer_ShouldReturnFalse()
    {
        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(20)).ReturnsAsync(true);

        var graph = new Dictionary<int, List<int>>
        {
            { 10, new List<int>() },
            { 20, new List<int> { 10 } }
        };
        _repository.Setup(r => r.GetSkillPrerequisiteGraphAsync()).ReturnsAsync(graph);

        var dto = CreatePrerequisiteDto(20);
        var result = await _service.CreatePrerequisiteAsync(10, dto);

        Assert.Equal(CreatePrerequisiteResult.CircularDependencyDetected, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_SameSkillInThirdLayer_ReturnsFalse()
    {
        // 10 -> 20 -> 30 -> 10
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

        var graph = new Dictionary<int, List<int>>
        {
            { 10, new List<int>() },
            { 30, new List<int> { 10 } },
            { 20, new List<int> { 30 } }
        };
        _repository.Setup(r => r.GetSkillPrerequisiteGraphAsync()).ReturnsAsync(graph);

        var dto = CreatePrerequisiteDto(20);
        var result = await _service.CreatePrerequisiteAsync(10, dto);

        Assert.Equal(CreatePrerequisiteResult.CircularDependencyDetected, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_UniquePrerequisites_ReturnsSuccess()
    {
        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(50)).ReturnsAsync(true);

        var graph = new Dictionary<int, List<int>>
        {
            { 10, new List<int>() },
            { 50, new List<int>() }
        };
        _repository.Setup(r => r.GetSkillPrerequisiteGraphAsync()).ReturnsAsync(graph);

        var dto = CreatePrerequisiteDto(50);
        var result = await _service.CreatePrerequisiteAsync(10, dto);

        Assert.Equal(CreatePrerequisiteResult.Success, result);
    }
}