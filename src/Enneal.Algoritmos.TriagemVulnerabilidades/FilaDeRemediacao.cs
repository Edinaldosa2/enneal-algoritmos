namespace Enneal.Algoritmos.TriagemVulnerabilidades;

/// <summary>
/// A mesma fila de CVEs ordenada de três jeitos, como no Reel.
/// </summary>
public static class FilaDeRemediacao
{
    /// <summary>
    /// Ordem 1, o hábito: maior CVSS primeiro (empate: ano e número do identificador).
    /// </summary>
    /// <param name="achados">As CVEs.</param>
    /// <returns>Nova lista ordenada.</returns>
    public static List<Achado> PorCvss(IEnumerable<Achado> achados) =>
        [.. achados.OrderByDescending(a => a.Cvss).ThenBy(a => a.Ano).ThenBy(a => a.Numero)];

    /// <summary>
    /// Ordem 2: CVSS + EPSS (a <see cref="Triagem.Mistura"/>), sem olhar a KEV.
    /// </summary>
    /// <param name="achados">As CVEs.</param>
    /// <returns>Nova lista ordenada.</returns>
    public static List<Achado> ComEpss(IEnumerable<Achado> achados) =>
        [.. achados.OrderByDescending(Triagem.Mistura).ThenBy(a => a.Ano).ThenBy(a => a.Numero)];

    /// <summary>
    /// Ordem 3, a triagem: por faixa (P1 primeiro), depois maior risco, depois maior EPSS.
    /// </summary>
    /// <param name="achados">As CVEs.</param>
    /// <returns>Nova lista ordenada.</returns>
    public static List<Achado> Triada(IEnumerable<Achado> achados) =>
        [.. achados.OrderBy(Triagem.Faixa, StringComparer.Ordinal)
            .ThenByDescending(Triagem.Risco)
            .ThenByDescending(a => a.Epss)];

    /// <summary>
    /// As posições (começando em 1) das CVEs que estão na KEV, numa ordem.
    /// No Reel: por CVSS, #9, #10 e #24; com EPSS, #1, #2 e #19; na triagem, #1, #2 e #3.
    /// </summary>
    /// <param name="ordem">Uma das três ordens.</param>
    /// <returns>As posições, em ordem crescente.</returns>
    public static List<int> PosicoesDasExploradas(IReadOnlyList<Achado> ordem)
    {
        ArgumentNullException.ThrowIfNull(ordem);
        var posicoes = new List<int>();
        for (int i = 0; i < ordem.Count; i++)
            if (ordem[i].Kev) posicoes.Add(i + 1);
        return posicoes;
    }
}
