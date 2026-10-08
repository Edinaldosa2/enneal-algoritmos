using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Enneal.Algoritmos.NotaTls;

/// <summary>
/// As 5 camadas por onde uma conexão HTTPS passa, na ordem.
/// </summary>
public static class Camadas
{
    /// <summary>DNS → TCP → TLS → Certificado → HTTP.</summary>
    public static IReadOnlyList<string> Ordem { get; } = ["DNS", "TCP", "TLS", "Certificado", "HTTP"];
}

/// <summary>
/// O que o cliente viu do certificado do servidor.
/// </summary>
/// <param name="NomeDns">Nome para o qual o certificado foi emitido.</param>
/// <param name="ValidoAte">Fim da validade (UTC).</param>
/// <param name="Cadeia">Problemas encontrados na cadeia (<c>NoError</c>, <c>NotTimeValid</c>, <c>UntrustedRoot</c>...).</param>
/// <param name="NomeErrado">Se o nome do certificado não cobre o site pedido.</param>
public sealed record CertificadoVisto(string NomeDns, DateTime ValidoAte, X509ChainStatusFlags Cadeia, bool NomeErrado);

/// <summary>
/// Onde a conexão parou e por quê.
/// </summary>
/// <param name="Camada">A última camada alcançada (uma de <see cref="Camadas.Ordem"/>).</param>
/// <param name="Motivo">Explicação curta, por exemplo "expirado" ou "200 · HSTS 365 dias".</param>
/// <param name="Quebrou">Se a conexão quebrou nessa camada (<see langword="false"/> = chegou no HTTP).</param>
/// <param name="Protocolo">Versão de TLS negociada (<c>None</c> se o handshake não terminou).</param>
/// <param name="Certificado">O certificado visto, se o handshake chegou até ele.</param>
public sealed record ResultadoConexao(string Camada, string Motivo, bool Quebrou = true,
    SslProtocols Protocolo = SslProtocols.None, CertificadoVisto? Certificado = null);
