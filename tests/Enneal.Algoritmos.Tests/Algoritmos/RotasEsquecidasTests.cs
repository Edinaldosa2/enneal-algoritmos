using Enneal.Algoritmos.RotasEsquecidas;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class RotasEsquecidasTests
{
    private static ResultadoAuditoria Auditar(bool corrigido) =>
        AuditoriaDeRotas.Auditar(SiteDeExemplo.DoReel(), AuditoriaDeRotas.ChecklistDoReel, corrigido);

    [Fact]
    public void AntesDaCorrecao_TemOsNumerosDoReel()
    {
        var a = Auditar(corrigido: false);

        Assert.Equal(15, a.Conferidas);
        Assert.Equal((6, 1, 1, 7), (a.Contar(200), a.Contar(302), a.Contar(403), a.Contar(404)));
        Assert.Equal(["/.git/config"], a.Expostas);
    }

    [Fact]
    public void DepoisDaCorrecao_TemOsNumerosDoReel()
    {
        var a = Auditar(corrigido: true);

        Assert.Equal(15, a.Conferidas);
        Assert.Equal((4, 1, 1, 9), (a.Contar(200), a.Contar(302), a.Contar(403), a.Contar(404)));
        Assert.Empty(a.Expostas);
    }

    [Theory]
    [InlineData("/.git/config")]
    [InlineData("/.env")]
    [InlineData("/backup/")]
    [InlineData("/server-status")]
    [InlineData("/qualquer-coisa-nova")]
    public void NegarPorPadrao_ForaDaListaSempre404(string caminho)
    {
        var r = SiteDeExemplo.DoReel().Responder(caminho, corrigido: true);

        Assert.Equal(new Rota(404, "negado-padrao", false), r);
    }

    [Fact]
    public void NegarPorPadrao_MesmoQueOArquivoSensivelContinueNoServidor()
    {
        // Propriedade: depois da correção, NENHUMA rota que não está na lista responde 200, para qualquer tabela.
        var rotas = new Dictionary<string, Rota>(SiteDeExemplo.RotasDoReel)
        {
            ["/db.sql"] = new(200, "backup", true),
            ["/.env"] = new(200, "segredos", true),
        };
        var site = new SiteDeExemplo(rotas, SiteDeExemplo.LiberadasDoReel);

        foreach (var caminho in rotas.Keys.Where(c => !SiteDeExemplo.LiberadasDoReel.Contains(c)))
            Assert.Equal(404, site.Responder(caminho, corrigido: true).Status);
        Assert.Equal(["/db.sql", "/.env", "/.git/config"], AuditoriaDeRotas.Auditar(site, AuditoriaDeRotas.ChecklistDoReel, false).Expostas);
        Assert.Empty(AuditoriaDeRotas.Auditar(site, AuditoriaDeRotas.ChecklistDoReel, true).Expostas);
    }

    [Fact]
    public void ListagemDeDiretorio_ViraProibidoDepoisDaCorrecao()
    {
        var site = SiteDeExemplo.DoReel();

        Assert.Equal("listagem", site.Responder("/uploads/", false).Tipo);
        Assert.Equal(new Rota(403, "sem-listagem", false), site.Responder("/uploads/", true));
    }

    [Fact]
    public void RotasLegitimas_ContinuamFuncionandoDepoisDaCorrecao()
    {
        var site = SiteDeExemplo.DoReel();
        foreach (var c in new[] { "/", "/login", "/robots.txt", "/api/status" })
            Assert.Equal(200, site.Responder(c, true).Status);
        Assert.Equal(302, site.Responder("/admin", true).Status); // continua pedindo login
    }

    [Fact]
    public void Callback_RecebeOsQuinzeCaminhosNaOrdem()
    {
        var vistos = new List<string>();
        AuditoriaDeRotas.Auditar(SiteDeExemplo.DoReel(), AuditoriaDeRotas.ChecklistDoReel, false, (i, c, _, _) => vistos.Add(c));

        Assert.Equal(AuditoriaDeRotas.ChecklistDoReel, vistos);
    }
}
