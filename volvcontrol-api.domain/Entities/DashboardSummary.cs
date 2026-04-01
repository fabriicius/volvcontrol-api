namespace volvcontrol_api.domain.Entities;

public class DashboardSummary
{
    public int TotalActiveClients { get; set; }
    public int TotalEquipments { get; set; }
    public int TotalOpenServiceRequests { get; set; }
    public int TotalCompletedServiceRequests { get; set; }
    public int TotalScheduledServiceRequests { get; set; }
    public int TotalWaitingPartsServiceRequests { get; set; }
    public IReadOnlyList<DashboardLatestServiceRequest> LatestServiceRequests { get; set; } = Array.Empty<DashboardLatestServiceRequest>();
}
