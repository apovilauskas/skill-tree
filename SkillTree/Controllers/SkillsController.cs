using Microsoft.AspNetCore.Mvc;
using skill_tree.Data;
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
    public async Task<IActionResult> CreateSkill(Skill skill)
    {
        await _skillService.CreateSkillAsync(skill);
        return CreatedAtAction(nameof(GetAllSkills), new { id = skill.Id }, skill);
    }

    [HttpPost("{skillId}/prerequisites")]
    public async Task<IActionResult> CreatePrerequisites(int skillId, [FromBody] int prerequisiteId)
    {
        if(!await _skillService.CreatePrerequisitesAsync(skillId, prerequisiteId)) return NotFound("Skill not found");
        return Ok("Prerequisite added");
    }
}