using static System.Math;

namespace Enneal.Algoritmos.NRainhas;

/// <summary>
/// Problema das N rainhas resolvido com backtracking ("tentar e voltar").
/// </summary>
/// <remarks>
/// <para>
/// O desafio: colocar N rainhas num tabuleiro N x N sem que nenhuma ataque outra
/// (mesma linha, mesma coluna ou mesma diagonal).
/// </para>
/// <para>
/// A ideia: coloca UMA rainha por linha, de cima para baixo, testando as colunas da
/// esquerda para a direita. Se nenhuma coluna da linha serve, volta, tira a rainha da
/// linha anterior e continua a partir da coluna seguinte dela.
/// </para>
/// <para>
/// É o mesmo algoritmo dos Reels: com N = 8 a primeira solução sai com
/// 876 tentativas e 105 voltas.
/// </para>
/// </remarks>
public static class ResolvedorNRainhas
{
    /// <summary>
    /// Procura a PRIMEIRA solução para um tabuleiro <paramref name="n"/> x <paramref name="n"/>
    /// e conta o esforço da busca.
    /// </summary>
    /// <param name="n">Tamanho do tabuleiro (0 ou mais).</param>
    /// <returns>A solução encontrada (ou -1 em todas as linhas, se não existir) e os contadores.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se <paramref name="n"/> for negativo.</exception>
    public static ResultadoNRainhas Resolver(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        var busca = new Busca(n);
        busca.Resolver(0);
        return new ResultadoNRainhas(n, busca.Rainhas, busca.Tentativas, busca.Voltas);
    }

    /// <summary>
    /// Conta TODAS as soluções de um tabuleiro <paramref name="n"/> x <paramref name="n"/>
    /// (o 8x8 tem 92). Usa o mesmo backtracking, mas sem parar na primeira.
    /// </summary>
    /// <param name="n">Tamanho do tabuleiro (0 ou mais).</param>
    /// <returns>Quantidade de soluções.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Se <paramref name="n"/> for negativo.</exception>
    public static int ContarSolucoes(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        var busca = new Busca(n);
        return busca.ContarTodas(0);
    }

    /// <summary>
    /// Confere se um vetor <c>rainhas[linha] = coluna</c> é uma solução válida:
    /// todas as linhas preenchidas e nenhuma rainha atacando outra.
    /// </summary>
    /// <param name="rainhas">Coluna da rainha em cada linha.</param>
    /// <returns><see langword="true"/> se for uma solução completa e sem ataques.</returns>
    public static bool SolucaoValida(IReadOnlyList<int> rainhas)
    {
        ArgumentNullException.ThrowIfNull(rainhas);

        int n = rainhas.Count;
        for (int l = 0; l < n; l++)
        {
            if (rainhas[l] < 0 || rainhas[l] >= n) return false;
            for (int i = 0; i < l; i++)
            {
                if (rainhas[i] == rainhas[l]) return false;                 // mesma coluna
                if (Abs(rainhas[i] - rainhas[l]) == l - i) return false;    // mesma diagonal
            }
        }
        return true;
    }

    /// <summary>
    /// Estado de uma busca: o tabuleiro e os contadores.
    /// </summary>
    private sealed class Busca(int n)
    {
        // rainhas[linha] = coluna da rainha daquela linha (-1 = linha vazia).
        // Como só pode existir uma rainha por linha, um vetor de N posições
        // representa o tabuleiro inteiro.
        public int[] Rainhas { get; } = Enumerable.Repeat(-1, n).ToArray();

        public int Tentativas { get; private set; } // casas testadas (cada chamada de Seguro)
        public int Voltas { get; private set; }     // rainhas retiradas (backtrack)

        // Resolver(linha): coloca rainhas da 'linha' até a última.
        // Devolve true quando o tabuleiro inteiro foi preenchido sem conflito.
        public bool Resolver(int linha)
        {
            // Caso base: passamos da última linha, então as N rainhas estão
            // posicionadas sem conflito. Achamos uma solução!
            if (linha == n) return true;

            for (int col = 0; col < n; col++)
            {
                // Casa atacada por alguma rainha de cima? Pula para a próxima coluna.
                if (!Seguro(linha, col)) continue;

                // Casa segura: coloca a rainha e tenta resolver o resto (recursão).
                Rainhas[linha] = col;
                if (Resolver(linha + 1)) return true;

                // O resto não teve solução com a rainha aqui: desfaz a escolha
                // (backtrack) e o laço segue para a próxima coluna.
                Rainhas[linha] = -1; // volta
                Voltas++;
            }

            // Nenhuma coluna desta linha funcionou: quem chamou vai ter que voltar.
            return false;
        }

        // Igual ao Resolver, mas conta todas as soluções em vez de parar na primeira.
        public int ContarTodas(int linha)
        {
            if (linha == n) return 1;

            int total = 0;
            for (int col = 0; col < n; col++)
            {
                if (!Seguro(linha, col)) continue;
                Rainhas[linha] = col;
                total += ContarTodas(linha + 1);
                Rainhas[linha] = -1;
            }
            return total;
        }

        // Seguro(l, c): nenhuma rainha das linhas de cima ataca a casa (l, c)?
        // Só olhamos as linhas 0..l-1, porque as de baixo ainda estão vazias.
        private bool Seguro(int l, int c)
        {
            Tentativas++;

            // Rainhas[..l] = as rainhas já colocadas. Para cada uma (coluna q, linha i):
            //   q == c                -> mesma coluna;
            //   Abs(q - c) == l - i   -> mesma diagonal (andou tantas colunas
            //                            quanto andou linhas).
            // Se nenhuma conflita (.Any() é false), a casa é segura.
            return !Rainhas[..l]
                .Where((q, i) => q == c ||          // coluna
                       Abs(q - c) == l - i)         // diagonal
                .Any();
        }
    }
}
