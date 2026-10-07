using Enneal.Algoritmos.ForcaBruta;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class ForcaBrutaTests
{
    // Poucas iterações deixam os testes rápidos; a lógica do bloqueio é a mesma.
    private const int IteracoesRapidas = 1_000;

    [Fact]
    public void Round1_SemProtecao_AbreOPinDoReelEm7392Tentativas()
    {
        Assert.Equal(7392, SimulacaoForcaBruta.TentativasSemProtecao("7391"));
    }

    [Theory]
    [InlineData("0000", 1)]
    [InlineData("0042", 43)]
    [InlineData("9999", 10_000)]
    public void Round1_SemProtecao_TentativaEhOPinMaisUm(string pin, int esperado)
    {
        Assert.Equal(esperado, SimulacaoForcaBruta.TentativasSemProtecao(pin));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("12a4")]
    [InlineData("")]
    public void Round1_SoAceitaPinDe4Digitos(string pin)
    {
        Assert.Throws<ArgumentException>(() => SimulacaoForcaBruta.TentativasSemProtecao(pin));
    }

    [Fact]
    public void Round2_ComBloqueio_ParaDepoisDe5Falhas()
    {
        var login = new LoginProtegido("7391", maxFalhas: 5, iteracoes: IteracoesRapidas);

        var r = SimulacaoForcaBruta.TentarContraLogin(login);

        Assert.False(r.Aberto);
        Assert.True(r.Bloqueado);
        Assert.Equal(5, r.Falhas);
        Assert.Equal(6, r.Pedidos); // o 6º pedido encontra a conta bloqueada
    }

    [Fact]
    public void Round2_ComOValorPadraoDeIteracoes_TemOsMesmosNumeros()
    {
        var login = new LoginProtegido("7391"); // 600.000 iterações, 5 falhas, 15 min

        var r = SimulacaoForcaBruta.TentarContraLogin(login);

        Assert.Equal(5, r.Falhas);
        Assert.Equal(6, r.Pedidos);
        Assert.Equal(TimeSpan.FromMinutes(15), login.TempoBloqueio);
    }

    [Fact]
    public void CustoDoBloqueio_TemOsNumerosDoReel()
    {
        var custo = SimulacaoForcaBruta.CalcularCustoDoBloqueio(7392, 5, 15);

        Assert.Equal(1478, custo.Bloqueios);
        Assert.Equal(15.4, Math.Round(custo.Dias, 1));
    }

    [Fact]
    public void Login_AceitaASenhaCertaEZeraAsFalhas()
    {
        var login = new LoginProtegido("7391", iteracoes: IteracoesRapidas);

        Assert.Equal(ResultadoLogin.SenhaErrada, login.Tentar("0000"));
        Assert.Equal(1, login.Falhas);
        Assert.Equal(ResultadoLogin.Aceito, login.Tentar("7391"));
        Assert.Equal(0, login.Falhas);
    }

    [Fact]
    public void Login_Bloqueado_RecusaAteASenhaCerta()
    {
        var login = new LoginProtegido("7391", maxFalhas: 3, iteracoes: IteracoesRapidas);
        for (int i = 0; i < 3; i++) login.Tentar("0000");

        Assert.True(login.Bloqueada);
        Assert.Equal(ResultadoLogin.ContaBloqueada, login.Tentar("7391"));
    }

    [Fact]
    public void Login_BloqueioVenceDepoisDoTempo()
    {
        var relogio = new RelogioDeTeste();
        var login = new LoginProtegido("7391", maxFalhas: 5, tempoBloqueio: TimeSpan.FromMinutes(15),
            iteracoes: IteracoesRapidas, relogio: relogio);
        for (int i = 0; i < 5; i++) login.Tentar("0000");

        relogio.Avancar(TimeSpan.FromMinutes(14));
        Assert.Equal(ResultadoLogin.ContaBloqueada, login.Tentar("7391"));

        relogio.Avancar(TimeSpan.FromMinutes(1));
        Assert.False(login.Bloqueada);
        Assert.Equal(ResultadoLogin.Aceito, login.Tentar("7391"));
    }

    [Fact]
    public void Login_DuasContasComAMesmaSenha_FuncionamSeparadas()
    {
        // Cada conta tem o seu salt; errar numa não afeta a outra.
        var a = new LoginProtegido("7391", iteracoes: IteracoesRapidas);
        var b = new LoginProtegido("7391", iteracoes: IteracoesRapidas);
        a.Tentar("0000");

        Assert.Equal(1, a.Falhas);
        Assert.Equal(0, b.Falhas);
        Assert.True(b.Entrar("7391"));
    }

    private sealed class RelogioDeTeste : TimeProvider
    {
        private DateTimeOffset _agora = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _agora;

        public void Avancar(TimeSpan tempo) => _agora += tempo;
    }
}
