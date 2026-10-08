// =====================================================================
//  Do F ao A+: um cabeçalho de segurança por vez
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Um site ASP.NET Core 8 (loja.example) ganha um cabeçalho por fase e a
//  nota sobe. Cada fase monta o pipeline de verdade (UseHttpsRedirection,
//  UseHsts, UseCookiePolicy e um middleware de cabeçalhos) e faz GET
//  http://loja.example/ seguindo os redirecionamentos, em memória: sem
//  servidor e sem rede.
//
//  A nota segue as regras do notebook (pontos e faixas no estilo do
//  securityheaders.com; não é a nota oficial):
//  kaggle.com/code/edinaldos/http-security-headers-a-to-f-grades-net-fix
//
//  O código está em src/Enneal.Algoritmos.CabecalhosSeguranca.
//  Números do Reel: 0 -> 30 -> 55 -> 75 -> 95 -> 115 -> 135 -> 160 pontos;
//  F, E, D, D, C, B, A, A, A, A (teto csp_unsafe_inline) e A+.
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run                   -> a nota de cada fase
//    dotnet run -- --cabecalhos   -> também cada cabeçalho, cookie e ponto
// =====================================================================

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Enneal.Algoritmos.CabecalhosSeguranca;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

bool mostrarCabecalhos = args.Contains("--cabecalhos");

for (int f = 0; f < LojaDeExemplo.Fases.Count; f++)
{
    var (r, a) = await LojaDeExemplo.AvaliarFaseAsync(f);
    if (mostrarCabecalhos) Mostrar(f, r, a);

    Console.WriteLine($"Fase {f}: {LojaDeExemplo.Fases[f]} | {a.Pontos}/{AvaliacaoCabecalhos.Maximo} | nota {a.Letra}"
        + (a.Tetos.Count > 0 ? $" (teto: {string.Join(", ", a.Tetos)})" : "")
        + (a.Avisos.Count > 0 ? $" | {a.Avisos.Count} aviso(s)" : ""));
}

// Com --cabecalhos: F (redirecionamentos e URL final), H (cada cabeçalho; o nonce vira *), C (cookies), P (pontos).
static void Mostrar(int f, RespostaHttp r, AvaliacaoCabecalhos a)
{
    Console.WriteLine($"F {f} {string.Join(">", r.Cadeia)} {r.Url} https {(r.Https ? "sim" : "nao")}");
    string[] nomes =
    [
        "strict-transport-security", "x-content-type-options", "x-frame-options", "referrer-policy",
        "permissions-policy", "cross-origin-opener-policy", "cross-origin-embedder-policy",
        "cross-origin-resource-policy", "content-security-policy",
    ];
    foreach (string nome in nomes)
        foreach (string v in r.Todos(nome))
            Console.WriteLine($"H {f} {nome}: {Regex.Replace(v, "'nonce-[^']+'", "'nonce-*'")}");
    foreach (string c in r.Cookies) Console.WriteLine($"C {f} {c}");

    // Página isolada (cross-origin isolated): COOP same-origin + COEP require-corp. Não vale ponto, só informa.
    string? coop = r.Primeiro("cross-origin-opener-policy"), coep = r.Primeiro("cross-origin-embedder-policy");
    bool isolada = coop == "same-origin" && coep is "require-corp" or "credentialless";
    Console.WriteLine($"P {f} {a.Pontos}/{AvaliacaoCabecalhos.Maximo} {a.Porcentagem:0.0}% nota {a.Letra} " +
                      $"tetos {(a.Tetos.Count == 0 ? "-" : string.Join(",", a.Tetos))} " +
                      $"avisos {(a.Avisos.Count == 0 ? "-" : string.Join(" | ", a.Avisos))} isolada {(isolada ? "sim" : "nao")}");
}
