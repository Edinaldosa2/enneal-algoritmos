using System.Security.Authentication;
using Enneal.Algoritmos.NotaTls;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class NotaTlsTests
{
    // ---------- Os 6 casos do Reel, no laboratório (servidores TLS em 127.0.0.1) ----------

    public static TheoryData<int, string, string, string?, double> CasosDoReel => new()
    {
        { 0, "DNS", "nome não existe", null, 0 },
        { 1, "TLS", "versão recusada", "C", 90 },
        { 2, "Certificado", "expirado", "T", 93 },
        { 3, "Certificado", "nome não bate com o site", "M", 93 },
        { 4, "Certificado", "autoassinado", "T", 93 },
        { 5, "HTTP", "200 · HSTS 365 dias", "A+", 93 },
    };

    [Theory]
    [MemberData(nameof(CasosDoReel))]
    public async Task Casos_TemOsNumerosDoReel(int indice, string camada, string motivo, string? letra, double pontos)
    {
        var s = ServidorDeTeste.CasosDoReel[indice];
        using var lab = new Laboratorio();
        lab.Subir(s);

        var q = await new VerificadorTls(lab).ChecarAsync(s.Host);

        Assert.Equal((camada, motivo), (q.Camada, q.Motivo));
        Assert.Equal(camada != "HTTP", q.Quebrou);
        if (letra is null) return;
        var nota = NotaSslLabs.Calcular(s, q);
        Assert.Equal(letra, nota.Letra);
        Assert.Equal(pontos, nota.Pontos, 6);
    }

    [Fact]
    public async Task SeuSite_NegociaTls13_ECertificadoConfiavel()
    {
        var s = ServidorDeTeste.CasosDoReel[5];
        using var lab = new Laboratorio();
        lab.Subir(s);

        var q = await new VerificadorTls(lab).ChecarAsync(s.Host);

        Assert.Equal(SslProtocols.Tls13, q.Protocolo);
        Assert.Equal("seu-site.example", q.Certificado!.NomeDns);
        Assert.Equal(System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError, q.Certificado.Cadeia);
        Assert.False(q.Certificado.NomeErrado);
    }

    [Fact]
    public async Task Expirado_4192DiasAntesDoScan()
    {
        var s = ServidorDeTeste.CasosDoReel[2];
        using var lab = new Laboratorio();
        lab.Subir(s);

        var q = await new VerificadorTls(lab).ChecarAsync(s.Host);

        Assert.Equal(4192, Math.Floor((VerificadorTls.DataDoScan - q.Certificado!.ValidoAte).TotalDays));
    }

    [Fact]
    public async Task MesmoCertificado_ValidadoNoutraData_DeixaDeEstarExpirado()
    {
        // O resultado depende da data da validação: em 2015-03-01 o certificado do caso 3 ainda valia.
        var s = ServidorDeTeste.CasosDoReel[2];
        using var lab = new Laboratorio();
        lab.Subir(s);

        var q = await new VerificadorTls(lab, new DateTime(2015, 3, 1, 0, 0, 0, DateTimeKind.Utc)).ChecarAsync(s.Host);

        Assert.Equal("HTTP", q.Camada);
        Assert.Equal("200 sem HSTS", q.Motivo);
    }

    [Fact]
    public void SemConfianca_CasosDeCertificadoSeriamAMenos()
    {
        foreach (int i in new[] { 2, 3, 4 })
            Assert.Equal("A-", NotaSslLabs.SemContarOCertificado(ServidorDeTeste.CasosDoReel[i]).Letra);
    }

    [Fact]
    public void CasosDoReel_UsamSoNomesReservados()
    {
        Assert.All(ServidorDeTeste.CasosDoReel, s =>
            Assert.True(s.Host.EndsWith(".invalid", StringComparison.Ordinal) || s.Host.EndsWith(".example", StringComparison.Ordinal)));
    }

    // ---------- A nota, sem rede ----------

    private static readonly ResultadoConexao ChegouNoHttp = new("HTTP", "200", Quebrou: false);

    [Fact]
    public void Tls10_TemOsTetosBCBAMenosAMenos_ENotaC()
    {
        var nota = NotaSslLabs.Calcular(ServidorDeTeste.CasosDoReel[1], ChegouNoHttp);

        Assert.Equal(["B", "C", "B", "A-", "A-"], nota.Tetos);
        Assert.Equal("C", nota.Letra);
        Assert.Equal(90, nota.Pontos, 6);
    }

    [Theory]
    [InlineData(0, false, "A-")]   // sem TLS 1.3 e sem HSTS
    [InlineData(365, false, "A-")] // HSTS não compensa a falta de TLS 1.3
    [InlineData(0, true, "A-")]    // TLS 1.3 sem HSTS
    [InlineData(179, true, "A")]   // HSTS curto demais para o A+
    [InlineData(180, true, "A+")]
    public void HstsETls13_DecidemEntreAMenosAEAMais(int hstsDias, bool tls13, string letra)
    {
        var s = new ServidorDeTeste("x.example", HstsDias: hstsDias,
            Protocolos: SslProtocols.Tls12 | (tls13 ? SslProtocols.Tls13 : 0),
            Melhor: tls13 ? SslProtocols.Tls13 : SslProtocols.Tls12);

        Assert.Equal(letra, NotaSslLabs.Calcular(s, ChegouNoHttp).Letra);
    }

    [Theory]
    [InlineData("nome não bate com o site", "M")]
    [InlineData("expirado", "T")]
    [InlineData("autoassinado", "T")]
    public void ProblemaDeCertificado_DerrubaQualquerNota(string motivo, string letra)
    {
        var otimo = ServidorDeTeste.CasosDoReel[5]; // seria A+
        Assert.Equal(letra, NotaSslLabs.Calcular(otimo, new ResultadoConexao("Certificado", motivo)).Letra);
    }

    [Fact]
    public void Propriedade_TetoNuncaMelhoraANota()
    {
        string[] ordem = ["A+", "A", "A-", "B", "C", "D", "E", "F"];
        foreach (int chave in new[] { 256, 1024, 2048, 4096 })
            foreach (int cifra in new[] { 0, 64, 128, 256 })
                foreach (bool aead in new[] { false, true })
                {
                    var s = new ServidorDeTeste("x.example", ChaveBits: chave, CifraMin: cifra, CifraMax: Math.Max(cifra, 128), Aead: aead);
                    var nota = NotaSslLabs.Calcular(s, ChegouNoHttp);
                    string semTetos = NotaSslLabs.Letra(nota.Pontos);
                    Assert.True(Array.IndexOf(ordem, nota.Letra) >= Array.IndexOf(ordem, semTetos));
                }
    }

    [Theory]
    [InlineData(80, "A")]
    [InlineData(79.9, "B")]
    [InlineData(65, "B")]
    [InlineData(50, "C")]
    [InlineData(35, "D")]
    [InlineData(20, "E")]
    [InlineData(19.9, "F")]
    public void Letra_FaixasDoGuia(double pontos, string letra) => Assert.Equal(letra, NotaSslLabs.Letra(pontos));

    [Fact]
    public void Tabelas_ProtocoloChaveECifra()
    {
#pragma warning disable SYSLIB0039
        Assert.Equal(90, NotaSslLabs.Protocolo(SslProtocols.Tls));
        Assert.Equal(95, NotaSslLabs.Protocolo(SslProtocols.Tls11));
#pragma warning restore SYSLIB0039
        Assert.Equal(100, NotaSslLabs.Protocolo(SslProtocols.Tls13));
        Assert.Equal(80, NotaSslLabs.Chave(1024));
        Assert.Equal(90, NotaSslLabs.Chave(2048));
        Assert.Equal(90, NotaSslLabs.Chave(4096)); // limitada em 3072 bits, como no notebook
        Assert.Equal(0, NotaSslLabs.Cifra(0));
        Assert.Equal(80, NotaSslLabs.Cifra(128));
        Assert.Equal(100, NotaSslLabs.Cifra(256));
    }
}
