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
    /// <summary>
    /// PostgreSQL implementation of IRoleRepository
    /// </summary>
    public class PgsqlRoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public PgsqlRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Code == code, ct);
        }

        public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            return await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == name, ct);
        }

        public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Roles
                .AsNoTracking()
                .ToListAsync(ct);
        }
    }
}






