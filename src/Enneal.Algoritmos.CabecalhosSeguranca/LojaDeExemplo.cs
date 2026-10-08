using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enneal.Algoritmos.CabecalhosSeguranca;

/// <summary>
/// O site do Reel (<c>loja.example</c>), um ASP.NET Core 8 de verdade que ganha um cabeçalho de segurança por fase.
/// O GET roda direto no pipeline, em memória (<see cref="DefaultHttpContext"/>): sem servidor e sem rede.
/// </summary>
public static class LojaDeExemplo
{
    /// <summary>Referrer-Policy usada a partir da fase 5.</summary>
    public const string ReferrerPolicy = "strict-origin-when-cross-origin";

    /// <summary>Permissions-Policy usada a partir da fase 6: desliga câmera, localização, microfone e pagamento.</summary>
    public const string PermissionsPolicy = "camera=(), geolocation=(), microphone=(), payment=()";

    /// <summary>A CSP da fase 9: boa, mas ainda com 'unsafe-inline' (por isso fica com A e não A+).</summary>
    public const string CspComUnsafeInline = "default-src 'self'; script-src 'self' 'unsafe-inline'; "
        + "object-src 'none'; base-uri 'none'; frame-ancestors 'none'";

    /// <summary>A CSP da fase 10: só roda script com o nonce sorteado nesta resposta (mais 'strict-dynamic').</summary>
    /// <param name="nonce">O nonce desta resposta.</param>
    public static string CspComNonce(string nonce) =>
        $"default-src 'self'; script-src 'nonce-{nonce}' 'strict-dynamic'; object-src 'none'; base-uri 'none'; frame-ancestors 'none'";

    /// <summary>O nome de cada fase, da 0 (nada) à 10 (CSP com nonce).</summary>
    public static IReadOnlyList<string> Fases { get; } =
    [
        "nada, só HTTP", "HTTPS", "HSTS", "X-Content-Type-Options", "X-Frame-Options", "Referrer-Policy",
        "Permissions-Policy", "cookies Secure/HttpOnly/SameSite", "COOP/COEP/CORP", "CSP com 'unsafe-inline'",
        "CSP com nonce + 'strict-dynamic'",
    ];

    /// <summary>
    /// Monta o site com tudo o que entra até a <paramref name="fase"/> informada.
    /// </summary>
    /// <param name="fase">De 0 a 10.</param>
    /// <returns>O <see cref="WebApplication"/> montado (não é iniciado: não abre porta nenhuma).</returns>
    public static WebApplication Montar(int fase)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fase);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fase, Fases.Count - 1);

        var b = WebApplication.CreateSlimBuilder();
        b.Logging.ClearProviders();
        b.Services.AddHttpsRedirection(o => o.HttpsPort = 443);
        b.Services.AddHsts(o =>
        {
            o.MaxAge = TimeSpan.FromDays(365);
            o.IncludeSubDomains = true;
        });
        b.Services.Configure<CookiePolicyOptions>(o =>
        {
            o.Secure = CookieSecurePolicy.Always;
            o.HttpOnly = HttpOnlyPolicy.Always;
            o.MinimumSameSitePolicy = SameSiteMode.Lax;
        });

        var app = b.Build();
        if (fase >= 1) app.UseHttpsRedirection(); // http -> https (307)
        if (fase >= 2) app.UseHsts();             // Strict-Transport-Security: 365 dias + subdomínios
        if (fase >= 7) app.UseCookiePolicy();     // cookies com Secure, HttpOnly e SameSite=Lax
        app.Use((ctx, next) => Cabecalhos(ctx, next, fase));
        app.Run(Pagina);
        return app;
    }

    /// <summary>
    /// Faz <c>GET</c> na URL seguindo até 5 redirecionamentos, como o scanner do notebook, direto no pipeline.
    /// </summary>
    /// <param name="app">O site montado.</param>
    /// <param name="url">URL inicial (por exemplo <c>http://loja.example/</c>).</param>
    /// <returns>A resposta final.</returns>
    public static async Task<RespostaHttp> PedirAsync(WebApplication app, string url)
    {
        ArgumentNullException.ThrowIfNull(app);
        RequestDelegate pipeline = ((IApplicationBuilder)app).Build();
        var cadeia = new List<int>();

        for (int salto = 0; salto < 5; salto++)
        {
            var u = new Uri(url);
            var ctx = new DefaultHttpContext { RequestServices = app.Services };
            ctx.Request.Method = "GET";
            ctx.Request.Scheme = u.Scheme;
            ctx.Request.Host = new HostString(u.Host);
            ctx.Request.Path = u.AbsolutePath;
            ctx.Response.Body = new MemoryStream();
            await pipeline(ctx);
            cadeia.Add(ctx.Response.StatusCode);

            if (ctx.Response.StatusCode is >= 300 and < 400)
            {
                url = ctx.Response.Headers.Location.ToString(); // segue o redirecionamento
                continue;
            }

            var cabecalhos = ctx.Response.Headers.ToDictionary(
                k => k.Key.ToLowerInvariant(), k => k.Value.Select(v => v ?? "").ToList());
            cabecalhos.Remove("set-cookie", out var cookies);
            return new RespostaHttp(url, cadeia, cabecalhos, cookies ?? []);
        }
        throw new InvalidOperationException("Redirecionamentos demais.");
    }

    /// <summary>
    /// Monta o site da fase, faz o GET em <c>http://loja.example/</c> e dá a nota.
    /// No Reel: F, E, D, D, C, B, A, A, A, A (teto <c>csp_unsafe_inline</c>) e A+.
    /// </summary>
    /// <param name="fase">De 0 a 10.</param>
    /// <returns>A resposta e a avaliação.</returns>
    public static async Task<(RespostaHttp Resposta, AvaliacaoCabecalhos Avaliacao)> AvaliarFaseAsync(int fase)
    {
        await using var app = Montar(fase);
        var r = await PedirAsync(app, "http://loja.example/");
        return (r, AvaliadorDeCabecalhos.Avaliar(r));
    }

    // O middleware de cabeçalhos: uma linha por cabeçalho, ligada pela fase.
    private static Task Cabecalhos(HttpContext ctx, Func<Task> next, int fase)
    {
        var h = ctx.Response.Headers;
        string nonce = NovoNonce(ctx); // o mesmo nonce vai no <script> da página
        if (fase >= 3) h.XContentTypeOptions = "nosniff";
        if (fase >= 4) h.XFrameOptions = "DENY";
        if (fase >= 5) h["Referrer-Policy"] = ReferrerPolicy;
        if (fase >= 6) h["Permissions-Policy"] = PermissionsPolicy;
        if (fase >= 8)
        {
            h["Cross-Origin-Opener-Policy"] = "same-origin";
            h["Cross-Origin-Embedder-Policy"] = "require-corp";
            h["Cross-Origin-Resource-Policy"] = "same-origin";
        }
        if (fase == 9) h.ContentSecurityPolicy = CspComUnsafeInline;
        if (fase >= 10) h.ContentSecurityPolicy = CspComNonce(nonce);
        return next();
    }

    // A página: um cookie de sessão de mentira e um <script> com o nonce da resposta.
    private static async Task Pagina(HttpContext ctx)
    {
        ctx.Response.Cookies.Append("sessao", "c0ffee", new CookieOptions { Path = "/" });
        ctx.Response.ContentType = "text/html; charset=utf-8";
        string nonce = (string)ctx.Items["nonce"]!;
        await ctx.Response.WriteAsync($"<!doctype html><p id=m>loja</p><script nonce=\"{nonce}\">m.textContent='ok'</script>");
    }

    // 16 bytes aleatórios por resposta: quem não sabe o nonce não consegue rodar script na página.
    private static string NovoNonce(HttpContext ctx)
    {
        string n = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        ctx.Items["nonce"] = n;
        return n;
    }
}
