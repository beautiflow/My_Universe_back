using Microsoft.EntityFrameworkCore;
using VisitorCounter.Models;

namespace VisitorCounter.Data;

public class VisitorContext : DbContext
{
    public VisitorContext(DbContextOptions<VisitorContext> options) : base(options) { }

    public DbSet<VisitorCount> VisitorCounts { get; set; }
}