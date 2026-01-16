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

        public DeleteAreaHandler(
            IAreaRepository areaRepository, 
            IAreaMapper areaMapper)
        {
            _areaRepository = areaRepository;
        }

        public async Task<DeleteAreaResponse> Handle(DeleteAreaRequest request, CancellationToken ct)
        {
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

            // 2. Check if user is Admin or SuperAdmin
            bool isAdmin = request.UserRole == "ADMIN" || request.UserRole == "SUPERADMIN";

            // 3. Authorization check with Admin override
            if (!isAdmin && area.UserId != request.UserId)
            {
                // Return 404 instead of 403 to prevent enumeration
                return new DeleteAreaResponse
                {
                    Success = false,
                    Message = "Area not found",
                    StatusCode = AreaStatusCode.NotFound
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
