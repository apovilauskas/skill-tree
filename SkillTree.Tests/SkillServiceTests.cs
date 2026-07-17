using Moq;
using skill_tree.Data;
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
    
}