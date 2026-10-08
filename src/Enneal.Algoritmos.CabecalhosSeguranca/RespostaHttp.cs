using System.Globalization;
using System.Text.RegularExpressions;

namespace Enneal.Algoritmos.CabecalhosSeguranca;

/// <summary>
/// A resposta final de um GET (depois de seguir os redirecionamentos), do jeito que a nota precisa.
/// </summary>
/// <param name="Url">URL final, depois dos redirecionamentos.</param>
/// <param name="Cadeia">Os status de cada salto (por exemplo 307 e 200).</param>
/// <param name="Cabecalhos">Cabeçalhos da resposta final, com o nome em minúsculas (sem <c>set-cookie</c>).</param>
/// <param name="Cookies">Cada <c>Set-Cookie</c> da resposta final.</param>
public sealed record RespostaHttp(string Url, IReadOnlyList<int> Cadeia,
    IReadOnlyDictionary<string, List<string>> Cabecalhos, IReadOnlyList<string> Cookies)
{
    /// <summary>Se a resposta final veio por HTTPS.</summary>
    public bool Https => Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    /// <summary>Todos os valores de um cabeçalho (vazio se não veio).</summary>
    /// <param name="nome">Nome do cabeçalho (qualquer caixa).</param>
    public IReadOnlyList<string> Todos(string nome) =>
        Cabecalhos.TryGetValue(nome.ToLowerInvariant(), out var v) ? v : [];

    /// <summary>O primeiro valor de um cabeçalho, sem espaços nas pontas, ou <see langword="null"/>.</summary>
    /// <param name="nome">Nome do cabeçalho (qualquer caixa).</param>
    public string? Primeiro(string nome) => Todos(nome).Select(v => v.Trim()).FirstOrDefault();

    /// <summary>A Content-Security-Policy (vários cabeçalhos viram uma lista separada por vírgula), ou <see langword="null"/>.</summary>
    public string? Csp => Cabecalhos.TryGetValue("content-security-policy", out var v) ? string.Join(",", v) : null;

    /// <summary>O <c>max-age</c> do HSTS em segundos (0 se não veio ou se veio mais de um).</summary>
    public long HstsMaxAge
    {
        get
        {
            string? v = Primeiro("strict-transport-security");
            if (v is null) return 0;
            var m = Regex.Matches(v, "max-age\\s*=\\s*\"?(\\d+)\"?", RegexOptions.IgnoreCase);
            return m.Count == 1 ? long.Parse(m[0].Groups[1].Value, CultureInfo.InvariantCulture) : 0;
        }
    }
}

/// <summary>
/// Os atributos de um cookie que a nota confere.
/// </summary>
/// <param name="Nome">Nome do cookie.</param>
/// <param name="Secure">Se só vai por HTTPS.</param>
/// <param name="HttpOnly">Se o JavaScript da página não consegue ler.</param>
public sealed record CookieLido(string Nome, bool Secure, bool HttpOnly)
{
    /// <summary>Lê uma linha de <c>Set-Cookie</c>.</summary>
    /// <param name="linha">Por exemplo <c>sessao=abc; path=/; secure; httponly</c>.</param>
    public static CookieLido Ler(string linha)
    {
        ArgumentNullException.ThrowIfNull(linha);
        string[] partes = linha.Split(';').Select(p => p.Trim()).ToArray();
        var atributos = partes.Skip(1).Where(p => p.Length > 0)
            .Select(p => p.Split('=')[0].Trim().ToLowerInvariant()).ToHashSet();
        return new CookieLido(partes[0].Split('=')[0].Trim(), atributos.Contains("secure"), atributos.Contains("httponly"));
    }
}
