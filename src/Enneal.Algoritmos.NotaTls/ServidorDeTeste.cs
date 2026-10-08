using System.Security.Authentication;

namespace Enneal.Algoritmos.NotaTls;

/// <summary>
/// Como um servidor do laboratório se comporta: que versões de TLS aceita, que certificado mostra e se manda HSTS.
/// </summary>
/// <param name="Host">Nome do site (fictício).</param>
/// <param name="Dns">Se o nome existe no "DNS" do laboratório.</param>
/// <param name="Protocolos">Versões de TLS que o servidor aceita.</param>
/// <param name="Pior">Pior versão aceita (entra na nota).</param>
/// <param name="Melhor">Melhor versão aceita (entra na nota).</param>
/// <param name="NomeNoCertificado">Para quem o certificado foi emitido (pode ser curinga, como <c>*.lab.example</c>).</param>
/// <param name="Validade">Fim da validade do certificado (UTC); <see langword="null"/> = 28/12/2026.</param>
/// <param name="Autoassinado">Se o certificado é autoassinado (nenhuma AC confia nele).</param>
/// <param name="ChaveBits">Tamanho da chave RSA.</param>
/// <param name="CifraMax">Bits da cifra mais forte aceita.</param>
/// <param name="CifraMin">Bits da cifra mais fraca aceita.</param>
/// <param name="Sigilo">Se tem sigilo futuro (forward secrecy).</param>
/// <param name="Aead">Se usa cifras AEAD (como AES-GCM).</param>
/// <param name="HstsDias">Dias do cabeçalho HSTS (0 = não manda HSTS).</param>
public sealed record ServidorDeTeste(
    string Host,
    bool Dns = true,
    SslProtocols Protocolos = SslProtocols.Tls12,
    SslProtocols Pior = SslProtocols.Tls12,
    SslProtocols Melhor = SslProtocols.Tls12,
    string NomeNoCertificado = "*.lab.example",
    DateTime? Validade = null,
    bool Autoassinado = false,
    int ChaveBits = 2048,
    int CifraMax = 256,
    int CifraMin = 128,
    bool Sigilo = true,
    bool Aead = true,
    int HstsDias = 0)
{
#pragma warning disable SYSLIB0039 // TLS 1.0 aparece só para imitar o servidor antigo do caso 2
    /// <summary>
    /// Os 6 casos do Reel, inspirados nos casos públicos de teste do badssl.com, mas com nomes fictícios
    /// (<c>.invalid</c> e <c>.example</c>, reservados pela RFC 2606). Tudo roda em 127.0.0.1.
    /// </summary>
    public static IReadOnlyList<ServidorDeTeste> CasosDoReel { get; } =
    [
        new("does-not-exist.invalid", Dns: false),
        new("tls-v1-0.lab.example", Protocolos: SslProtocols.Tls, Pior: SslProtocols.Tls, Melhor: SslProtocols.Tls, Aead: false),
        new("expired.lab.example", Validade: new DateTime(2015, 4, 12, 23, 59, 59, DateTimeKind.Utc)),
        new("wrong.host.lab.example"),
        new("self-signed.lab.example", Autoassinado: true, Validade: new DateTime(2028, 9, 28, 21, 1, 43, DateTimeKind.Utc)),
        new("seu-site.example", NomeNoCertificado: "seu-site.example", Protocolos: SslProtocols.Tls12 | SslProtocols.Tls13,
            Melhor: SslProtocols.Tls13, HstsDias: 365),
    ];
#pragma warning restore SYSLIB0039
}
