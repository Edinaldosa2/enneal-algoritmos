using System.Text.RegularExpressions;

namespace Enneal.Algoritmos.UrlMaliciosa;

/// <summary>
/// As 30 pistas léxicas de uma URL: tudo é tirado SÓ do texto (nada de DNS, WHOIS ou abrir a página).
/// </summary>
/// <remarks>
/// É a porta fiel do extrator do notebook (versão <c>url-lexical-v2</c>): mesmas 30 pistas, mesma ordem, todas
/// inteiras (frações em "por mil" e entropia × 1000). Por serem inteiras, o modelo dá exatamente o mesmo
/// resultado em Python e em C#.
/// </remarks>
public static class PistasDaUrl
{
    /// <summary>Versão do extrator (tem de ser a mesma com que o modelo foi treinado).</summary>
    public const string Versao = "url-lexical-v2";

    /// <summary>Os nomes das 30 pistas, na ordem do vetor.</summary>
    public static IReadOnlyList<string> Nomes { get; } =
    [
        "url_len", "host_len", "rest_len", "host_dots", "host_hyphens", "host_digits", "host_is_ipv4",
        "has_port", "has_at", "n_slash", "n_query_params", "n_eq", "n_pct", "n_special", "digit_permille",
        "letter_permille", "upper_permille", "entropy_milli", "tld_len", "suspicious_tld", "n_suspicious_words",
        "has_punycode", "is_shortener", "path_depth", "has_double_slash", "longest_token",
        "ext_risky", "ext_php", "ext_html", "n_subdomains",
    ];

    private static readonly Regex Esquema = new(@"^[A-Za-z][A-Za-z0-9+.\-]*://");
    private static readonly Regex Ipv4 = new(@"^[0-9]{1,3}(\.[0-9]{1,3}){3}$");
    private static readonly Regex Corte = new(@"[/.?=&_\-:@~%+]");

    private static readonly string[] Palavras =
    [
        "login", "signin", "verify", "account", "update", "secure", "banking", "confirm",
        "password", "webscr", "paypal", "ebayisapi", "wp-admin", "wp-includes", "free", "bonus",
    ];

    private static readonly HashSet<string> TldsSuspeitos =
    [
        "tk", "ml", "ga", "cf", "gq", "xyz", "top", "zip", "click", "work", "country",
        "kim", "men", "loan", "review", "download", "racing", "win", "bid", "stream",
    ];

    private static readonly HashSet<string> Encurtadores =
    [
        "bit.ly", "goo.gl", "tinyurl.com", "ow.ly", "t.co", "is.gd", "buff.ly", "adf.ly",
        "bit.do", "cutt.ly", "shorte.st", "tiny.cc", "rb.gy", "tr.im", "x.co",
    ];

    private static readonly string[] ExtensoesDeRisco =
        [".exe", ".zip", ".rar", ".apk", ".scr", ".bin", ".dll", ".jar", ".msi", ".bat", ".js"];

    /// <summary>
    /// Extrai as 30 pistas de uma URL.
    /// </summary>
    /// <param name="url">A URL, como texto (não é acessada).</param>
    /// <returns>Vetor de 30 números, na ordem de <see cref="Nomes"/>.</returns>
    public static float[] Extrair(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        string u = Normalizar(url);                        // sem esquema e sem "www."
        var (hostTodo, resto) = Separar(u);                // host | caminho + query + fragmento
        string host = Minusculas(hostTodo);
        host = host[(host.LastIndexOf('@') + 1)..];        // o que vem antes do @ é enfeite
        bool porta = host.Contains(':');
        if (porta) host = host[..host.IndexOf(':')];
        int pontos = host.Count(c => c == '.');
        string tld = pontos > 0 ? host.Split('.')[^1] : "";

        // As 6 pistas que o Reel acende (o modelo usa as 30).
        bool ip = Ipv4.IsMatch(host);                      // IP no lugar do nome
        bool arroba = u.Contains('@');                     // usuario@host engana quem lê rápido
        int subdominios = Math.Max(pontos - 1, 0);
        int caminho = resto.Length;
        bool tldSuspeito = TldsSuspeitos.Contains(tld);
        bool punycode = host.Contains("xn--", StringComparison.Ordinal); // letra "falsa" (á, а cirílico...)

        int n = u.Length, digitos = 0, letras = 0, maiusculas = 0, especiais = 0;
        foreach (char c in u)
        {
            bool d = char.IsAsciiDigit(c), l = char.IsAsciiLetter(c);
            if (d) digitos++;
            if (l) letras++;
            if (char.IsAsciiLetterUpper(c)) maiusculas++;
            if (!d && !l) especiais++;
        }
        string path = resto.Split('?', 2)[0].Split('#', 2)[0];
        string pathMin = Minusculas(path);
        int q = resto.IndexOf('?');
        string query = q != -1 ? resto[(q + 1)..] : "";
        string uMin = Minusculas(u);
        int maiorPedaco = Corte.Split(u).Max(t => t.Length);
        double PorMil(int k) => n > 0 ? Math.Round(1000.0 * k / n) : 0;

        double[] f =
        [
            n, host.Length, caminho, host.Count(c => c == '.'), host.Count(c => c == '-'),
            host.Count(char.IsAsciiDigit), ip ? 1 : 0, porta ? 1 : 0, arroba ? 1 : 0, resto.Count(c => c == '/'),
            query.Split('&').Count(p => p.Length > 0), u.Count(c => c == '='), u.Count(c => c == '%'), especiais,
            PorMil(digitos), PorMil(letras), PorMil(maiusculas), Math.Round(1000.0 * Entropia(u)), tld.Length,
            tldSuspeito ? 1 : 0, Palavras.Count(w => uMin.Contains(w, StringComparison.Ordinal)), punycode ? 1 : 0,
            Encurtadores.Contains(host) ? 1 : 0, path.Split('/').Count(s => s.Length > 0),
            resto.Contains("//", StringComparison.Ordinal) ? 1 : 0, maiorPedaco,
            ExtensoesDeRisco.Any(e => pathMin.EndsWith(e, StringComparison.Ordinal)) ? 1 : 0,
            pathMin.EndsWith(".php", StringComparison.Ordinal) ? 1 : 0,
            pathMin.EndsWith(".html", StringComparison.Ordinal) || pathMin.EndsWith(".htm", StringComparison.Ordinal) ? 1 : 0,
            subdominios,
        ];
        return Array.ConvertAll(f, x => (float)x);
    }

    /// <summary>O TLD (a última parte do nome do site), em minúsculas, ou vazio.</summary>
    /// <param name="url">A URL.</param>
    public static string Tld(string url)
    {
        ArgumentNullException.ThrowIfNull(url);
        string host = Minusculas(Separar(Normalizar(url)).Host);
        host = host[(host.LastIndexOf('@') + 1)..];
        if (host.Contains(':')) host = host[..host.IndexOf(':')];
        return host.Contains('.') ? host[(host.LastIndexOf('.') + 1)..] : "";
    }

    // Tira espaços, o esquema (http://) e um "www." do começo: no dataset eles eram ruído de rotulagem.
    private static string Normalizar(string url)
    {
        string u = Esquema.Replace(url.Trim(' ', '\t', '\r', '\n'), "", 1);
        return u.Length >= 4 && Minusculas(u[..4]) == "www." ? u[4..] : u;
    }

    // O host vai até o primeiro /, ? ou #.
    private static (string Host, string Resto) Separar(string u)
    {
        int corte = u.Length;
        foreach (char ch in "/?#")
        {
            int i = u.IndexOf(ch);
            if (i != -1 && i < corte) corte = i;
        }
        return (u[..corte], u[corte..]);
    }

    // Minúsculas só em A-Z (sem regras de cultura), igual ao Python do notebook.
    private static string Minusculas(string s) => string.Create(s.Length, s, (destino, origem) =>
    {
        for (int i = 0; i < origem.Length; i++)
            destino[i] = char.IsAsciiLetterUpper(origem[i]) ? (char)(origem[i] + 32) : origem[i];
    });

    // Entropia de Shannon em bits por caractere: texto aleatório tem entropia alta.
    private static double Entropia(string s)
    {
        if (s.Length == 0) return 0.0;
        var contagem = new Dictionary<char, int>();
        foreach (char ch in s) contagem[ch] = contagem.GetValueOrDefault(ch) + 1;
        double n = s.Length, h = 0.0;
        foreach (int c in contagem.Values)
        {
            double p = c / n;
            h -= p * Math.Log2(p);
        }
        return h;
    }
}
