namespace volvcontrol_api.service.firebase;

/// <summary>
/// Implementação do armazenamento de fotos com regras de pastas: Company/IdCliente/[tipo]/IdEntidade.
/// Delega operações ao Firebase Storage; o caminho é montado a partir dos segmentos informados na chamada.
/// </summary>
public class PhotoStorage : IPhotoStorage
{
    private readonly IFirebaseStorageService _firebase;

    public PhotoStorage(IFirebaseStorageService firebase)
    {
        _firebase = firebase ?? throw new ArgumentNullException(nameof(firebase));
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

        var path = string.Join("/", sanitized);
        var safeFileName = Path.GetFileName(fileName).Trim();
        if (string.IsNullOrEmpty(safeFileName))
            throw new ArgumentException("Nome do arquivo inválido.", nameof(fileName));

        return $"{path}/{safeFileName}";
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
