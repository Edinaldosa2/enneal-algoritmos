namespace Enneal.Algoritmos.Ordenacoes;

/// <summary>
/// Bubble Sort, Quick Sort e Merge Sort, exatamente como na corrida dos Reels.
/// Cada método ordena o vetor no lugar (crescente) e devolve quanto trabalho deu.
/// </summary>
public static class Ordenacao
{
    /// <summary>
    /// Bubble Sort: compara vizinhos e troca os que estão fora de ordem. A cada volta
    /// o maior número que sobrou "borbulha" até o fim. Para quando uma volta inteira
    /// passa sem troca. Tempo O(n²) no caso médio e no pior; O(n) se já estiver ordenado.
    /// </summary>
    /// <param name="v">Vetor a ordenar (é alterado).</param>
    /// <returns>Comparações, trocas e escritas feitas.</returns>
    public static ContadorDePassos BubbleSort(int[] v)
    {
        ArgumentNullException.ThrowIfNull(v);
        var c = new ContadorDePassos();

        int fim = v.Length - 1;
        bool trocou = true;
        while (trocou)
        {
            trocou = false;
            for (int j = 0; j < fim; j++)
            {
                if (c.Menor(v[j + 1], v[j])) // vizinho da direita é menor?
                {
                    c.Troca(v, j, j + 1);
                    trocou = true;
                }
            }
            fim--; // o último da volta já está no lugar certo
        }
        return c;
    }

    /// <summary>
    /// Quick Sort: escolhe um pivô (o último elemento do trecho), joga os menores
    /// para a esquerda (partição de Lomuto), põe o pivô no meio e repete em cada lado.
    /// Tempo O(n log n) em média e O(n²) no pior caso.
    /// </summary>
    /// <param name="v">Vetor a ordenar (é alterado).</param>
    /// <returns>Comparações, trocas e escritas feitas.</returns>
    public static ContadorDePassos QuickSort(int[] v)
    {
        ArgumentNullException.ThrowIfNull(v);
        var c = new ContadorDePassos();
        Quick(v, 0, v.Length - 1, c);
        return c;
    }

    /// <summary>
    /// Merge Sort: divide o trecho ao meio, ordena cada metade e intercala as duas,
    /// sempre pegando o menor da frente. Tempo O(n log n) sempre; memória extra O(n).
    /// É estável: valores iguais mantêm a ordem original.
    /// </summary>
    /// <param name="v">Vetor a ordenar (é alterado).</param>
    /// <returns>Comparações, trocas e escritas feitas.</returns>
    public static ContadorDePassos MergeSort(int[] v)
    {
        ArgumentNullException.ThrowIfNull(v);
        var c = new ContadorDePassos();
        var aux = new int[v.Length]; // memória extra usada na intercalação
        Merge(v, 0, v.Length - 1, aux, c);
        return c;
    }

    private static void Quick(int[] v, int ini, int fim, ContadorDePassos c)
    {
        if (ini >= fim) return; // trecho com 0 ou 1 elemento já está ordenado

        int pivo = v[fim], i = ini; // i = onde entra o próximo número menor que o pivô
        for (int j = ini; j < fim; j++)
        {
            if (c.Menor(v[j], pivo))
                c.Troca(v, i++, j);
        }
        c.Troca(v, i, fim); // pivô vai para a posição final dele

        Quick(v, ini, i - 1, c); // ordena os menores
        Quick(v, i + 1, fim, c); // ordena os maiores
    }

    private static void Merge(int[] v, int ini, int fim, int[] aux, ContadorDePassos c)
    {
        if (ini >= fim) return;

        int meio = (ini + fim) / 2;
        Merge(v, ini, meio, aux, c);     // ordena a metade esquerda
        Merge(v, meio + 1, fim, aux, c); // ordena a metade direita

        // Copia o trecho para 'aux' e intercala de volta em 'v'.
        v[ini..(fim + 1)].CopyTo(aux, ini);
        int i = ini, j = meio + 1; // i anda na metade esquerda, j na direita
        for (int k = ini; k <= fim; k++)
        {
            // Pega da esquerda se a direita acabou, ou se a esquerda ainda tem
            // números e o da direita NÃO é menor (isso mantém a ordem estável).
            if (j > fim || (i <= meio && !c.Menor(aux[j], aux[i])))
                c.Escreve(v, k, aux[i++]);
            else
                c.Escreve(v, k, aux[j++]);
        }
    }
}
