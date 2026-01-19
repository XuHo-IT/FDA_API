using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG61_AdministrativeAreaDelete
{
    public record DeleteAdministrativeAreaRequest(
        Guid Id
    ) : IFeatureRequest<DeleteAdministrativeAreaResponse>;
}

