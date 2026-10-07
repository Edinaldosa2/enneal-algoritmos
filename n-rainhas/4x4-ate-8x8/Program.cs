// =====================================================================
//  N-Rainhas de 4x4 até 8x8 com backtracking
//  Enneal · @enneal.it · https://enneal.com.br
//
//  O mesmo algoritmo da versão básica (pasta ../basico), rodando em cinco
//  tabuleiros seguidos: 4x4, 5x5, 6x6, 7x7 e 8x8. Para cada um, o programa
//  para na PRIMEIRA solução encontrada e mostra quanto trabalho deu.
//
//  Curiosidade que o Reel mostra: tabuleiro maior nem sempre é mais
//  difícil. O 5x5 sai sem nenhuma volta, e o 6x6 dá mais trabalho que o 7x7.
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;
using static System.Math;

Console.OutputEncoding = Encoding.UTF8;

// Estado do tabuleiro atual. Fica fora do laço porque Resolver e Seguro
// (funções locais lá embaixo) leem e alteram essas variáveis.
int N = 4;             // tamanho do tabuleiro da rodada atual
int tentativas = 0;    // casas testadas (cada chamada de Seguro)
int voltas = 0;        // rainhas retiradas (backtrack)
int[] rainhas = [];    // rainhas[linha] = coluna da rainha (-1 = vazia)

var resumo = new List<(int N, int Tentativas, int Voltas)>();

for (N = 4; N <= 8; N++)
{
    // Tabuleiro novo, vazio, e contadores zerados.
    rainhas = Enumerable.Repeat(-1, N).ToArray();
    tentativas = 0;
    voltas = 0;

    Resolver(0);

    // Mesma linha de saída do programa mostrado no Reel:
    Console.WriteLine($"{N}x{N}: {string.Join(" ", rainhas)} | tentativas {tentativas} | voltas {voltas}");
    DesenharTabuleiro();
    Console.WriteLine();

    resumo.Add((N, tentativas, voltas));
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

// ---------------------------------------------------------------------
// Resolver(linha): coloca rainhas da 'linha' até a última.
// Devolve true quando o tabuleiro inteiro foi preenchido sem conflito.
// ---------------------------------------------------------------------
bool Resolver(int linha)
{
    if (linha == N) return true; // todas as linhas têm rainha: solução!

    for (int col = 0; col < N; col++)
    {
        if (!Seguro(linha, col)) continue; // casa atacada: próxima coluna

        rainhas[linha] = col;                 // coloca a rainha
        if (Resolver(linha + 1)) return true; // e tenta o resto

        rainhas[linha] = -1; // o resto falhou: tira a rainha (volta)
        voltas++;
    }
    return false; // nenhuma coluna serviu: a linha de cima precisa mudar
}

// ---------------------------------------------------------------------
// Seguro(l, c): nenhuma rainha das linhas de cima ataca a casa (l, c)?
//   q == c               -> mesma coluna
//   Abs(q - c) == l - i  -> mesma diagonal
// ---------------------------------------------------------------------
bool Seguro(int l, int c)
{
    tentativas++;
    return !rainhas[..l]
        .Where((q, i) => q == c || Abs(q - c) == l - i)
        .Any();
}

// Desenha o tabuleiro atual: R = rainha, . = casa vazia.
void DesenharTabuleiro()
{
    for (int linha = 0; linha < N; linha++)
    {
        var casas = Enumerable.Range(0, N).Select(col => rainhas[linha] == col ? "R" : ".");
        Console.WriteLine("   " + string.Join(" ", casas));
    }
}
