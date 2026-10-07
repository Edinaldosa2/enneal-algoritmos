namespace Enneal.Algoritmos.CaminhoInvasor;

/// <summary>
/// Os dois mapas de rede do Reel. Legenda:
/// <c>I</c> = Internet (início), <c>B</c> = banco de dados, <c>#</c> = parede, <c>.</c> = livre.
/// </summary>
public static class MapasDoReel
{
    /// <summary>
    /// Rede SEM defesas: o invasor chega no banco em 93 passos.
    /// </summary>
    public static IReadOnlyList<string> SemDefesas { get; } =
    [
        "......I......",
        ".............",
        ".##..###..##.",
        ".............",
        "...#.....#...",   // firewall: sem WAF
        ".#....#....#.",   // login: sem MFA
        "....#...#....",   // app: acesso total
        ".............",   // rede do banco: aberta
        "......B......",
    ];

    /// <summary>
    /// Rede com defesa EM CAMADAS: cada camada tem furos, mas os furos não se alinham.
    /// Resultado: 0 caminhos até o banco (48 células exploradas).
    /// </summary>
    public static IReadOnlyList<string> EmCamadas { get; } =
    [
        "......I......",
        ".............",
        ".##..###..##.",
        ".............",
        "##.######.###",   // camada 1: WAF (firewall de aplicação web)
        "####.####.###",   // camada 2: MFA no login
        "#.#########.#",   // camada 3: menor privilégio
        "#######.#####",   // camada 4: criptografia + segmentação de rede
        "......B......",
    ];
}
