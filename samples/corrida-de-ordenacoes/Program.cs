// =====================================================================
//  Corrida de ordenações: Bubble x Quick x Merge
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Os três algoritmos ordenam o MESMO vetor de 28 números e contamos o
//  trabalho. Regra: 1 passo = 1 comparação, 1 troca ou 1 escrita.
//
//  Os algoritmos estão em src/Enneal.Algoritmos.Ordenacoes/Ordenacao.cs.
//  Resultado do Reel: 1º Quick (185) · 2º Merge (235) · 3º Bubble (536)
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;
using Enneal.Algoritmos.Ordenacoes;

Console.OutputEncoding = Encoding.UTF8;

var vetor = CorridaDeOrdenacoes.VetorDoReel;
Console.WriteLine("Vetor inicial: " + string.Join(" ", vetor));
Console.WriteLine();

var resultados = CorridaDeOrdenacoes.Correr(vetor);
foreach (var r in resultados)
    Console.WriteLine($"{r.Nome}: comparações {r.Comparacoes} | trocas {r.Trocas} | escritas {r.Escritas} | passos {r.Passos} | ordenado {r.Ordenado}");

// Pódio: menos passos = chegou antes.
var podio = CorridaDeOrdenacoes.Podio(resultados)
    .Select((r, i) => $"{i + 1}º {r.Nome} ({r.Passos})");
Console.WriteLine();
Console.WriteLine("Chegada: " + string.Join(" · ", podio));
