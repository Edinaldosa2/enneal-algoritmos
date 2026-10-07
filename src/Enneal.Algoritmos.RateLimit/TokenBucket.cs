using static System.Math;

namespace Enneal.Algoritmos.RateLimit;

/// <summary>
/// Token bucket ("balde de fichas"): o algoritmo clássico de rate limit.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>O balde começa cheio (<c>capacidade</c> fichas) e recarrega <c>recargaPorTick</c> fichas
/// por unidade de tempo, sem nunca passar da capacidade.</item>
/// <item>Cada requisição gasta 1 ficha. Sem ficha, a requisição é recusada (HTTP 429).</item>
/// </list>
/// Assim cada cliente pode fazer pequenas rajadas (até a capacidade), mas no longo prazo
/// fica limitado à taxa de recarga. Custo O(1) por requisição.
/// </remarks>
public sealed class TokenBucket
{
    private readonly int _capacidade;
    private readonly double _recargaPorTick;
    private int _ultimoTick; // tick da última recarga

    /// <summary>
    /// Cria um balde cheio.
    /// </summary>
    /// <param name="capacidade">Máximo de fichas (tamanho da rajada permitida).</param>
    /// <param name="recargaPorTick">Fichas ganhas por tick (taxa sustentada).</param>
    public TokenBucket(int capacidade, double recargaPorTick)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacidade, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(recargaPorTick);

        _capacidade = capacidade;
        _recargaPorTick = recargaPorTick;
        Fichas = capacidade;
    }

    /// <summary>Fichas disponíveis agora (pode ser fracionário).</summary>
    public double Fichas { get; private set; }

    /// <summary>
    /// Recarrega pelo tempo que passou e tenta gastar 1 ficha.
    /// </summary>
    /// <param name="tick">Momento atual (não pode voltar no tempo).</param>
    /// <returns><see langword="true"/> se havia ficha (requisição liberada).</returns>
    public bool TentarConsumir(int tick)
    {
        Fichas = Min(_capacidade, Fichas + (tick - _ultimoTick) * _recargaPorTick); // recarrega
        _ultimoTick = tick;

        if (Fichas < 1) return false; // balde vazio: recusa
        Fichas--;                     // gasta uma ficha
        return true;
    }
}
