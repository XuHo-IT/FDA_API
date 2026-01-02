using FastEndpoints;
using FDAAPI.App.Common.Features;
using FDAAPI.App.FeatG17;
using FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat17.DTOs;

namespace FDAAPI.Presentation.FastEndpointBasedApi.Endpoints.Feat17
{
    /// <summary>
    /// Endpoint to check identifier and determine authentication method
    /// POST /api/v1/auth/check-identifier
    /// </summary>
    public class CheckIdentifierEndpoint : Endpoint<CheckIdentifierRequestDto, CheckIdentifierResponseDto>
    {
        private readonly IFeatureHandler<CheckIdentifierRequest, CheckIdentifierResponse> _handler;

        public CheckIdentifierEndpoint(IFeatureHandler<CheckIdentifierRequest, CheckIdentifierResponse> handler)
        {
            _handler = handler;
        }

        public override void Configure()
        {
            Post("/api/v1/auth/check-identifier");
            AllowAnonymous();
            Tags("Authentication", "Progressive Login");
            Summary(s =>
            {
                s.Summary = "Check identifier and get required authentication method";
                s.Description = "Determines if identifier requires password or OTP login";
                s.ExampleRequest = new CheckIdentifierRequestDto
                {
                    Identifier = "user@email.com"
                };
            });
        }

        public override async Task HandleAsync(CheckIdentifierRequestDto req, CancellationToken ct)
        {
            var appRequest = new CheckIdentifierRequest
            {
                Identifier = req.Identifier
            };

            var result = await _handler.ExecuteAsync(appRequest, ct);

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
