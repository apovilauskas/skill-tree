using Microsoft.EntityFrameworkCore;
using skill_tree.Entities;

namespace skill_tree.Data;

public class SkillDbContext : DbContext
{
    public SkillDbContext(DbContextOptions<SkillDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Skill> Skills  => Set<Skill>();
    
}