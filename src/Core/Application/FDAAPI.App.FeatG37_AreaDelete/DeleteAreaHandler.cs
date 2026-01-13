using FDAAPI.App.Common.Models.Areas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Repositories;
using FluentValidation;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG37_AreaDelete
{
    public class DeleteAreaHandler : IRequestHandler<DeleteAreaRequest, DeleteAreaResponse>
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IValidator<DeleteAreaRequest> _validator;
        private readonly IAreaMapper _areaMapper;

        public DeleteAreaHandler(
            IAreaRepository areaRepository, 
            IValidator<DeleteAreaRequest> validator,
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
            _validator = validator;
            _areaMapper = areaMapper;
        }

        public async Task<DeleteAreaResponse> Handle(DeleteAreaRequest request, CancellationToken ct)
        {
            var validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return new DeleteAreaResponse
                {
                    Success = false,
                    Message = string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage)),
                    StatusCode = AreaStatusCode.BadRequest
                };
            }

            var area = await _areaRepository.GetByIdAsync(request.Id, ct);

            if (area == null)
            {
                return new DeleteAreaResponse
                {
                    Success = false,
                    Message = "Area not found",
                    StatusCode = AreaStatusCode.NotFound
                };
            }

            // Authorization check: Only the owner can delete the area
            if (area.UserId != request.UserId)
            {
                return new DeleteAreaResponse
                {
                    Success = false,
                    Message = "Unauthorized to delete this area",
                    StatusCode = AreaStatusCode.Forbidden
                };
            }

            var result = await _areaRepository.DeleteAsync(request.Id, ct);

            if (!result)
            {
                return new DeleteAreaResponse
                {
                    Success = false,
                    Message = "Failed to delete area",
                    StatusCode = AreaStatusCode.InternalServerError
                };
            }

            return new DeleteAreaResponse
            {
                Success = true,
                Message = "Area deleted successfully",
                StatusCode = AreaStatusCode.Success
            };
        }
    }
}
