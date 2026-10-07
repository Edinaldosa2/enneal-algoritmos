namespace Enneal.Algoritmos.Busca;

/// <summary>
/// Resultado de uma busca.
/// </summary>
/// <param name="Posicao">Índice onde o alvo foi encontrado, ou -1 se não estiver no vetor.</param>
/// <param name="Comparacoes">Quantas comparações a busca fez.</param>
public sealed record ResultadoBusca(int Posicao, long Comparacoes)
{
    /// <summary>Se o alvo foi encontrado.</summary>
    public bool Encontrado => Posicao >= 0;
}

/// <summary>
/// Busca linear e busca binária, exatamente como no Reel.
/// Cada comparação entre um item do vetor e o alvo conta 1.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b>Linear:</b> olha item por item, do começo ao fim. Funciona em qualquer vetor.
/// Tempo O(n): no pior caso, n comparações.</item>
/// <item><b>Binária:</b> só funciona em vetor ORDENADO. Olha o meio e descarta a metade onde o alvo
/// não pode estar. Tempo O(log n): com 1.024 itens, no máximo 11 comparações; com 1 milhão, 20.</item>
/// </list>
/// </remarks>
public static class Buscas
{
    /// <summary>
    /// Busca linear: compara o alvo com cada item, do primeiro ao último.
    /// </summary>
    /// <param name="v">Vetor (não precisa estar ordenado).</param>
    /// <param name="alvo">Valor procurado.</param>
    /// <param name="aoComparar">Opcional: chamado a cada comparação com (item do vetor, alvo).</param>
    /// <returns>Posição do alvo (ou -1) e quantas comparações foram feitas.</returns>
    public static ResultadoBusca Linear(IReadOnlyList<int> v, int alvo, Action<int, int>? aoComparar = null)
    {
        ArgumentNullException.ThrowIfNull(v);
        long comparacoes = 0;

        for (int i = 0; i < v.Count; i++)
        {
            if (Compara(v[i], alvo, ref comparacoes, aoComparar) == 0)
                return new ResultadoBusca(i, comparacoes);
        }
        return new ResultadoBusca(-1, comparacoes);
    }

    /// <summary>
    /// Busca binária: olha o item do meio; se o alvo é maior, descarta a metade da esquerda,
    /// se é menor, descarta a da direita. Repete até achar ou não sobrar nada.
    /// </summary>
    /// <param name="v">Vetor em ordem CRESCENTE.</param>
    /// <param name="alvo">Valor procurado.</param>
    /// <param name="aoComparar">Opcional: chamado a cada comparação com (item do vetor, alvo).</param>
    /// <returns>Posição do alvo (ou -1) e quantas comparações foram feitas.</returns>
    public static ResultadoBusca Binaria(IReadOnlyList<int> v, int alvo, Action<int, int>? aoComparar = null)
    {
        ArgumentNullException.ThrowIfNull(v);
        long comparacoes = 0;

        int lo = 0, hi = v.Count - 1; // o alvo, se existir, está entre lo e hi
        while (lo <= hi)
        {
            int meio = lo + (hi - lo) / 2; // assim não estoura com vetores enormes
            int c = Compara(v[meio], alvo, ref comparacoes, aoComparar);
            if (c == 0) return new ResultadoBusca(meio, comparacoes);
            if (c < 0) lo = meio + 1;  // item do meio é menor: alvo à direita
            else hi = meio - 1;        // item do meio é maior: alvo à esquerda
        }
        return new ResultadoBusca(-1, comparacoes);
    }

    // Uma comparação: devolve negativo, zero ou positivo, como CompareTo.
    private static int Compara(int item, int alvo, ref long comparacoes, Action<int, int>? aoComparar)
    {
        comparacoes++;
        aoComparar?.Invoke(item, alvo);
        return item.CompareTo(alvo);
    }
}
