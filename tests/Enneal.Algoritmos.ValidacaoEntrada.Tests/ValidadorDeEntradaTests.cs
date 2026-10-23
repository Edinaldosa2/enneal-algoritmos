using Enneal.Algoritmos.ValidacaoEntrada;

namespace Enneal.Algoritmos.ValidacaoEntrada.Tests;

public class ValidadorDeEntradaTests
{
    private readonly IValidadorDeEntrada _v = new ValidadorDeEntrada();

    private static readonly RegraDeCampo Texto = new("nome", TipoDeDado.Texto, 60);
    private static readonly RegraDeCampo Idade = new("idade", TipoDeDado.Inteiro, 3, 0, 130);
    private static readonly RegraDeCampo Preco = new("preco", TipoDeDado.Decimal, 12, 0, 1_000_000);
    private static readonly RegraDeCampo Email = new("email", TipoDeDado.Email, 254);
    private static readonly RegraDeCampo Data = new("data", TipoDeDado.Data, 10);

    [Theory]
    [InlineData("42", TipoDeDado.Inteiro)]
    [InlineData("-7", TipoDeDado.Inteiro)]
    [InlineData("19.90", TipoDeDado.Decimal)]
    [InlineData("2026-10-23", TipoDeDado.Data)]
    [InlineData("ana@exemplo.example", TipoDeDado.Email)]
    [InlineData("1e9", TipoDeDado.Texto)]          // expoente não é aceito como número
    [InlineData("0x1F", TipoDeDado.Texto)]         // hexadecimal também não
    [InlineData("1,000", TipoDeDado.Texto)]        // nem separador de milhar
    [InlineData("2026-02-30", TipoDeDado.Texto)]   // data que não existe
    [InlineData("99999999999", TipoDeDado.Decimal)] // estoura o int, cabe no decimal
    [InlineData("Olá, mundo", TipoDeDado.Texto)]
    public void DetectarTipo(string entrada, TipoDeDado esperado) =>
        Assert.Equal(esperado, _v.DetectarTipo(entrada));

    [Theory]
    [InlineData("34", 34)]
    [InlineData(" 34 ", 34)]
    [InlineData("0", 0)]
    [InlineData("130", 130)]
    public void Inteiro_Valido_ViraInt(string entrada, int esperado)
    {
        var r = _v.Validar(entrada, Idade);
        Assert.True(r.Valido);
        Assert.Equal(esperado, r.Valor);
    }

    [Theory]
    [InlineData("34 OR 1=1")]
    [InlineData("34;--")]
    [InlineData("-1")]
    [InlineData("131")]
    [InlineData("3.5")]
    [InlineData("1e2")]
    [InlineData("٣٤")]       // dígitos arábicos: o int.TryParse do .NET não aceita; e o ASCII é exigido
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("99999999999999999999")]
    public void Inteiro_Malicioso_OuForaDaFaixa_ERecusado(string? entrada) =>
        Assert.False(_v.Validar(entrada, Idade).Valido);

    [Fact]
    public void Inteiro_DeLarguraTotal_ENormalizadoAntes()
    {
        // "３４" (largura total) vira "34" no NFKC, e só então é validado.
        var r = _v.Validar("\uFF13\uFF14", Idade);
        Assert.True(r.Valido);
        Assert.Equal(34, r.Valor);
    }

    [Theory]
    [InlineData("19.90")]
    [InlineData("0")]
    [InlineData("1000000")]
    public void Decimal_Valido(string entrada) => Assert.True(_v.Validar(entrada, Preco).Valido);

    [Theory]
    [InlineData("19,90")]   // vírgula: o contrato da API usa ponto (cultura invariante)
    [InlineData("-0.01")]
    [InlineData("1e3")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("$19.90")]
    public void Decimal_Invalido_ERecusado(string entrada) => Assert.False(_v.Validar(entrada, Preco).Valido);

    [Theory]
    [InlineData("ana@exemplo.example")]
    [InlineData("ana.souza+loja@exemplo.example")]
    public void Email_Valido(string entrada) => Assert.True(_v.Validar(entrada, Email).Valido);

    [Theory]
    [InlineData("\"Davi\" <davi@exemplo.example>")] // nome de exibição
    [InlineData("davi@exemplo")]                     // domínio sem ponto
    [InlineData("davi@@exemplo.example")]
    [InlineData("davi@exemplo..example")]
    [InlineData("davi exemplo.example")]
    [InlineData("davi@exemplo.example\r\nBcc: x@y.example")] // injeção de cabeçalho
    [InlineData("davi@exémplo.example")]             // não ASCII
    public void Email_Malicioso_ERecusado(string entrada) => Assert.False(_v.Validar(entrada, Email).Valido);

    [Theory]
    [InlineData("2006-02-30")]
    [InlineData("2006-13-01")]
    [InlineData("01/02/2006")]
    [InlineData("2006-1-1")]
    [InlineData("2006-01-01T00:00:00")]
    [InlineData("2006-01-01'; DROP TABLE clientes;--")]
    public void Data_Invalida_ERecusada(string entrada) => Assert.False(_v.Validar(entrada, Data).Valido);

    [Fact]
    public void Data_Valida_ViraDateOnly() =>
        Assert.Equal(new DateOnly(1992, 5, 17), _v.Validar("1992-05-17", Data).Valor);

    [Theory]
    [InlineData("Fábio\u202Etxt.exe")] // U+202E: inverte a direção do texto (troca de extensão disfarçada)
    [InlineData("Ana\u200BSouza")]     // espaço de largura zero
    [InlineData("Ana\0Souza")]          // byte nulo
    [InlineData("Ana\r\nSouza")]        // quebra de linha (injeção em log/cabeçalho)
    [InlineData("Ana\tSouza")]
    [InlineData("Ana\u2028Souza")]      // separador de linha Unicode: só o espaço comum é aceito
    [InlineData("\uD800abc")]           // substituto solto: Unicode inválido
    public void Texto_ComCaractereProibido_ERecusado(string entrada) =>
        Assert.False(_v.Validar(entrada, Texto).Valido);

    [Fact]
    public void Texto_LongoDemais_ERecusado_AntesENDepoisDeNormalizar()
    {
        Assert.Equal("nome: grande demais", _v.Validar(new string('A', 60 * ValidadorDeEntrada.FatorTetoBruto + 1), Texto).Erro);
        Assert.Equal("nome: mais de 60 caracteres", _v.Validar(new string('A', 61), Texto).Erro);
        Assert.True(_v.Validar(new string('A', 60), Texto).Valido);
    }

    [Fact]
    public void Texto_NormalizacaoPodeAumentar_ELimiteValeDepois()
    {
        // U+FDFA vira 18 caracteres no NFKC: 4 deles passam do limite de 60.
        string s = new string('\uFDFA', 4);
        Assert.True(s.Normalize(System.Text.NormalizationForm.FormKC).Length > 60);
        Assert.False(_v.Validar(s, Texto).Valido);
    }

    [Theory]
    [InlineData("José da Silva")]
    [InlineData("O'Brien")]
    [InlineData("Ana-Clara (loja 2)")]
    [InlineData("<script>alert(1)</script>")] // texto pode ter < e >: a defesa contra XSS é na saída
    public void Texto_Legitimo_EAceito(string entrada) => Assert.True(_v.Validar(entrada, Texto).Valido);

    [Fact]
    public void Texto_DisfarceDeLarguraTotal_ViraAscii()
    {
        // "＜script＞" vira "<script>": validar e codificar a forma canônica, não o disfarce.
        Assert.Equal("<script>", _v.Validar("\uFF1Cscript\uFF1E", Texto).Valor);
    }

    [Fact]
    public void Texto_EspacoNaoSeparavel_ViraEspacoComum() =>
        Assert.Equal("Ana Souza", _v.Validar("Ana\u00A0Souza", Texto).Valor);

    [Fact]
    public void Texto_FormasCompostaEDecompostaViramAMesma()
    {
        string composta = "Jos\u00E9", decomposta = "Jose\u0301";
        Assert.NotEqual(composta, decomposta);
        Assert.Equal(_v.Validar(composta, Texto).Valor, _v.Validar(decomposta, Texto).Valor);
    }
}
