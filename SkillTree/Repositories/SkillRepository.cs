using Microsoft.EntityFrameworkCore;
using skill_tree.Common;
using skill_tree.Data;
using skill_tree.DTOs;
using skill_tree.Entities;
using skill_tree.SkillMappingExtensions;

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
    
    public async Task<IEnumerable<Skill>> GetAllSkillsWithPrerequisitesAsync()
    {
        return await _context.Skills
            .Include(s => s.Prerequisites)
            .ToListAsync();
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
    
    public async Task<Skill?> GetSkillAsync(int skillId)
    {
        return await _context.Skills
            .Include(skill => skill.Prerequisites)
            .ThenInclude(p => p.Prerequisite)
            .Include(skill => skill.SkillLogs)
            .FirstOrDefaultAsync(s => s.Id == skillId);
    }

    public async Task<IEnumerable<Skill>> GetCompletedSortedRecentSkillsAsync()
    {
        return await _context.Skills
            .Where(d => d.Status == SkillStatus.Completed)
            .OrderByDescending(s => s.CompletedAt)
            .Take(10).ToListAsync();
    }

    public async Task<IEnumerable<Skill>> GetUnlockedSkillsAsync()
    {
        return await _context.Skills
            .Where(s => s.Status == SkillStatus.InProgress || s.Status == SkillStatus.Locked)
            .Where(s => s.Prerequisites.All(p => p.Prerequisite.Status == SkillStatus.Completed))
            .ToListAsync();
    }
    
    public async Task<Dictionary<int, List<int>>> GetSkillPrerequisiteGraphAsync()
    {
        var data = await _context.Skills
            .Select(s => new 
            {
                SkillId = s.Id,
                PrerequisiteIds = s.Prerequisites
                    .Select(p => p.PrerequisiteId)
                    .ToList()
            })
            .ToListAsync();

        return data.ToDictionary(
            x => x.SkillId,
            x => x.PrerequisiteIds
        );
    }

    public async Task<IEnumerable<SkillRecommendation>> GetRecommendedSkills()
    {
        return await _context.Skills
            .Where(s => s.Status == SkillStatus.InProgress || s.Status == SkillStatus.Locked)
            .Where(s => s.Prerequisites.All(p => p.Prerequisite.Status == SkillStatus.Completed))
            .Select(s => new SkillRecommendation
                {
                    LastLog = s.SkillLogs.Max(log => (DateTime?)log.Date),
                    Skill = s,
                    UnlockCount = _context.Prerequisites.Count(sp => sp.PrerequisiteId == s.Id),
                })
            .ToListAsync();
    }
    
    public async Task UpdateAsync(Skill skill)
    {
        _context.Skills.Update(skill);
        await _context.SaveChangesAsync();
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}