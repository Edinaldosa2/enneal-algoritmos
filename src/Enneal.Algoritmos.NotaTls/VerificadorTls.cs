using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Enneal.Algoritmos.NotaTls;

/// <summary>
/// O cliente que confere um site camada por camada: DNS → TCP → TLS → Certificado → HTTP.
/// Usa as peças de verdade do .NET (<see cref="TcpClient"/>, <see cref="SslStream"/>, <see cref="X509Chain"/>),
/// mas só conversa com os servidores do <see cref="Laboratorio"/>.
/// </summary>
public sealed class VerificadorTls
{
    /// <summary>
    /// A data e hora do scan do notebook (04/10/2026 11:40:25 UTC). A cadeia é validada nesse instante para o
    /// resultado ser sempre o mesmo, em qualquer dia que você rodar.
    /// </summary>
    public static readonly DateTime DataDoScan = new(2026, 10, 4, 11, 40, 25, DateTimeKind.Utc);

    private readonly Laboratorio _lab;
    private readonly DateTime _agora;

    /// <summary>
    /// Cria o verificador.
    /// </summary>
    /// <param name="lab">O laboratório (o "DNS" e a AC confiável).</param>
    /// <param name="agora">Instante usado para validar as datas do certificado; padrão: <see cref="DataDoScan"/>.</param>
    public VerificadorTls(Laboratorio lab, DateTime? agora = null)
    {
        _lab = lab ?? throw new ArgumentNullException(nameof(lab));
        _agora = agora ?? DataDoScan;
    }

    /// <summary>
    /// Confere o site e diz em que camada a conexão quebrou (ou que chegou no HTTP).
    /// </summary>
    /// <param name="host">Nome do site.</param>
    /// <returns>Camada, motivo, versão de TLS e o certificado visto.</returns>
    public async Task<ResultadoConexao> ChecarAsync(string host)
    {
        // 1) DNS: o nome existe?
        if (!_lab.Resolver(host, out int porta))
            return new ResultadoConexao("DNS", "nome não existe");

        // 2) TCP: abre a conexão (no laboratório, sempre em 127.0.0.1).
        using var tcp = new TcpClient();
        try { await tcp.ConnectAsync(IPAddress.Loopback, porta); }
        catch (SocketException) { return new ResultadoConexao("TCP", "conexão recusada"); }

        // 3) TLS: o handshake, aceitando só TLS 1.2 e 1.3 (como um cliente atual).
        var erros = SslPolicyErrors.None;
        var cadeia = X509ChainStatusFlags.NoError;
        X509Certificate2? visto = null;

        bool Validar(object remetente, X509Certificate? cert, X509Chain? _, SslPolicyErrors e)
        {
            erros = e;                          // o SslStream já conferiu o nome do site
            if (cert is null) return true;
            visto = new X509Certificate2(cert);
            using var ch = new X509Chain();     // a cadeia, contra a AC do laboratório, na data do scan
            ch.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            ch.ChainPolicy.CustomTrustStore.Add(_lab.Ac);
            ch.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            ch.ChainPolicy.DisableCertificateDownloads = true;
            ch.ChainPolicy.VerificationTime = _agora;
            ch.Build(visto);
            foreach (var st in ch.ChainStatus) cadeia |= st.Status;
            return true;                        // deixa o handshake terminar: quem decide é o código abaixo
        }

        using var ssl = new SslStream(tcp.GetStream(), false, Validar);
        try
        {
            await ssl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = host,
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            });
        }
        catch (Exception e) when (e is AuthenticationException or IOException)
        {
            return new ResultadoConexao("TLS", "versão recusada");
        }

        var protocolo = ssl.SslProtocol;
        CertificadoVisto? info = visto is null ? null : new CertificadoVisto(
            visto.GetNameInfo(X509NameType.DnsName, false),
            visto.NotAfter.ToUniversalTime(),
            cadeia,
            erros.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch));
        visto?.Dispose();

        // 4) Certificado: nome, validade e confiança, nessa ordem.
        if (erros.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
            return new ResultadoConexao("Certificado", "nome não bate com o site", true, protocolo, info);
        if (cadeia.HasFlag(X509ChainStatusFlags.NotTimeValid))
            return new ResultadoConexao("Certificado", "expirado", true, protocolo, info);
        if (cadeia.HasFlag(X509ChainStatusFlags.UntrustedRoot))
            return new ResultadoConexao("Certificado", "autoassinado", true, protocolo, info);

        // 5) HTTP: GET / e lê o status e o HSTS.
        string http = await GetAsync(ssl, host);
        return new ResultadoConexao("HTTP", http, false, protocolo, info);
    }

    private static async Task<string> GetAsync(SslStream ssl, string host)
    {
        await ssl.WriteAsync(Encoding.ASCII.GetBytes($"GET / HTTP/1.1\r\nHost: {host}\r\nConnection: close\r\n\r\n"));
        string resposta = await new StreamReader(ssl, Encoding.ASCII).ReadToEndAsync();
        string[] linhas = resposta.Split("\r\n");
        int status = int.Parse(linhas[0].Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture);
        string? hsts = linhas.FirstOrDefault(l => l.StartsWith("Strict-Transport-Security:", StringComparison.OrdinalIgnoreCase));
        if (hsts is null) return $"{status} sem HSTS";
        long segundos = long.Parse(hsts.Split("max-age=")[1].Split(';')[0], System.Globalization.CultureInfo.InvariantCulture);
        return $"{status} · HSTS {segundos / 86400} dias";
    }
}
