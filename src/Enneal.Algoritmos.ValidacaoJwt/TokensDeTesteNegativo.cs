using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;

namespace Enneal.Algoritmos.ValidacaoJwt;

/// <summary>
/// Um token de teste e se ele DEVERIA ser recusado.
/// </summary>
/// <param name="Nome">Descrição curta do caso.</param>
/// <param name="Token">O token.</param>
/// <param name="Ruim">Se a API certa precisa recusar este token.</param>
public sealed record CasoDeToken(string Nome, string Token, bool Ruim);

/// <summary>
/// Os testes negativos que a SUA API precisa recusar: tokens editados, sem assinatura, de outra API ou vencidos.
/// </summary>
/// <remarks>
/// As edições são só manipulação de texto em base64url, como qualquer cliente consegue fazer com o próprio token:
/// header e payload de um JWT não são criptografados, só assinados. Use casos assim nos testes da sua API.
/// </remarks>
public static class TokensDeTesteNegativo
{
    /// <summary>
    /// Troca o <c>role</c> no payload e mantém a assinatura antiga (que deixa de conferir).
    /// </summary>
    /// <param name="jwt">O token original.</param>
    /// <param name="papel">O novo papel.</param>
    /// <returns>O token editado.</returns>
    public static string TrocarPapel(string jwt, string papel)
    {
        ArgumentNullException.ThrowIfNull(jwt);
        var p = jwt.Split('.');
        string json = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(p[1]));
        json = Regex.Replace(json, "\"role\":\"[^\"]*\"", $"\"role\":\"{papel}\"");
        return $"{p[0]}.{Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(json))}.{p[2]}";
    }

    /// <summary>
    /// Troca o header por <c>{"alg":"none"}</c> e tira a assinatura.
    /// </summary>
    /// <param name="jwt">O token.</param>
    /// <returns>O token sem assinatura.</returns>
    public static string SemAssinatura(string jwt)
    {
        ArgumentNullException.ThrowIfNull(jwt);
        var p = jwt.Split('.');
        return $"{Base64UrlEncoder.Encode("{\"alg\":\"none\",\"typ\":\"JWT\"}")}.{p[1]}.";
    }

    /// <summary>
    /// Decodifica o header ou o payload (base64url → JSON). Qualquer um consegue: não coloque segredo no payload.
    /// </summary>
    /// <param name="parte">A parte do token (antes ou depois do primeiro ponto).</param>
    /// <returns>O JSON.</returns>
    public static string Decodificar(string parte) => Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(parte));

    /// <summary>
    /// Os 6 tokens do Reel: o original (cliente), 4 que a API certa precisa recusar e 1 admin válido.
    /// </summary>
    /// <param name="chave">A chave que assina os tokens.</param>
    /// <param name="agora">O instante de referência (no Reel, <see cref="EmissorDeTokens.RelogioDaDemo"/>).</param>
    /// <returns>Os 6 casos, na ordem do Reel.</returns>
    public static IReadOnlyList<CasoDeToken> CasosDoReel(byte[] chave, DateTime agora)
    {
        string original = EmissorDeTokens.Emitir(chave, "ana", "cliente", EmissorDeTokens.Publico, agora.AddMinutes(-5));
        return
        [
            new("original (cliente)", original, false),
            new("role editado + alg none", SemAssinatura(TrocarPapel(original, "admin")), true),
            new("role editado, assinatura antiga", TrocarPapel(original, "admin"), true),
            new("admin de outra API (aud)", EmissorDeTokens.Emitir(chave, "bia", "admin", EmissorDeTokens.OutraApi, agora.AddMinutes(-5)), true),
            new("admin vencido (exp)", EmissorDeTokens.Emitir(chave, "bia", "admin", EmissorDeTokens.Publico, agora.AddHours(-2)), true),
            new("admin de verdade", EmissorDeTokens.Emitir(chave, "bia", "admin", EmissorDeTokens.Publico, agora.AddMinutes(-5)), false),
        ];
    }
}
