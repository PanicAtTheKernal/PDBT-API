using Microsoft.EntityFrameworkCore;
using PDBT.Models;

namespace PDBT.Data;

public class PdbtContext : DbContext
{
    public DbSet<Issue> Issues { get; set; } = null!;
    public DbSet<Label> Labels { get; set; } = null!;
    public DbSet<LinkedIssue> LinkedIssues { get; set; } = null!;

    public PdbtContext(DbContextOptions<PdbtContext> options) : base(options)
    {
    }
}