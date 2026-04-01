namespace volvcontrol_api.domain.Model.Response;

public class DashboardSummaryResponse
{
    public int TotalActiveClients { get; set; }
    public int TotalEquipments { get; set; }
    public int TotalOpenServiceRequests { get; set; }
    public int TotalCompletedServiceRequests { get; set; }
    public int TotalScheduledServiceRequests { get; set; }
    public int TotalWaitingPartsServiceRequests { get; set; }
    public IReadOnlyList<DashboardLatestServiceRequestResponse> LatestServiceRequests { get; set; } = Array.Empty<DashboardLatestServiceRequestResponse>();
}
