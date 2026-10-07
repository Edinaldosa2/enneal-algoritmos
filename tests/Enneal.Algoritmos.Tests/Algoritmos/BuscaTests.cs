using Enneal.Algoritmos.Busca;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class BuscaTests
{
    private static readonly int[] VetorDoReel = VetorOrdenado.Gerar(1024);

    [Theory]
    [InlineData(6, 19, 7, 10)]        // rodada 1
    [InlineData(612, 1565, 613, 10)]  // rodada 2
    [InlineData(1023, 2562, 1024, 11)] // rodada 3: último item, pior caso da linear
    public void Rodadas_TemOsNumerosDoReel(int posicao, int alvo, long linear, long binaria)
    {
        Assert.Equal(alvo, VetorDoReel[posicao]);

        var l = Buscas.Linear(VetorDoReel, alvo);
        var b = Buscas.Binaria(VetorDoReel, alvo);

        Assert.Equal((posicao, linear), (l.Posicao, l.Comparacoes));
        Assert.Equal((posicao, binaria), (b.Posicao, b.Comparacoes));
    }

    [Fact]
    public void Escala1024_LinearAte1024_BinariaAte11()
    {
        var e = Escala.Medir(VetorDoReel);

        Assert.Equal(1024, e.LinearPior);
        Assert.Equal(11, e.BinariaPior);
        Assert.Equal(9.01, Math.Round(e.BinariaMedia, 2));
    }

    [Fact]
    public void Escala1Milhao_LinearAte1Milhao_BinariaAte20()
    {
        var e = Escala.Medir(VetorOrdenado.Gerar(1_000_000));

        Assert.Equal(1_000_000, e.LinearPior);
        Assert.Equal(20, e.BinariaPior);
        Assert.Equal(18.95, Math.Round(e.BinariaMedia, 2));
    }

    [Fact]
    public void VetorGerado_EhCrescenteSemRepetidos()
    {
        for (int i = 1; i < VetorDoReel.Length; i++)
        {
            int salto = VetorDoReel[i] - VetorDoReel[i - 1];
            Assert.InRange(salto, 1, 4);
        }
        Assert.InRange(VetorDoReel[0], 3, 7);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(7)]
    [InlineData(100)]
    [InlineData(1024)]
    [InlineData(5000)]
    public void Binaria_NuncaPassaDeLog2MaisUm(int n)
    {
        int[] v = VetorOrdenado.Gerar(n, semente: (uint)n);
        long limite = (long)Math.Floor(Math.Log2(n)) + 1;

        foreach (int alvo in v.Append(v[^1] + 1).Prepend(v[0] - 1))
            Assert.True(Buscas.Binaria(v, alvo).Comparacoes <= limite);
    }

    [Theory]
    [InlineData(1u)]
    [InlineData(2u)]
    [InlineData(3u)]
    public void AsDuasBuscas_AchamAMesmaPosicaoQueArrayBinarySearch(uint semente)
    {
        int[] v = VetorOrdenado.Gerar(300, semente);
        int menor = v[0] - 2, maior = v[^1] + 2;

        for (int alvo = menor; alvo <= maior; alvo++)
        {
            int esperado = Array.BinarySearch(v, alvo);
            if (esperado < 0) esperado = -1;

            Assert.Equal(esperado, Buscas.Linear(v, alvo).Posicao);
            Assert.Equal(esperado, Buscas.Binaria(v, alvo).Posicao);
        }
    }

    [Fact]
    public void Linear_AlvoNaPosicaoK_FazKMaisUmaComparacoes()
    {
        for (int k = 0; k < 50; k++)
            Assert.Equal(k + 1, Buscas.Linear(VetorDoReel, VetorDoReel[k]).Comparacoes);
    }

    [Fact]
    public void VetorVazio_NaoEncontra()
    {
        Assert.Equal(new ResultadoBusca(-1, 0), Buscas.Linear([], 5));
        Assert.Equal(new ResultadoBusca(-1, 0), Buscas.Binaria([], 5));
    }

    [Fact]
    public void AoComparar_RecebeCadaComparacao()
    {
        var comparacoes = new List<(int Item, int Alvo)>();
        var r = Buscas.Binaria(VetorDoReel, 19, (item, alvo) => comparacoes.Add((item, alvo)));

        Assert.Equal(r.Comparacoes, comparacoes.Count);
        Assert.All(comparacoes, c => Assert.Equal(19, c.Alvo));
        Assert.Equal(19, comparacoes[^1].Item);
    }
}
