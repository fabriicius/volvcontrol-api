using System.Net.Http;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace volvcontrol_api.service.firebase;

/// <summary>Implementação da integração com Firebase Storage usando o bucket volvcontrol-bucket.</summary>
public class FirebaseStorageService : IFirebaseStorageService
{
    private const string BucketName = "volvcontrol-ebe86.firebasestorage.app";
    private readonly StorageClient _storageClient;
    private readonly UrlSigner _urlSigner;

    public FirebaseStorageService(string credentialsJsonPath)
    {
        if (string.IsNullOrWhiteSpace(credentialsJsonPath))
            throw new ArgumentNullException(nameof(credentialsJsonPath));
        if (!File.Exists(credentialsJsonPath))
            throw new FileNotFoundException("Arquivo de credenciais Firebase não encontrado.", credentialsJsonPath);

        var credential = GoogleCredential.FromFile(credentialsJsonPath);
        _storageClient = StorageClient.Create(credential);
        _urlSigner = UrlSigner.FromCredential(credential);
    }

    public async Task<string> UploadAsync(Stream content, string objectName, string contentType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            throw new ArgumentNullException(nameof(objectName));

        var name = objectName.TrimStart('/');
        var obj = await _storageClient.UploadObjectAsync(
            BucketName,
            name,
            contentType ?? "application/octet-stream",
            content,
            cancellationToken: cancellationToken);

        // Firebase download URL pública usa token em metadata.
        // Garantimos o token no upload para evitar AccessDenied em URL sem autenticação.
        obj.Metadata ??= new Dictionary<string, string>();
        if (!obj.Metadata.TryGetValue("firebaseStorageDownloadTokens", out var tokenValue) || string.IsNullOrWhiteSpace(tokenValue))
        {
            obj.Metadata["firebaseStorageDownloadTokens"] = Guid.NewGuid().ToString("N");
            obj = await _storageClient.UpdateObjectAsync(obj, cancellationToken: cancellationToken);
        }

        return obj.Name;
    }

    public async Task<string> GetSignedDownloadUrlAsync(string objectName, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            throw new ArgumentNullException(nameof(objectName));

        var name = objectName.TrimStart('/');
        var url = await _urlSigner.SignAsync(
            BucketName,
            name,
            expiration,
            HttpMethod.Get,
            null,
            cancellationToken);
        return url;
    }

    public string GetPermanentUrl(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            throw new ArgumentNullException(nameof(objectName));

        var name = objectName.TrimStart('/');
        var safeName = Uri.EscapeDataString(name);

        var obj = _storageClient.GetObject(BucketName, name);
        if (obj.Metadata is not null
            && obj.Metadata.TryGetValue("firebaseStorageDownloadTokens", out var tokens)
            && !string.IsNullOrWhiteSpace(tokens))
        {
            var token = tokens
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

            if (!string.IsNullOrWhiteSpace(token))
                return $"https://firebasestorage.googleapis.com/v0/b/{BucketName}/o/{safeName}?alt=media&token={token}";
        }
        
        return $"https://storage.googleapis.com/{BucketName}/{safeName}";
    }

    public async Task<byte[]> DownloadAsync(string objectName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            throw new ArgumentNullException(nameof(objectName));

        await using var content = new MemoryStream();
        await _storageClient.DownloadObjectAsync(
            BucketName,
            objectName.TrimStart('/'),
            content,
            cancellationToken: cancellationToken);
        return content.ToArray();
    }

    public async Task DeleteAsync(string objectName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            throw new ArgumentNullException(nameof(objectName));

        await _storageClient.DeleteObjectAsync(
            BucketName,
            objectName.TrimStart('/'),
            cancellationToken: cancellationToken);
    }
}
