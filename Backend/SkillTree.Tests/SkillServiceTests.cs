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
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly ISkillService _service;
    private readonly string _testUserId = "AugustasId";

    public SkillServiceTests()
    {
        _repository = new Mock<ISkillRepository>();
        _currentUserService = new Mock<ICurrentUserService>();
        _currentUserService.Setup(c => c.GetUserId()).Returns(_testUserId);
        _service = new SkillService(_repository.Object, _currentUserService.Object);
    }

    private Skill CreateSkill(int id, double target = 100, List<SkillPrerequisite>? prerequisites = null)
    {
        return new Skill
        {
            Id = id,
            Target = target,
            Prerequisites = prerequisites ?? new List<SkillPrerequisite>()
        };
    }

    private SkillPrerequisite CreatePrerequisite(int skillId, int prerequisiteId)
    {
        return new SkillPrerequisite
        {
            SkillId = skillId,
            PrerequisiteId = prerequisiteId
        };
    }

    private PrerequisiteIdDto CreatePrerequisiteIdDto(int prerequisiteId)
    {
        return new PrerequisiteIdDto { Id = prerequisiteId };
    }

    private List<SkillLog> CreateLogsWithStreak(int daysInARow, int logsPerDay, double amountPerLog)
    {
        var logs = new List<SkillLog>();
        for (int i = 0; i < daysInARow; i++)
        {
            for (int j = 0; j < logsPerDay; j++)
            {
                logs.Add(new SkillLog
                {
                    Amount = amountPerLog,
                    Date = DateTime.UtcNow.Date.AddDays(-i)
                });
            }
        }
        return logs;
    }

    [Fact]
    public async Task CanStart_NonExistingSkill_ReturnsNotFound()
    {
        _repository.Setup(r => r.GetSkillAsync(-5)).ReturnsAsync((Skill?)null);
        var result = await _service.CanStartAsync(-5);
        Assert.Equal(CanStartResult.SkillNotFound, result);
    }

    [Fact]
    public async Task CanStart_IncompletePrerequisites_ReturnsLocked()
    {
        var skill = CreateSkill(10, 100, new List<SkillPrerequisite>
        {
            CreatePrerequisite(10, 1) // requires skill id 1
        });
        _repository.Setup(r => r.GetSkillAsync(10)).ReturnsAsync(skill);
        _repository.Setup(r => r.GetCompletedSkillsIds(_testUserId)).ReturnsAsync(new List<int>()); // mock the user having no completed skills
        var result = await _service.CanStartAsync(10);
        Assert.Equal(CanStartResult.LockedByPrerequisites, result);
    }

    [Fact]
    public async Task CanStart_CompletePrerequisites_ReturnsAvailable()
    {
        var skill = CreateSkill(20, 100, new List<SkillPrerequisite>
        {
            CreatePrerequisite(20, 5) // requires id 5
        });
        _repository.Setup(r => r.GetSkillAsync(20)).ReturnsAsync(skill);
        _repository.Setup(r => r.GetCompletedSkillsIds(_testUserId)).ReturnsAsync(new List<int> {5});        // mock the user having completed skill ID 5
        var result = await _service.CanStartAsync(20);
        Assert.Equal(CanStartResult.Available, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_BadSkillId_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);
        var dto = CreatePrerequisiteIdDto(1);
        var result = await _service.CreatePrerequisiteAsync(999, dto);
        Assert.Equal(CreatePrerequisiteResult.SkillNotFound, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_BadPrerequisiteId_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);
        var dto = CreatePrerequisiteIdDto(999);
        var result = await _service.CreatePrerequisiteAsync(10, dto);
        Assert.Equal(CreatePrerequisiteResult.SkillNotFound, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_SameSkillInSecondLayer_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(20)).ReturnsAsync(true);
        var graph = new Dictionary<int, List<int>>
        {
            {10, new List<int>()},
            {20, new List<int> {10}}
        };
        _repository.Setup(r => r.GetSkillPrerequisiteGraphAsync()).ReturnsAsync(graph);
        var dto = CreatePrerequisiteIdDto(20);
        var result = await _service.CreatePrerequisiteAsync(10, dto);
        Assert.Equal(CreatePrerequisiteResult.CircularDependencyDetected, result);
    }

    [Fact]
    public async Task CreatePrerequisiteAsync_SameSkillInThirdLayer_ReturnsFalse()
    {
        _repository.Setup(r => r.ExistsAsync(10)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(20)).ReturnsAsync(true);
        _repository.Setup(r => r.ExistsAsync(30)).ReturnsAsync(true);
        
        var graph = new Dictionary<int, List<int>>
        {
            {10, new List<int>()},
            {30, new List<int> {10}},
            {20, new List<int> {30}}
        };
        _repository.Setup(r => r.GetSkillPrerequisiteGraphAsync()).ReturnsAsync(graph);
        var dto = CreatePrerequisiteIdDto(20);
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
            {10, new List<int>() },
            {50, new List<int>() }
        };
        _repository.Setup(r => r.GetSkillPrerequisiteGraphAsync()).ReturnsAsync(graph);
        var dto = CreatePrerequisiteIdDto(50);
        var result = await _service.CreatePrerequisiteAsync(10, dto);
        Assert.Equal(CreatePrerequisiteResult.Success, result);
    }

    [Fact]
    public async Task GetUnlockedSkills_ZeroLogs_ReturnsZeroProgress()
    {
        var skill = CreateSkill(1, target: 40);
        var progress = new UserSkillProgress { SkillId = 1, Skill = skill, StartedAt = DateTime.UtcNow };
        
        _repository.Setup(r => r.GetUnlockedSkillsAsync(_testUserId)).ReturnsAsync(new List<UserSkillProgress> { progress });
        _repository.Setup(r => r.GetLogsBySkillIdsAsync(_testUserId, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new Dictionary<int, List<SkillLog>>()); // zero logs

        var result = await _service.GetUnlockedSkillsAsync();
        var dto = result.First();
        Assert.Equal(0, dto.Progress);
    }

    [Fact]
    public async Task GetUnlockedSkills_30DayStreak_CalculatesMaxConsistency()
    {
        var skill = CreateSkill(2, target: 90);
        var progress = new UserSkillProgress { SkillId = 2, Skill = skill, StartedAt = DateTime.UtcNow.AddDays(-30) };
        var logs = CreateLogsWithStreak(30, 1, 1.5);
        
        _repository.Setup(r => r.GetUnlockedSkillsAsync(_testUserId)).ReturnsAsync(new List<UserSkillProgress> { progress });
        _repository.Setup(r => r.GetLogsBySkillIdsAsync(_testUserId, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new Dictionary<int, List<SkillLog>> { { 2, logs } });

        var result = await _service.GetUnlockedSkillsAsync();
        var dto = result.First();
        Assert.Equal(60, dto.Progress, 2);
    }
    
    [Fact]
    public async Task GetUnlockedSkills_30DaysStreakWithMultipleLogsDaily_CalculatesMaxConsistency()
    {
        var skill = CreateSkill(3, target: 90);
        var progress = new UserSkillProgress { SkillId = 3, Skill = skill, StartedAt = DateTime.UtcNow.AddDays(-30) };
        var logs = CreateLogsWithStreak(30, 3, 0.5);
        
        _repository.Setup(r => r.GetUnlockedSkillsAsync(_testUserId)).ReturnsAsync(new List<UserSkillProgress> { progress });
        _repository.Setup(r => r.GetLogsBySkillIdsAsync(_testUserId, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new Dictionary<int, List<SkillLog>> {{3,logs}});

        var result = await _service.GetUnlockedSkillsAsync();
        var dto = result.First();
        Assert.Equal(60, dto.Progress, 2);
    }
}