using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Persistence.Repositories
{
    public class PgsqlAnalyticsJobRepository : IAnalyticsJobRepository
    {
        private readonly AppDbContext _context;

        public PgsqlAnalyticsJobRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsJob?> GetByJobTypeAsync(
            string jobType,
            CancellationToken ct = default)
        {
            return await _context.AnalyticsJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.JobType == jobType, ct);
        }

        public async Task<Guid> CreateAsync(
            AnalyticsJob job,
            CancellationToken ct = default)
        {
            _context.AnalyticsJobs.Add(job);
            await _context.SaveChangesAsync(ct);
            return job.Id;
        }

        public async Task<bool> UpdateAsync(
            AnalyticsJob job,
            CancellationToken ct = default)
        {
            _context.AnalyticsJobs.Update(job);
            var rowsAffected = await _context.SaveChangesAsync(ct);
            return rowsAffected > 0;
        }
    }
}

