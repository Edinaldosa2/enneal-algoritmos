namespace Enneal.Algoritmos.Busca;

/// <summary>
/// Gera os vetores ordenados do Reel.
/// </summary>
public static class VetorOrdenado
{
    /// <summary>A semente usada no Reel.</summary>
    public const uint SementeDoReel = 2026;

    /// <summary>
    /// Gera um vetor em ordem crescente e sem repetidos: o primeiro item fica entre 3 e 7 e
    /// cada item seguinte é o anterior + 1 a 4. Mesma semente, mesmo vetor.
    /// </summary>
    /// <param name="n">Quantidade de itens (1 ou mais).</param>
    /// <param name="semente">Semente do gerador pseudoaleatório.</param>
    /// <returns>O vetor ordenado.</returns>
    public static int[] Gerar(int n, uint semente = SementeDoReel)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 1);

        // Gerador congruencial linear: simples e reproduzível.
        uint estado = semente;
        uint Rand()
        {
            estado = estado * 1664525u + 1013904223u;
            return estado >> 8;
        }

        var v = new int[n];
        v[0] = 3 + (int)(Rand() % 5);
        for (int i = 1; i < n; i++)
            v[i] = v[i - 1] + 1 + (int)(Rand() % 4);
        return v;
    }
}
