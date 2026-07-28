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

    public async Task<IEnumerable<SkillLog>> GetLogsAsync(int skillId, string userId)
    {
        return await _context.SkillLogs.Where(s => s.UserId == userId).Where(l => l.SkillId == skillId).ToListAsync();
    }

    public async Task AddLogAsync(SkillLog skillLog, string userId)
    {
        skillLog.UserId = userId;
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

    public async Task<IEnumerable<Skill>> GetCompletedSortedRecentSkillsAsync(string userId)
    {
        return await _context.UserSkillProgresses
            .Where(s => s.UserId == userId)
            .Where(s => s.SkillStatus == SkillStatus.Completed)
            .Select(s => s.Skill)
            .OrderByDescending(s => s.CompletedAt)
            .Take(10)
            .ToListAsync();
    }

    public async Task<IEnumerable<Skill>> GetUnlockedSkillsAsync(string userId)
    {
        return await _context.UserSkillProgresses
            .Where(s => s.UserId == userId)
            .Where(s => s.SkillStatus == SkillStatus.InProgress || s.SkillStatus == SkillStatus.Locked)
            .Where(s => s.Skill.Prerequisites
                .All(p => _context.UserSkillProgresses
                    .Any(sp => sp.SkillStatus == SkillStatus.Completed && sp.UserId == userId && sp.SkillId == p.PrerequisiteId)))
            .Select(s => s.Skill)
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

    public async Task<IEnumerable<SkillRecommendation>> GetRecommendedSkills(string userId)
    {
        return await _context.UserSkillProgresses
            .Where(s => s.UserId == userId)
            .Where(s => s.SkillStatus == SkillStatus.InProgress || s.SkillStatus == SkillStatus.Locked)
            .Where(s => s.Skill.Prerequisites
                .All(p => _context.UserSkillProgresses
                    .Any(sp => sp.SkillStatus == SkillStatus.Completed && sp.UserId == userId && sp.SkillId == p.PrerequisiteId)))
            .Select(u => new SkillRecommendation
            {
                LastLog = _context.SkillLogs.Where(s => s.UserId == userId).Where(s => s.SkillId == u.SkillId).Max(log => (DateTime?)log.Date),
                Skill = u.Skill,
                UnlockCount = _context.Prerequisites.Count(sp => sp.PrerequisiteId == u.Skill.Id),
            })
            .ToListAsync();
    }

    public async Task UpdateAsync(int skillId, string userId, SkillStatus newStatus)
    {
        var a = await _context.UserSkillProgresses
            .Where(s => s.UserId == userId)
            .FirstOrDefaultAsync(s => s.SkillId == skillId);
        if (a != null)
        {
            a.SkillStatus = newStatus;
        }
        await _context.SaveChangesAsync();
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}