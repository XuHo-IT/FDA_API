using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.RealationalDB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public class PgsqlWaterLevelRepository : IWaterLevelRepository
    {
        private readonly AppDbContext _context;

        public PgsqlWaterLevelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WaterLevel?> GetByIdAsync(long id)
        {
            return await _context.WaterLevels.FindAsync(id);
        }

        public async Task<long> CreateAsync(WaterLevel entity)
        {
            _context.WaterLevels.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id; 
        }

        public async Task<bool> UpdateAsync(WaterLevel entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true; 
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var waterLevel = await _context.WaterLevels.FindAsync(id);
            if (waterLevel == null)
            {
                return false;
            }

            _context.WaterLevels.Remove(waterLevel);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<WaterLevel>> ListAsync(int skip = 0, int take = 50)
        {
            return await _context.WaterLevels.Skip(skip).Take(take).ToListAsync();
        }
    }
}
