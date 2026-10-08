using System.Globalization;

namespace Enneal.Algoritmos.TriagemVulnerabilidades;

/// <summary>
/// Uma CVE encontrada nos seus sistemas, com os três sinais que a triagem usa.
/// </summary>
/// <param name="Cve">Identificador, por exemplo <c>CVE-2024-23113</c>.</param>
/// <param name="Cvss">Nota CVSS base (0 a 10), da NVD: o tamanho do estrago SE alguém explorar.</param>
/// <param name="Epss">EPSS (0 a 1), da FIRST: a chance de a CVE ser explorada nos próximos 30 dias.</param>
/// <param name="Kev">Se está no catálogo KEV da CISA: já foi explorada de verdade.</param>
/// <param name="Ransomware">Se a KEV marca uso conhecido em campanhas de ransomware.</param>
public sealed record Achado(string Cve, double Cvss, double Epss, bool Kev, bool Ransomware)
{
    /// <summary>Ano do identificador (CVE-<b>2024</b>-23113), usado para desempatar.</summary>
    public int Ano => int.Parse(Cve.Split('-')[1], CultureInfo.InvariantCulture);

    /// <summary>Número do identificador (CVE-2024-<b>23113</b>), usado para desempatar.</summary>
    public int Numero => int.Parse(Cve.Split('-')[2], CultureInfo.InvariantCulture);
}
