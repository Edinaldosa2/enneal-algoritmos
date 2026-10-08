using Enneal.Algoritmos.CabecalhosSeguranca;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class CabecalhosSegurancaTests
{
    // ---------- As 11 fases do Reel (ASP.NET Core 8 de verdade, em memória) ----------

    [Theory]
    [InlineData(0, 0, "F", "", 1)]
    [InlineData(1, 30, "E", "", 2)]
    [InlineData(2, 55, "D", "", 2)]
    [InlineData(3, 75, "D", "", 2)]
    [InlineData(4, 95, "C", "", 2)]
    [InlineData(5, 115, "B", "", 2)]
    [InlineData(6, 135, "A", "", 2)]
    [InlineData(7, 135, "A", "", 0)]
    [InlineData(8, 135, "A", "", 0)]
    [InlineData(9, 160, "A", "csp_unsafe_inline", 0)]
    [InlineData(10, 160, "A+", "", 0)]
    public async Task Fases_TemOsNumerosDoReel(int fase, int pontos, string letra, string tetos, int avisos)
    {
        var (_, a) = await LojaDeExemplo.AvaliarFaseAsync(fase);

        Assert.Equal(pontos, a.Pontos);
        Assert.Equal(letra, a.Letra);
        Assert.Equal(tetos, string.Join(",", a.Tetos));
        Assert.Equal(avisos, a.Avisos.Count);
    }

    [Fact]
    public async Task Propriedade_PontosNuncaCaemDeUmaFaseParaAProxima()
    {
        int anterior = -1;
        for (int f = 0; f < LojaDeExemplo.Fases.Count; f++)
        {
            var (_, a) = await LojaDeExemplo.AvaliarFaseAsync(f);
            Assert.True(a.Pontos >= anterior, $"fase {f}");
            anterior = a.Pontos;
        }
    }

    [Fact]
    public async Task Fase1_RedirecionaParaHttpsCom307()
    {
        var (r, _) = await LojaDeExemplo.AvaliarFaseAsync(1);

        Assert.Equal([307, 200], r.Cadeia);
        Assert.Equal("https://loja.example/", r.Url);
        Assert.True(r.Https);
    }

    [Fact]
    public async Task Fase2_Hsts365DiasComSubdominios()
    {
        var (r, _) = await LojaDeExemplo.AvaliarFaseAsync(2);

        Assert.Equal(365L * 24 * 60 * 60, r.HstsMaxAge);
        Assert.Contains("includeSubDomains", r.Primeiro("strict-transport-security"));
    }

    [Fact]
    public async Task Fase7_CookieGanhaSecureHttpOnlyESameSite()
    {
        var (antes, _) = await LojaDeExemplo.AvaliarFaseAsync(6);
        var (depois, _) = await LojaDeExemplo.AvaliarFaseAsync(7);

        Assert.Equal(new CookieLido("sessao", false, false), CookieLido.Ler(antes.Cookies.Single()));
        Assert.Equal(new CookieLido("sessao", true, true), CookieLido.Ler(depois.Cookies.Single()));
        Assert.Contains("samesite=lax", depois.Cookies.Single(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Fase10_NonceMudaACadaResposta()
    {
        var (r1, _) = await LojaDeExemplo.AvaliarFaseAsync(10);
        var (r2, _) = await LojaDeExemplo.AvaliarFaseAsync(10);

        Assert.Contains("'nonce-", r1.Csp);
        Assert.Contains("'strict-dynamic'", r1.Csp);
        Assert.NotEqual(r1.Csp, r2.Csp); // um nonce por resposta: quem não sabe o da vez não roda script
    }

    // ---------- As regras da nota, sem ASP.NET ----------

    private static RespostaHttp Resposta(string url, params (string Nome, string Valor)[] cabecalhos) =>
        new(url, [200],
            cabecalhos.GroupBy(c => c.Nome.ToLowerInvariant()).ToDictionary(g => g.Key, g => g.Select(c => c.Valor).ToList()),
            []);

    [Fact]
    public void HttpSemNada_ZeroPontosNotaF()
    {
        var a = AvaliadorDeCabecalhos.Avaliar(Resposta("http://x.example/"));
        Assert.Equal((0, "F"), (a.Pontos, a.Letra));
    }

    [Fact]
    public void HstsSemHttps_NaoValePonto()
    {
        var a = AvaliadorDeCabecalhos.Avaliar(Resposta("http://x.example/", ("Strict-Transport-Security", "max-age=31536000")));
        Assert.Equal(0, a.Pontos);
    }

    [Fact]
    public void HstsCurto_ViraTeto()
    {
        var a = AvaliadorDeCabecalhos.Avaliar(Resposta("https://x.example/", ("Strict-Transport-Security", "max-age=86400")));
        Assert.Equal(55, a.Pontos);
        Assert.Contains("hsts_short", a.Tetos);
    }

    [Fact]
    public void FrameAncestors_ValeComoXFrameOptions()
    {
        var com = AvaliadorDeCabecalhos.Avaliar(Resposta("https://x.example/", ("Content-Security-Policy", "frame-ancestors 'none'")));
        var curinga = AvaliadorDeCabecalhos.Avaliar(Resposta("https://x.example/", ("Content-Security-Policy", "frame-ancestors *")));

        Assert.Equal(30 + 25 + 20, com.Pontos);     // HTTPS + CSP válida + proteção contra moldura
        Assert.Equal(30 + 25, curinga.Pontos);      // '*' não protege
    }

    [Theory]
    [InlineData("DENY", true)]
    [InlineData("sameorigin", true)]
    [InlineData("DENY, SAMEORIGIN", false)] // dois valores diferentes: inválido
    [InlineData("ALLOW-FROM https://a.example", false)]
    public void XFrameOptions_SoUmValorValido(string valor, bool valido)
    {
        Assert.Equal(valido, AvaliadorDeCabecalhos.XFrameOptionsValido(Resposta("https://x.example/", ("X-Frame-Options", valor))));
    }

    [Theory]
    [InlineData("script-src 'self' 'unsafe-inline'", "csp_unsafe_inline")]
    [InlineData("script-src 'self' 'unsafe-inline' 'nonce-abc'", "")] // com nonce, o unsafe-inline é ignorado pelo navegador
    [InlineData("script-src 'self' 'unsafe-eval'", "csp_unsafe_eval")]
    [InlineData("script-src https:", "csp_wildcard_scripts")]
    [InlineData("script-src 'nonce-abc' 'strict-dynamic' https:", "")]
    [InlineData("img-src 'self'", "csp_no_script_restriction")]
    [InlineData("default-src *", "csp_wildcard_scripts")]
    public void Csp_ProblemasDeScript(string csp, string esperado)
    {
        Assert.Equal(esperado, string.Join(",", Csp.ProblemasDeScript(csp)));
    }

    [Fact]
    public void Csp_DuasPoliticas_ProblemaSoContaSeEstiverEmTodas()
    {
        Assert.Empty(Csp.ProblemasDeScript("script-src 'unsafe-inline', script-src 'self'"));
        Assert.Equal(["csp_unsafe_inline"], Csp.ProblemasDeScript("script-src 'unsafe-inline', default-src 'unsafe-inline'"));
    }

    [Fact]
    public void ReferrerUnsafeUrl_ValePontoMasViraTeto()
    {
        var a = AvaliadorDeCabecalhos.Avaliar(Resposta("https://x.example/", ("Referrer-Policy", "unsafe-url")));
        Assert.Equal(50, a.Pontos);
        Assert.Contains("referrer_unsafe_url", a.Tetos);
    }

    [Fact]
    public void TudoCertoMasComTeto_FicaEmA()
    {
        var a = AvaliadorDeCabecalhos.Avaliar(Resposta("https://x.example/",
            ("Strict-Transport-Security", "max-age=31536000"),
            ("Content-Security-Policy", LojaDeExemplo.CspComUnsafeInline),
            ("X-Content-Type-Options", "nosniff"),
            ("Referrer-Policy", "no-referrer"),
            ("Permissions-Policy", "camera=()")));

        Assert.Equal(160, a.Pontos);
        Assert.Equal("A", a.Letra);
    }

    [Theory]
    [InlineData("sessao=1; path=/", "http://x.example/", 0, 1)]   // em HTTP não reclama de Secure
    [InlineData("sessao=1; path=/", "https://x.example/", 1, 1)]
    [InlineData("tema=escuro; path=/", "https://x.example/", 1, 0)] // não parece sessão: HttpOnly não é exigido
    [InlineData("sid=1; secure; httponly", "https://x.example/", 0, 0)]
    public void AvisosDeCookie(string cookie, string url, int semSecure, int semHttpOnly)
    {
        var r = new RespostaHttp(url, [200], new Dictionary<string, List<string>>(), [cookie]);
        var avisos = AvaliadorDeCabecalhos.Avisos(r);

        Assert.Equal(semSecure, avisos.Count(a => a.Contains("without Secure")));
        Assert.Equal(semHttpOnly, avisos.Count(a => a.Contains("without HttpOnly")));
    }
}
