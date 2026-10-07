namespace Enneal.Algoritmos.TorreHanoi;

/// <summary>
/// Um movimento da Torre de Hanói: o disco que saiu do topo de um pino e foi para o topo de outro.
/// </summary>
/// <param name="Numero">Ordem do movimento, começando em 1.</param>
/// <param name="Disco">Tamanho do disco movido (1 = o menor).</param>
/// <param name="De">Pino de onde o disco saiu ('A', 'B' ou 'C').</param>
/// <param name="Para">Pino para onde o disco foi ('A', 'B' ou 'C').</param>
public readonly record struct Movimento(long Numero, int Disco, char De, char Para)
{
    /// <summary>Texto curto do movimento, por exemplo <c>disco 1: A -&gt; C</c>.</summary>
    public override string ToString() => $"disco {Disco}: {De} -> {Para}";
}
