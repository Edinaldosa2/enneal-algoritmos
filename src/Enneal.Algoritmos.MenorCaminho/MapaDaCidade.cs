namespace Enneal.Algoritmos.MenorCaminho;

/// <summary>
/// O mapa de cidade do Reel (9 linhas x 17 colunas). Legenda:
/// <list type="table">
/// <item><term><c>A</c></term><description>partida</description></item>
/// <item><term><c>B</c></term><description>destino</description></item>
/// <item><term><c>.</c></term><description>rua livre (custo 1)</description></item>
/// <item><term><c>m</c></term><description>trânsito (custo 3)</description></item>
/// <item><term><c>T</c></term><description>engarrafamento (custo 6)</description></item>
/// <item><term><c>#</c></term><description>prédio (bloqueia)</description></item>
/// <item><term><c>P</c></term><description>praça (bloqueia)</description></item>
/// </list>
/// </summary>
public static class MapaDaCidade
{
    /// <summary>
    /// O mapa usado no Reel: Dijkstra acha a rota de custo 18 explorando 89 nós;
    /// A* acha a mesma rota explorando só 27.
    /// </summary>
    public static IReadOnlyList<string> DoReel { get; } =
    [
        "........m........",
        ".##.###.#.###.##.",
        ".##.###...###.##.",
        ".##...........##.",
        "A.....TTTT......B",
        ".##.mmmmmm....##.",
        ".##.PPP...###.##.",
        ".##.PPP.#.###.##.",
        "........m........",
    ];

    /// <summary>
    /// Custo de ENTRAR numa esquina: trânsito 3, engarrafamento 6, o resto 1.
    /// </summary>
    /// <param name="simbolo">Caractere do mapa.</param>
    /// <returns>Custo do passo.</returns>
    public static int Peso(char simbolo) => simbolo switch
    {
        'm' => 3,
        'T' => 6,
        _ => 1,
    };

    /// <summary>
    /// Se dá para passar pela esquina (prédios e praças bloqueiam).
    /// </summary>
    /// <param name="simbolo">Caractere do mapa.</param>
    /// <returns><see langword="true"/> se for transitável.</returns>
    public static bool Transitavel(char simbolo) => simbolo is not ('#' or 'P');
}
