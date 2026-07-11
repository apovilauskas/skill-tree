using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skill_tree.Data;
using skill_tree.Entities;

namespace skill_tree.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private SkillDbContext _context;
    
    public SkillsController(SkillDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSkills()
    {
        var skills = await _context.Skills.ToListAsync();
        return Ok(skills);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSkill(Skill skill)
    {
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAllSkills), new { id = skill.Id }, skill);
    }

    [HttpPost("{skillId}/prerequisites")]
    public async Task<IActionResult> CreatePrerequisites(int skillId, [FromBody] int prerequisiteId)
    {
        if (!await _context.Skills.AnyAsync(s => s.Id == skillId) || !await _context.Skills.AnyAsync(p => p.Id == prerequisiteId)) return NotFound("Skill not found");
        var skillPrerequisite = new SkillPrerequisite()
        {
            SkillId = skillId,
            PrerequisiteId = prerequisiteId
        };
                
        _context.Prerequisites.Add(skillPrerequisite);
        await _context.SaveChangesAsync();
        return Ok("Prerequisite added");
    }
}