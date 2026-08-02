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
    
    public async Task<IEnumerable<Skill>> GetAllSkillsWithPrerequisitesAsync()
    {
        return await _context.Skills
            .Include(s => s.Prerequisites)
            .ThenInclude(s => s.Prerequisite)
            .ToListAsync();
    }
    
    public async Task<Dictionary<int, UserSkillProgress>> GetAllUserSkillProgressesAsync(string userId)
    {
        return await _context.UserSkillProgresses
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SkillId);
    }

    public async Task<bool> RemoveSkillAsync(int skillId)
    {
        var response = await _context.Skills.FirstOrDefaultAsync(s => s.Id == skillId);
        if (response == null) return false;
        _context.Skills.Remove(response);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EditSkillAsync(int skillId, string name, string description, string metric)
    {
        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == skillId);
        if (skill == null) return false;
        if (!string.IsNullOrEmpty(name)) skill.Name = name;
        if (!string.IsNullOrEmpty(description)) skill.Description = description;
        if (!string.IsNullOrEmpty(metric)) skill.Metric = metric;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePrerequisiteAsync(int skillPrerequisiteId)
    {
        var skillPrerequisite = await _context.Prerequisites.FirstOrDefaultAsync(s => s.Id == skillPrerequisiteId);
        if (skillPrerequisite == null) return false;
        _context.Prerequisites.Remove(skillPrerequisite);
        await _context.SaveChangesAsync();
        return true;
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
        var unlockedSkillsWithProgress = await _context.Skills
            .Where(s => s.Prerequisites.All(p => 
                _context.UserSkillProgresses.Any(sp => 
                    sp.UserId == userId && 
                    sp.SkillId == p.PrerequisiteId && 
                    sp.SkillStatus == SkillStatus.Completed)))
            .Select(s => new
            {
                Skill = s,
                Progress = _context.UserSkillProgresses
                    .FirstOrDefault(p => p.UserId == userId && p.SkillId == s.Id)
            })
            .Where(x => x.Progress == null || x.Progress.SkillStatus != SkillStatus.Completed)
            .ToListAsync();

        return unlockedSkillsWithProgress.Select(x => x.Progress ?? new UserSkillProgress
        {
            UserId = userId,
            SkillId = x.Skill.Id,
            Skill = x.Skill,
            SkillStatus = SkillStatus.Locked,
            StartedAt = x.Skill.CreatedAt
        });
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
        return await _context.Skills
            .Where(s => s.Prerequisites.All(p => 
                _context.UserSkillProgresses.Any(sp => 
                    sp.UserId == userId && 
                    sp.SkillId == p.PrerequisiteId && 
                    sp.SkillStatus == SkillStatus.Completed)))
            .Select(s => new
            {
                Skill = s,
                Progress = _context.UserSkillProgresses
                    .FirstOrDefault(p => p.UserId == userId && p.SkillId == s.Id)
            })
            .Where(x => x.Progress == null || x.Progress.SkillStatus != SkillStatus.Completed)
            .Select(x => new SkillRecommendation
            {
                Skill = x.Skill,
                LastLog = _context.SkillLogs
                    .Where(l => l.UserId == userId && l.SkillId == x.Skill.Id)
                    .Max(log => (DateTime?)log.Date),
                UnlockCount = _context.Prerequisites
                    .Count(sp => sp.PrerequisiteId == x.Skill.Id),
                StartedAt = x.Progress != null ? x.Progress.StartedAt : x.Skill.CreatedAt
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

    public async Task<UserSkillProgress?> GetUserSkillProgressAsync(string userId, int skillId)
    {
        return await _context.UserSkillProgresses.Where(us => us.UserId == userId).FirstOrDefaultAsync(us => us.SkillId == skillId);
    }

    public async Task<UserSkillProgress> AddUserSkillProgressAsync(string userId, int skillId)
    {
        var us = new UserSkillProgress
        {
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