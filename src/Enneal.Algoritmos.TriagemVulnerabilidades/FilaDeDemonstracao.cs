using System.Globalization;
using System.Reflection;
using System.Text;

namespace Enneal.Algoritmos.TriagemVulnerabilidades;

/// <summary>
/// Uma linha da fila de demonstração, com as colunas que o notebook calculou (para conferência).
/// </summary>
/// <param name="Achado">A CVE e os seus sinais (CVSS, EPSS, KEV, ransomware).</param>
/// <param name="FaixaDoNotebook">A coluna <c>tier</c> do CSV.</param>
/// <param name="RiscoDoNotebook">A coluna <c>risk</c> do CSV.</param>
public sealed record LinhaDaFila(Achado Achado, string FaixaDoNotebook, double RiscoDoNotebook);

/// <summary>
/// A fila de demonstração do notebook: 400 CVEs públicas (publicadas desde 2023) com o CVSS da NVD, o EPSS da
/// FIRST e a KEV da CISA (catálogo 2026.10.02). O arquivo vai dentro da DLL; veja <c>dados/README.md</c>.
/// </summary>
public static class FilaDeDemonstracao
{
    /// <summary>
    /// Lê as 400 linhas, na ordem do CSV (que já é a ordem da triagem calculada pelo notebook).
    /// </summary>
    /// <returns>As linhas da fila.</returns>
    public static IReadOnlyList<LinhaDaFila> Carregar()
    {
        using var fluxo = typeof(FilaDeDemonstracao).Assembly.GetManifestResourceStream("remediation_queue_demo.csv")
            ?? throw new InvalidOperationException("A fila de demonstração não está embutida na DLL.");
        using var leitor = new StreamReader(fluxo, Encoding.UTF8);
        return Ler(leitor);
    }

    /// <summary>
    /// Lê uma fila no mesmo formato do notebook (CSV com cabeçalho e as colunas
    /// <c>tier, risk, cve_id, base_score, epss_score, kev, why</c>).
    /// </summary>
    /// <param name="leitor">O texto do CSV.</param>
    /// <returns>As linhas da fila.</returns>
    public static IReadOnlyList<LinhaDaFila> Ler(TextReader leitor)
    {
        ArgumentNullException.ThrowIfNull(leitor);
        var inv = CultureInfo.InvariantCulture;

        string cabecalho = leitor.ReadLine() ?? throw new InvalidDataException("CSV vazio.");
        string[] nomes = Campos(cabecalho);
        int Col(string nome) => Array.IndexOf(nomes, nome) is var i and >= 0
            ? i : throw new InvalidDataException($"Falta a coluna {nome}.");
        int cCve = Col("cve_id"), cCvss = Col("base_score"), cEpss = Col("epss_score"), cKev = Col("kev"),
            cWhy = Col("why"), cTier = Col("tier"), cRisk = Col("risk");

        var linhas = new List<LinhaDaFila>();
        for (string? linha = leitor.ReadLine(); linha is not null; linha = leitor.ReadLine())
        {
            if (linha.Length == 0) continue;
            string[] c = Campos(linha);
            var achado = new Achado(c[cCve],
                double.Parse(c[cCvss], inv),
                double.Parse(c[cEpss], inv),
                c[cKev] == "True",
                c[cWhy].Contains("ransomware", StringComparison.Ordinal)); // "Known exploited + ransomware use"
            linhas.Add(new LinhaDaFila(achado, c[cTier], double.Parse(c[cRisk], inv)));
        }
        return linhas;
    }

    // CSV simples do pandas: vírgula separa, aspas só em volta de campos que têm vírgula.
    private static string[] Campos(string linha)
    {
        var campos = new List<string>();
        var atual = new StringBuilder();
        bool dentroDeAspas = false;
        foreach (char ch in linha)
        {
            if (ch == '"') dentroDeAspas = !dentroDeAspas;
            else if (ch == ',' && !dentroDeAspas) { campos.Add(atual.ToString()); atual.Clear(); }
            else atual.Append(ch);
        }
        campos.Add(atual.ToString());
        return [.. campos];
    }
}
