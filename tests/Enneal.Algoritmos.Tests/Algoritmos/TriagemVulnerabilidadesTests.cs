using Enneal.Algoritmos.TriagemVulnerabilidades;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class TriagemVulnerabilidadesTests
{
    private static readonly IReadOnlyList<LinhaDaFila> Linhas = FilaDeDemonstracao.Carregar();
    private static readonly List<Achado> Achados = Linhas.Select(l => l.Achado).ToList();

    [Fact]
    public void Fila_Tem400CvesUnicas()
    {
        Assert.Equal(400, Achados.Count);
        Assert.Equal(400, Achados.Select(a => a.Cve).Distinct().Count());
        Assert.All(Achados, a => Assert.Matches(@"^CVE-\d{4}-\d{4,}$", a.Cve));
        Assert.Equal(3, Achados.Count(a => a.Kev));
    }

    [Fact]
    public void TresOrdens_TemAsPosicoesDoReel()
    {
        Assert.Equal([9, 10, 24], FilaDeRemediacao.PosicoesDasExploradas(FilaDeRemediacao.PorCvss(Achados)));
        Assert.Equal([1, 2, 19], FilaDeRemediacao.PosicoesDasExploradas(FilaDeRemediacao.ComEpss(Achados)));
        Assert.Equal([1, 2, 3], FilaDeRemediacao.PosicoesDasExploradas(FilaDeRemediacao.Triada(Achados)));
    }

    [Fact]
    public void Faixas_TemOsNumerosDoReel()
    {
        var porFaixa = Achados.GroupBy(Triagem.Faixa).ToDictionary(g => g.Key, g => g.Count());

        Assert.Equal(3, porFaixa["P1"]);
        Assert.Equal(5, porFaixa["P2"]);
        Assert.Equal(197, porFaixa["P3"]);
        Assert.Equal(195, porFaixa["P4"]);
        Assert.Equal(31, Achados.Count(a => a.Cvss >= 9));
    }

    [Fact]
    public void Triagem_BateComONotebook_Risco_Faixa_EOrdem()
    {
        Assert.All(Linhas, l =>
        {
            Assert.Equal(l.RiscoDoNotebook, Triagem.Risco(l.Achado), 1);
            Assert.Equal(l.FaixaDoNotebook, Triagem.Faixa(l.Achado));
        });
        Assert.Equal(Achados, FilaDeRemediacao.Triada(Achados)); // o CSV já vem na ordem da triagem
    }

    [Theory]
    [InlineData("CVE-2024-50623", 100.0, 10, 1, 1)] // KEV + ransomware
    [InlineData("CVE-2024-23113", 99.3, 9, 2, 2)]
    [InlineData("CVE-2025-22224", 97.6, 24, 19, 3)] // EPSS 0,016: só a KEV põe no topo
    [InlineData("CVE-2023-2915", 85.0, 138, 3, 4)]
    [InlineData("CVE-2024-42010", 79.3, 144, 4, 5)]
    public void CvesDoReel_RiscoEPosicoes(string cve, double risco, int porCvss, int comEpss, int triada)
    {
        var a = Achados.Single(x => x.Cve == cve);

        Assert.Equal(risco, Triagem.Risco(a));
        Assert.Equal(porCvss, FilaDeRemediacao.PorCvss(Achados).IndexOf(a) + 1);
        Assert.Equal(comEpss, FilaDeRemediacao.ComEpss(Achados).IndexOf(a) + 1);
        Assert.Equal(triada, FilaDeRemediacao.Triada(Achados).IndexOf(a) + 1);
    }

    [Fact]
    public void Propriedade_TodaKevEhP1_EVemAntesDeTodaNaoKev()
    {
        var fila = FilaDeRemediacao.Triada(Achados);
        int ultimaKev = fila.FindLastIndex(a => a.Kev);
        int primeiraNaoKev = fila.FindIndex(a => !a.Kev);

        Assert.All(Achados.Where(a => a.Kev), a => Assert.Equal("P1", Triagem.Faixa(a)));
        Assert.True(ultimaKev < primeiraNaoKev);
    }

    [Fact]
    public void Propriedade_RiscoSempreEntre0E100_EKevSempreAcimaDe65()
    {
        Assert.All(Achados, a => Assert.InRange(Triagem.Risco(a), 0, 100));

        // Na KEV a chance é 1, então o risco é pelo menos 65 (mesmo com CVSS 0).
        Assert.Equal(65.0, Triagem.Risco(new Achado("CVE-2026-0001", 0, 0, true, false)));
        Assert.Equal(100.0, Triagem.Risco(new Achado("CVE-2026-0002", 10, 1, true, true))); // limitado em 100
    }

    [Theory]
    [InlineData(9.8, 0.98, true, "P1")]
    [InlineData(5.0, 0.10, false, "P2")]  // EPSS alto já é P2, mesmo com CVSS médio
    [InlineData(9.0, 0.05, false, "P2")]  // CVSS crítico + EPSS de 5%
    [InlineData(9.8, 0.049, false, "P3")] // crítico, mas sem sinal de exploração: 90 dias
    [InlineData(7.0, 0.01, false, "P3")]
    [InlineData(6.9, 0.09, false, "P4")]
    public void Faixa_RegrasDaTabela(double cvss, double epss, bool kev, string faixa)
    {
        Assert.Equal(faixa, Triagem.Faixa(new Achado("CVE-2026-1", cvss, epss, kev, false)));
    }

    [Fact]
    public void Prazos_7_30_90EBacklog()
    {
        Assert.Equal(7, Triagem.PrazoEmDias("P1"));
        Assert.Equal(30, Triagem.PrazoEmDias("P2"));
        Assert.Equal(90, Triagem.PrazoEmDias("P3"));
        Assert.Null(Triagem.PrazoEmDias("P4"));
    }

    [Fact]
    public void LerCsv_AceitaCamposComAspasEVirgula()
    {
        const string csv = "tier,risk,cve_id,base_score,epss_score,kev,why\n" +
                           "P1,100.0,CVE-2024-1,9.8,0.5,True,\"Known exploited, ransomware use\"\n";
        var l = FilaDeDemonstracao.Ler(new StringReader(csv)).Single();

        Assert.True(l.Achado.Kev);
        Assert.True(l.Achado.Ransomware);
        Assert.Equal((2024, 1), (l.Achado.Ano, l.Achado.Numero));
    }
}
