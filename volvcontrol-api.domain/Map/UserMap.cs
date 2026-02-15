using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.domain.Map;

public static class UserMap
{
    public static UserResponse ToResponse(User entity)
    {
        return new UserResponse
        {
            Id = entity.Id,
            Company = entity.Company,
            PositionDescription = entity.PositionDescription,
            Name = entity.Name,
            Email = entity.Email,
            Status = entity.Status,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate
        };
    }
}
