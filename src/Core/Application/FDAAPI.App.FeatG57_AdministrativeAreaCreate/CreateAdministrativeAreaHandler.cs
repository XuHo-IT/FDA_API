using FDAAPI.App.Common.Models.AdministrativeAreas;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FDAAPI.App.FeatG57_AdministrativeAreaCreate
{
    public class CreateAdministrativeAreaHandler : IRequestHandler<CreateAdministrativeAreaRequest, CreateAdministrativeAreaResponse>
    {
        private readonly IAdministrativeAreaRepository _repository;
        private readonly IAdministrativeAreaMapper _mapper;

        public CreateAdministrativeAreaHandler(
            IAdministrativeAreaRepository repository,
            IAdministrativeAreaMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CreateAdministrativeAreaResponse> Handle(CreateAdministrativeAreaRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate level
                if (request.Level != "city" && request.Level != "district" && request.Level != "ward")
                {
                    return new CreateAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "Level must be one of: city, district, ward",
                        StatusCode = AdministrativeAreaStatusCode.InvalidData
                    };
                }

                // Validate parent relationship
                if (request.Level == "city" && request.ParentId.HasValue)
                {
                    return new CreateAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "City level areas cannot have a parent",
                        StatusCode = AdministrativeAreaStatusCode.InvalidData
                    };
                }

                if (request.Level == "district" && !request.ParentId.HasValue)
                {
                    return new CreateAdministrativeAreaResponse
                    {
                        Success = false,
                        Message = "District level areas must have a parent (city)",
                        StatusCode = AdministrativeAreaStatusCode.InvalidData
                    };
                }

                if (request.Level == "ward" && !request.ParentId.HasValue)
                {
                    return new CreateAdministrativeAreaResponse
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
                        return new CreateAdministrativeAreaResponse
                        {
                            Success = false,
                            Message = "Parent area not found",
                            StatusCode = AdministrativeAreaStatusCode.NotFound
                        };
                    }

                    // Validate parent level
                    if (request.Level == "district" && parent.Level != "city")
                    {
                        return new CreateAdministrativeAreaResponse
                        {
                            Success = false,
                            Message = "District must have a city as parent",
                            StatusCode = AdministrativeAreaStatusCode.InvalidData
                        };
                    }

                    if (request.Level == "ward" && parent.Level != "district")
                    {
                        return new CreateAdministrativeAreaResponse
                        {
                            Success = false,
                            Message = "Ward must have a district as parent",
                            StatusCode = AdministrativeAreaStatusCode.InvalidData
                        };
                    }
                }

                var area = new AdministrativeArea
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Level = request.Level,
                    ParentId = request.ParentId,
                    Code = request.Code,
                    Geometry = request.Geometry
                };

                var id = await _repository.CreateAsync(area, cancellationToken);
                area.Id = id;

                return new CreateAdministrativeAreaResponse
                {
                    Success = true,
                    Message = "Administrative area created successfully",
                    StatusCode = AdministrativeAreaStatusCode.Created,
                    Data = _mapper.MapToDto(area)
                };
            }
            catch (Exception ex)
            {
                return new CreateAdministrativeAreaResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = AdministrativeAreaStatusCode.UnknownError
                };
            }
        }
    }
}

