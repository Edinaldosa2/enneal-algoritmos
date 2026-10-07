namespace Enneal.Algoritmos.TorreHanoi;

/// <summary>
/// Resultado de uma partida da Torre de Hanói.
/// </summary>
/// <param name="Discos">Quantidade de discos.</param>
/// <param name="Movimentos">Quantos movimentos a recursão fez (sempre 2^n - 1).</param>
/// <param name="Resolvida">Se a torre terminou inteira no pino C, na ordem certa.</param>
public sealed record ResultadoHanoi(int Discos, long Movimentos, bool Resolvida);

/// <summary>
/// A Torre de Hanói resolvida com recursão, exatamente como no Reel.
/// </summary>
/// <remarks>
/// <para>Regra: levar a torre do pino A para o C, um disco por vez, sem nunca pôr um disco maior
/// sobre um menor. O pino B serve de apoio.</para>
/// <para>A ideia da recursão: para mover <c>n</c> discos de A para C,</para>
/// <list type="number">
/// <item>mova os <c>n - 1</c> de cima de A para B (usando C como apoio);</item>
/// <item>mova o disco <c>n</c> (o maior) de A para C;</item>
/// <item>mova os <c>n - 1</c> de B para C, por cima dele (usando A como apoio).</item>
/// </list>
/// <para>Os passos 1 e 3 são o mesmo problema, só que menor. Por isso
/// <c>T(n) = 2 · T(n - 1) + 1</c>, com <c>T(0) = 0</c>, que dá <c>2^n - 1</c> movimentos.</para>
/// </remarks>
public static class ResolvedorHanoi
{
    /// <summary>
    /// Maior número de discos que <see cref="Resolver"/> aceita (2^30 - 1 ≈ 1 bilhão de movimentos).
    /// Para mais discos, use <see cref="ContaDeHanoi.MovimentosNecessarios"/>, que só faz a conta.
    /// </summary>
    public const int MaximoDeDiscos = 30;

    /// <summary>
    /// Resolve a torre de <paramref name="discos"/> discos, movendo de verdade disco por disco.
    /// No Reel: 3 discos = 7 movimentos, 4 discos = 15, 6 discos = 63.
    /// </summary>
    /// <param name="discos">Quantidade de discos, de 0 a <see cref="MaximoDeDiscos"/>.</param>
    /// <param name="aoMover">Opcional: chamado a cada movimento (para desenhar ou registrar).</param>
    /// <returns>Quantos movimentos foram feitos e se a torre terminou certa no pino C.</returns>
    public static ResultadoHanoi Resolver(int discos, Action<Movimento>? aoMover = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(discos);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(discos, MaximoDeDiscos);

        var pinos = new Pinos(discos);
        Hanoi(pinos, discos, 'A', 'C', 'B', aoMover);
        return new ResultadoHanoi(discos, pinos.Movimentos, pinos.Resolvida);
    }

    /// <summary>
    /// A recursão do Reel: leva <paramref name="n"/> discos do pino <paramref name="de"/> para o pino
    /// <paramref name="para"/>, usando <paramref name="aux"/> como apoio.
    /// </summary>
    /// <param name="pinos">A torre (é alterada).</param>
    /// <param name="n">Quantos discos, do topo de <paramref name="de"/>, devem ser movidos.</param>
    /// <param name="de">Pino de origem.</param>
    /// <param name="para">Pino de destino.</param>
    /// <param name="aux">Pino de apoio.</param>
    /// <param name="aoMover">Opcional: chamado a cada movimento.</param>
    public static void Hanoi(Pinos pinos, int n, char de, char para, char aux, Action<Movimento>? aoMover = null)
    {
        ArgumentNullException.ThrowIfNull(pinos);

        if (n == 0) return;                          // caso base: zero discos, nada a fazer
        Hanoi(pinos, n - 1, de, aux, para, aoMover); // 1) os n - 1 de cima vão para o apoio
        var m = pinos.Mover(de, para);               // 2) o disco n (o maior desta chamada) vai para o destino
        aoMover?.Invoke(m);
        Hanoi(pinos, n - 1, aux, para, de, aoMover); // 3) os n - 1 voltam, por cima do disco n
    }
}
