using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Features
{
    public interface IFeatureHandler<TRequest, TResponse>
     where TRequest : IFeatureRequest<TResponse>
     where TResponse : IFeatureResponse
    {
        Task<TResponse> ExecuteAsync(TRequest request, CancellationToken ct);
    }
}
