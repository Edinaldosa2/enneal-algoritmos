// =====================================================================
//  N-Rainhas de 4x4 até 8x8 com backtracking
//  Enneal · @enneal.it · https://enneal.com.br
//
//  O mesmo algoritmo do exemplo 8x8, rodando em cinco tabuleiros seguidos.
//  Para cada um, para na PRIMEIRA solução e mostra quanto trabalho deu.
//
//  Curiosidade que o Reel mostra: tabuleiro maior nem sempre é mais
//  difícil. O 5x5 sai sem nenhuma volta, e o 6x6 dá mais trabalho que o 7x7.
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;
using Enneal.Algoritmos.NRainhas;

Console.OutputEncoding = Encoding.UTF8;

var resumo = new List<ResultadoNRainhas>();

for (int n = 4; n <= 8; n++)
{
    var r = ResolvedorNRainhas.Resolver(n);

    // Mesma linha de saída do programa mostrado no Reel:
    Console.WriteLine($"{n}x{n}: {string.Join(" ", r.Rainhas)} | tentativas {r.Tentativas} | voltas {r.Voltas}");
    DesenharTabuleiro(r.Rainhas);
    Console.WriteLine();

    resumo.Add(r);
}

// Ranking do mais fácil ao mais difícil (por número de tentativas).
Console.WriteLine("Do mais fácil ao mais difícil (tentativas até a 1ª solução):");
foreach (var r in resumo.OrderBy(r => r.Tentativas))
    Console.WriteLine($"  {r.N}x{r.N}: {r.Tentativas,4} tentativas, {r.Voltas,3} voltas");

// Esperado:
//   4x4: 1 3 0 2         | tentativas  26 | voltas   4
//   5x5: 0 2 4 1 3       | tentativas  15 | voltas   0
//   6x6: 1 3 5 0 2 4     | tentativas 171 | voltas  25
//   7x7: 0 2 4 6 1 3 5   | tentativas  42 | voltas   2
//   8x8: 0 4 7 5 2 6 1 3 | tentativas 876 | voltas 105

// Desenha o tabuleiro: R = rainha, . = casa vazia.
static void DesenharTabuleiro(IReadOnlyList<int> rainhas)
{
    int n = rainhas.Count;
    for (int linha = 0; linha < n; linha++)
    {
        var casas = Enumerable.Range(0, n).Select(col => rainhas[linha] == col ? "R" : ".");
        Console.WriteLine("   " + string.Join(" ", casas));
    }
}
