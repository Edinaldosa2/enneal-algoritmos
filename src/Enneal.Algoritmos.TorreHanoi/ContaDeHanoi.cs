namespace Enneal.Algoritmos.TorreHanoi;

/// <summary>
/// A conta da Torre de Hanói sem mover disco nenhum: quantos movimentos são necessários e quanto
/// tempo isso levaria. Usa <see cref="UInt128"/> porque 2^64 - 1 não cabe num <see cref="long"/>.
/// </summary>
public static class ContaDeHanoi
{
    /// <summary>
    /// Segundos num ano de 365,25 dias (365,25 × 24 × 60 × 60), a média que conta os anos bissextos.
    /// </summary>
    public const long SegundosPorAno = 31_557_600;

    /// <summary>
    /// Quantos movimentos a torre de <paramref name="discos"/> discos precisa: <c>2^n - 1</c>.
    /// Com 64 discos: 18.446.744.073.709.551.615.
    /// </summary>
    /// <param name="discos">Quantidade de discos, de 0 a 128.</param>
    /// <returns>O número exato de movimentos.</returns>
    public static UInt128 MovimentosNecessarios(int discos)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(discos);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(discos, 128);

        // 2^n é 1 deslocado n bits para a esquerda. Com 128 discos, 2^128 não cabe em 128 bits,
        // mas 2^128 - 1 cabe: é o maior UInt128 (todos os bits ligados).
        return discos == 128 ? UInt128.MaxValue : (UInt128.One << discos) - 1;
    }

    /// <summary>
    /// Quantos anos completos levaria fazer <paramref name="movimentos"/> movimentos a 1 por segundo.
    /// Com 64 discos: 584.542.046.090 anos.
    /// </summary>
    /// <param name="movimentos">Total de movimentos (= segundos).</param>
    /// <returns>Anos completos (a divisão inteira descarta a fração).</returns>
    public static UInt128 AnosAUmMovimentoPorSegundo(UInt128 movimentos) => movimentos / SegundosPorAno;

    /// <summary>
    /// Arredonda anos para bilhões de anos (meio bilhão para cima). 584.542.046.090 anos ≈ 585 bilhões.
    /// </summary>
    /// <param name="anos">Quantidade de anos.</param>
    /// <returns>Bilhões de anos, arredondado.</returns>
    public static UInt128 BilhoesDeAnos(UInt128 anos) => (anos + 500_000_000) / 1_000_000_000;
}
