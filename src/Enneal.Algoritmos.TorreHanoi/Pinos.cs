namespace Enneal.Algoritmos.TorreHanoi;

/// <summary>
/// Os três pinos da torre (A, B e C). Cada pino é uma pilha: só dá para tirar ou pôr o disco do topo.
/// </summary>
/// <remarks>
/// No Reel, <c>Mover</c> é só <c>pino[para].Push(pino[de].Pop())</c>. Aqui ele faz o mesmo, mas antes
/// confere a regra do jogo: se alguém tentar pôr um disco maior sobre um menor (ou tirar disco de um
/// pino vazio), dá erro. Assim, se a recursão estivesse errada, o programa avisaria na hora.
/// </remarks>
public sealed class Pinos
{
    /// <summary>Os nomes dos pinos, na ordem em que aparecem no Reel.</summary>
    public static IReadOnlyList<char> Nomes { get; } = ['A', 'B', 'C'];

    private readonly Dictionary<char, Stack<int>> _pino = new()
    {
        ['A'] = new(),
        ['B'] = new(),
        ['C'] = new(),
    };

    /// <summary>
    /// Monta a torre inicial: <paramref name="discos"/> discos no pino A, o maior embaixo.
    /// </summary>
    /// <param name="discos">Quantidade de discos (0 ou mais).</param>
    public Pinos(int discos)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(discos);
        for (int d = discos; d >= 1; d--) _pino['A'].Push(d); // empilha do maior para o menor
        Discos = discos;
    }

    /// <summary>Quantidade total de discos no jogo.</summary>
    public int Discos { get; }

    /// <summary>Quantos movimentos já foram feitos.</summary>
    public long Movimentos { get; private set; }

    /// <summary>
    /// Os discos de um pino, do topo para a base (por exemplo 1, 2, 3).
    /// </summary>
    /// <param name="nome">'A', 'B' ou 'C'.</param>
    /// <returns>Cópia dos discos do pino; mexer nela não mexe na torre.</returns>
    public IReadOnlyList<int> DiscosNoPino(char nome) => [.. Pilha(nome)];

    /// <summary>
    /// Move o disco do topo de <paramref name="de"/> para o topo de <paramref name="para"/>.
    /// </summary>
    /// <param name="de">Pino de origem.</param>
    /// <param name="para">Pino de destino.</param>
    /// <returns>O movimento feito (número, disco, origem e destino).</returns>
    /// <exception cref="InvalidOperationException">
    /// Se o pino de origem estiver vazio ou se o disco ficaria sobre um disco menor.
    /// </exception>
    public Movimento Mover(char de, char para)
    {
        var origem = Pilha(de);
        var destino = Pilha(para);

        if (origem.Count == 0)
            throw new InvalidOperationException($"O pino {de} está vazio.");
        if (destino.Count > 0 && destino.Peek() < origem.Peek())
            throw new InvalidOperationException(
                $"Regra quebrada: disco {origem.Peek()} sobre o disco {destino.Peek()} no pino {para}.");

        int disco = origem.Pop();
        destino.Push(disco);
        Movimentos++;
        return new Movimento(Movimentos, disco, de, para);
    }

    /// <summary>
    /// Se a torre inteira está no pino C, na ordem certa (1, 2, ..., n do topo para a base),
    /// e os pinos A e B estão vazios.
    /// </summary>
    public bool Resolvida =>
        _pino['A'].Count == 0 && _pino['B'].Count == 0 &&
        _pino['C'].SequenceEqual(Enumerable.Range(1, Discos));

    private Stack<int> Pilha(char nome) =>
        _pino.TryGetValue(nome, out var pilha)
            ? pilha
            : throw new ArgumentOutOfRangeException(nameof(nome), nome, "Os pinos são 'A', 'B' e 'C'.");
}
