using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<IActionResult> GetAllSkills()
    {
        var skills = await _skillService.GetAllSkillsAsync();
        return Ok(skills);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSkill(CreateSkillDto skill)
    {
        SkillResponseDto response = await _skillService.CreateSkillAsync(skill);
        return Ok(response);
    }

    [HttpPost("{skillId}/prerequisites")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePrerequisite(int skillId, [FromBody] PrerequisiteIdDto prerequisiteId)
    {
        var result =  await _skillService.CreatePrerequisiteAsync(skillId, prerequisiteId);
        if(result == CreatePrerequisiteResult.SkillNotFound) return NotFound("Skill not found");
        if(result == CreatePrerequisiteResult.CircularDependencyDetected) return BadRequest("Circular dependency detected");
        return Ok("Prerequisite added");
    }

    [HttpGet("{skillId}/logs")]
    [Authorize]
    public async Task<IActionResult> GetSkillLogs(int skillId)
    {
        var logs = await _skillService.GetSkillLogsAsync(skillId);
        if(logs == null) return NotFound("Skill not found");
        return Ok(logs);
    }

    [HttpPost("{skillId}/logs")]
    [Authorize]
    public async Task<IActionResult> CreateSkillLog(int skillId, [FromBody] CreateSkillLogDto skillLog)
    {
        if (!await _skillService.CreateSkillLogAsync(skillId, skillLog))
        {
            return NotFound("Skill not found");
        }
        return Ok("Log added");
    }

    [HttpGet("canStart/{skillId}")]
    [Authorize]
    public async Task<IActionResult> CanStartAsync(int skillId)
    {
        var response = await _skillService.CanStartAsync(skillId);
        if(response == CanStartResult.SkillNotFound) return NotFound("Skill not found");
        if(response == CanStartResult.LockedByPrerequisites) return BadRequest("Skill is locked");
        return Ok("Can Start");
    }

    [HttpGet("unlocked")]
    [Authorize]
    public async Task<IActionResult> Unlocked()
    {
        var response = await _skillService.GetUnlockedSkillsAsync();
        return Ok(response);
    }

    [HttpGet("completed")]
    [Authorize]
    public async Task<IActionResult> Completed()
    {
        var response = await _skillService.GetCompletedSkillsAsync();
        return Ok(response);
    }

    [HttpGet("recommended")]
    [Authorize]
    public async Task<IActionResult> Recommended()
    {
        var response = await _skillService.GetRecommendationsAsync();
        return Ok(response);
    }

    [HttpDelete("deleteSkill/{skillId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSkill(int skillId)
    {
        var response = await _skillService.DeleteSkillAsync(skillId);
        if (!response) return NotFound("Skill not found");
        return Ok("Skill deleted");
    }
    
    [HttpPut("editSkill/{skillId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditSkill(int skillId, [FromBody] EditSkillDto editSkillDto)
    {
        var response = await _skillService.EditSkillAsync(skillId, editSkillDto);
        if (!response) return NotFound("Skill not found");
        return Ok("Skill edited");
    }
    
    [HttpDelete("deletePrerequisite/{skillPrerequisiteId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePrerequisite(int skillPrerequisiteId)
    {
        var response = await _skillService.DeletePrerequisiteAsync(skillPrerequisiteId);
        if (!response) return NotFound("Relationship not found"); 
        return Ok("Prerequisite removed");
    }
    
}