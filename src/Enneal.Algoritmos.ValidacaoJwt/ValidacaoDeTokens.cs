using Microsoft.IdentityModel.Tokens;

namespace Enneal.Algoritmos.ValidacaoJwt;

/// <summary>
/// As duas configurações de validação do Reel: a frouxa (o erro) e a certa.
/// </summary>
public static class ValidacaoDeTokens
{
    /// <summary>
    /// ERRADO: aceita token sem assinatura e não confere emissor, audience nem validade.
    /// É o tipo de "conserto" que aparece quando alguém tenta fazer um token passar sem entender o erro.
    /// </summary>
    /// <param name="chave">A chave HMAC.</param>
    /// <returns>Parâmetros de validação frouxos.</returns>
    public static TokenValidationParameters Frouxa(byte[] chave) => new()
    {
        RequireSignedTokens = false,       // aceita alg "none"
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        IssuerSigningKey = new SymmetricSecurityKey(chave),
    };

    /// <summary>
    /// CERTO: algoritmo fixo, assinatura obrigatória, chave forte, emissor, audience e validade com folga de 30 s.
    /// </summary>
    /// <param name="chave">A chave HMAC (256 bits).</param>
    /// <returns>Parâmetros de validação corretos.</returns>
    public static TokenValidationParameters Certa(byte[] chave) => new()
    {
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        RequireSignedTokens = true,
        IssuerSigningKey = new SymmetricSecurityKey(chave), // 256 bits
        ValidateIssuerSigningKey = true,
        ValidIssuer = EmissorDeTokens.Emissor,
        ValidAudience = EmissorDeTokens.Publico,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
    };

    /// <summary>
    /// SÓ PARA TESTE: a configuração certa, mas com a validade conferida num relógio fixo.
    /// </summary>
    /// <param name="chave">A chave HMAC.</param>
    /// <param name="agora">O instante usado como "agora" (no Reel, <see cref="EmissorDeTokens.RelogioDaDemo"/>).</param>
    /// <returns>Os parâmetros de <see cref="Certa"/> com um <see cref="TokenValidationParameters.LifetimeValidator"/>.</returns>
    /// <remarks>
    /// Os tokens do Reel são emitidos num instante fixo para saírem iguais em qualquer dia. Para a validade ser
    /// conferida contra esse mesmo instante, o teste troca o relógio pela mesma regra do <c>ValidateLifetime</c>
    /// (<see cref="ValidadeNoRelogio"/>). <b>Em produção não se mexe nisso:</b> o padrão usa o relógio do servidor.
    /// Por causa dessa troca, o 401 do token vencido diz "The token lifetime is invalid" (com o relógio do sistema
    /// seria "The token expired at ...").
    /// </remarks>
    public static TokenValidationParameters CertaNoRelogio(byte[] chave, DateTime agora)
    {
        var p = Certa(chave);
        p.LifetimeValidator = (nbf, exp, _, v) => ValidadeNoRelogio(nbf, exp, v, agora);
        return p;
    }

    /// <summary>
    /// A regra do <c>ValidateLifetime</c> da biblioteca aplicada a um relógio informado:
    /// <c>exp</c> obrigatório, <c>nbf &lt;= agora + folga</c> e <c>exp &gt;= agora - folga</c>.
    /// </summary>
    /// <param name="nbf">Início da validade (pode faltar).</param>
    /// <param name="exp">Fim da validade.</param>
    /// <param name="v">Os parâmetros (de onde vem a folga, <see cref="TokenValidationParameters.ClockSkew"/>).</param>
    /// <param name="agora">O instante de referência.</param>
    /// <returns><see langword="true"/> se o token está dentro da validade.</returns>
    public static bool ValidadeNoRelogio(DateTime? nbf, DateTime? exp, TokenValidationParameters v, DateTime agora)
    {
        ArgumentNullException.ThrowIfNull(v);
        return exp.HasValue && (!nbf.HasValue || nbf.Value <= agora + v.ClockSkew) && exp.Value >= agora - v.ClockSkew;
    }
}
