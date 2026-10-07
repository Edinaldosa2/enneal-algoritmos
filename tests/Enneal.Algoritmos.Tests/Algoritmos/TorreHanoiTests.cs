using Enneal.Algoritmos.TorreHanoi;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class TorreHanoiTests
{
    // ---------- Números do Reel ----------

    [Theory]
    [InlineData(3, 7)]   // rodada 1
    [InlineData(4, 15)]  // rodada 2: 2 x 7 + 1
    [InlineData(6, 63)]  // rodada 3 (modo turbo)
    public void Rodadas_TemOsNumerosDoReel(int discos, long movimentos)
    {
        var r = ResolvedorHanoi.Resolver(discos);

        Assert.Equal(movimentos, r.Movimentos);
        Assert.True(r.Resolvida);
    }

    [Fact]
    public void QuatroDiscos_SaoDuasVezesTresDiscosMaisUm()
    {
        Assert.Equal(2 * ResolvedorHanoi.Resolver(3).Movimentos + 1, ResolvedorHanoi.Resolver(4).Movimentos);
    }

    [Fact]
    public void SessentaEQuatroDiscos_TemOsNumerosDoReel()
    {
        UInt128 total = ContaDeHanoi.MovimentosNecessarios(64);
        UInt128 anos = ContaDeHanoi.AnosAUmMovimentoPorSegundo(total);

        Assert.Equal(UInt128.Parse("18446744073709551615"), total);
        Assert.Equal((UInt128)ulong.MaxValue, total); // 2^64 - 1 é o maior ulong: não cabe num long
        Assert.True(total > long.MaxValue);
        Assert.Equal((UInt128)584_542_046_090, anos);
        Assert.Equal((UInt128)585, ContaDeHanoi.BilhoesDeAnos(anos));
    }

    // ---------- Propriedades ----------

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(12)]
    [InlineData(16)]
    public void Movimentos_SaoSempreDoisElevadoANMenosUm(int n)
    {
        var r = ResolvedorHanoi.Resolver(n);

        Assert.Equal((1L << n) - 1, r.Movimentos);
        Assert.Equal((UInt128)r.Movimentos, ContaDeHanoi.MovimentosNecessarios(n));
        Assert.True(r.Resolvida);
    }

    [Fact]
    public void Movimentos_SeguemARecorrenciaTnIgualDuasVezesTnMenos1MaisUm()
    {
        long anterior = ResolvedorHanoi.Resolver(0).Movimentos;
        Assert.Equal(0, anterior);

        for (int n = 1; n <= 14; n++)
        {
            long atual = ResolvedorHanoi.Resolver(n).Movimentos;
            Assert.Equal(2 * anterior + 1, atual);
            anterior = atual;
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(9)]
    [InlineData(12)]
    public void NenhumMovimento_PoeDiscoMaiorSobreMenor(int n)
    {
        // Refaz a partida com pilhas próprias do teste, sem usar a classe Pinos,
        // e confere a regra a cada movimento que a recursão anunciou.
        var pino = new Dictionary<char, Stack<int>> { ['A'] = new(), ['B'] = new(), ['C'] = new() };
        for (int d = n; d >= 1; d--) pino['A'].Push(d);
        long esperado = 0;

        ResolvedorHanoi.Resolver(n, m =>
        {
            Assert.Equal(++esperado, m.Numero);
            Assert.NotEqual(m.De, m.Para);
            Assert.True(pino[m.De].TryPeek(out int topo), $"movimento {m.Numero}: pino {m.De} vazio");
            Assert.Equal(topo, m.Disco); // só o disco do topo pode sair

            if (pino[m.Para].TryPeek(out int embaixo))
                Assert.True(embaixo > m.Disco, $"movimento {m.Numero}: disco {m.Disco} sobre o disco {embaixo}");

            pino[m.Para].Push(pino[m.De].Pop());
        });

        Assert.Empty(pino['A']);
        Assert.Empty(pino['B']);
        Assert.Equal(Enumerable.Range(1, n), pino['C']); // do topo para a base: 1, 2, ..., n
    }

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(10)]
    public void DiscoK_MoveDoisElevadoANMenosKVezes(int n)
    {
        var vezes = new long[n + 1];
        ResolvedorHanoi.Resolver(n, m => vezes[m.Disco]++);

        for (int k = 1; k <= n; k++)
            Assert.Equal(1L << (n - k), vezes[k]); // o maior move 1 vez; o menor, 2^(n-1)
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(8)]
    public void MenorDisco_MoveEmTodoMovimentoImparESempreNoMesmoSentido(int n)
    {
        // Padrão clássico: o disco 1 se move nos movimentos 1, 3, 5, ... e gira sempre no mesmo
        // sentido (A->C->B->A com n ímpar, A->B->C->A com n par).
        string ciclo = n % 2 == 1 ? "ACB" : "ABC";

        ResolvedorHanoi.Resolver(n, m =>
        {
            Assert.Equal(m.Numero % 2 == 1, m.Disco == 1);
            if (m.Disco == 1)
                Assert.Equal(ciclo[(ciclo.IndexOf(m.De) + 1) % 3], m.Para);
        });
    }

    [Fact]
    public void TresDiscos_FazemExatamenteOsMovimentosDoReel()
    {
        var feitos = new List<string>();
        ResolvedorHanoi.Resolver(3, m => feitos.Add($"{m.Disco}{m.De}{m.Para}"));

        Assert.Equal(["1AC", "2AB", "1CB", "3AC", "1BA", "2BC", "1AC"], feitos);
    }

    [Fact]
    public void ZeroDiscos_NaoMoveNada()
    {
        var r = ResolvedorHanoi.Resolver(0, _ => Assert.Fail("não deveria mover"));

        Assert.Equal(0, r.Movimentos);
        Assert.True(r.Resolvida);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(ResolvedorHanoi.MaximoDeDiscos + 1)]
    public void Resolver_ForaDoIntervalo_DaErro(int discos)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ResolvedorHanoi.Resolver(discos));
    }

    // ---------- A conta com UInt128 ----------

    [Fact]
    public void MovimentosNecessarios_Extremos()
    {
        Assert.Equal(UInt128.Zero, ContaDeHanoi.MovimentosNecessarios(0));
        Assert.Equal((UInt128)long.MaxValue, ContaDeHanoi.MovimentosNecessarios(63));
        Assert.Equal(UInt128.MaxValue, ContaDeHanoi.MovimentosNecessarios(128));
        Assert.Throws<ArgumentOutOfRangeException>(() => ContaDeHanoi.MovimentosNecessarios(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ContaDeHanoi.MovimentosNecessarios(129));
    }

    [Fact]
    public void MovimentosNecessarios_CadaDiscoAMaisDobraESomaUm()
    {
        for (int n = 1; n <= 128; n++)
            Assert.Equal(2 * ContaDeHanoi.MovimentosNecessarios(n - 1) + 1, ContaDeHanoi.MovimentosNecessarios(n));
    }

    [Fact]
    public void SegundosPorAno_SaoDe365DiasEUmQuarto()
    {
        Assert.Equal((long)(365.25 * 24 * 60 * 60), ContaDeHanoi.SegundosPorAno);
    }

    // ---------- Os pinos ----------

    [Fact]
    public void Pinos_TorreInicialNoPinoA_MaiorEmbaixo()
    {
        var p = new Pinos(4);

        Assert.Equal([1, 2, 3, 4], p.DiscosNoPino('A'));
        Assert.Empty(p.DiscosNoPino('B'));
        Assert.Empty(p.DiscosNoPino('C'));
        Assert.False(p.Resolvida);
        Assert.Equal(0, p.Movimentos);
    }

    [Fact]
    public void Pinos_RecusaDiscoMaiorSobreMenor()
    {
        var p = new Pinos(3);
        p.Mover('A', 'C'); // disco 1 no C

        var erro = Assert.Throws<InvalidOperationException>(() => p.Mover('A', 'C')); // disco 2 sobre o 1
        Assert.Contains("disco 2 sobre o disco 1", erro.Message);
        Assert.Equal(1, p.Movimentos); // o movimento recusado não conta
        Assert.Equal([2, 3], p.DiscosNoPino('A'));
    }

    [Fact]
    public void Pinos_RecusaPinoVazioEPinoInexistente()
    {
        var p = new Pinos(2);

        Assert.Throws<InvalidOperationException>(() => p.Mover('B', 'C'));
        Assert.Throws<ArgumentOutOfRangeException>(() => p.Mover('A', 'D'));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Pinos(-1));
    }

    [Fact]
    public void Movimento_ToString_EhLegivel()
    {
        Assert.Equal("disco 3: A -> C", new Movimento(4, 3, 'A', 'C').ToString());
    }
}
