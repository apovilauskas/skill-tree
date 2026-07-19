[1mdiff --git a/SkillTree/Repositories/ISkillRepository.cs b/SkillTree/Repositories/ISkillRepository.cs[m
[1mindex 96fb33c..3029c91 100644[m
[1m--- a/SkillTree/Repositories/ISkillRepository.cs[m
[1m+++ b/SkillTree/Repositories/ISkillRepository.cs[m
[36m@@ -6,7 +6,7 @@[m [mnamespace skill_tree.Repositories;[m
 public interface ISkillRepository[m
 {[m
     public Task<IEnumerable<Skill>> GetAllAsync();[m
[31m-    public Task<List<Skill>> GetAllSkillsWithPrerequisitesAsync();[m
[32m+[m[32m    public Task<IEnumerable<Skill>> GetAllSkillsWithPrerequisitesAsync();[m
     public Task AddAsync(Skill skill);[m
     public Task AddPrerequisitesAsync(SkillPrerequisite skillPrerequisite);[m
     public Task<bool> ExistsAsync(int id);[m
[36m@@ -14,4 +14,6 @@[m [mpublic interface ISkillRepository[m
     public Task AddLogAsync(SkillLog skillLog);[m
     public Task<Skill?> GetSkillAsync(int skillId);[m
     public Task<IEnumerable<Skill>> GetCompletedSortedRecentSkillsAsync();[m
[32m+[m[32m    public Task<IEnumerable<Skill>> GetUnlockedSkillsAsync();[m
[32m+[m[32m    Task<Dictionary<int, List<int>>> GetSkillPrerequisiteGraphAsync();[m
 }[m
\ No newline at end of file[m
[1mdiff --git a/SkillTree/Repositories/SkillRepository.cs b/SkillTree/Repositories/SkillRepository.cs[m
[1mindex c7f448e..2078bcc 100644[m
[1m--- a/SkillTree/Repositories/SkillRepository.cs[m
[1m+++ b/SkillTree/Repositories/SkillRepository.cs[m
[36m@@ -20,7 +20,7 @@[m [mpublic class SkillRepository : ISkillRepository[m
         return await _context.Skills.ToListAsync();[m
     }[m
     [m
[31m-    public async Task<List<Skill>> GetAllSkillsWithPrerequisitesAsync()[m
[32m+[m[32m    public async Task<IEnumerable<Skill>> GetAllSkillsWithPrerequisitesAsync()[m
     {[m
         return await _context.Skills[m
             .Include(s => s.Prerequisites)[m
[36m@@ -70,4 +70,30 @@[m [mpublic class SkillRepository : ISkillRepository[m
             .OrderByDescending(s => s.CompletedAt)[m
             .Take(10).ToListAsync();[m
     }[m
[32m+[m
[32m+[m[32m    public async Task<IEnumerable<Skill>> GetUnlockedSkillsAsync()[m
[32m+[m[32m    {[m
[32m+[m[32m        return await _context.Skills[m
[32m+[m[32m            .Where(s => s.Status == SkillStatus.InProgress || s.Status == SkillStatus.Locked)[m
[32m+[m[32m            .Where(s => s.Prerequisites.All(p => p.Prerequisite.Status == SkillStatus.Completed))[m
[32m+[m[32m            .ToListAsync();[m
[32m+[m[32m    }[m
[32m+[m[41m    [m
[32m+[m[32m    public async Task<Dictionary<int, List<int>>> GetSkillPrerequisiteGraphAsync()[m
[32m+[m[32m    {[m
[32m+[m[32m        var data = await _context.Skills[m
[32m+[m[32m            .Select(s => new[m[41m [m
[32m+[m[32m            {[m
[32m+[m[32m                SkillId = s.Id,[m
[32m+[m[32m                PrerequisiteIds = s.Prerequisites[m
[32m+[m[32m                    .Select(p => p.PrerequisiteId)[m
[32m+[m[32m                    .ToList()[m
[32m+[m[32m            })[m
[32m+[m[32m            .ToListAsync();[m
[32m+[m
[32m+[m[32m        return data.ToDictionary([m
[32m+[m[32m            x => x.SkillId,[m
[32m+[m[32m            x => x.PrerequisiteIds[m
[32m+[m[32m        );[m
[32m+[m[32m    }[m
 }[m
\ No newline at end of file[m
[1mdiff --git a/SkillTree/Services/SkillService.cs b/SkillTree/Services/SkillService.cs[m
[1mindex 3bfad63..870a33a 100644[m
[1m--- a/SkillTree/Services/SkillService.cs[m
[1m+++ b/SkillTree/Services/SkillService.cs[m
[36m@@ -22,17 +22,10 @@[m [mpublic class SkillService : ISkillService[m
         if (skill.Prerequisites.Any(sp => sp.Prerequisite.Status != SkillStatus.Completed)) return CanStartResult.LockedByPrerequisites;[m
         return CanStartResult.Available;[m
     }[m
[31m-[m
[32m+[m[41m    [m
     private async Task<Dictionary<int, List<int>>> BuildGraphAsync()[m
     {[m
[31m-        var skills = await _repository.GetAllSkillsWithPrerequisitesAsync();[m
[31m-        var graph = skills.ToDictionary([m
[31m-            skill => skill.Id,[m
[31m-            skill => skill.Prerequisites[m
[31m-                .Select(p => p.PrerequisiteId)[m
[31m-                .ToList()[m
[31m-            );[m
[31m-        return graph;[m
[32m+[m[32m        return await _repository.GetSkillPrerequisiteGraphAsync();[m
     }[m
     [m
     private bool IsValidPrerequisite(int skillId, int prerequisiteId, Dictionary<int, List<int>> graph)[m
[36m@@ -104,18 +97,15 @@[m [mpublic class SkillService : ISkillService[m
         return true;[m
     }[m
 [m
[31m-    public async Task<IEnumerable<UnlockedSkillResponseDto>> GetUnlockedSkillsAsync()[m
[31m-    {[m
[31m-        var skills = await _repository.GetAllSkillsWithPrerequisitesAsync();[m
[31m-        return skills[m
[31m-            .Where(s => s.Status == SkillStatus.InProgress || s.Status == SkillStatus.Locked)[m
[31m-            .Where(s=> s.Prerequisites.All(s => s.Prerequisite.Status == SkillStatus.Completed))[m
[31m-            .Select(s => s.ToUnlockedDto());[m
[31m-    }[m
[31m-[m
     public async Task<IEnumerable<CompletedSkillResponseDto>> GetCompletedSkillsAsync()[m
     {[m
         var skills = await _repository.GetCompletedSortedRecentSkillsAsync();[m
         return skills.Select(s => s.ToCompletedDto());[m
     }[m
[32m+[m[41m    [m
[32m+[m[32m    public async Task<IEnumerable<UnlockedSkillResponseDto>> GetUnlockedSkillsAsync()[m
[32m+[m[32m    {[m
[32m+[m[32m        var skills = await _repository.GetUnlockedSkillsAsync();[m
[32m+[m[32m        return skills.Select(s => s.ToUnlockedDto());[m
[32m+[m[32m    }[m
 }[m
\ No newline at end of file[m
