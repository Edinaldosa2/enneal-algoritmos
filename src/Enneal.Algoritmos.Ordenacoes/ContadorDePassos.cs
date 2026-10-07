namespace Enneal.Algoritmos.Ordenacoes;

/// <summary>
/// Conta o trabalho de um algoritmo de ordenação.
/// </summary>
/// <remarks>
/// Regra da corrida dos Reels: <b>1 passo = 1 comparação, 1 troca ou 1 escrita no vetor</b>.
/// Todos os algoritmos deste projeto mexem no vetor SÓ por estes três métodos,
/// então a contagem é justa.
/// </remarks>
public sealed class ContadorDePassos
{
    /// <summary>Quantas comparações foram feitas (chamadas de <see cref="Menor"/>).</summary>
    public int Comparacoes { get; private set; }

    /// <summary>Quantas trocas de posição foram feitas (chamadas de <see cref="Troca"/>).</summary>
    public int Trocas { get; private set; }

    /// <summary>Quantas escritas diretas no vetor foram feitas (chamadas de <see cref="Escreve"/>).</summary>
    public int Escritas { get; private set; }

    /// <summary>Total de passos: comparações + trocas + escritas.</summary>
    public int Passos => Comparacoes + Trocas + Escritas;

    /// <summary>Compara dois valores e conta 1 comparação.</summary>
    /// <param name="a">Primeiro valor.</param>
    /// <param name="b">Segundo valor.</param>
    /// <returns><see langword="true"/> se <paramref name="a"/> for menor que <paramref name="b"/>.</returns>
    public bool Menor(int a, int b)
    {
        Comparacoes++;
        return a < b;
    }

    /// <summary>Troca duas posições do vetor e conta 1 troca.</summary>
    /// <param name="v">Vetor.</param>
    /// <param name="a">Primeira posição.</param>
    /// <param name="b">Segunda posição.</param>
    public void Troca(int[] v, int a, int b)
    {
        (v[a], v[b]) = (v[b], v[a]);
        Trocas++;
    }

    /// <summary>Escreve um valor numa posição do vetor e conta 1 escrita.</summary>
    /// <param name="v">Vetor.</param>
    /// <param name="k">Posição.</param>
    /// <param name="x">Valor.</param>
    public void Escreve(int[] v, int k, int x)
    {
        v[k] = x;
        Escritas++;
    }
}
