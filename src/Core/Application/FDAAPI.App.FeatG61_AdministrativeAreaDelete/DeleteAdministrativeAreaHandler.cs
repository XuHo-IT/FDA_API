using FDAAPI.App.Common.Models.AdministrativeAreas;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG61_AdministrativeAreaDelete
{
    public class DeleteAdministrativeAreaHandler : IRequestHandler<DeleteAdministrativeAreaRequest, DeleteAdministrativeAreaResponse>
    {
        private readonly IAdministrativeAreaRepository _repository;

        public DeleteAdministrativeAreaHandler(
            IAdministrativeAreaRepository repository)
        {
            _repository = repository;
        }

        public async Task<DeleteAdministrativeAreaResponse> Handle(DeleteAdministrativeAreaRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if area exists
                var area = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (area == null)
                {
                    return new DeleteAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "Administrative area not found",
                        StatusCode = AdministrativeAreaStatusCode.NotFound
                    };
                }

                // Check if area has children (prevent orphaned records)
                // Use GetAdministrativeAreasAsync with parentId filter to check for children
                var (children, childCount) = await _repository.GetAdministrativeAreasAsync(
                    null, // no search term
                    null, // no level filter
                    request.Id, // filter by parentId
                    1, // page 1
                    1, // only need to know if any exist
                    cancellationToken);

                if (childCount > 0)
                {
                    return new DeleteAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "Cannot delete administrative area that has child areas. Please delete or reassign child areas first.",
                        StatusCode = AdministrativeAreaStatusCode.Conflict
                    };
                }

                var success = await _repository.DeleteAsync(request.Id, cancellationToken);
                if (!success)
                {
                    return new DeleteAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "Administrative area could not be deleted",
                        StatusCode = AdministrativeAreaStatusCode.UnknownError
                    };
                }

                return new DeleteAdministrativeAreaResponse
                {
                    Success = true,
                    Message = "Administrative area deleted successfully",
                    StatusCode = AdministrativeAreaStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new DeleteAdministrativeAreaResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = AdministrativeAreaStatusCode.UnknownError
                };
            }
        }
    }
}

