namespace Enneal.Algoritmos.ControleDeAcesso;

/// <summary>
/// A "API" de pedidos do Reel: uma função em memória que responde <c>GET /api/pedidos/{id}</c>.
/// Nada aqui gera tráfego de rede.
/// </summary>
/// <remarks>
/// O erro clássico (IDOR, <i>Insecure Direct Object Reference</i>) é a API achar o pedido pelo id da URL e
/// entregar, sem conferir se ele pertence a quem está logado. A correção é uma linha: se o dono do pedido não é
/// o usuário da sessão, responde 403 e não devolve dado nenhum.
/// </remarks>
public sealed class ApiDePedidos
{
    private readonly Dictionary<int, Pedido> _pedidos;

    /// <summary>
    /// Cria a API com os pedidos informados.
    /// </summary>
    /// <param name="pedidos">Os pedidos que existem (o id de cada um não pode repetir).</param>
    public ApiDePedidos(IEnumerable<Pedido> pedidos)
    {
        ArgumentNullException.ThrowIfNull(pedidos);
        _pedidos = pedidos.ToDictionary(p => p.Id);
    }

    /// <summary>
    /// Os 5 pedidos do Reel (ids 41 a 45, cada um de um cliente diferente). Dados fictícios.
    /// </summary>
    public static IReadOnlyList<Pedido> PedidosDoReel { get; } =
    [
        new(41, 41, "Ana Souza", "***.***.789-04", 12890),
        new(42, 42, "Bruno Lima", "***.***.221-37", 254000),
        new(43, 43, "Carla Nunes", "***.***.654-90", 8990),
        new(44, 44, "Diego Alves", "***.***.112-55", 129900),
        new(45, 45, "Erica Dias", "***.***.980-18", 34990),
    ];

    /// <summary>A API com os pedidos do Reel.</summary>
    public static ApiDePedidos DoReel() => new(PedidosDoReel);

    /// <summary>
    /// Responde <c>GET /api/pedidos/{id}</c> para o usuário da sessão.
    /// </summary>
    /// <param name="id">Id do pedido pedido na URL.</param>
    /// <param name="usuarioDaSessao">Id de quem está logado. Vem da sessão, nunca da URL.</param>
    /// <param name="checarDono">
    /// <see langword="false"/> = a API com o erro (entrega qualquer pedido);
    /// <see langword="true"/> = a API corrigida (só entrega pedido do próprio usuário).
    /// </param>
    /// <returns>200 com o pedido, 403 sem dados ou 404 sem dados.</returns>
    public RespostaApi Buscar(int id, int usuarioDaSessao, bool checarDono)
    {
        if (!_pedidos.TryGetValue(id, out var p))
            return new RespostaApi(404, null);   // não existe
        if (checarDono && p.Dono != usuarioDaSessao)
            return new RespostaApi(403, null);   // é de outra pessoa: nega e não mostra nada
        return new RespostaApi(200, p);          // sem a checagem, entrega o que achou
    }
}
