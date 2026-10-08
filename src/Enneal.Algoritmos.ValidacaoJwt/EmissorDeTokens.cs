using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Enneal.Algoritmos.ValidacaoJwt;

/// <summary>
/// Emite os tokens HS256 de brinquedo do Reel com o <see cref="JsonWebTokenHandler"/> (o mesmo do ASP.NET Core).
/// </summary>
/// <remarks>
/// Os hosts são fictícios (<c>seu-site.example</c>) e a chave é de <b>demonstração</b>: o SHA-256 de uma frase fixa,
/// para o exemplo ser reproduzível. Em produção, use 32 bytes aleatórios
/// (<see cref="RandomNumberGenerator.GetBytes(int)"/>) guardados num cofre de segredos, ou uma chave assimétrica
/// (RS256/ES256).
/// </remarks>
public static class EmissorDeTokens
{
    /// <summary>Quem emite os tokens (o <c>iss</c>).</summary>
    public const string Emissor = "https://login.seu-site.example";

    /// <summary>Para qual API o token vale (o <c>aud</c>).</summary>
    public const string Publico = "api.seu-site.example";

    /// <summary>A audience de outra API do mesmo emissor (o token 3 do Reel).</summary>
    public const string OutraApi = "relatorios.seu-site.example";

    /// <summary>Quanto tempo o token vale depois de emitido.</summary>
    public static readonly TimeSpan Validade = TimeSpan.FromMinutes(15);

    /// <summary>
    /// O relógio fixo da demonstração (08/10/2026 12:00 UTC): os tokens saem byte a byte iguais em qualquer dia.
    /// </summary>
    public static readonly DateTime RelogioDaDemo = new(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// A chave de DEMONSTRAÇÃO (256 bits). Pública de propósito: não use em lugar nenhum.
    /// </summary>
    public static byte[] ChaveDeDemo() => SHA256.HashData(Encoding.UTF8.GetBytes("chave de demo da Enneal, troque em producao"));

    /// <summary>
    /// Emite um token HS256 com <c>sub</c>, <c>role</c>, <c>aud</c>, <c>iss</c>, <c>exp</c> (15 min),
    /// <c>iat</c> e <c>nbf</c>.
    /// </summary>
    /// <param name="chave">Chave HMAC. O .NET recusa chave com menos de 128 bits.</param>
    /// <param name="sub">Quem é o usuário.</param>
    /// <param name="papel">O papel (<c>role</c>), por exemplo <c>cliente</c> ou <c>admin</c>.</param>
    /// <param name="aud">Para qual API o token vale.</param>
    /// <param name="emitido">Quando o token foi emitido (UTC).</param>
    /// <returns>O token no formato <c>header.payload.assinatura</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se a chave for curta demais (IDX10653).</exception>
    public static string Emitir(byte[] chave, string sub, string papel, string aud, DateTime emitido)
    {
        var credenciais = new SigningCredentials(new SymmetricSecurityKey(chave), SecurityAlgorithms.HmacSha256);
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Claims = new Dictionary<string, object> { ["sub"] = sub, ["role"] = papel },
            Audience = aud,
            Issuer = Emissor,
            Expires = emitido.Add(Validade),
            IssuedAt = emitido,
            NotBefore = emitido,
            SigningCredentials = credenciais,
        });
    }

    /// <summary>
    /// Tenta assinar com uma chave fraca e devolve o que o .NET respondeu.
    /// No Reel, <c>"segredo123"</c> (80 bits) é recusada com IDX10653: o mínimo da biblioteca é 128 bits.
    /// </summary>
    /// <param name="chaveFraca">A chave testada, em texto.</param>
    /// <returns>Se foi recusada, o código do erro (IDX...) e o mínimo de bits exigido.</returns>
    public static (bool Recusada, int Bits, string Codigo, string Minimo) TestarChaveFraca(string chaveFraca)
    {
        byte[] fraca = Encoding.UTF8.GetBytes(chaveFraca);
        try
        {
            Emitir(fraca, "ana", "cliente", Publico, RelogioDaDemo);
            return (false, fraca.Length * 8, "", "");
        }
        catch (ArgumentOutOfRangeException e)
        {
            string codigo = System.Text.RegularExpressions.Regex.Match(e.Message, @"IDX\d+").Value;
            string minimo = System.Text.RegularExpressions.Regex.Match(e.Message, @"(?:at least|greater than:) '(\d+)'").Groups[1].Value;
            return (true, fraca.Length * 8, codigo, minimo);
        }
    }
}
