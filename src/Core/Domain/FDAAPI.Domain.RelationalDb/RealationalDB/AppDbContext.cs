using FDAAPI.Domain.RelationalDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.RealationalDB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<SensorReading> SensorReadings { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<OtpCode> OtpCodes { get; set; } = null!;
        public DbSet<UserOAuthProvider> UserOAuthProviders { get; set; } = null!;
        public DbSet<Station> Stations { get; set; } = null!;
        public DbSet<UserPreference> UserPreferences { get; set; } = null!;
        public DbSet<Area> Areas { get; set; } = null!;
        public DbSet<Alert> Alerts { get; set; } = null!;
        public DbSet<AlertRule> AlertRules { get; set; } = null!;
        public DbSet<NotificationLog> NotificationLogs { get; set; } = null!;
        public DbSet<UserAlertSubscription> UserAlertSubscriptions { get; set; } = null!;
        public DbSet<SensorHourlyAgg> SensorHourlyAggs { get; set; }
        public DbSet<SensorDailyAgg> SensorDailyAggs { get; set; }
        public DbSet<AdministrativeArea> AdministrativeAreas { get; set; } = null!;
        public DbSet<FloodAnalyticsFrequency> FloodAnalyticsFrequencies { get; set; } = null!;
        public DbSet<FloodAnalyticsSeverity> FloodAnalyticsSeverities { get; set; } = null!;
        public DbSet<FloodAnalyticsHotspot> FloodAnalyticsHotspots { get; set; } = null!;
        public DbSet<AnalyticsJob> AnalyticsJobs { get; set; } = null!;
        public DbSet<AnalyticsJobRun> AnalyticsJobRuns { get; set; } = null!;
        public DbSet<FloodEvent> FloodEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
