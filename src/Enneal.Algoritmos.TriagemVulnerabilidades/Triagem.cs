namespace Enneal.Algoritmos.TriagemVulnerabilidades;

/// <summary>
/// As contas da triagem, iguais às do notebook "High CVSS ≠ Exploited: CVSS, EPSS &amp; KEV" (célula 29).
/// </summary>
/// <remarks>
/// <para>A ideia: CVSS mede o <b>estrago possível</b>, não a <b>chance</b> de alguém explorar. A triagem junta:</para>
/// <list type="bullet">
/// <item><b>KEV</b> (CISA): já explorada de verdade → P1, prazo de 7 dias;</item>
/// <item><b>EPSS</b> (FIRST): chance de exploração nos próximos 30 dias;</item>
/// <item><b>CVSS</b> (NVD): o impacto, que desempata.</item>
/// </list>
/// </remarks>
public static class Triagem
{
    /// <summary>
    /// Risco de 0 a 100: 65% chance de exploração + 35% impacto, +5 se há uso em ransomware.
    /// A chance é 1 se a CVE está na KEV e √EPSS se não está.
    /// </summary>
    /// <param name="a">A CVE.</param>
    /// <returns>Risco arredondado em 1 casa.</returns>
    public static double Risco(Achado a)
    {
        ArgumentNullException.ThrowIfNull(a);
        double chance = a.Kev ? 1.0 : Math.Sqrt(a.Epss); // a raiz "abre" os EPSS pequenos
        double impacto = a.Cvss / 10.0;
        double s = 100 * (0.65 * chance + 0.35 * impacto);
        if (a.Ransomware) s += 5;
        return Math.Round(Math.Clamp(s, 0, 100), 1);
    }

    /// <summary>
    /// A mesma conta sem a KEV (a ordem 2 do Reel): 0,65 × √EPSS + 0,35 × CVSS/10.
    /// </summary>
    /// <param name="a">A CVE.</param>
    /// <returns>Mistura de 0 a 1.</returns>
    public static double Mistura(Achado a)
    {
        ArgumentNullException.ThrowIfNull(a);
        return 0.65 * Math.Sqrt(a.Epss) + 0.35 * (a.Cvss / 10.0);
    }

    /// <summary>
    /// A faixa de prioridade. A regra vem antes; o risco só ordena dentro da faixa.
    /// </summary>
    /// <param name="a">A CVE.</param>
    /// <returns>
    /// P1 (na KEV, 7 dias), P2 (EPSS ≥ 0,10, ou CVSS ≥ 9 e EPSS ≥ 0,05: 30 dias), P3 (CVSS ≥ 7: 90 dias)
    /// ou P4 (backlog).
    /// </returns>
    public static string Faixa(Achado a)
    {
        ArgumentNullException.ThrowIfNull(a);
        return a.Kev ? "P1"
            : a.Epss >= 0.10 ? "P2"
            : a.Cvss >= 9 && a.Epss >= 0.05 ? "P2"
            : a.Cvss >= 7 ? "P3"
            : "P4";
    }

    /// <summary>Prazo sugerido para corrigir, em dias, por faixa (P4 não tem prazo: backlog).</summary>
    /// <param name="faixa">"P1", "P2", "P3" ou "P4".</param>
    /// <returns>7, 30, 90 ou <see langword="null"/>.</returns>
    public static int? PrazoEmDias(string faixa) => faixa switch
    {
        "P1" => 7,
        "P2" => 30,
        "P3" => 90,
        _ => null,
    };
}
