namespace Enneal.Algoritmos.Ordenacoes;

/// <summary>
/// Resultado de um algoritmo na corrida.
/// </summary>
/// <param name="Nome">Nome do algoritmo (Bubble, Quick ou Merge).</param>
/// <param name="Comparacoes">Comparações feitas.</param>
/// <param name="Trocas">Trocas feitas.</param>
/// <param name="Escritas">Escritas diretas no vetor.</param>
/// <param name="Ordenado">Se o vetor terminou em ordem crescente.</param>
public sealed record ResultadoCorrida(string Nome, int Comparacoes, int Trocas, int Escritas, bool Ordenado)
{
    /// <summary>Total de passos: comparações + trocas + escritas.</summary>
    public int Passos => Comparacoes + Trocas + Escritas;
}

/// <summary>
/// A corrida dos Reels: Bubble x Quick x Merge ordenando o MESMO vetor.
/// Quem precisar de menos passos vence.
/// </summary>
public static class CorridaDeOrdenacoes
{
    /// <summary>
    /// O vetor do Reel: os números de 1 a 28 embaralhados UMA vez com semente fixa (2026).
    /// Resultado com ele: 1º Quick (185), 2º Merge (235), 3º Bubble (536).
    /// </summary>
    public static IReadOnlyList<int> VetorDoReel { get; } =
    [
        13, 9, 7, 3, 25, 6, 19, 22, 27, 5, 12, 2, 10, 1,
        23, 15, 16, 14, 18, 24, 20, 8, 28, 21, 26, 17, 11, 4,
    ];

    /// <summary>
    /// Roda os três algoritmos, cada um com a sua CÓPIA do vetor, na ordem
    /// Bubble, Quick e Merge.
    /// </summary>
    /// <param name="vetor">Vetor de partida (não é alterado).</param>
    /// <returns>O resultado de cada algoritmo, na ordem em que correram.</returns>
    public static IReadOnlyList<ResultadoCorrida> Correr(IReadOnlyList<int> vetor)
    {
        ArgumentNullException.ThrowIfNull(vetor);

        return
        [
            Correr("Bubble", vetor, Ordenacao.BubbleSort),
            Correr("Quick", vetor, Ordenacao.QuickSort),
            Correr("Merge", vetor, Ordenacao.MergeSort),
        ];
    }

    /// <summary>
    /// Ordena os resultados do menor para o maior número de passos (quem chegou antes).
    /// Em caso de empate, mantém a ordem de largada.
    /// </summary>
    /// <param name="resultados">Resultados da corrida.</param>
    /// <returns>O pódio, do 1º ao último.</returns>
    public static IReadOnlyList<ResultadoCorrida> Podio(IEnumerable<ResultadoCorrida> resultados)
    {
        ArgumentNullException.ThrowIfNull(resultados);
        return resultados.OrderBy(r => r.Passos).ToList();
    }

    /// <summary>
    /// Confere se o vetor está em ordem crescente (aceita valores repetidos).
    /// </summary>
    /// <param name="v">Vetor.</param>
    /// <returns><see langword="true"/> se cada valor for menor ou igual ao seguinte.</returns>
    public static bool EstaOrdenado(IReadOnlyList<int> v)
    {
        ArgumentNullException.ThrowIfNull(v);
        for (int i = 1; i < v.Count; i++)
            if (v[i - 1] > v[i]) return false;
        return true;
    }

    private static ResultadoCorrida Correr(string nome, IReadOnlyList<int> vetor, Func<int[], ContadorDePassos> ordenar)
    {
        int[] v = vetor.ToArray(); // cópia: todos largam do mesmo vetor
        var passos = ordenar(v);
        return new ResultadoCorrida(nome, passos.Comparacoes, passos.Trocas, passos.Escritas, EstaOrdenado(v));
    }
}
