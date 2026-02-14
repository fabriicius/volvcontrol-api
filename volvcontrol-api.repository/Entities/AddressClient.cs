namespace volvcontrol_api.repository.Entities;

public class AddressClient
{
    public int Id { get; set; }
    public int AddressId { get; set; }
    public int ClientId { get; set; }

    public Address Address { get; set; }
    public Client Client { get; set; }
}
