using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG17;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat17.DTOs;
using MediatR;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat17
{
    /// <summary>
    /// Endpoint to check identifier and determine authentication method
    /// POST /api/v1/auth/check-identifier
    /// </summary>
    public class CheckIdentifierEndpoint : Endpoint<CheckIdentifierRequestDto, CheckIdentifierResponseDto>
    {
        private readonly IMediator _mediator;

        public CheckIdentifierEndpoint(IMediator mediator) => _mediator = mediator;

        public override void Configure()
        {
            Post("/api/v1/auth/check-identifier");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Check identifier and get required authentication method";
                s.Description = "Determines if identifier requires password or OTP login";
                s.ExampleRequest = new CheckIdentifierRequestDto
                {
                    Identifier = "user@email.com"
                };
            });
            Tags("Authentication", "Progressive Login");
        }

        public override async Task HandleAsync(CheckIdentifierRequestDto req, CancellationToken ct)
        {
            var command = new CheckIdentifierRequest(req.Identifier);

            var result = await _mediator.Send(command, ct);

            var response = new CheckIdentifierResponseDto
            {
                Success = result.Success,
                Message = result.Message,
                IdentifierType = result.IdentifierType,
                AccountExists = result.AccountExists,
                HasPassword = result.HasPassword,
                RequiredMethod = result.RequiredMethod
            };

            await SendAsync(response, cancellation: ct);
        }
    }
}
