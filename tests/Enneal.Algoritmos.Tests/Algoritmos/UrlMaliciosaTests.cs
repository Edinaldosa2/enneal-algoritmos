using System.Text.Json;
using Enneal.Algoritmos.UrlMaliciosa;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class UrlMaliciosaTests
{
    [Theory]
    [InlineData("https://www.example.com/produtos/tenis-corrida", "0.165815", false)]
    [InlineData("https://banco.example.com@198.51.100.23/login/verify", "0.981784", true)]
    [InlineData("http://xn--bnco-0qa.example.com.login.conta.example.net", "0.980968", true)]
    [InlineData("http://203.0.113.7:8080/bin/update.exe", "0.999031", true)]
    [InlineData("http://escola.example.org/index.php?option=com_content&view=article&id=18&Itemid=7", "0.849362", true)]
    [InlineData("https://login-banco.example.com/", "0.107498", false)] // phishing que passa: o modelo erra
    public void UrlsDoReel_TemAProbabilidadeEOVereditoDoReel(string url, string probabilidade, bool maliciosa)
    {
        var a = ClassificadorDeUrl.Avaliar(url);

        Assert.Equal(probabilidade, a.Probabilidade.ToString("F6", System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(maliciosa, a.Maliciosa);
    }

    [Theory]
    [InlineData("https://www.example.com/produtos/tenis-corrida", "34 11 23 1 0 0 0 0 0 2 0 0 0 4 0 882 0 4043 3 0 0 0 0 2 0 8 0 0 0 0")]
    [InlineData("https://banco.example.com@198.51.100.23/login/verify", "44 13 13 3 0 10 1 0 1 2 0 0 0 8 227 591 0 4508 2 0 2 0 0 2 0 7 0 0 0 2")]
    [InlineData("http://xn--bnco-0qa.example.com.login.conta.example.net", "48 48 0 6 3 1 0 0 0 0 0 0 0 9 21 792 0 3866 3 0 1 1 0 0 0 7 0 0 0 5")]
    public void Pistas_IguaisAsDoReel(string url, string pistas)
    {
        Assert.Equal(pistas, string.Join(" ", PistasDaUrl.Extrair(url)));
    }

    public static TheoryData<int> CasosDoNotebook => new() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

    [Theory]
    [MemberData(nameof(CasosDoNotebook))]
    public void Paridade_ComOsCasosDificeisDoNotebook(int indice)
    {
        // Pistas iguais às do extrator Python do notebook e probabilidade igual à do ONNX Runtime (até 1e-6).
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Dados", "paridade_notebook.json")));
        var c = doc.RootElement[indice];
        string url = c.GetProperty("url").GetString()!;
        float[] esperadas = [.. c.GetProperty("features").EnumerateArray().Select(x => (float)x.GetDouble())];

        float[] pistas = PistasDaUrl.Extrair(url);

        Assert.Equal(esperadas, pistas);
        Assert.Equal(c.GetProperty("probability").GetDouble(), ModeloDeArvores.DoReel.Probabilidade(pistas), 1e-6);
    }

    [Fact]
    public void Modelo_212ArvoresE30Pistas_BateComOsMetadados()
    {
        var m = ModeloDeArvores.DoReel;
        var meta = MetadadosDoModelo.DoReel;

        Assert.Equal(212, m.Arvores);
        Assert.Equal(26500, m.Nos);
        Assert.Equal(30, m.QuantidadeDePistas);
        Assert.Equal(PistasDaUrl.Versao, meta.VersaoDasPistas);
        Assert.Equal(PistasDaUrl.Nomes, meta.NomesDasPistas);
        Assert.Equal(ClassificadorDeUrl.Limiar, meta.Limiar);
    }

    [Fact]
    public void Probabilidade_SempreEntre0E1()
    {
        string[] urls = ["", "?", "a", "http://", "x.tk", new string('a', 2048), "münchen.de/straße", "user@1.2.3.4:80//a.exe"];
        Assert.All(urls, u => Assert.InRange(ClassificadorDeUrl.Avaliar(u).Probabilidade, 0f, 1f));
    }

    [Theory]
    [InlineData("HTTP://WWW.EXAMPLE.COM/a", "example.com/a")]
    [InlineData("https://example.com/a", "example.com/a")]
    [InlineData("  example.com/a\t", "example.com/a")]
    public void Normalizacao_EsquemaWwwEEspacosNaoMudamAsPistasDoHost(string a, string b)
    {
        float[] pa = PistasDaUrl.Extrair(a), pb = PistasDaUrl.Extrair(b);
        Assert.Equal(pb[1], pa[1]); // host_len
        Assert.Equal(pb[3], pa[3]); // host_dots
        Assert.Equal(pb[29], pa[29]); // subdomínios
    }

    [Theory]
    [InlineData("https://banco.example.com@198.51.100.23/x", 6, 1f)]    // IP no lugar do nome
    [InlineData("https://banco.example.com@198.51.100.23/x", 8, 1f)]    // @
    [InlineData("http://203.0.113.7:8080/a", 7, 1f)]                    // porta
    [InlineData("http://xn--bnco-0qa.example.com/", 21, 1f)]            // punycode
    [InlineData("bit.ly/abc", 22, 1f)]                                  // encurtador
    [InlineData("a.example.com/b/update.exe", 26, 1f)]                  // extensão de risco
    [InlineData("a.example.com/index.php", 27, 1f)]
    [InlineData("a.example.com/p.html", 28, 1f)]
    [InlineData("a.b.c.example.com/", 29, 3f)]                          // subdomínios: pontos - 1
    public void PistasIndividuais(string url, int pista, float valor)
    {
        Assert.Equal(valor, PistasDaUrl.Extrair(url)[pista]);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("example.com", true)]
    public void Aceita_UrlVaziaVira400(string? url, bool aceita) => Assert.Equal(aceita, ClassificadorDeUrl.Aceita(url));

    [Fact]
    public void Aceita_LimiteDe2048Caracteres()
    {
        Assert.True(ClassificadorDeUrl.Aceita(new string('a', 2048)));
        Assert.False(ClassificadorDeUrl.Aceita(new string('a', 2049)));
    }

    [Fact]
    public void Carregar_ArquivoQueNaoEhOnnx_DaErro()
    {
        Assert.ThrowsAny<Exception>(() => ModeloDeArvores.Carregar([0x0A, 0x02, 0x41, 0x42]));
    }

    [Fact]
    public void Tld_UltimaParteDoNome()
    {
        Assert.Equal("net", PistasDaUrl.Tld("http://xn--bnco-0qa.example.com.login.conta.example.net"));
        Assert.Equal("23", PistasDaUrl.Tld("https://banco.example.com@198.51.100.23/login/verify"));
        Assert.Equal("", PistasDaUrl.Tld("localhost/a"));
    }
}
