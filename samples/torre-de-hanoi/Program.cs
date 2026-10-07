// =====================================================================
//  Torre de Hanói (recursão)
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Regra: levar a torre do pino A para o C, um disco por vez, sem nunca
//  pôr um disco maior sobre um menor. O pino B serve de apoio.
//
//  A recursão está em src/Enneal.Algoritmos.TorreHanoi/ResolvedorHanoi.cs.
//  Números do Reel:
//    3 discos  -> 7 movimentos
//    4 discos  -> 15 movimentos (= 2 x 7 + 1)
//    6 discos  -> 63 movimentos
//    64 discos -> 18.446.744.073.709.551.615 movimentos (UInt128)
//                 = 584.542.046.090 anos a 1 movimento por segundo
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run                   -> as três rodadas e a conta dos 64 discos
//    dotnet run -- --movimentos   -> também imprime cada movimento
// =====================================================================

using System.Globalization;
using System.Text;
using Enneal.Algoritmos.TorreHanoi;

Console.OutputEncoding = Encoding.UTF8;

bool mostrarMovimentos = args.Contains("--movimentos");

foreach (int n in new[] { 3, 4, 6 })
{
    // Com --movimentos, cada linha é: H<discos> <nº do movimento> <disco> <de> <para>.
    Action<Movimento>? aoMover = mostrarMovimentos
        ? m => Console.WriteLine($"H{n} {m.Numero} {m.Disco} {m.De} {m.Para}")
        : null;

    var r = ResolvedorHanoi.Resolver(n, aoMover);
    UInt128 formula = ContaDeHanoi.MovimentosNecessarios(n);

    Console.WriteLine($"{n} discos: {r.Movimentos} movimentos = 2^{n} - 1 = {formula}{(r.Resolvida ? "" : " ERRO")}");
}

// Cada disco a mais dobra o trabalho e soma 1: com 64 discos são 2^64 - 1 movimentos.
// Esse número não cabe num long (máximo 2^63 - 1), por isso UInt128.
UInt128 total = ContaDeHanoi.MovimentosNecessarios(64);
UInt128 anos = ContaDeHanoi.AnosAUmMovimentoPorSegundo(total);
UInt128 bilhoes = ContaDeHanoi.BilhoesDeAnos(anos);

// Números no jeito brasileiro: ponto separa os milhares.
var br = new NumberFormatInfo { NumberGroupSeparator = ".", NumberDecimalSeparator = "," };
Console.WriteLine($"64 discos = {total.ToString("N0", br)} movimentos");
Console.WriteLine($"a 1 movimento por segundo: {anos.ToString("N0", br)} anos (≈ {bilhoes} bilhões de anos)");
