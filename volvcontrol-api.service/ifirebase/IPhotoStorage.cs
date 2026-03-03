namespace volvcontrol_api.service.firebase;

/// <summary>
/// Serviço de armazenamento de fotos com regras de organização em pastas no Firebase.
/// O caminho é montado conforme os segmentos informados na chamada (ex: Company/IdCliente/Equipamento/IdEquipamento ou Company/IdCliente/manutencao/IdEquipamento).
/// </summary>
public interface IPhotoStorage
{
    /// <summary>
    /// Monta o nome do objeto (caminho completo) a partir dos segmentos e do nome do arquivo.
    /// Exemplo: BuildObjectName("foto.jpg", "Company", "123", "Equipamento", "456") => "Company/123/Equipamento/456/foto.jpg"
    /// </summary>
    /// <param name="fileName">Nome do arquivo (ex: foto.jpg).</param>
    /// <param name="pathSegments">Segmentos do caminho (ex: "Company", idCliente, "Equipamento", idEquipamento).</param>
    /// <returns>Caminho completo do objeto no storage.</returns>
    string BuildObjectName(string fileName, params string[] pathSegments);

    /// <summary>
    /// Faz upload de uma foto para o Firebase no caminho definido pelos segmentos.
    /// </summary>
    /// <param name="content">Conteúdo do arquivo.</param>
    /// <param name="fileName">Nome do arquivo (ex: foto.jpg).</param>
    /// <param name="contentType">Tipo MIME (ex: image/jpeg).</param>
    /// <param name="pathSegments">Segmentos do caminho (ex: "Company", idCliente, "Equipamento", idEquipamento).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Nome do objeto no bucket (caminho completo) para uso em GetSignedDownloadUrlAsync ou DeleteAsync.</returns>
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default, params string[] pathSegments);

    /// <summary>
    /// Gera uma URL assinada para download do objeto.
    /// </summary>
    /// <param name="objectName">Nome do objeto no storage (caminho retornado por UploadAsync ou BuildObjectName).</param>
    /// <param name="expiration">Tempo de validade da URL.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>URL assinada para download.</returns>
    Task<string> GetSignedDownloadUrlAsync(string objectName, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna uma URL permanente para acesso à imagem.
    /// </summary>
    /// <param name="objectName">Nome do objeto no storage (caminho retornado por UploadAsync ou BuildObjectName).</param>
    /// <returns>URL definitiva da imagem.</returns>
    string GetPermanentUrl(string objectName);

    /// <summary>
    /// Baixa uma foto do storage em binário.
    /// </summary>
    /// <param name="objectName">Nome do objeto no storage (caminho retornado por UploadAsync ou BuildObjectName).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Conteúdo binário da foto.</returns>
    Task<byte[]> DownloadAsync(string objectName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um objeto do bucket.
    /// </summary>
    /// <param name="objectName">Nome do objeto no storage (caminho retornado por UploadAsync ou BuildObjectName).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task DeleteAsync(string objectName, CancellationToken cancellationToken = default);
}
