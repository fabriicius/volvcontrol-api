namespace volvcontrol_api.Authorization;

/// <summary>
/// Constantes para autenticação e autorização baseada em position_description.
/// </summary>
public static class AuthConstants
{
    /// <summary>Nome do claim no JWT que armazena o cargo/função do usuário.</summary>
    public const string PositionDescriptionClaim = "position_description";

    /// <summary>Policy que exige position_description = ADM.</summary>
    public const string AdmOnlyPolicy = "AdmOnly";

    /// <summary>Valor do cargo para administrador (acesso total).</summary>
    public const string PositionAdm = "ADM";
}
