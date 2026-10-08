// =====================================================================
//  URL maliciosa ou não?
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Um classificador que lê SÓ o texto da URL (nada de DNS, WHOIS ou abrir
//  a página): 30 pistas léxicas e o LightGBM treinado no notebook (dataset
//  público "Malicious URLs", CC0), exportado para ONNX. Aqui o modelo é
//  avaliado em C# puro, servido por uma minimal API do ASP.NET Core 8 em
//  127.0.0.1 (porta livre). Nada aqui acessa a rede além do localhost.
//
//  As URLs usam só nomes reservados para documentação (example.com/.net/
//  .org, RFC 2606) e IPs de documentação (RFC 5737).
//
//  Notebook: kaggle.com/code/edinaldos/malicious-url-classifier-onnx-asp-net-core-8
//  O código está em src/Enneal.Algoritmos.UrlMaliciosa. Números do Reel:
//    6 URLs, 4 marcadas (limiar 0,768444); a URL 6 (phishing) passa: 0,107
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run               -> o resultado de cada URL
//    dotnet run -- --pistas   -> também as 30 pistas e o JSON da API
// =====================================================================

using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Enneal.Algoritmos.UrlMaliciosa;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = Encoding.UTF8;
var inv = CultureInfo.InvariantCulture;
var web = new JsonSerializerOptions(JsonSerializerDefaults.Web);
bool mostrarPistas = args.Contains("--pistas");

(string Url, string Imita)[] exemplos =
[
    ("https://www.example.com/produtos/tenis-corrida", "normal"),
    ("https://banco.example.com@198.51.100.23/login/verify", "phishing"),
    ("http://xn--bnco-0qa.example.com.login.conta.example.net", "phishing"),
    ("http://203.0.113.7:8080/bin/update.exe", "malware"),
    ("http://escola.example.org/index.php?option=com_content&view=article&id=18&Itemid=7", "defacement"),
    ("https://login-banco.example.com/", "phishing"),
];

// A minimal API: POST /score {"url": "..."} -> {"probabilidade", "veredito", "limiar"}.
var b = WebApplication.CreateBuilder();
b.Logging.ClearProviders();
b.WebHost.UseUrls("http://127.0.0.1:0"); // só localhost, porta livre
await using var app = b.Build();

app.MapPost("/score", (Pedido pedido) =>
{
    if (!ClassificadorDeUrl.Aceita(pedido.Url))
        return Results.BadRequest("url obrigatória");
    var a = ClassificadorDeUrl.Avaliar(pedido.Url!); // só lê o texto: nunca acessa a URL
    return Results.Ok(new Resposta(a.Probabilidade, a.Veredito, ClassificadorDeUrl.Limiar));
});

await app.StartAsync();
using var http = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };

int n = 0, maliciosas = 0;
foreach (var (url, imita) in exemplos)
{
    n++;
    var resp = await http.PostAsJsonAsync("/score", new Pedido(url));
    string json = await resp.Content.ReadAsStringAsync();
    var r = JsonSerializer.Deserialize<Resposta>(json, web)!;
    if (r.Veredito == "maliciosa") maliciosas++;

    if (mostrarPistas)
    {
        float[] f = PistasDaUrl.Extrair(url);
        Console.WriteLine($"U {n} {url}");
        Console.WriteLine($"X {n} " + string.Join(" ", f.Select(x => x.ToString(inv))));
        Console.WriteLine($"K {n} ip={f[6]} arroba={f[8]} subdominios={f[29]} caminho={f[2]} " +
                          $"tld={PistasDaUrl.Tld(url)} tld_suspeito={f[19]} punycode={f[21]}");
        Console.WriteLine($"J {n} {(int)resp.StatusCode} {json}");
    }

    // "Imita" é o tipo que a URL de exemplo foi escrita para parecer; o modelo só diz maliciosa ou ok.
    string acerto = (r.Veredito == "maliciosa") == (imita != "normal") ? "acertou" : "errou";
    Console.WriteLine($"URL {n} ({imita}): p={r.Probabilidade.ToString("F6", inv)} -> {r.Veredito} ({acerto})");
}

var vazia = await http.PostAsJsonAsync("/score", new Pedido(""));
Console.WriteLine($"Resumo: {n} URLs, {maliciosas} maliciosas pelo modelo (limiar {ClassificadorDeUrl.Limiar.ToString(inv)}); " +
                  $"URL vazia -> {(int)vazia.StatusCode}");
await app.StopAsync();

record Pedido(string? Url);
record Resposta(float Probabilidade, string Veredito, float Limiar);
