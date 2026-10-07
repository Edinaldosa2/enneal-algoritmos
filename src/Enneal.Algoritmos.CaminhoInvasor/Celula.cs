namespace Enneal.Algoritmos.CaminhoInvasor;

/// <summary>
/// Uma posição no mapa: linha (de cima para baixo) e coluna (da esquerda para a direita),
/// ambas a partir de 0.
/// </summary>
/// <param name="Linha">Linha da célula.</param>
/// <param name="Coluna">Coluna da célula.</param>
public readonly record struct Celula(int Linha, int Coluna);
