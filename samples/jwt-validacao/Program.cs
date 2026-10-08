// =====================================================================
//  Validando JWT do jeito certo (ASP.NET Core 8 + JwtBearer)
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Emite tokens HS256 de verdade com valores de brinquedo, edita o "role"
//  de um deles e testa os mesmos 6 tokens em duas APIs locais: uma com a
//  validação frouxa e outra com a certa (algoritmo fixo, assinatura
//  obrigatória, chave forte, emissor, audience e validade).
//
//  Só 127.0.0.1, hosts fictícios (seu-site.example) e chave de demo.
//  Só no teste, a validade é conferida no relógio fixo da demo
//  (08/10/2026 12:00 UTC); em produção, deixe o padrão.
//
//  A validação está em src/Enneal.Algoritmos.ValidacaoJwt.
//  Números do Reel:
//    tokens ruins aceitos (200): API frouxa 3 de 4, API certa 0 de 4
//    "segredo123" (80 bits) recusada pelo .NET: mínimo 128
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run               -> o status de cada token nas duas APIs
//    dotnet run -- --tokens   -> também os tokens, as partes decodificadas e o motivo de cada 401
// =====================================================================

using System.Text;
using Enneal.Algoritmos.ValidacaoJwt;

Console.OutputEncoding = Encoding.UTF8;

bool mostrarTokens = args.Contains("--tokens");
byte[] chave = EmissorDeTokens.ChaveDeDemo();
var agora = EmissorDeTokens.RelogioDaDemo;
var casos = TokensDeTesteNegativo.CasosDoReel(chave, agora);

if (mostrarTokens)
{
    var p = casos[0].Token.Split('.');
    Console.WriteLine($"T original {casos[0].Token}");
    Console.WriteLine($"D header {TokensDeTesteNegativo.Decodificar(p[0])}");
    Console.WriteLine($"D payload {TokensDeTesteNegativo.Decodificar(p[1])}");
    for (int i = 1; i < casos.Count; i++) Console.WriteLine($"T {i} {casos[i].Token}");
}

// Chave fraca: o próprio .NET recusa assinar com ela.
var fraca = EmissorDeTokens.TestarChaveFraca("segredo123");
Console.WriteLine(fraca.Recusada
    ? $"Chave fraca \"segredo123\": recusada ({fraca.Bits} bits, {fraca.Codigo}: mínimo {fraca.Minimo})"
    : "Chave fraca: aceita");

// As duas APIs (só em 127.0.0.1) e os mesmos tokens nas duas.
await using var apiFrouxa = await ApiDeTeste.SubirAsync(ValidacaoDeTokens.Frouxa(chave));
await using var apiCerta = await ApiDeTeste.SubirAsync(ValidacaoDeTokens.CertaNoRelogio(chave, agora));

int ruinsFrouxa = 0, ruinsCerta = 0, ruins = 0;
for (int i = 0; i < casos.Count; i++)
{
    var f = await apiFrouxa.PedirAsync(casos[i].Token);
    var c = await apiCerta.PedirAsync(casos[i].Token);
    if (casos[i].Ruim)
    {
        ruins++;
        if (f.Status == 200) ruinsFrouxa++;
        if (c.Status == 200) ruinsCerta++;
    }
    if (mostrarTokens) Console.WriteLine($"R {i} frouxa {f.Status} {f.Motivo} | certa {c.Status} {c.Motivo}");
    Console.WriteLine($"Token {i} {casos[i].Nome}: API frouxa {f.Status} | API certa {c.Status}");
}
Console.WriteLine($"Resumo: tokens ruins aceitos (200): API frouxa {ruinsFrouxa} de {ruins}, API certa {ruinsCerta} de {ruins}");
