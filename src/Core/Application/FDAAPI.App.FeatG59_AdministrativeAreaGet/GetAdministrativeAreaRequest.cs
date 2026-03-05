using FDAAPI.App.Common.Features;
using MediatR;
using System;

namespace FDAAPI.App.FeatG59_AdministrativeAreaGet
{
    public record GetAdministrativeAreaRequest(
        Guid Id
    ) : IFeatureRequest<GetAdministrativeAreaResponse>;
}

