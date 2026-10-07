namespace Enneal.Algoritmos.MenorCaminho;

/// <summary>
/// Uma esquina (célula) do mapa: linha e coluna, a partir de 0.
/// </summary>
/// <param name="Linha">Linha, de cima para baixo.</param>
/// <param name="Coluna">Coluna, da esquerda para a direita.</param>
public readonly record struct Esquina(int Linha, int Coluna)
{
    /// <summary>Texto no formato <c>linha,coluna</c> (ex.: <c>4,0</c>).</summary>
    /// <returns>A posição como texto.</returns>
    public override string ToString() => $"{Linha},{Coluna}";
}
