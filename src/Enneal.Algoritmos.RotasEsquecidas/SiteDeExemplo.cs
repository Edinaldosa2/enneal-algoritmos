namespace Enneal.Algoritmos.RotasEsquecidas;

/// <summary>
/// O site fictício do Reel (<c>seu-site.example</c>): a tabela de rotas que o app responde hoje, em memória.
/// Nada aqui gera tráfego de rede.
/// </summary>
/// <remarks>
/// O problema do Reel: um <c>/.git/config</c> subiu junto no deploy e responde 200. Com ele, dá para baixar o
/// repositório inteiro do site. A correção é <b>negar por padrão</b>: só existem as rotas da lista de liberadas;
/// qualquer outra responde 404, mesmo que o arquivo esteja no servidor. E a listagem de diretório é desligada.
/// </remarks>
public sealed class SiteDeExemplo
{
    private readonly IReadOnlyDictionary<string, Rota> _rotas;
    private readonly IReadOnlySet<string> _liberadas;

    /// <summary>
    /// Cria o site.
    /// </summary>
    /// <param name="rotas">O que o servidor responde hoje em cada caminho (o resto é 404).</param>
    /// <param name="liberadas">As rotas que o app deve expor depois da correção (a lista de "permitidas").</param>
    public SiteDeExemplo(IReadOnlyDictionary<string, Rota> rotas, IReadOnlySet<string> liberadas)
    {
        _rotas = rotas ?? throw new ArgumentNullException(nameof(rotas));
        _liberadas = liberadas ?? throw new ArgumentNullException(nameof(liberadas));
    }

    /// <summary>
    /// O que o site do Reel responde hoje: 8 caminhos, um deles o <c>/.git/config</c> esquecido.
    /// </summary>
    public static IReadOnlyDictionary<string, Rota> RotasDoReel { get; } = new Dictionary<string, Rota>
    {
        ["/"] = new(200, "pagina", false),
        ["/login"] = new(200, "pagina", false),
        ["/robots.txt"] = new(200, "texto", false),
        ["/admin"] = new(302, "login", false),        // pede login: ok
        ["/api/status"] = new(200, "json", false),
        ["/uploads/"] = new(200, "listagem", false),  // listagem de diretório aberta
        ["/server-status"] = new(403, "proibido", false),
        ["/.git/config"] = new(200, "git", true),     // sobra do deploy: expõe o repositório
    };

    /// <summary>As 6 rotas que o site do Reel deve expor (negar por padrão: o resto não existe).</summary>
    public static IReadOnlySet<string> LiberadasDoReel { get; } = new HashSet<string>
    {
        "/", "/login", "/robots.txt", "/admin", "/api/status", "/uploads/",
    };

    /// <summary>O site do Reel.</summary>
    public static SiteDeExemplo DoReel() => new(RotasDoReel, LiberadasDoReel);

    /// <summary>
    /// Responde um caminho.
    /// </summary>
    /// <param name="caminho">Caminho pedido, por exemplo <c>/admin</c>.</param>
    /// <param name="corrigido">
    /// <see langword="false"/> = o site de hoje (entrega o que achar);
    /// <see langword="true"/> = negar por padrão e sem listagem de diretório.
    /// </param>
    /// <returns>O que o site responde.</returns>
    public Rota Responder(string caminho, bool corrigido)
    {
        ArgumentNullException.ThrowIfNull(caminho);

        if (corrigido && !_liberadas.Contains(caminho))
            return new Rota(404, "negado-padrao", false);  // não está na lista: não existe
        if (!_rotas.TryGetValue(caminho, out var r))
            return new Rota(404, "nao-existe", false);
        if (corrigido && r.Tipo == "listagem")
            return new Rota(403, "sem-listagem", false);   // listagem de diretório desligada
        return r;                                          // antes: entrega o que achar
    }
}
