using Enneal.Algoritmos.CaminhoInvasor;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class CaminhoInvasorTests
{
    [Fact]
    public void SemDefesas_InvadeEm93Passos()
    {
        var r = BuscaInvasor.Invadir(MapasDoReel.SemDefesas);

        Assert.True(r.Invadido);
        Assert.Equal(93, r.Passos);
    }

    [Fact]
    public void SemDefesas_CaminhoMaisCurtoTem12MovimentosEEhContinuo()
    {
        var r = BuscaInvasor.Invadir(MapasDoReel.SemDefesas);

        Assert.Equal(13, r.Caminho.Count); // 13 células = 12 movimentos
        Assert.Equal(new Celula(0, 6), r.Caminho[0]);
        Assert.Equal('B', MapasDoReel.SemDefesas[r.Caminho[^1].Linha][r.Caminho[^1].Coluna]);
        for (int i = 1; i < r.Caminho.Count; i++)
        {
            int distancia = Math.Abs(r.Caminho[i].Linha - r.Caminho[i - 1].Linha)
                          + Math.Abs(r.Caminho[i].Coluna - r.Caminho[i - 1].Coluna);
            Assert.Equal(1, distancia);
            Assert.NotEqual('#', MapasDoReel.SemDefesas[r.Caminho[i].Linha][r.Caminho[i].Coluna]);
        }
    }

    [Fact]
    public void EmCamadas_Protegido_ZeroCaminhosE48CelulasExploradas()
    {
        var r = BuscaInvasor.Invadir(MapasDoReel.EmCamadas);

        Assert.False(r.Invadido);
        Assert.Equal(48, r.Passos);
        Assert.Empty(r.Caminho);
        Assert.Equal(48, r.Alcancadas.Count);
    }

    [Fact]
    public void EmCamadas_PassaPorFurosDasDuasPrimeirasCamadas_MasNaoDaTerceira()
    {
        var r = BuscaInvasor.Invadir(MapasDoReel.EmCamadas);

        Assert.Contains(r.Alcancadas, c => c.Linha == 4); // furo no WAF
        Assert.Contains(r.Alcancadas, c => c.Linha == 5); // furo no MFA
        Assert.DoesNotContain(r.Alcancadas, c => c.Linha >= 6);
    }

    [Theory]
    [InlineData(new[] { "I..", "...", "..B" }, true, 4)]
    [InlineData(new[] { "I#B" }, false, 0)]
    public void MapasPequenos_CaminhoMaisCurto(string[] mapa, bool invadido, int movimentos)
    {
        var r = BuscaInvasor.Invadir(mapa);

        Assert.Equal(invadido, r.Invadido);
        Assert.Equal(invadido ? movimentos + 1 : 0, r.Caminho.Count);
    }

    [Theory]
    [InlineData("..B")]        // sem início
    [InlineData("I.I|..B")]    // dois inícios
    [InlineData("I..|B")]      // linhas de tamanhos diferentes
    public void MapaInvalido_LancaExcecao(string linhas)
    {
        string[] mapa = linhas.Split('|'); // '|' separa as linhas do mapa
        Assert.Throws<ArgumentException>(() => BuscaInvasor.Invadir(mapa));
    }
}
