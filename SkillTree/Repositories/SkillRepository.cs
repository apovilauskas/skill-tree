using Microsoft.EntityFrameworkCore;
using skill_tree.Data;
using skill_tree.Entities;

namespace skill_tree.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly SkillDbContext _context;

    public SkillRepository(SkillDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Skill>> GetAllAsync()
    {
        return await _context.Skills.ToListAsync();
    }

    public async Task AddAsync(Skill skill)
    {
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
    }

    public async Task AddPrerequisitesAsync(SkillPrerequisite skillPrerequisite)
    {
        _context.Prerequisites.Add(skillPrerequisite);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Skills.AnyAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<SkillLog>> GetLogsAsync(int id)
    {
        return await _context.SkillLogs.Where(l => l.SkillId == id).ToListAsync();
    }

    public async Task AddLogAsync(SkillLog skillLog)
    {
        _context.SkillLogs.Add(skillLog);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CanStart(int skillId)
    {
        var skill = await _context.Skills
            .Include(skill => skill.Prerequisites)
            .ThenInclude(p => p.Prerequisite)
            .FirstOrDefaultAsync(s => s.Id == skillId);
        if (skill == null) return false;
        return skill.Prerequisites.All(sp => sp.Prerequisite.Status == SkillStatus.Completed);
    }
}