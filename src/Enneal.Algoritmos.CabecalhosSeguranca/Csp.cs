namespace Enneal.Algoritmos.CabecalhosSeguranca;

/// <summary>
/// Leitura da Content-Security-Policy (CSP), com as mesmas regras do notebook.
/// </summary>
public static class Csp
{
    private static readonly string[] Diretivas =
    [
        "default-src", "script-src", "script-src-elem", "script-src-attr", "style-src", "style-src-elem",
        "style-src-attr", "img-src", "font-src", "connect-src", "media-src", "object-src", "frame-src", "child-src",
        "worker-src", "manifest-src", "prefetch-src", "base-uri", "form-action", "frame-ancestors", "sandbox",
        "upgrade-insecure-requests", "block-all-mixed-content", "report-uri", "report-to", "require-trusted-types-for",
        "trusted-types", "plugin-types", "navigate-to", "require-sri-for", "fenced-frame-src", "webrtc",
    ];

    /// <summary>
    /// Separa a CSP em políticas (separadas por vírgula) e cada política em diretivas (separadas por ponto e
    /// vírgula). Só conta a política que tem pelo menos uma diretiva conhecida.
    /// </summary>
    /// <param name="csp">O valor do cabeçalho (pode ser <see langword="null"/>).</param>
    /// <returns>Uma lista de políticas; cada uma mapeia diretiva → valores.</returns>
    public static List<Dictionary<string, List<string>>> Politicas(string? csp)
    {
        var lista = new List<Dictionary<string, List<string>>>();
        foreach (string pol in (csp ?? "").Split(','))
        {
            var d = new Dictionary<string, List<string>>();
            foreach (string parte in pol.Split(';'))
            {
                string[] t = parte.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (t.Length > 0 && !d.ContainsKey(t[0].ToLowerInvariant()))
                    d[t[0].ToLowerInvariant()] = t.Skip(1).Select(s => s.StartsWith('\'') ? s.ToLowerInvariant() : s).ToList();
            }
            if (d.Keys.Any(Diretivas.Contains)) lista.Add(d);
        }
        return lista;
    }

    /// <summary>Se existe pelo menos uma política válida.</summary>
    /// <param name="csp">O valor do cabeçalho.</param>
    public static bool Valida(string? csp) => Politicas(csp).Count > 0;

    /// <summary>
    /// Se alguma política tem <c>frame-ancestors</c> restrito (sem <c>*</c>, <c>http:</c> ou <c>https:</c>):
    /// vale como proteção contra clickjacking, igual ao X-Frame-Options.
    /// </summary>
    /// <param name="csp">O valor do cabeçalho.</param>
    public static bool FrameAncestorsOk(string? csp) => Politicas(csp)
        .Where(p => p.ContainsKey("frame-ancestors"))
        .Any(p => p["frame-ancestors"].Count > 0 && !p["frame-ancestors"].Any(x => x is "*" or "http:" or "https:"));

    /// <summary>
    /// Os problemas de script que tiram o A+ (tetos): <c>csp_unsafe_inline</c> ('unsafe-inline' sem nonce nem
    /// hash), <c>csp_unsafe_eval</c>, <c>csp_wildcard_scripts</c> (fontes curinga sem 'strict-dynamic') e
    /// <c>csp_no_script_restriction</c> (nenhuma política restringe script).
    /// </summary>
    /// <param name="csp">O valor do cabeçalho.</param>
    /// <returns>Os problemas presentes em TODAS as políticas, em ordem alfabética.</returns>
    public static List<string> ProblemasDeScript(string? csp)
    {
        var politicas = Politicas(csp);
        if (politicas.Count == 0) return [];

        var porPolitica = new List<HashSet<string>>();
        foreach (var p in politicas)
        {
            string? chave = p.ContainsKey("script-src") ? "script-src" : p.ContainsKey("default-src") ? "default-src" : null;
            if (chave is null) continue;
            var fontes = p[chave];
            var problemas = new HashSet<string>();
            bool nonceOuHash = fontes.Any(x => x.StartsWith("'nonce-", StringComparison.Ordinal) ||
                x.StartsWith("'sha256-", StringComparison.Ordinal) || x.StartsWith("'sha384-", StringComparison.Ordinal) ||
                x.StartsWith("'sha512-", StringComparison.Ordinal));
            if (fontes.Contains("'unsafe-inline'") && !nonceOuHash) problemas.Add("csp_unsafe_inline");
            if (fontes.Contains("'unsafe-eval'")) problemas.Add("csp_unsafe_eval");
            if (!fontes.Contains("'strict-dynamic'") && fontes.Any(x => x is "*" or "http:" or "https:" or "data:"))
                problemas.Add("csp_wildcard_scripts");
            porPolitica.Add(problemas);
        }
        if (porPolitica.Count == 0) return ["csp_no_script_restriction"];

        // Várias políticas valem juntas: um problema só conta se aparece em todas.
        var comum = new HashSet<string>(porPolitica[0]);
        foreach (var s in porPolitica.Skip(1)) comum.IntersectWith(s);
        return [.. comum.OrderBy(x => x, StringComparer.Ordinal)];
    }
}
