// =====================================================================
//  Corrida de ordenações: Bubble x Quick x Merge
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Os três algoritmos ordenam o MESMO vetor embaralhado de 28 números
//  (1 a 28) e contamos o trabalho de cada um.
//
//  Regra da corrida (a mesma do Reel):
//    1 passo = 1 comparação  OU  1 troca  OU  1 escrita no vetor.
//  Quem precisar de menos passos para deixar o vetor ordenado, vence.
//
//  Resultado esperado (o mesmo do Reel):
//    1º Quick (185)  ·  2º Merge (235)  ·  3º Bubble (536)
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;

Console.OutputEncoding = Encoding.UTF8;

// Os números de 1 a 28, embaralhados UMA vez com semente fixa (2026) e
// escritos aqui. Assim todo mundo roda a corrida com o mesmo vetor do vídeo.
int[] embaralhado = [13, 9, 7, 3, 25, 6, 19, 22, 27, 5, 12, 2, 10, 1,
                     23, 15, 16, 14, 18, 24, 20, 8, 28, 21, 26, 17, 11, 4];

int[] aux = new int[embaralhado.Length]; // memória extra que o Merge Sort usa

// Contadores da corrida (zerados antes de cada algoritmo).
int cmp = 0, trocas = 0, escritas = 0;
var chegada = new List<(string Nome, int Passos)>();

Console.WriteLine("Vetor inicial: " + string.Join(" ", embaralhado));
Console.WriteLine();

Correr("Bubble", Bubble);
Correr("Quick", v => Quick(v, 0, v.Length - 1));
Correr("Merge", v => Merge(v, 0, v.Length - 1));

// Pódio: ordena pelo número de passos (menos passos = chegou antes).
var podio = chegada
    .OrderBy(c => c.Passos)
    .Select((c, i) => $"{i + 1}º {c.Nome} ({c.Passos})");
Console.WriteLine();
Console.WriteLine("Chegada: " + string.Join(" · ", podio));

// ---------------------------------------------------------------------
// Correr: dá a cada algoritmo uma CÓPIA do vetor embaralhado, ordena,
// confere se ficou certo e anota os passos.
// ---------------------------------------------------------------------
void Correr(string nome, Action<int[]> ordenar)
{
    var v = (int[])embaralhado.Clone();
    cmp = trocas = escritas = 0;

    ordenar(v);

    bool ok = v.SequenceEqual(Enumerable.Range(1, v.Length)); // 1, 2, 3, ..., 28?
    int passos = cmp + trocas + escritas;
    chegada.Add((nome, passos));
    Console.WriteLine($"{nome}: comparações {cmp} | trocas {trocas} | escritas {escritas} | passos {passos} | ordenado {ok}");
}

// Operações básicas. Todo algoritmo usa SÓ estas três, então a contagem é justa.
bool Menor(int a, int b) { cmp++; return a < b; }                      // 1 comparação
void Troca(int[] v, int a, int b) { (v[a], v[b]) = (v[b], v[a]); trocas++; } // 1 troca
void Escreve(int[] v, int k, int x) { v[k] = x; escritas++; }            // 1 escrita

// =====================================================================
// Bubble Sort - O(n²)
// Percorre o vetor comparando vizinhos e trocando os que estão fora de
// ordem. A cada volta, o maior número que sobrou "borbulha" até o fim,
// então a parte final já fica pronta e não precisa ser revisitada.
// Para quando uma volta inteira passa sem nenhuma troca.
// =====================================================================
void Bubble(int[] v)
{
    int fim = v.Length - 1;
    bool trocou = true;
    while (trocou)
    {
        trocou = false;
        for (int j = 0; j < fim; j++)
        {
            if (Menor(v[j + 1], v[j])) // vizinho da direita é menor?
            {
                Troca(v, j, j + 1);
                trocou = true;
            }
        }
        fim--; // o último da volta já está no lugar certo
    }
}

// =====================================================================
// Quick Sort - O(n log n) em média, O(n²) no pior caso
// Escolhe um pivô (aqui, o último elemento do trecho), joga os menores
// que ele para a esquerda (partição de Lomuto) e põe o pivô no meio.
// Depois ordena, do mesmo jeito, o lado esquerdo e o lado direito.
// =====================================================================
void Quick(int[] v, int ini, int fim)
{
    if (ini >= fim) return; // trecho com 0 ou 1 elemento já está ordenado

    int pivo = v[fim], i = ini; // i = onde entra o próximo número menor que o pivô
    for (int j = ini; j < fim; j++)
    {
        if (Menor(v[j], pivo))
            Troca(v, i++, j);
    }
    Troca(v, i, fim); // pivô vai para a posição final dele

    Quick(v, ini, i - 1); // ordena os menores
    Quick(v, i + 1, fim); // ordena os maiores
}

// =====================================================================
// Merge Sort - O(n log n) sempre, usa memória extra O(n)
// Divide o trecho ao meio, ordena cada metade (recursão) e depois
// INTERCALA as duas metades já ordenadas, sempre pegando o menor da frente.
// =====================================================================
void Merge(int[] v, int ini, int fim)
{
    if (ini >= fim) return;

    int meio = (ini + fim) / 2;
    Merge(v, ini, meio);     // ordena a metade esquerda
    Merge(v, meio + 1, fim); // ordena a metade direita

    // Copia o trecho para 'aux' e intercala de volta em 'v'.
    v[ini..(fim + 1)].CopyTo(aux, ini);
    int i = ini, j = meio + 1; // i anda na metade esquerda, j na direita
    for (int k = ini; k <= fim; k++)
    {
        // Pega da esquerda se a direita acabou, ou se a esquerda ainda tem
        // números e o da direita NÃO é menor (isso mantém a ordem estável).
        if (j > fim || (i <= meio && !Menor(aux[j], aux[i])))
            Escreve(v, k, aux[i++]);
        else
            Escreve(v, k, aux[j++]);
    }
}
