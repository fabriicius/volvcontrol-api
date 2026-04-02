using Microsoft.AspNetCore.Http;

namespace volvcontrol_api.service.firebase;

/// <summary>
/// Implementação do armazenamento de fotos com regras de pastas: Company/IdCliente/[tipo]/IdEntidade.
/// Delega operações ao Firebase Storage; o caminho é montado a partir dos segmentos informados na chamada.
/// </summary>
public class PhotoStorage : IPhotoStorage
{
    private readonly IFirebaseStorageService _firebase;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PhotoStorage(IFirebaseStorageService firebase, IHttpContextAccessor httpContextAccessor)
    {
        _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public string BuildObjectName(string fileName, params string[] pathSegments)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Nome do arquivo é obrigatório.", nameof(fileName));
        if (pathSegments is null || pathSegments.Length == 0)
            throw new ArgumentException("É necessário informar ao menos um segmento do caminho (ex: Company, IdCliente, Equipamento, IdEquipamento).", nameof(pathSegments));

        var sanitized = pathSegments
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim().Replace("/", "-").Replace("\\", "-"))
            .ToArray();

        if (sanitized.Length == 0)
            throw new ArgumentException("Nenhum segmento válido informado.", nameof(pathSegments));

        var envSegment = ResolveEnvironmentSegment();
        var path = sanitized.Length > 0 && IsEnvironmentSegment(sanitized[0])
            ? string.Join("/", sanitized)
            : $"{envSegment}/{string.Join("/", sanitized)}";
        var safeFileName = Path.GetFileName(fileName).Trim();
        if (string.IsNullOrEmpty(safeFileName))
            throw new ArgumentException("Nome do arquivo inválido.", nameof(fileName));

        return $"{path}/{safeFileName}";
    }

    private string ResolveEnvironmentSegment()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return "dev";

        var hostCandidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Host.Host))
            hostCandidates.Add(request.Host.Host);

        AddHostsFromHeader(request.Headers["X-Forwarded-Host"], hostCandidates);
        AddHostsFromHeader(request.Headers["X-Original-Host"], hostCandidates);

        foreach (var host in hostCandidates)
        {
            var normalizedHost = host.Trim().ToLowerInvariant();
            if (normalizedHost == "api.volvcontrol.com.br")
                return "producao";
            if (normalizedHost == "api.hml.volvcontrol.com.br")
                return "hml";
            if (normalizedHost == "localhost" || normalizedHost == "127.0.0.1" || normalizedHost == "::1")
                return "dev";
        }

        return "dev";
    }

    private static void AddHostsFromHeader(string? headerValue, ICollection<string> hostCandidates)
    {
        if (string.IsNullOrWhiteSpace(headerValue))
            return;

        var items = headerValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var item in items)
        {
            var host = item;
            var colonIndex = host.IndexOf(':');
            if (colonIndex > 0)
                host = host[..colonIndex];
            if (!string.IsNullOrWhiteSpace(host))
                hostCandidates.Add(host);
        }
    }

    private static bool IsEnvironmentSegment(string segment)
    {
        return segment.Equals("producao", StringComparison.OrdinalIgnoreCase)
            || segment.Equals("hml", StringComparison.OrdinalIgnoreCase)
            || segment.Equals("dev", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default, params string[] pathSegments)
    {
        var objectName = BuildObjectName(fileName, pathSegments);
        return await _firebase.UploadAsync(content, objectName, contentType ?? "application/octet-stream", cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> GetSignedDownloadUrlAsync(string objectName, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        return _firebase.GetSignedDownloadUrlAsync(objectName, expiration, cancellationToken);
    }

    /// <inheritdoc />
    public string GetPermanentUrl(string objectName)
    {
        return _firebase.GetPermanentUrl(objectName);
    }

    /// <inheritdoc />
    public Task<byte[]> DownloadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        return _firebase.DownloadAsync(objectName, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        return _firebase.DeleteAsync(objectName, cancellationToken);
    }
}
