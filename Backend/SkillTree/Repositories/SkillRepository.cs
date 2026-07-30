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
    
    public async Task<Dictionary<int, UserSkillProgress>> GetAllUserSkillProgressesAsync(string userId)
    {
        return await _context.UserSkillProgresses
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SkillId);
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
    
    public async Task<Dictionary<int, List<SkillLog>>> GetLogsBySkillIdsAsync(string userId, IEnumerable<int> skillIds)
    {
        return await _context.SkillLogs
            .Where(l => l.UserId == userId && skillIds.Contains(l.SkillId))
            .GroupBy(l => l.SkillId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());
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
            .FirstOrDefaultAsync(s => s.Id == skillId);
    }
    
    public async Task<IEnumerable<UserSkillProgress>> GetCompletedSortedRecentSkillsAsync(string userId)
    {
        return await _context.UserSkillProgresses
            .Where(us => us.UserId == userId)
            .Where(us => us.SkillStatus == SkillStatus.Completed)
            .Include(us => us.Skill)
            .OrderByDescending(us => us.CompletedAt)
            .Take(10)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<UserSkillProgress>> GetUnlockedSkillsAsync(string userId)
    {
        return await _context.UserSkillProgresses
            .Where(s => s.UserId == userId)
            .Where(s => s.SkillStatus == SkillStatus.InProgress || s.SkillStatus == SkillStatus.Locked)
            .Where(s => s.Skill.Prerequisites
                .All(p => _context.UserSkillProgresses
                    .Any(sp => sp.SkillStatus == SkillStatus.Completed && sp.UserId == userId && sp.SkillId == p.PrerequisiteId)))
            .Include(s => s.Skill)
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
        var progress = await _context.UserSkillProgresses
            .Where(s => s.UserId == userId)
            .FirstOrDefaultAsync(s => s.SkillId == skillId);

        if (progress != null)
        {
            if (newStatus == SkillStatus.Completed && progress.SkillStatus != SkillStatus.Completed)
            {
                progress.CompletedAt = DateTime.UtcNow;
            }
            progress.SkillStatus = newStatus;
        }
        await _context.SaveChangesAsync();
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<UserSkillProgress?> GetUserSkillProgressAsync(string userId, int skillId)
    {
        return await _context.UserSkillProgresses.Where(us => us.UserId == userId).FirstOrDefaultAsync(us => us.SkillId == skillId);
    }

    public async Task<UserSkillProgress> AddUserSkillProgressAsync(string userId, int skillId)
    {
        var us = new UserSkillProgress
        {
            Skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == skillId),
            UserId = userId,
            SkillId = skillId,
            StartedAt = DateTime.UtcNow,
            SkillStatus =  SkillStatus.Locked,
        };
        await _context.UserSkillProgresses.AddAsync(us);
        await _context.SaveChangesAsync();
        return us;
    }

    public async Task<IEnumerable<int>> GetCompletedSkillsIds(string userId)
    {
        return await _context.UserSkillProgresses
            .Where(us => us.UserId == userId)
            .Where(us => us.SkillStatus == SkillStatus.Completed)
            .Select(us => us.SkillId)
            .Distinct()
            .ToListAsync();
    }
}