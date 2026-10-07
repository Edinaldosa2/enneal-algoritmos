using Enneal.Algoritmos.NRainhas;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class NRainhasTests
{
    [Fact]
    public void Tabuleiro8x8_TemOsNumerosDoReel()
    {
        var r = ResolvedorNRainhas.Resolver(8);

        Assert.True(r.Resolvido);
        Assert.Equal([0, 4, 7, 5, 2, 6, 1, 3], r.Rainhas);
        Assert.Equal(876, r.Tentativas);
        Assert.Equal(105, r.Voltas);
    }

    [Theory]
    [InlineData(4, new[] { 1, 3, 0, 2 }, 26, 4)]
    [InlineData(5, new[] { 0, 2, 4, 1, 3 }, 15, 0)]
    [InlineData(6, new[] { 1, 3, 5, 0, 2, 4 }, 171, 25)]
    [InlineData(7, new[] { 0, 2, 4, 6, 1, 3, 5 }, 42, 2)]
    [InlineData(8, new[] { 0, 4, 7, 5, 2, 6, 1, 3 }, 876, 105)]
    public void De4x4Ate8x8_TemOsNumerosDoReel(int n, int[] solucao, int tentativas, int voltas)
    {
        var r = ResolvedorNRainhas.Resolver(n);

        Assert.Equal(solucao, r.Rainhas);
        Assert.Equal(tentativas, r.Tentativas);
        Assert.Equal(voltas, r.Voltas);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(10)]
    public void PrimeiraSolucao_EhValida(int n)
    {
        var r = ResolvedorNRainhas.Resolver(n);

        Assert.True(r.Resolvido);
        Assert.True(ResolvedorNRainhas.SolucaoValida(r.Rainhas));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void TabuleirosSemSolucao_DevolvemLinhasVazias(int n)
    {
        var r = ResolvedorNRainhas.Resolver(n);

        Assert.False(r.Resolvido);
        Assert.All(r.Rainhas, coluna => Assert.Equal(-1, coluna));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 2)]
    [InlineData(5, 10)]
    [InlineData(6, 4)]
    [InlineData(7, 40)]
    [InlineData(8, 92)]
    public void ContarSolucoes_BateComASequenciaConhecida(int n, int esperado)
    {
        Assert.Equal(esperado, ResolvedorNRainhas.ContarSolucoes(n));
    }

    [Fact]
    public void SolucaoValida_RecusaAtaques()
    {
        Assert.False(ResolvedorNRainhas.SolucaoValida([0, 0, 0, 0])); // mesma coluna
        Assert.False(ResolvedorNRainhas.SolucaoValida([0, 1, 2, 3])); // mesma diagonal
        Assert.False(ResolvedorNRainhas.SolucaoValida([1, 3, 0, -1])); // linha vazia
        Assert.True(ResolvedorNRainhas.SolucaoValida([1, 3, 0, 2]));
    }

    [Fact]
    public void TamanhoNegativo_LancaExcecao()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ResolvedorNRainhas.Resolver(-1));
    }
}
