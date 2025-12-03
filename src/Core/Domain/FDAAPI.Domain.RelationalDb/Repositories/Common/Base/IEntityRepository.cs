using FDAAPI.Domain.RelationalDb.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Repositories.Common.Base;
/// <summary>
///     The base interface for the common entity repositories.
/// </summary>
/// <typeparam name="TEntity">
///     The entity this repository will interact with.
/// </typeparam>
public interface IEntityRepository<TEntity>
    where TEntity : class, IEntity
{
}