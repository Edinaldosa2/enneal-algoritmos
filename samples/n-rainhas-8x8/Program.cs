// =====================================================================
//  N-Rainhas 8x8 com backtracking
//  Enneal · @enneal.it · https://enneal.com.br
//
//  O desafio: colocar 8 rainhas num tabuleiro 8x8 sem que nenhuma ataque
//  outra (mesma linha, mesma coluna ou mesma diagonal).
//
//  O algoritmo está em src/Enneal.Algoritmos.NRainhas/ResolvedorNRainhas.cs,
//  todo comentado. Este programa chama o resolvedor e mostra o resultado
//  com os mesmos números do Reel: 876 tentativas e 105 voltas.
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;
using Enneal.Algoritmos.NRainhas;

Console.OutputEncoding = Encoding.UTF8;

const int N = 8; // tamanho do tabuleiro (troque para 4, 5, 6... e veja o que muda)

Console.WriteLine($"N-Rainhas {N}x{N} (backtracking)");
Console.WriteLine();

var resultado = ResolvedorNRainhas.Resolver(N);

if (resultado.Resolvido)
{
    Console.WriteLine("Solução (coluna da rainha em cada linha, de cima para baixo):");
    Console.WriteLine(string.Join(" ", resultado.Rainhas)); // 0 4 7 5 2 6 1 3
    Console.WriteLine();
    Console.WriteLine($"tentativas {resultado.Tentativas} | voltas {resultado.Voltas}"); // 876 | 105
    Console.WriteLine();
    DesenharTabuleiro(resultado.Rainhas);
}
else
{
    Console.WriteLine("Não existe solução para esse tamanho de tabuleiro.");
}

// Desenha o tabuleiro: R = rainha, . = casa vazia.
static void DesenharTabuleiro(IReadOnlyList<int> rainhas)
{
    int n = rainhas.Count;
    Console.WriteLine("    " + string.Join(" ", Enumerable.Range(0, n)));
    for (int linha = 0; linha < n; linha++)
    {
        var casas = Enumerable.Range(0, n).Select(col => rainhas[linha] == col ? "R" : ".");
        Console.WriteLine($"{linha,2}  {string.Join(" ", casas)}");
    }
}
