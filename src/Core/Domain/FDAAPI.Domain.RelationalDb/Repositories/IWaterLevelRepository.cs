
namespace FDAAPI.Domain.RelationalDb.Repositories
{
    public interface IWaterLevelRepository
    {
        Task<WaterLevel?> GetByIdAsync(long id);
        Task<long> CreateAsync(WaterLevel entity);
        Task<bool> UpdateAsync(WaterLevel entity);
        Task<bool> DeleteAsync(long id);
        Task<IEnumerable<WaterLevel>> ListAsync(int skip = 0, int take = 50);
    }
}
