using volvcontrol_api.domain.Entities;
using volvcontrol_api.domain.Model.Request;
using volvcontrol_api.domain.Model.Response;

namespace volvcontrol_api.domain.Map;

public static class CompanyMap
{
    public static Company ToEntity(CompanyCreateRequest request)
    {
        return new Company
        {
            Name = request.Name,
            Document = request.Document
        };
    }

    public static Company ToEntity(CompanyUpdateRequest request)
    {
        return new Company
        {
            Id = request.Id,
            Name = request.Name,
            Document = request.Document
        };
    }

    public static CompanyResponse ToResponse(Company entity)
    {
        return new CompanyResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Document = entity.Document,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate
        };
    }
}
