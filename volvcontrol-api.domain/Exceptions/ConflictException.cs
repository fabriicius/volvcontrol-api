namespace volvcontrol_api.domain.Exceptions;

/// <summary>Exceção para recurso duplicado (ex.: email ou documento já cadastrado). Retornar 409 Conflict.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
    public ConflictException(string message, Exception inner) : base(message, inner) { }
}
