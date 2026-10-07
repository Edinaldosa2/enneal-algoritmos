namespace Enneal.Algoritmos.Busca;

/// <summary>
/// Como as duas buscas se comportam num vetor de <paramref name="Itens"/> itens.
/// </summary>
/// <param name="Itens">Tamanho do vetor.</param>
/// <param name="LinearPior">Comparações da linear no pior caso (alvo ausente: percorre tudo).</param>
/// <param name="BinariaPior">Máximo de comparações da binária (procurando cada item e um ausente).</param>
/// <param name="BinariaMedia">Média de comparações da binária procurando cada item do vetor.</param>
public sealed record EscalaDaBusca(int Itens, long LinearPior, long BinariaPior, double BinariaMedia);

/// <summary>
/// Mede o pior caso da linear e o pior caso e a média da binária num vetor.
/// </summary>
public static class Escala
{
    /// <summary>
    /// Mede as duas buscas no vetor: a linear procurando um alvo ausente (o pior caso) e a
    /// binária procurando o ausente e CADA item do vetor.
    /// No Reel: 1.024 itens = linear até 1.024, binária até 11 (média 9,01);
    /// 1.000.000 de itens = linear até 1.000.000, binária até 20 (média 18,95).
    /// </summary>
    /// <param name="v">Vetor em ordem crescente, sem repetidos.</param>
    /// <returns>Pior caso da linear, pior caso e média da binária.</returns>
    public static EscalaDaBusca Medir(IReadOnlyList<int> v)
    {
        ArgumentNullException.ThrowIfNull(v);
        if (v.Count == 0) throw new ArgumentException("O vetor não pode ser vazio.", nameof(v));

        int ausente = v[^1] + 1; // maior que todos: não está no vetor
        long linearPior = Buscas.Linear(v, ausente).Comparacoes;
        long binariaPior = Buscas.Binaria(v, ausente).Comparacoes;

        long soma = 0;
        for (int i = 0; i < v.Count; i++)
        {
            long c = Buscas.Binaria(v, v[i]).Comparacoes;
            binariaPior = Math.Max(binariaPior, c);
            soma += c;
        }
        return new EscalaDaBusca(v.Count, linearPior, binariaPior, (double)soma / v.Count);
    }
}
