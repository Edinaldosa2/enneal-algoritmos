namespace Enneal.Algoritmos.RateLimit;

/// <summary>
/// Destino de uma requisição na simulação. Os valores são os códigos HTTP correspondentes.
/// </summary>
public enum StatusRequisicao
{
    /// <summary>Ainda esperando na fila.</summary>
    NaFila = 0,

    /// <summary>200 OK: atendida.</summary>
    Ok = 200,

    /// <summary>408 Request Timeout: esperou demais na fila.</summary>
    Timeout = 408,

    /// <summary>429 Too Many Requests: passou da cota do IP (rate limit).</summary>
    Bloqueada = 429,

    /// <summary>503 Service Unavailable: chegou com a fila cheia.</summary>
    FilaCheia = 503,
}

/// <summary>
/// Uma requisição simulada (só um objeto na memória; nada vai para a rede).
/// </summary>
/// <param name="Id">Número da requisição na rodada.</param>
/// <param name="Ip">Cliente que mandou (0 a 5 legítimos; 6 em diante, a enxurrada).</param>
/// <param name="Legitima">Se veio de um usuário legítimo.</param>
/// <param name="Chegada">Tick em que chegou.</param>
public sealed record Requisicao(int Id, int Ip, bool Legitima, int Chegada)
{
    /// <summary>O que aconteceu com ela.</summary>
    public StatusRequisicao Status { get; set; } = StatusRequisicao.NaFila;
}
