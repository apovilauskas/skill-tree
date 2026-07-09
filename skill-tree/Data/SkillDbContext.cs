using Microsoft.EntityFrameworkCore;
using skill_tree.Entities;

namespace skill_tree.Data;

public class SkillDbContext : DbContext
{
    public SkillDbContext(DbContextOptions<SkillDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Skill> Skills  => Set<Skill>();
    public DbSet<SkillPrerequisite> Prerequisites => Set<SkillPrerequisite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<SkillPrerequisite>().
            HasOne(sp => sp.Skill).
            WithMany(s => s.Prerequisites).
            HasForeignKey(f => f.SkillId).
            OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SkillPrerequisite>().
            HasOne(sp => sp.Prerequisite).
            WithMany().
            HasForeignKey(f => f.PrerequisiteId).
            OnDelete(DeleteBehavior.Cascade);
    }
}