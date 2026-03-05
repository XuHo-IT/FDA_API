using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlAreaRepository : IAreaRepository
    {
        private readonly AppDbContext _context;

        public PgsqlAreaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Area?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return await _context.Areas
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<(List<Area> Areas, int TotalCount)> GetByUserIdAsync(Guid userId, string? searchTerm, int pageNumber, int pageSize, CancellationToken ct)
        {
            var query = _context.Areas
                .AsNoTracking()
                .Where(a => a.UserId == userId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a => a.Name.ToLower().Contains(searchTerm) || 
                                         a.AddressText.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(ct);
            var areas = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (areas, totalCount);
        }

        public async Task<(List<Area> Areas, int TotalCount)> GetAdminAreasAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken ct)
        {
            var query = _context.Areas
                .AsNoTracking()
                .Where(a => _context.UserRoles
                    .Any(ur => ur.UserId == a.CreatedBy && 
                               (ur.Role.Code == "ADMIN" || ur.Role.Code == "SUPERADMIN" || ur.Role.Code == "AUTHORITY")));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a => a.Name.ToLower().Contains(searchTerm) || 
                                         a.AddressText.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(ct);
            var areas = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (areas, totalCount);
        }

        public async Task<Guid> CreateAsync(Area area, CancellationToken ct)
        {
            _context.Areas.Add(area);
            await _context.SaveChangesAsync(ct);
            return area.Id;
        }

        public async Task<bool> UpdateAsync(Area area, CancellationToken ct)
        {
            _context.Areas.Update(area);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            var area = await _context.Areas.FirstOrDefaultAsync(a => a.Id == id, ct);
            if (area == null) return false;

            _context.Areas.Remove(area);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await _context.Areas
                .Where(a => a.UserId == userId)
                .CountAsync(ct);
        }

        public async Task<Area?> GetByUserIdAndNameAsync(Guid userId, string name, CancellationToken ct)
        {
            return await _context.Areas
                .AsNoTracking()
                .FirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.Name.ToLower() == name.ToLower(), ct);
        }

        public async Task<List<Area>> GetUserAreasWithinRadiusAsync(Guid userId, decimal latitude, decimal longitude, int radiusMeters, CancellationToken ct)
        {
            // Fetch all user's areas (in-memory distance calculation)
            var userAreas = await _context.Areas
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .ToListAsync(ct);

            // Note: Haversine filtering done in Handler for simplicity
            // PostGIS alternative can be added later for scale
            return userAreas;
        }

        public async Task<List<Area>> GetAreasContainingStationAsync(
            Guid stationId,
            decimal stationLat,
            decimal stationLng,
            CancellationToken ct = default)
                {
                    var allAreas = await _context.Areas
                        .Where(a => a.Latitude != 0 && a.Longitude != 0)
                        .ToListAsync(ct);

                    var areasContainingStation = allAreas
                        .Where(area =>
                        {
                            var distance = CalculateDistance(
                                (double)area.Latitude, (double)area.Longitude,
                                (double)stationLat, (double)stationLng) * 1000; // meters

                            return distance <= area.RadiusMeters;
                        })
                        .ToList();

                    return areasContainingStation;
                }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180;
    }
}

