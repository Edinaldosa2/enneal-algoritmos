// =====================================================================
//  N-Rainhas 8x8 com backtracking
//  Enneal · @enneal.it · https://enneal.com.br
//
//  O desafio: colocar 8 rainhas num tabuleiro de xadrez 8x8 sem que
//  nenhuma ataque outra (mesma linha, mesma coluna ou mesma diagonal).
//
//  A ideia do backtracking ("tentar e voltar"):
//    1. coloque UMA rainha por linha, de cima para baixo;
//    2. em cada linha, teste as colunas da esquerda para a direita;
//    3. se a casa é segura, coloque a rainha e vá para a próxima linha;
//    4. se nenhuma coluna da linha serve, VOLTE: tire a rainha da linha
//       anterior e continue testando a partir da coluna seguinte dela.
//
//  É o mesmo algoritmo do Reel. Os contadores "tentativas" e "voltas"
//  impressos aqui são os mesmos que aparecem na tela do vídeo.
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;
using static System.Math;

Console.OutputEncoding = Encoding.UTF8;

const int N = 8; // tamanho do tabuleiro (troque para 4, 5, 6... e veja o que muda)

// rainhas[linha] = coluna onde está a rainha daquela linha (-1 = linha vazia).
// Como só pode existir uma rainha por linha, um vetor de N posições
// representa o tabuleiro inteiro.
int[] rainhas = Enumerable.Repeat(-1, N).ToArray();

int tentativas = 0; // quantas casas foram testadas (cada chamada de Seguro)
int voltas = 0;     // quantas vezes uma rainha foi retirada (o "backtrack")

Console.WriteLine($"N-Rainhas {N}x{N} (backtracking)");
Console.WriteLine();

if (Resolver(0))
{
    Console.WriteLine("Solução (coluna da rainha em cada linha, de cima para baixo):");
    Console.WriteLine(string.Join(" ", rainhas)); // 0 4 7 5 2 6 1 3
    Console.WriteLine();
    Console.WriteLine($"tentativas {tentativas} | voltas {voltas}"); // 876 | 105
    Console.WriteLine();
    DesenharTabuleiro();
}
else
{
    Console.WriteLine("Não existe solução para esse tamanho de tabuleiro.");
}

// ---------------------------------------------------------------------
// Resolver(linha): tenta colocar rainhas da 'linha' até o fim do tabuleiro.
// Devolve true quando consegue completar todas as linhas.
// ---------------------------------------------------------------------
bool Resolver(int linha)
{
    // Caso base: passamos da última linha, então todas as N rainhas estão
    // posicionadas sem conflito. Achamos uma solução!
    if (linha == N) return true;

    for (int col = 0; col < N; col++)
    {
        // Casa atacada por alguma rainha de cima? Pula para a próxima coluna.
        if (!Seguro(linha, col)) continue;

        // Casa segura: coloca a rainha e tenta resolver o resto (recursão).
        rainhas[linha] = col;
        if (Resolver(linha + 1)) return true;

        // O resto não teve solução com a rainha aqui. Desfaz a escolha
        // (backtrack) e o laço segue para a próxima coluna.
        rainhas[linha] = -1; // volta
        voltas++;
    }

    // Nenhuma coluna desta linha funcionou: quem chamou vai ter que voltar.
    return false;
}

// ---------------------------------------------------------------------
// Seguro(l, c): a casa (linha l, coluna c) está livre de ataques?
// Só olhamos as linhas de cima (0 até l-1), porque as de baixo ainda
// estão vazias.
// ---------------------------------------------------------------------
bool Seguro(int l, int c)
{
    tentativas++;

    // rainhas[..l] = as rainhas já colocadas nas linhas 0..l-1.
    // Para cada uma (coluna q, linha i), existe conflito se:
    //   q == c                 -> mesma coluna;
    //   Abs(q - c) == l - i    -> mesma diagonal (andou tantas colunas
    //                             quanto andou linhas).
    // Se nenhuma rainha conflita (.Any() é false), a casa é segura.
    return !rainhas[..l]
        .Where((q, i) => q == c ||          // coluna
               Abs(q - c) == l - i)         // diagonal
        .Any();

    // A mesma verificação escrita com um laço comum, se preferir:
    //   for (int i = 0; i < l; i++)
    //       if (rainhas[i] == c || Abs(rainhas[i] - c) == l - i) return false;
    //   return true;
}

// ---------------------------------------------------------------------
// Desenha o tabuleiro no console: R = rainha, . = casa vazia.
// ---------------------------------------------------------------------
void DesenharTabuleiro()
{
    Console.WriteLine("    " + string.Join(" ", Enumerable.Range(0, N)));
    for (int linha = 0; linha < N; linha++)
    {
        var casas = Enumerable.Range(0, N).Select(col => rainhas[linha] == col ? "R" : ".");
        Console.WriteLine($"{linha,2}  {string.Join(" ", casas)}");
    }
}
