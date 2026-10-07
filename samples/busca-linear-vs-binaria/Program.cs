// =====================================================================
//  Busca linear x busca binária
//  Enneal · @enneal.it · https://enneal.com.br
//
//  As duas buscas procuram o MESMO alvo no MESMO vetor ordenado de 1.024
//  itens (semente fixa), em três rodadas. Depois, a conta para 1 milhão.
//
//  As buscas estão em src/Enneal.Algoritmos.Busca/Buscas.cs. Números do Reel:
//    rodada 1 (alvo 19)    -> linear 7    x binária 10
//    rodada 2 (alvo 1565)  -> linear 613  x binária 10
//    rodada 3 (último)     -> linear 1024 x binária 11
//    1 milhão de itens     -> linear até 1.000.000 x binária até 20 (média 18,95)
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run                -> relatório das 3 rodadas e das escalas
//    dotnet run -- --eventos   -> também imprime cada comparação
// =====================================================================

using System.Text;
using Enneal.Algoritmos.Busca;

Console.OutputEncoding = Encoding.UTF8;

bool mostrarEventos = args.Contains("--eventos");

int[] v = VetorOrdenado.Gerar(1024);
int[] posicoes = [6, 612, 1023]; // rodada 3 = último item: o pior caso da linear

for (int r = 0; r < posicoes.Length; r++)
{
    int alvo = v[posicoes[r]];
    string rodada = $"R{r + 1}";

    var linear = Buscas.Linear(v, alvo, Registrar(rodada, "L"));
    var binaria = Buscas.Binaria(v, alvo, Registrar(rodada, "B"));

    Console.WriteLine($"Rodada {r + 1}: alvo {alvo} | linear {linear.Comparacoes} comparações (posição {linear.Posicao}) | " +
                      $"binária {binaria.Comparacoes} comparações (posição {binaria.Posicao})");
}

// A mesma conta para vetores maiores: a linear cresce junto com o vetor,
// a binária quase não sai do lugar (cada comparação corta o vetor pela metade).
foreach (int n in new[] { 1024, 1_000_000 })
{
    int[] w = n == v.Length ? v : VetorOrdenado.Gerar(n);
    var e = Escala.Medir(w);
    Console.WriteLine($"{e.Itens} itens: linear até {e.LinearPior} | binária até {e.BinariaPior} (média {e.BinariaMedia:0.00} achando cada item)");
}

// Com --eventos, imprime cada comparação: rodada, L/B, número da comparação, item e alvo.
Action<int, int>? Registrar(string rodada, string lado)
{
    if (!mostrarEventos) return null;
    long passo = 0;
    return (item, alvo) => Console.WriteLine($"{rodada} {lado} {++passo} {item} {alvo}");
}
