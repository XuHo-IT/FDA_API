using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Entities.Base;
/// <summary>
///     Represent the base interface that all entity classes
///     that need to track the temporarily removed must inherit from.
/// </summary>
/// <typeparam name="TTemporarilyRemovedBy">
///     The type of <see cref="TemporarilyRemovedBy"/> property.
/// </typeparam>
public interface ITemporarilyRemovedEntity<TTemporarilyRemovedBy>
{
    TTemporarilyRemovedBy TemporarilyRemovedBy { get; set; }

    DateTime TemporarilyRemovedAt { get; set; }

    bool IsTemporarilyRemoved { get; set; }
}






