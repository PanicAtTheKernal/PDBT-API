using Microsoft.EntityFrameworkCore;
using PDBT.Models;
using PDBT;

namespace PDBT.Data;

public class PdbtContext : DbContext
{
    public DbSet<Issue> Issues { get; set; } = null!;
    public DbSet<Label> Labels { get; set; } = null!;
    public DbSet<LabelDetail> LabelDetails { get; set; } = null!;
    public DbSet<LinkedIssue> LinkedIssues { get; set; } = null!;

    public PdbtContext(DbContextOptions<PdbtContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LabelDetail>()
            .HasKey(ld => new { ld.IssueId, ld.LabelId });
    }
}