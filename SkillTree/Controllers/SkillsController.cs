using Microsoft.AspNetCore.Mvc;
using skill_tree.Common;
using skill_tree.DTOs;
using skill_tree.Entities;
using skill_tree.Services;

namespace skill_tree.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;
    
    public SkillsController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSkills()
    {
        var skills = await _skillService.GetAllSkillsAsync();
        return Ok(skills);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSkill(CreateSkillDto skill)
    {
        SkillResponseDto response = await _skillService.CreateSkillAsync(skill);
        return Ok(response);
    }

    [HttpPost("{skillId}/prerequisites")]
    public async Task<IActionResult> CreatePrerequisites(int skillId, [FromBody] PrerequisiteIdDto prerequisiteId)
    {
        var result =  await _skillService.CreatePrerequisiteAsync(skillId, prerequisiteId);
        if(result == CreatePrerequisiteResult.SkillNotFound) return NotFound("Skill not found");
        if(result == CreatePrerequisiteResult.CircularDependencyDetected) return BadRequest("Circular dependency detected");
        return Ok("Prerequisite added");
    }

    [HttpGet("{skillId}/logs")]
    public async Task<IActionResult> GetSkillLogs(int skillId)
    {
        var logs = await _skillService.GetSkillLogsAsync(skillId);
        if(logs == null) return NotFound("Skill not found");
        return Ok(logs);
    }

    [HttpPost("{skillId}/logs")]
    public async Task<IActionResult> CreateSkillLog(int skillId, [FromBody] CreateSkillLogDto skillLog)
    {
        if (!await _skillService.CreateSkillLogAsync(skillId, skillLog))
        {
            return NotFound("Skill not found");
        }
        return Ok("Log added");
    }

    [HttpGet("canStart/{skillId}")]
    public async Task<IActionResult> CanStartAsync(int skillId)
    {
        var response = await _skillService.CanStartAsync(skillId);
        if(response == CanStartResult.SkillNotFound) return NotFound("Skill not found");
        if(response == CanStartResult.LockedByPrerequisites) return BadRequest("Skill is locked");
        return Ok("Can Start");
    }

    [HttpGet("unlocked")] //in progress, but not completed skills
    public async Task<IActionResult> Unlocked()
    {
        var response = await _skillService.GetUnlockedSkillsAsync();
        return Ok(response);
    }
}