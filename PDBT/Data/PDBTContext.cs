using Microsoft.EntityFrameworkCore;
using PDBT.Models;
using PDBT;

namespace PDBT.Data;

public class PdbtContext : DbContext, IPdbtContext
{
    public DbSet<Issue> Issues { get; set; } = null!;
    public DbSet<Label> Labels { get; set; } = null!;
    public DbSet<LinkedIssue> LinkedIssues { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public PdbtContext(DbContextOptions<PdbtContext> options) : base(options)
    {
    }
}