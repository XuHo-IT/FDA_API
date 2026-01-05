using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Infra.Services.Cache
{
    public interface IStateCache
    {
        Task SetStateAsync(string state, string? returnUrl, TimeSpan expiration, CancellationToken ct = default);
        Task<string?> GetStateAsync(string state, CancellationToken ct = default);
        Task RemoveStateAsync(string state, CancellationToken ct = default);
    }
}






