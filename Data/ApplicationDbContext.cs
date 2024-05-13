using Microsoft.EntityFrameworkCore;
using VolueEnergyTrader.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<OutputBidPacketApiModel> Bidresults { get; set; }
    public DbSet<OutputBidApiModel> Series { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<BidPacketHistoryApiModel> UpdateHistories { get; set; }
    
    
    
    
}