using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using skill_tree.Entities;

namespace skill_tree.Data;

public class SkillDbContext : IdentityDbContext<ApplicationUser>
{
    public SkillDbContext(DbContextOptions<SkillDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Skill> Skills  => Set<Skill>();
    public DbSet<SkillPrerequisite> Prerequisites => Set<SkillPrerequisite>();
    public DbSet<SkillLog> SkillLogs => Set<SkillLog>();
    public DbSet<UserSkillProgress> UserSkillProgresses => Set<UserSkillProgress>();

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
        
        modelBuilder.Entity<UserSkillProgress>().
            
            HasOne(s => s.Skill).
            WithMany().
            HasForeignKey(f => f.SkillId).
            OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<UserSkillProgress>().
            HasIndex(s => new {s.UserId, s.SkillId}).
            IsUnique();
        
        modelBuilder.Entity<UserSkillProgress>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SkillLog>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<SkillLog>() //optional
            .HasOne(sl => sl.Skill)
            .WithMany()
            .HasForeignKey(f => f.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}