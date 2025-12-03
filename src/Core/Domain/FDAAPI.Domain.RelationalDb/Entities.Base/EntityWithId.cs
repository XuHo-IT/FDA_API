using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Entities.Base;

/// <summary>
///     The base class for any entity that have id as a primary key to extends from.
/// </summary>
/// <typeparam name="TEntityId">
///     The type of the id (primary key).
/// </typeparam>
public abstract class EntityWithId<TEntityId> : IEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public TEntityId Id { get; set; }
}
