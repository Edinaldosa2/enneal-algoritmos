// =====================================================================
//  Qual a nota do HTTPS do seu site?
//  Enneal · @enneal.it · https://enneal.com.br
//
//  A conexão passa por DNS -> TCP -> TLS -> Certificado -> HTTP. O
//  verificador mostra em que camada ela quebra (SslStream + validação X509
//  de verdade) e a nota segue o SSL Server Rating Guide do SSL Labs (2009r).
//
//  Laboratório local: cada caso é um servidor TLS em 127.0.0.1 com um
//  certificado gerado na hora (nomes fictícios, inspirados nos casos de
//  teste do badssl.com). Nada sai da máquina.
//
//  Notebook: kaggle.com/code/edinaldos/https-tls-health-monitor-grades-net-worker
//  O código está em src/Enneal.Algoritmos.NotaTls. Números do Reel:
//    DNS: sem nota | TLS 1.0: C (90 pontos) | expirado: T | nome errado: M
//    autoassinado: T | seu-site.example: A+ (93 pontos, TLS 1.3, HSTS 365 dias)
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run                -> o resultado de cada caso
//    dotnet run -- --camadas   -> também cada camada e os tetos da nota
// =====================================================================

using System.Globalization;
using System.Text;
using Enneal.Algoritmos.NotaTls;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

bool mostrarCamadas = args.Contains("--camadas");
using var lab = new Laboratorio();
var verificador = new VerificadorTls(lab);

int caso = 0;
foreach (var s in ServidorDeTeste.CasosDoReel)
{
    caso++;
    lab.Subir(s);
    var q = await verificador.ChecarAsync(s.Host);
    if (mostrarCamadas) MostrarCamadas(caso, q);

    if (q.Camada is "DNS" or "TCP")
    {
        Console.WriteLine($"Caso {caso}: {s.Host} | quebra no {q.Camada}: {q.Motivo} | sem nota");
        continue;
    }

    var nota = NotaSslLabs.Calcular(s, q);
    string onde = q.Quebrou ? $"quebra no {q.Camada}: {q.Motivo}" : $"chegou no HTTP: {q.Motivo}";
    string semConfianca = q.Camada == "Certificado" ? $" (sem a confiança: {NotaSslLabs.SemContarOCertificado(s).Letra})" : "";
    if (mostrarCamadas)
        Console.WriteLine($"N {caso} tetos {(nota.Tetos.Count == 0 ? "-" : string.Join(",", nota.Tetos))} final {nota.Letra}");
    Console.WriteLine($"Caso {caso}: {s.Host} | {onde} | nota {nota.Letra}{semConfianca} | {nota.Pontos:0.0} pontos");
}

// Com --camadas: uma linha por camada até onde a conexão chegou (T caso camada ok|quebra detalhes).
static void MostrarCamadas(int caso, ResultadoConexao q)
{
    foreach (string c in Camadas.Ordem)
    {
        bool aqui = c == q.Camada;
        string detalhe = c switch
        {
            "TLS" when q.Protocolo != System.Security.Authentication.SslProtocols.None => $" {q.Protocolo}",
            "Certificado" when q.Certificado is { } cert =>
                $" {cert.NomeDns} até {cert.ValidoAte:yyyy-MM-dd}" +
                $" ({Math.Floor((VerificadorTls.DataDoScan - cert.ValidoAte).TotalDays)} dias) cadeia {cert.Cadeia}" +
                $" nome {(cert.NomeErrado ? "errado" : "ok")}",
            _ => "",
        };
        Console.WriteLine($"T {caso} {c} {(aqui && q.Quebrou ? "quebra" : "ok")}{detalhe}{(aqui ? $" | {q.Motivo}" : "")}");
        if (aqui) break;
    }
}
