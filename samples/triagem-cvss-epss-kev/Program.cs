// =====================================================================
//  CVSS alto não é explorada: triagem com CVSS x EPSS x CISA KEV
//  Enneal · @enneal.it · https://enneal.com.br
//
//  A fila de demonstração do notebook (400 CVEs públicas com o CVSS da
//  NVD, o EPSS da FIRST e a KEV da CISA) ordenada de 3 jeitos:
//    1) maior CVSS primeiro (o hábito)
//    2) + EPSS (chance de exploração nos próximos 30 dias)
//    3) + KEV (já explorada de verdade) = P1
//  e conferida com as colunas risk e tier que o notebook calculou.
//
//  Notebook: kaggle.com/code/edinaldos/high-cvss-exploited-cvss-epss-kev-vulnerabil
//  A triagem está em src/Enneal.Algoritmos.TriagemVulnerabilidades/Triagem.cs.
//  Números do Reel: exploradas em #9, #10, #24 -> #1, #2, #19 -> #1, #2, #3;
//  P1 3, P2 5, P3 197, P4 195; 31 CVEs com CVSS >= 9; P1+P2 = 8.
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run             -> o resumo das 3 ordens
//    dotnet run -- --fila   -> também cada CVE (sinais, risco, faixa e posições)
// =====================================================================

using System.Globalization;
using System.Text;
using Enneal.Algoritmos.TriagemVulnerabilidades;

Console.OutputEncoding = Encoding.UTF8;
var inv = CultureInfo.InvariantCulture;

var linhas = FilaDeDemonstracao.Carregar();
var achados = linhas.Select(l => l.Achado).ToList();

var porCvss = FilaDeRemediacao.PorCvss(achados);
var comEpss = FilaDeRemediacao.ComEpss(achados);
var fila = FilaDeRemediacao.Triada(achados);

if (args.Contains("--fila"))
{
    // Posição de cada CVE nas 3 ordens (os records são comparados por valor, e cada CVE aparece uma vez).
    var pCvss = Posicoes(porCvss);
    var pEpss = Posicoes(comEpss);
    var pFila = Posicoes(fila);
    foreach (var a in achados)
        Console.WriteLine($"Q {a.Cve} cvss={a.Cvss.ToString(inv)} epss={a.Epss.ToString(inv)} " +
                          $"kev={(a.Kev ? 1 : 0)} ransomware={(a.Ransomware ? 1 : 0)} " +
                          $"mistura={Triagem.Mistura(a).ToString("F6", inv)} risco={Triagem.Risco(a).ToString("F1", inv)} " +
                          $"faixa={Triagem.Faixa(a)} cvss#={pCvss[a.Cve]} epss#={pEpss[a.Cve]} fila#={pFila[a.Cve]}");
}

string Kev(List<Achado> ordem) => string.Join(", ", FilaDeRemediacao.PosicoesDasExploradas(ordem).Select(p => "#" + p));
var faixas = fila.GroupBy(Triagem.Faixa).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => $"{g.Key} {g.Count()}");

Console.WriteLine($"1) CVSS primeiro: exploradas (KEV) em {Kev(porCvss)}");
Console.WriteLine($"2) + EPSS: exploradas em {Kev(comEpss)}");
Console.WriteLine($"3) + KEV: exploradas em {Kev(fila)} | {string.Join(", ", faixas)}");

int topoCvss = achados.Count(a => a.Cvss >= 9);
var urgentes = fila.Where(a => Triagem.Faixa(a) is "P1" or "P2").ToList();
Console.WriteLine($"Topo do 'CVSS >= 9 primeiro': {topoCvss} | P1+P2: {urgentes.Count} ({urgentes.Count(a => a.Kev)} exploradas)");

// A mesma conta do notebook? Compara com as colunas risk e tier e com a ordem do CSV.
int riscoIgual = linhas.Count(l => Triagem.Risco(l.Achado).ToString("F1", inv) == l.RiscoDoNotebook.ToString("F1", inv));
int faixaIgual = linhas.Count(l => Triagem.Faixa(l.Achado) == l.FaixaDoNotebook);
bool ordemIgual = fila.SequenceEqual(achados); // o CSV do notebook já vem na ordem da fila
Console.WriteLine($"Conferência com o notebook: risco {riscoIgual}/{achados.Count}, faixa {faixaIgual}/{achados.Count}, " +
                  $"ordem da fila {(ordemIgual ? "igual" : "DIFERENTE")}");

return riscoIgual == achados.Count && faixaIgual == achados.Count && ordemIgual ? 0 : 1;

static Dictionary<string, int> Posicoes(List<Achado> ordem) =>
    ordem.Select((a, i) => (a.Cve, i + 1)).ToDictionary(x => x.Cve, x => x.Item2);
