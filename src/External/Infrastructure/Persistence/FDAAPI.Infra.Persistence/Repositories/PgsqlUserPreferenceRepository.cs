using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using FDAAPI.Domain.RelationalDb.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlUserPreferenceRepository : IUserPreferenceRepository
    {
        private readonly AppDbContext _context;

        public PgsqlUserPreferenceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserPreference?> GetByUserAndKeyAsync(
            Guid userId,
            string preferenceKey,
            CancellationToken ct = default)
        {
            return await _context.UserPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.UserId == userId && p.PreferenceKey == preferenceKey,
                    ct);
        }

        public async Task<Guid> CreateAsync(
            UserPreference preference,
            CancellationToken ct = default)
        {
            _context.UserPreferences.Add(preference);
            await _context.SaveChangesAsync(ct);
            return preference.Id;
        }

        public async Task<bool> UpdateAsync(
            UserPreference preference,
            CancellationToken ct = default)
        {
            _context.Entry(preference).State = EntityState.Modified;
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var preference = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (preference == null)
                return false;

            _context.UserPreferences.Remove(preference);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }
    }
}
