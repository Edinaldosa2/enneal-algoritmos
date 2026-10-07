namespace Enneal.Algoritmos.NRainhas;

/// <summary>
/// Resultado da busca pela primeira solução do problema das N rainhas.
/// </summary>
/// <param name="N">Tamanho do tabuleiro (N x N).</param>
/// <param name="Rainhas">
/// Coluna da rainha em cada linha, de cima para baixo (índices a partir de 0).
/// Quando não existe solução, todas as posições valem -1.
/// </param>
/// <param name="Tentativas">Casas testadas até a primeira solução (cada chamada de <c>Seguro</c>).</param>
/// <param name="Voltas">Rainhas retiradas durante a busca (o "backtrack").</param>
public sealed record ResultadoNRainhas(int N, IReadOnlyList<int> Rainhas, int Tentativas, int Voltas)
{
    /// <summary>
    /// <see langword="true"/> quando todas as linhas receberam uma rainha.
    /// </summary>
    public bool Resolvido => Rainhas.All(coluna => coluna >= 0);
}
