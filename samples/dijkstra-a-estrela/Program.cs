// =====================================================================
//  Como o GPS acha a rota: Dijkstra x A* (A-estrela)
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Mapa de cidade simplificado (grade). Andar numa rua custa 1, no
//  trânsito custa 3 e no engarrafamento custa 6. Prédios e a praça
//  bloqueiam. As duas buscas acham a rota mais barata; o A* só muda a
//  prioridade da fila: custo + heurística (distância até o destino).
//
//  As buscas estão em src/Enneal.Algoritmos.MenorCaminho/BuscaDeRota.cs.
//  Números do Reel:
//    Dijkstra -> rota de custo 18, 89 nós explorados
//    A*       -> rota de custo 18, 27 nós explorados (70% menos)
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;
using Enneal.Algoritmos.MenorCaminho;

Console.OutputEncoding = Encoding.UTF8;

var mapa = MapaDaCidade.DoReel;

var dijkstra = BuscaDeRota.Dijkstra(mapa);
var aEstrela = BuscaDeRota.AEstrela(mapa);

// Mesmas linhas do programa mostrado no Reel:
Console.WriteLine($"Dijkstra: rota de custo {dijkstra.Custo}, {dijkstra.NosExplorados} nós explorados");
Console.WriteLine($"A*: rota de custo {aEstrela.Custo}, {aEstrela.NosExplorados} nós explorados");
Console.WriteLine($"A* explorou {BuscaDeRota.PorcentagemAMenos(dijkstra.NosExplorados, aEstrela.NosExplorados)}% menos");
Console.WriteLine($"rota Dijkstra: {string.Join(" ", dijkstra.Rota)}");
Console.WriteLine($"rota A*: {string.Join(" ", aEstrela.Rota)}");

Console.WriteLine();
Console.WriteLine("Legenda: * = rota   o = esquina explorada   m = trânsito   T = engarrafamento   # = prédio   P = praça");
Console.WriteLine();
Console.WriteLine($"Dijkstra ({dijkstra.NosExplorados} nós explorados):");
Desenhar(mapa, dijkstra);
Console.WriteLine();
Console.WriteLine($"A* ({aEstrela.NosExplorados} nós explorados):");
Desenhar(mapa, aEstrela);

// Desenha o mapa marcando a rota (*) e as esquinas exploradas (o).
static void Desenhar(IReadOnlyList<string> mapa, ResultadoRota resultado)
{
    var naRota = resultado.Rota.ToHashSet();
    for (int l = 0; l < mapa.Count; l++)
    {
        var linha = new StringBuilder("   ");
        for (int c = 0; c < mapa[l].Length; c++)
        {
            var e = new Esquina(l, c);
            char ch = mapa[l][c];
            if (ch is not ('A' or 'B'))
            {
                if (naRota.Contains(e)) ch = '*';
                else if (ch == '.' && resultado.Explorados.Contains(e)) ch = 'o';
            }
            linha.Append(ch).Append(' ');
        }
        Console.WriteLine(linha.ToString().TrimEnd());
    }
}
