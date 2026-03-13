namespace volvcontrol_api.domain.Entities;

public class ServiceRequestListItem
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientDocument { get; set; } = string.Empty;
    public int StatusServiceRequestId { get; set; }
    public string StatusServiceRequestDescription { get; set; } = string.Empty;
    public int TypeMaintenanceRecordId { get; set; }
    public string TypeMaintenanceRecordDescription { get; set; } = string.Empty;
}
