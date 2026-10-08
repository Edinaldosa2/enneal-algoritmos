using System.Text.RegularExpressions;

namespace Enneal.Algoritmos.CabecalhosSeguranca;

/// <summary>
/// A avaliação dos cabeçalhos de uma resposta.
/// </summary>
/// <param name="Pontos">Pontos de 0 a 160.</param>
/// <param name="Letra">A+, A, B, C, D, E ou F.</param>
/// <param name="Tetos">Problemas que impedem o A+ (por exemplo <c>csp_unsafe_inline</c>).</param>
/// <param name="Avisos">Avisos sobre cookies (não mudam a nota).</param>
public sealed record AvaliacaoCabecalhos(int Pontos, string Letra, IReadOnlyList<string> Tetos, IReadOnlyList<string> Avisos)
{
    /// <summary>Pontuação máxima.</summary>
    public const int Maximo = 160;

    /// <summary>Pontos em porcentagem, com 1 casa.</summary>
    public double Porcentagem => Math.Round(100.0 * Pontos / Maximo, 1);
}

/// <summary>
/// As regras de nota do notebook "HTTP security headers: A to F grades + .NET fix": pontos e faixas no estilo
/// do securityheaders.com. Não é a nota oficial de nenhum serviço; é para estudar o que cada cabeçalho protege.
/// </summary>
/// <remarks>
/// <list type="table">
/// <listheader><term>Item</term><description>Pontos</description></listheader>
/// <item><term>HTTPS (a resposta final veio por https)</term><description>30</description></item>
/// <item><term>HSTS (com HTTPS)</term><description>25</description></item>
/// <item><term>Content-Security-Policy válida</term><description>25</description></item>
/// <item><term>X-Frame-Options ou CSP frame-ancestors</term><description>20</description></item>
/// <item><term>X-Content-Type-Options: nosniff</term><description>20</description></item>
/// <item><term>Referrer-Policy válida</term><description>20</description></item>
/// <item><term>Permissions-Policy válida</term><description>20</description></item>
/// </list>
/// <para>Faixas pela porcentagem: A+ ≥ 95, A ≥ 75, B ≥ 60, C ≥ 50, D ≥ 29, E ≥ 14, F abaixo. Qualquer teto
/// derruba o A+ para A.</para>
/// </remarks>
public static class AvaliadorDeCabecalhos
{
    private static readonly string[] ReferrersValidos =
    [
        "no-referrer", "no-referrer-when-downgrade", "origin", "origin-when-cross-origin", "same-origin",
        "strict-origin", "strict-origin-when-cross-origin", "unsafe-url",
    ];

    private static readonly Regex ItemDePermissao =
        new("^[a-z][a-z0-9-]*\\s*=\\s*(\\*|self|src|\\([^()]*\\)|\"[^\"]*\")$", RegexOptions.IgnoreCase);

    private static readonly Regex CookieDeSessao =
        new("sess|sid\\b|^sid|auth|token|login|jwt|csrf|xsrf|identity|remember", RegexOptions.IgnoreCase);

    /// <summary>
    /// Dá a nota de uma resposta.
    /// </summary>
    /// <param name="r">A resposta final.</param>
    /// <returns>Pontos, letra, tetos e avisos.</returns>
    public static AvaliacaoCabecalhos Avaliar(RespostaHttp r)
    {
        ArgumentNullException.ThrowIfNull(r);

        int p = 0;
        if (r.Https) p += 30;
        if (r.Https && r.HstsMaxAge > 0) p += 25;
        if (Csp.Valida(r.Csp)) p += 25;
        if (XFrameOptionsValido(r) || Csp.FrameAncestorsOk(r.Csp)) p += 20;
        if (Nosniff(r)) p += 20;
        if (ReferrerPolicy(r) is not null) p += 20;
        if (PermissionsPolicyValida(r)) p += 20;

        double pct = Math.Round(100.0 * p / AvaliacaoCabecalhos.Maximo, 1);
        string letra = pct >= 95 ? "A+" : pct >= 75 ? "A" : pct >= 60 ? "B" : pct >= 50 ? "C"
            : pct >= 29 ? "D" : pct >= 14 ? "E" : "F";

        var tetos = new List<string>(Csp.ProblemasDeScript(r.Csp));
        if (r.Https && r.HstsMaxAge is > 0 and < 31_536_000) tetos.Add("hsts_short"); // HSTS de menos de 1 ano
        if (ReferrerPolicy(r) == "unsafe-url") tetos.Add("referrer_unsafe_url");
        if (letra == "A+" && tetos.Count > 0) letra = "A";

        return new AvaliacaoCabecalhos(p, letra, tetos, Avisos(r));
    }

    /// <summary>X-Frame-Options com um único valor, DENY ou SAMEORIGIN.</summary>
    /// <param name="r">A resposta.</param>
    public static bool XFrameOptionsValido(RespostaHttp r)
    {
        ArgumentNullException.ThrowIfNull(r);
        var v = r.Todos("x-frame-options").SelectMany(x => x.Split(',')).Select(x => x.Trim().ToUpperInvariant())
            .Where(x => x.Length > 0).Distinct().ToList();
        return v.Count == 1 && v[0] is "DENY" or "SAMEORIGIN";
    }

    /// <summary>A Referrer-Policy efetiva (o último valor conhecido), ou <see langword="null"/>.</summary>
    /// <param name="r">A resposta.</param>
    public static string? ReferrerPolicy(RespostaHttp r)
    {
        ArgumentNullException.ThrowIfNull(r);
        return r.Todos("referrer-policy").SelectMany(x => x.Split(',')).Select(x => x.Trim().ToLowerInvariant())
            .Where(ReferrersValidos.Contains).LastOrDefault();
    }

    /// <summary>Permissions-Policy com pelo menos um item bem formado (por exemplo <c>camera=()</c>).</summary>
    /// <param name="r">A resposta.</param>
    public static bool PermissionsPolicyValida(RespostaHttp r)
    {
        ArgumentNullException.ThrowIfNull(r);
        return r.Todos("permissions-policy").SelectMany(x => x.Split(',')).Any(i => ItemDePermissao.IsMatch(i.Trim()));
    }

    /// <summary>X-Content-Type-Options contém <c>nosniff</c>.</summary>
    /// <param name="r">A resposta.</param>
    public static bool Nosniff(RespostaHttp r)
    {
        ArgumentNullException.ThrowIfNull(r);
        return (r.Primeiro("x-content-type-options") ?? "").Split(',')
            .Select(t => t.Trim().ToLowerInvariant()).Contains("nosniff");
    }

    /// <summary>
    /// Avisos de cookie (em inglês, como no notebook): cookies sem <c>Secure</c> numa resposta HTTPS e cookies
    /// com cara de sessão sem <c>HttpOnly</c>.
    /// </summary>
    /// <param name="r">A resposta.</param>
    public static List<string> Avisos(RespostaHttp r)
    {
        ArgumentNullException.ThrowIfNull(r);
        var avisos = new List<string>();
        var cookies = r.Cookies.Select(CookieLido.Ler).ToList();
        int semSecure = r.Https ? cookies.Count(c => !c.Secure) : 0;
        if (semSecure > 0) avisos.Add($"{semSecure} cookie(s) without Secure");
        int semHttpOnly = cookies.Where(c => CookieDeSessao.IsMatch(c.Nome)).Count(c => !c.HttpOnly);
        if (semHttpOnly > 0) avisos.Add($"{semHttpOnly} session-like cookie(s) without HttpOnly");
        return avisos;
    }
}
