using Microsoft.EntityFrameworkCore;
using VolueEnergyTrading.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<BidResult> Bidresults { get; set; }
    public DbSet<Serie> Series { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<UpdateHistory> UpdateHistories { get; set; }
}