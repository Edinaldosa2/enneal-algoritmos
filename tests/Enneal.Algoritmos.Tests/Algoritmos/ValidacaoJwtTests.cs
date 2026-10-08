using Enneal.Algoritmos.ValidacaoJwt;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class ValidacaoJwtTests
{
    private static readonly byte[] Chave = EmissorDeTokens.ChaveDeDemo();
    private static readonly DateTime Agora = EmissorDeTokens.RelogioDaDemo;

    [Fact]
    public async Task Reel_TokensRuinsAceitos_Frouxa3De4_Certa0De4()
    {
        var casos = TokensDeTesteNegativo.CasosDoReel(Chave, Agora);
        await using var frouxa = await ApiDeTeste.SubirAsync(ValidacaoDeTokens.Frouxa(Chave));
        await using var certa = await ApiDeTeste.SubirAsync(ValidacaoDeTokens.CertaNoRelogio(Chave, Agora));

        var f = new List<RespostaDaApi>();
        var c = new List<RespostaDaApi>();
        foreach (var caso in casos)
        {
            f.Add(await frouxa.PedirAsync(caso.Token));
            c.Add(await certa.PedirAsync(caso.Token));
        }

        Assert.Equal([403, 200, 401, 200, 200, 200], f.Select(r => r.Status));
        Assert.Equal([403, 401, 401, 401, 401, 200], c.Select(r => r.Status));
        Assert.Equal(3, casos.Where((x, i) => x.Ruim && f[i].Status == 200).Count());
        Assert.DoesNotContain(casos.Where((x, i) => x.Ruim && c[i].Status == 200), _ => true);

        Assert.Equal("The signature is invalid", c[1].Motivo);
        Assert.Equal("The signature key was not found", c[2].Motivo);
        Assert.Equal("The audience 'relatorios.seu-site.example' is invalid", c[3].Motivo);
        Assert.Equal("The token lifetime is invalid", c[4].Motivo);
        Assert.Equal("ok", c[5].Motivo);
    }

    [Fact]
    public void Reel_ChaveFraca_RecusadaPeloDotNet()
    {
        var r = EmissorDeTokens.TestarChaveFraca("segredo123");

        Assert.True(r.Recusada);
        Assert.Equal(80, r.Bits);
        Assert.Equal("IDX10653", r.Codigo);
        Assert.Equal("128", r.Minimo);
    }

    [Theory]
    [InlineData("dezesseis letras", 128, "IDX10720", "256")]                      // passa do 1º mínimo (128), mas o HS256 pede 256
    [InlineData("trinta e uma letras de chave...", 248, "IDX10720", "256")]
    public void ChaveCurtaParaHs256_TambemERecusada(string chave, int bits, string codigo, string minimo)
    {
        var r = EmissorDeTokens.TestarChaveFraca(chave);

        Assert.True(r.Recusada, r.Codigo);
        Assert.Equal((bits, codigo, minimo), (r.Bits, r.Codigo, r.Minimo));
    }

    [Fact]
    public void ChaveDe256Bits_Assina()
    {
        Assert.Equal(32, Chave.Length);
        Assert.False(EmissorDeTokens.TestarChaveFraca("trinta e duas letras de chave...").Recusada);
    }

    [Fact]
    public void Emitir_EDeterministico_EOHeaderEOPayloadSaoLegiveis()
    {
        var casos = TokensDeTesteNegativo.CasosDoReel(Chave, Agora);
        var deNovo = TokensDeTesteNegativo.CasosDoReel(Chave, Agora);
        string[] partes = casos[0].Token.Split('.');

        Assert.Equal(casos.Select(x => x.Token), deNovo.Select(x => x.Token));
        Assert.Equal(3, partes.Length);
        Assert.Equal("{\"alg\":\"HS256\",\"typ\":\"JWT\"}", TokensDeTesteNegativo.Decodificar(partes[0]));
        Assert.Equal("{\"sub\":\"ana\",\"role\":\"cliente\",\"aud\":\"api.seu-site.example\",\"iss\":\"https://login.seu-site.example\","
            + "\"exp\":1791461400,\"iat\":1791460500,\"nbf\":1791460500}", TokensDeTesteNegativo.Decodificar(partes[1]));
    }

    [Fact]
    public void Edicoes_TrocamSoOQueDizem()
    {
        string original = TokensDeTesteNegativo.CasosDoReel(Chave, Agora)[0].Token;
        string[] o = original.Split('.');
        string[] editado = TokensDeTesteNegativo.TrocarPapel(original, "admin").Split('.');
        string semAssinatura = TokensDeTesteNegativo.SemAssinatura(original);

        Assert.Equal(o[0], editado[0]);                       // header igual
        Assert.Equal(o[2], editado[2]);                       // assinatura antiga
        Assert.Contains("\"role\":\"admin\"", TokensDeTesteNegativo.Decodificar(editado[1]));
        Assert.EndsWith(".", semAssinatura);                  // sem assinatura
        Assert.Equal("{\"alg\":\"none\",\"typ\":\"JWT\"}", TokensDeTesteNegativo.Decodificar(semAssinatura.Split('.')[0]));
    }

    [Theory]
    [InlineData(-5, true)]      // emitido há 5 min: vale
    [InlineData(-15, true)]     // venceu agora: ainda dentro da folga de 30 s
    [InlineData(-16, false)]    // venceu há 1 min: passou da folga
    [InlineData(-120, false)]   // o token 4 do Reel
    [InlineData(1, false)]      // emitido daqui a 1 min (nbf no futuro): ainda não vale
    public void ValidadeNoRelogio_MesmaRegraDoValidateLifetime(int emitidoEmMinutos, bool valido)
    {
        var emitido = Agora.AddMinutes(emitidoEmMinutos);
        var p = ValidacaoDeTokens.Certa(Chave);

        Assert.Equal(valido, ValidacaoDeTokens.ValidadeNoRelogio(emitido, emitido.Add(EmissorDeTokens.Validade), p, Agora));
        Assert.False(ValidacaoDeTokens.ValidadeNoRelogio(emitido, null, p, Agora)); // exp é obrigatório
    }

    [Fact]
    public async Task ConfiguracaoCerta_DiretoNoHandler_SemHttp()
    {
        var casos = TokensDeTesteNegativo.CasosDoReel(Chave, Agora);
        var handler = new JsonWebTokenHandler();
        var p = ValidacaoDeTokens.CertaNoRelogio(Chave, Agora);

        var validos = new List<bool>();
        foreach (var caso in casos) validos.Add((await handler.ValidateTokenAsync(caso.Token, p)).IsValid);

        Assert.Equal([true, false, false, false, false, true], validos); // o 0 é válido; quem nega é o RequireRole
    }

    [Fact]
    public void ConfiguracaoCerta_FixaAlgoritmoEExigeTudo()
    {
        var p = ValidacaoDeTokens.Certa(Chave);

        Assert.Equal([SecurityAlgorithms.HmacSha256], p.ValidAlgorithms);
        Assert.True(p.RequireSignedTokens);
        Assert.True(p.ValidateLifetime);
        Assert.True(p.RequireExpirationTime);
        Assert.Equal(EmissorDeTokens.Emissor, p.ValidIssuer);
        Assert.Equal(EmissorDeTokens.Publico, p.ValidAudience);
        Assert.Equal(TimeSpan.FromSeconds(30), p.ClockSkew);
        Assert.Null(p.LifetimeValidator);                                    // produção: relógio do servidor
        Assert.NotNull(ValidacaoDeTokens.CertaNoRelogio(Chave, Agora).LifetimeValidator); // só no teste
    }
}
