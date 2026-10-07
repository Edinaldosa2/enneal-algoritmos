using Enneal.Algoritmos.RateLimit;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class RateLimitTests
{
    [Fact]
    public void SemLimite_TemOsNumerosDoReel()
    {
        var r = SimulacaoRateLimit.Rodar(comLimite: false);

        Assert.Equal(1173, r.Requisicoes);
        Assert.Equal(202, r.Atendidas);
        Assert.Equal(0, r.Bloqueadas);
        Assert.Equal(851, r.FilaCheia);
        Assert.Equal(120, r.Timeouts);
        Assert.Equal((22, 84), (r.LegitimasAtendidas, r.LegitimasTotal));
        Assert.Equal(26, r.PorcentagemLegitimasAtendidas);
        Assert.Equal(180, r.FloodAtendido);
        Assert.Equal(36, r.Ticks);
    }

    [Fact]
    public void ComTokenBucket_TemOsNumerosDoReel()
    {
        var r = SimulacaoRateLimit.Rodar(comLimite: true);

        Assert.Equal(1173, r.Requisicoes);
        Assert.Equal(268, r.Atendidas);
        Assert.Equal(886, r.Bloqueadas);
        Assert.Equal(14, r.FilaCheia);
        Assert.Equal(5, r.Timeouts);
        Assert.Equal((81, 84), (r.LegitimasAtendidas, r.LegitimasTotal));
        Assert.Equal(96, r.PorcentagemLegitimasAtendidas);
        Assert.Equal(187, r.FloodAtendido);
        Assert.Equal(34, r.Ticks);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TodaRequisicaoTemUmDestino(bool comLimite)
    {
        var r = SimulacaoRateLimit.Rodar(comLimite);

        Assert.Equal(r.Requisicoes, r.Atendidas + r.Bloqueadas + r.FilaCheia + r.Timeouts);
    }

    [Fact]
    public void MesmaSemente_MesmoTrafego()
    {
        var eventosA = new List<string>();
        var eventosB = new List<string>();
        SimulacaoRateLimit.Rodar(true, aoRegistrarEvento: eventosA.Add);
        SimulacaoRateLimit.Rodar(true, aoRegistrarEvento: eventosB.Add);

        Assert.Equal(eventosA, eventosB);
        Assert.StartsWith("R2 0 req ", eventosA[0]);
    }

    [Fact]
    public void Eventos_UmaLinhaDeChegadaPorRequisicao()
    {
        var eventos = new List<string>();
        var r = SimulacaoRateLimit.Rodar(false, aoRegistrarEvento: eventos.Add);

        Assert.Equal(r.Requisicoes, eventos.Count(e => e.Contains(" req ", StringComparison.Ordinal)));
    }

    [Fact]
    public void TokenBucket_PermiteRajadaDaCapacidadeEDepoisATaxa()
    {
        var balde = new TokenBucket(capacidade: 3, recargaPorTick: 0.5);

        // Rajada: 3 fichas no tick 0, a 4ª é recusada.
        Assert.True(balde.TentarConsumir(0));
        Assert.True(balde.TentarConsumir(0));
        Assert.True(balde.TentarConsumir(0));
        Assert.False(balde.TentarConsumir(0));

        // Meia ficha por tick: no tick 1 ainda não dá, no tick 2 dá uma.
        Assert.False(balde.TentarConsumir(1));
        Assert.True(balde.TentarConsumir(2));
        Assert.False(balde.TentarConsumir(2));
    }

    [Fact]
    public void TokenBucket_NuncaPassaDaCapacidade()
    {
        var balde = new TokenBucket(capacidade: 3, recargaPorTick: 0.5);

        Assert.True(balde.TentarConsumir(1000));
        Assert.Equal(2, balde.Fichas);
    }

    [Fact]
    public void SemEnxurrada_ORateLimitNaoAtrapalhaOsLegitimos()
    {
        var calmo = ConfiguracaoSimulacao.DoReel with { IpsFlood = 0 };

        var sem = SimulacaoRateLimit.Rodar(false, calmo);
        var com = SimulacaoRateLimit.Rodar(true, calmo);

        Assert.Equal(100, sem.PorcentagemLegitimasAtendidas);
        Assert.Equal(100, com.PorcentagemLegitimasAtendidas);
        Assert.Equal(0, com.Bloqueadas);
    }
}
