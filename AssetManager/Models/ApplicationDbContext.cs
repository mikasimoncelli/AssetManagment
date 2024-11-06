using AssetManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AssetManager.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CheckedOutAsset> CheckedOutAssets { get; set; }

        public DbSet<AssetDamage> AssetDamages { get; set; }

        public DbSet<AssetDisposal> AssetDisposals { get; set; }



    }
}
