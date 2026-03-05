using FDAAPI.App.Common.Models.Stations;
using FDAAPI.App.Common.Services.Mapping;
using FDAAPI.Domain.RelationalDb.Entities;
using FDAAPI.Domain.RelationalDb.Repositories;
using FDAAPI.Infra.Services.Alerts;
using MediatR;

namespace FDAAPI.App.FeatG23_StationCreate
{
    public class CreateStationHandler : IRequestHandler<CreateStationRequest, CreateStationResponse>
    {
        private readonly IStationRepository _stationRepository;
        private readonly IStationMapper _stationMapper;
        private readonly IAlertRuleRepository _alertRuleRepository;
        private readonly IGlobalThresholdService _globalThresholdService;

        public CreateStationHandler(
            IStationRepository stationRepository,
            IStationMapper stationMapper,
            IAlertRuleRepository alertRuleRepository,
            IGlobalThresholdService globalThresholdService)
        {
            _stationRepository = stationRepository;
            _stationMapper = stationMapper;
            _alertRuleRepository = alertRuleRepository;
            _globalThresholdService = globalThresholdService;
        }

        public async Task<CreateStationResponse> Handle(CreateStationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var station = new Station
                {
                    Id = Guid.NewGuid(),
                    Code = request.Code,
                    Name = request.Name,
                    LocationDesc = request.LocationDesc,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    RoadName = request.RoadName,
                    Direction = request.Direction,
                    Status = request.Status,
                    ThresholdWarning = request.ThresholdWarning,
                    ThresholdCritical = request.ThresholdCritical,
                    AdministrativeAreaId = request.AdministrativeAreaId,
                    InstalledAt = request.InstalledAt,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.AdminId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = request.AdminId
                };

                await _stationRepository.CreateAsync(station, cancellationToken);

                // ===== AUTO-CREATE DEFAULT ALERT RULES =====
                // Create global default AlertRules for caution, warning, critical
                var severities = new[] { "caution", "warning", "critical" };
                foreach (var severity in severities)
                {
                    var threshold = _globalThresholdService.GetThresholdForSeverity(severity);
                    var rule = new AlertRule
                    {
                        Id = Guid.NewGuid(),
                        StationId = station.Id,
                        Name = $"Global {severity} threshold",
                        RuleType = "threshold",
                        ThresholdValue = threshold,
                        Severity = severity,
                        IsActive = true,
                        IsGlobalDefault = true, // Mark as auto-generated from global config
                        CreatedBy = request.AdminId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = request.AdminId,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _alertRuleRepository.CreateAsync(rule, cancellationToken);
                }

                return new CreateStationResponse
                {
                    Success = true,
                    Message = "Station created successfully with default alert rules",
                    StatusCode = StationStatusCode.Success,
                    Data = _stationMapper.MapToDto(station)
                };
            }
            catch (Exception ex)
            {
                return new CreateStationResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}",
                    StatusCode = StationStatusCode.UnknownError
                };
            }
        }
    }
}