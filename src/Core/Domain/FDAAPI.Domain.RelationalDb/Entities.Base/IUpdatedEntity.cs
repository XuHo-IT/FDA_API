using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.Domain.RelationalDb.Entities.Base;
/// <summary>
///     Represent the base interface that all entity classes
///     that need to track the updating must inherit from.
/// </summary>
/// <typeparam name="TUpdatedBy">
///     The type of <see cref="UpdatedBy"/> property.
/// </typeparam>
public interface IUpdatedEntity<TUpdatedBy>
{
    TUpdatedBy UpdatedBy { get; set; }

    DateTime UpdatedAt { get; set; }
}

