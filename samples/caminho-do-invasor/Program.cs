// =====================================================================
//  Caminho do invasor: busca em largura (BFS) e defesa em camadas
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Demo educativa de DEFESA EM CAMADAS. O "invasor" é só uma busca num
//  mapa abstrato. Nada de rede, IPs, ferramentas ou alvos reais.
//
//  A busca está em src/Enneal.Algoritmos.CaminhoInvasor/BuscaInvasor.cs
//  e os mapas em MapasDoReel.cs. Números do Reel:
//    sem defesas -> INVADIDO em 93 passos
//    em camadas  -> PROTEGIDO, 0 caminhos até o banco (48 células exploradas)
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;
using Enneal.Algoritmos.CaminhoInvasor;

Console.OutputEncoding = Encoding.UTF8;

// ---- Round 1 ----
Console.WriteLine("== Round 1: rede SEM defesas ==");
var semDefesas = BuscaInvasor.Invadir(MapasDoReel.SemDefesas);
Console.WriteLine($"sem defesas: {(semDefesas.Invadido ? "INVADIDO" : "PROTEGIDO")} em {semDefesas.Passos} passos");
if (semDefesas.Invadido)
{
    Console.WriteLine($"(caminho mais curto até o banco: {semDefesas.Caminho.Count - 1} movimentos, marcado com *)");
    Desenhar(MapasDoReel.SemDefesas, semDefesas, marcarCaminho: true);
}
Console.WriteLine();

// ---- Round 2 ----
Console.WriteLine("== Round 2: rede com defesa EM CAMADAS ==");
var emCamadas = BuscaInvasor.Invadir(MapasDoReel.EmCamadas);
Console.WriteLine(emCamadas.Invadido
    ? $"em camadas: INVADIDO em {emCamadas.Passos} passos"
    : $"em camadas: PROTEGIDO, 0 caminhos até o banco ({emCamadas.Passos} células exploradas)");
Console.WriteLine("(o = células que o invasor conseguiu alcançar)");
Desenhar(MapasDoReel.EmCamadas, emCamadas, marcarCaminho: false);

// Desenha o mapa: * = caminho do invasor, o = célula alcançada.
static void Desenhar(IReadOnlyList<string> mapa, ResultadoInvasao resultado, bool marcarCaminho)
{
    var noCaminho = resultado.Caminho.ToHashSet();
    for (int l = 0; l < mapa.Count; l++)
    {
        var linha = new StringBuilder("   ");
        for (int c = 0; c < mapa[l].Length; c++)
        {
            char ch = mapa[l][c];
            var celula = new Celula(l, c);
            if (ch == '.' && marcarCaminho && noCaminho.Contains(celula)) ch = '*';
            else if (ch == '.' && !marcarCaminho && resultado.Alcancadas.Contains(celula)) ch = 'o';
            linha.Append(ch).Append(' ');
        }
        Console.WriteLine(linha.ToString().TrimEnd());
    }
}
