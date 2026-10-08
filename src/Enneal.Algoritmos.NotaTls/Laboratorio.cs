using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Enneal.Algoritmos.NotaTls;

/// <summary>
/// O laboratório: servidores TLS de verdade em 127.0.0.1, com certificados gerados na hora, e um "DNS"
/// que é só um dicionário nome → porta. Nada sai da máquina.
/// </summary>
/// <remarks>
/// Cada servidor atende uma única conexão e fecha. A autoridade certificadora (AC) do laboratório também é
/// gerada na hora e só existe na memória deste processo: o verificador confia nela, e em mais nada.
/// </remarks>
public sealed class Laboratorio : IDisposable
{
    // Validade padrão dos certificados (como um certificado de 90 dias emitido em outubro de 2026).
    private static readonly DateTime ValidadePadrao = new(2026, 12, 28, 20, 2, 55, DateTimeKind.Utc);

    private readonly Dictionary<string, int> _dns = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<TcpListener> _servidores = [];

    /// <summary>Cria o laboratório e a sua AC.</summary>
    public Laboratorio()
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest("CN=AC do laboratorio", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        Ac = req.CreateSelfSigned(new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero),
                                  new DateTimeOffset(2040, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    /// <summary>A autoridade certificadora do laboratório (a única em que o verificador confia).</summary>
    public X509Certificate2 Ac { get; }

    /// <summary>
    /// Procura o nome no "DNS" do laboratório.
    /// </summary>
    /// <param name="host">Nome do site.</param>
    /// <param name="porta">Porta local do servidor, se o nome existe.</param>
    /// <returns>Se o nome existe.</returns>
    public bool Resolver(string host, out int porta) => _dns.TryGetValue(host, out porta);

    /// <summary>
    /// Sobe o servidor de um caso em 127.0.0.1 (porta livre) e registra o nome no "DNS".
    /// Casos com <see cref="ServidorDeTeste.Dns"/> falso não sobem nada: o nome simplesmente não existe.
    /// </summary>
    /// <param name="s">Como o servidor deve se comportar.</param>
    public void Subir(ServidorDeTeste s)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (!s.Dns) return;

        var escuta = new TcpListener(IPAddress.Loopback, 0); // porta 0: o sistema escolhe uma livre
        escuta.Start();
        _servidores.Add(escuta);
        _dns[s.Host] = ((IPEndPoint)escuta.LocalEndpoint).Port;
        var certificado = Certificado(s);

        _ = Task.Run(async () =>
        {
            try
            {
                using var cliente = await escuta.AcceptTcpClientAsync();
                escuta.Stop();
                using var ssl = new SslStream(cliente.GetStream());
                await ssl.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                {
                    ServerCertificate = certificado,
                    EnabledSslProtocols = s.Protocolos,
                });

                // Lê o pedido até a linha em branco e responde 200, com HSTS se o caso tiver.
                var leitor = new StreamReader(ssl, Encoding.ASCII);
                while (!string.IsNullOrEmpty(await leitor.ReadLineAsync())) { }
                string hsts = s.HstsDias > 0
                    ? $"Strict-Transport-Security: max-age={s.HstsDias * 86400L}; includeSubDomains\r\n"
                    : "";
                await ssl.WriteAsync(Encoding.ASCII.GetBytes(
                    $"HTTP/1.1 200 OK\r\n{hsts}Content-Length: 2\r\nConnection: close\r\n\r\nok"));
            }
            catch (Exception e) when (e is IOException or System.Security.Authentication.AuthenticationException
                                          or ObjectDisposedException or SocketException)
            {
                // Handshake recusado ou laboratório fechado: é exatamente o caso que estamos testando.
            }
        });
    }

    /// <summary>Fecha os servidores que ainda estiverem esperando conexão.</summary>
    public void Dispose()
    {
        foreach (var s in _servidores) s.Stop();
        Ac.Dispose();
    }

    // Gera o certificado do servidor: assinado pela AC do laboratório ou autoassinado.
    private X509Certificate2 Certificado(ServidorDeTeste s)
    {
        using var rsa = RSA.Create(s.ChaveBits);
        var req = new CertificateRequest($"CN={s.NomeNoCertificado}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var nomes = new SubjectAlternativeNameBuilder();
        nomes.AddDnsName(s.NomeNoCertificado);
        if (s.NomeNoCertificado.StartsWith("*.", StringComparison.Ordinal)) nomes.AddDnsName(s.NomeNoCertificado[2..]);
        req.CertificateExtensions.Add(nomes.Build());
        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("1.3.6.1.5.5.7.3.1")], false)); // autenticação de servidor

        var fim = new DateTimeOffset(s.Validade ?? ValidadePadrao);
        var inicio = s.Autoassinado ? fim.AddYears(-2) : fim.AddDays(-89);
        using X509Certificate2 c = s.Autoassinado
            ? req.CreateSelfSigned(inicio, fim)
            : req.Create(Ac, inicio, fim, RandomNumberGenerator.GetBytes(8)).CopyWithPrivateKey(rsa);

        // Exporta e importa de novo: assim a chave privada funciona no SslStream em qualquer sistema.
#pragma warning disable SYSLIB0057 // o construtor a partir de PKCS#12 é o jeito que funciona no .NET 8
        return new X509Certificate2(c.Export(X509ContentType.Pkcs12));
#pragma warning restore SYSLIB0057
    }
}
