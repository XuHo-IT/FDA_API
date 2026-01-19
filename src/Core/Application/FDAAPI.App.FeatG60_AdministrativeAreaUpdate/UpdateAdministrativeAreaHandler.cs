using FDAAPI.App.Common.Models.AdministrativeAreas;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG60_AdministrativeAreaUpdate
{
    public class UpdateAdministrativeAreaHandler : IRequestHandler<UpdateAdministrativeAreaRequest, UpdateAdministrativeAreaResponse>
    {
        private readonly IAdministrativeAreaRepository _repository;

        public UpdateAdministrativeAreaHandler(
            IAdministrativeAreaRepository repository)
        {
            _repository = repository;
        }

        public async Task<UpdateAdministrativeAreaResponse> Handle(UpdateAdministrativeAreaRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var area = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (area == null)
                {
                    return new UpdateAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "Administrative area not found",
                        StatusCode = AdministrativeAreaStatusCode.NotFound
                    };
                }

                // Validate level
                if (request.Level != "city" && request.Level != "district" && request.Level != "ward")
                {
                    return new UpdateAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "Level must be one of: city, district, ward",
                        StatusCode = AdministrativeAreaStatusCode.InvalidData
                    };
                }

                // Validate parent relationship
                if (request.Level == "city" && request.ParentId.HasValue)
                {
                    return new UpdateAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "City level areas cannot have a parent",
                        StatusCode = AdministrativeAreaStatusCode.InvalidData
                    };
                }

                if (request.Level == "district" && !request.ParentId.HasValue)
                {
                    return new UpdateAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "District level areas must have a parent (city)",
                        StatusCode = AdministrativeAreaStatusCode.InvalidData
                    };
                }

                if (request.Level == "ward" && !request.ParentId.HasValue)
                {
                    return new UpdateAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "Ward level areas must have a parent (district)",
                        StatusCode = AdministrativeAreaStatusCode.InvalidData
                    };
                }

                // Validate parent exists and has correct level
                if (request.ParentId.HasValue)
                {
                    var parent = await _repository.GetByIdAsync(request.ParentId.Value, cancellationToken);
                    if (parent == null)
                    {
                        return new UpdateAdministrativeAreaResponse
                        {
                            Success = false,
                            Message = "Parent area not found",
                            StatusCode = AdministrativeAreaStatusCode.NotFound
                        };
                    }

                    // Validate parent level
                    if (request.Level == "district" && parent.Level != "city")
                    {
                        return new UpdateAdministrativeAreaResponse
                        {
                            Success = false,
                            Message = "District must have a city as parent",
                            StatusCode = AdministrativeAreaStatusCode.InvalidData
                        };
                    }

                    if (request.Level == "ward" && parent.Level != "district")
                    {
                        return new UpdateAdministrativeAreaResponse
                        {
                            Success = false,
                            Message = "Ward must have a district as parent",
                            StatusCode = AdministrativeAreaStatusCode.InvalidData
                        };
                    }

                    // Prevent circular reference
                    if (request.ParentId.Value == request.Id)
                    {
                        return new UpdateAdministrativeAreaResponse
                        {
                            Success = false,
                            Message = "An area cannot be its own parent",
                            StatusCode = AdministrativeAreaStatusCode.InvalidData
                        };
                    }
                }

                // Update properties
                area.Name = request.Name;
                area.Level = request.Level;
                area.ParentId = request.ParentId;
                area.Code = request.Code;
                area.Geometry = request.Geometry;

                await _repository.UpdateAsync(area, cancellationToken);

                return new UpdateAdministrativeAreaResponse
                {
                    Success = true,
                    Message = "Administrative area updated successfully",
                    StatusCode = AdministrativeAreaStatusCode.Success
                };
            }
            catch (Exception ex)
            {
                return new UpdateAdministrativeAreaResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = AdministrativeAreaStatusCode.UnknownError
                };
            }
        }
    }
}

