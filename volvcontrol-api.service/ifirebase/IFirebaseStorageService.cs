namespace volvcontrol_api.service.firebase;

/// <summary>Serviço de integração com Firebase Storage (bucket volvcontrol-bucket).</summary>
public interface IFirebaseStorageService
{
    /// <summary>Faz upload de um arquivo para o bucket.</summary>
    /// <param name="content">Conteúdo do arquivo.</param>
    /// <param name="objectName">Nome do objeto no storage (ex: pasta/arquivo.jpg).</param>
    /// <param name="contentType">Tipo MIME (ex: image/jpeg).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Nome do objeto no bucket (caminho).</returns>
    Task<string> UploadAsync(Stream content, string objectName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>Gera uma URL assinada para download do objeto (válida por um período).</summary>
    /// <param name="objectName">Nome do objeto no storage.</param>
    /// <param name="expiration">Tempo de validade da URL.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>URL assinada para download.</returns>
    Task<string> GetSignedDownloadUrlAsync(string objectName, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>Retorna uma URL permanente para o objeto.</summary>
    /// <param name="objectName">Nome do objeto no storage.</param>
    /// <returns>URL definitiva da imagem.</returns>
    string GetPermanentUrl(string objectName);

    /// <summary>Baixa um objeto do bucket em memória.</summary>
    /// <param name="objectName">Nome do objeto no storage.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Conteúdo binário do objeto.</returns>
    Task<byte[]> DownloadAsync(string objectName, CancellationToken cancellationToken = default);

    /// <summary>Remove um objeto do bucket.</summary>
    /// <param name="objectName">Nome do objeto no storage.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task DeleteAsync(string objectName, CancellationToken cancellationToken = default);
}
