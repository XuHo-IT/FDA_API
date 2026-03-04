using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlFloodReportRepository : IFloodReportRepository
    {
        private readonly AppDbContext _context;

        public PgsqlFloodReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(FloodReport entity, CancellationToken ct = default)
        {
            _context.FloodReports.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var report = await _context.FloodReports.FindAsync(new object[] { id }, ct);
            if (report == null)
            {
                return false;
            }
            _context.FloodReports.Remove(report);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<FloodReport?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.FloodReports
                .Include(r => r.Media)
                .Include(r => r.Reporter)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<bool> UpdateAsync(FloodReport entity, CancellationToken ct = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<FloodReport>> FindNearbyPublishedReportsAsync(
            decimal latitude,
            decimal longitude,
            int radiusMeters,
            TimeSpan timeWindow,
            CancellationToken ct = default)
        {
            var cutoffTime = DateTime.UtcNow - timeWindow;

            // Get all published reports within time window
            var allReports = await _context.FloodReports
                .AsNoTracking()
                .Where(r => r.Status == "published")
                .Where(r => r.CreatedAt >= cutoffTime)
                .ToListAsync(ct);

            // Filter by distance using Haversine formula
            var nearbyReports = new List<FloodReport>();

            foreach (var report in allReports)
            {
                var distance = CalculateHaversineDistance(
                    latitude, longitude,
                    report.Latitude, report.Longitude);

                if (distance <= radiusMeters)
                {
                    nearbyReports.Add(report);
                }
            }

            return nearbyReports.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public async Task<List<FloodReport>> GetByStatusAsync(string status, CancellationToken ct = default)
        {
            return await _context.FloodReports
                .AsNoTracking()
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<FloodReport>> GetByReporterIdAsync(Guid reporterId, CancellationToken ct = default)
        {
            return await _context.FloodReports
                .AsNoTracking()
                .Where(r => r.ReporterUserId == reporterId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<(IReadOnlyList<FloodReport> Items, int TotalCount)> ListAsync(
            string? status,
            string? severity,
            DateTime? from,
            DateTime? to,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.FloodReports
                .AsNoTracking()
                .Include(r => r.Media)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(severity))
            {
                query = query.Where(r => r.Severity == severity);
            }

            if (from.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= to.Value);
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.TrustScore)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        // Haversine Distance Calculation
        private double CalculateHaversineDistance(
            decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371000; // Earth radius in meters

            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) *
                    Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in meters
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}

