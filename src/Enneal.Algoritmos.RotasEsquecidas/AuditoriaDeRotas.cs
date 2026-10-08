namespace Enneal.Algoritmos.RotasEsquecidas;

/// <summary>
/// O resultado de uma auditoria de rotas.
/// </summary>
/// <param name="Conferidas">Quantas rotas do checklist foram conferidas.</param>
/// <param name="PorStatus">Quantas respostas de cada status (200, 302, 403, 404...).</param>
/// <param name="Expostas">As rotas sensíveis que responderam 200, na ordem do checklist.</param>
public sealed record ResultadoAuditoria(int Conferidas, IReadOnlyDictionary<int, int> PorStatus, IReadOnlyList<string> Expostas)
{
    /// <summary>Quantas respostas tiveram o status informado (0 se nenhuma).</summary>
    /// <param name="status">Status HTTP.</param>
    public int Contar(int status) => PorStatus.GetValueOrDefault(status);
}

/// <summary>
/// A auditoria que você roda no SEU site antes de cada deploy: confere uma lista de caminhos e aponta quais
/// conteúdos sensíveis estão públicos.
/// </summary>
public static class AuditoriaDeRotas
{
    /// <summary>
    /// O checklist do Reel: 15 caminhos que você confere no seu site (páginas normais e sobras comuns de deploy).
    /// </summary>
    public static IReadOnlyList<string> ChecklistDoReel { get; } =
    [
        "/", "/login", "/robots.txt", "/admin", "/api/status",
        "/uploads/", "/server-status", "/debug", "/old/", "/backup/",
        "/db.sql", "/config.bak", "/logs/", "/.env", "/.git/config",
    ];

    /// <summary>
    /// Confere cada caminho do checklist no site.
    /// No Reel: antes da correção, 6 respostas 200, 1 302, 1 403, 7 404 e 1 rota exposta (<c>/.git/config</c>);
    /// depois, 4 respostas 200, 1 302, 1 403, 9 404 e nenhuma exposta.
    /// </summary>
    /// <param name="site">O site auditado (o seu).</param>
    /// <param name="checklist">Os caminhos conferidos.</param>
    /// <param name="corrigido">Se o site já nega por padrão.</param>
    /// <param name="aoConferir">Opcional: chamado a cada caminho com (índice, caminho, resposta, exposta).</param>
    /// <returns>Contagem por status e a lista de rotas expostas.</returns>
    public static ResultadoAuditoria Auditar(SiteDeExemplo site, IReadOnlyList<string> checklist, bool corrigido,
        Action<int, string, Rota, bool>? aoConferir = null)
    {
        ArgumentNullException.ThrowIfNull(site);
        ArgumentNullException.ThrowIfNull(checklist);

        var porStatus = new SortedDictionary<int, int>();
        var expostas = new List<string>();
        for (int i = 0; i < checklist.Count; i++)
        {
            var r = site.Responder(checklist[i], corrigido);
            bool exposta = r.Status == 200 && r.Sensivel; // conteúdo sensível acessível
            if (exposta) expostas.Add(checklist[i]);
            porStatus[r.Status] = porStatus.GetValueOrDefault(r.Status) + 1;
            aoConferir?.Invoke(i, checklist[i], r, exposta);
        }
        return new ResultadoAuditoria(checklist.Count, porStatus, expostas);
    }
}
