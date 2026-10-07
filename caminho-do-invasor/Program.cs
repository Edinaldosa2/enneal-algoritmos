// =====================================================================
//  Caminho do invasor: busca em largura (BFS) e defesa em camadas
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Demo educativa de DEFESA EM CAMADAS. O "invasor" é só uma busca num
//  mapa abstrato deste programa. Nada de rede, IPs, ferramentas ou alvos
//  reais.
//
//  O mapa é uma grade: o invasor começa na Internet (I) e procura
//  QUALQUER caminho até o banco de dados (B), andando para cima, baixo,
//  esquerda e direita, sem atravessar paredes (#).
//
//  Round 1 (sem defesas):  o invasor chega no banco em 93 passos.
//  Round 2 (em camadas):   4 camadas, cada uma com furos, mas os furos
//                          não se alinham -> 0 caminhos até o banco
//                          (48 células exploradas).
//
//  "Passo" = uma célula tirada da fila e examinada pela busca. É o mesmo
//  contador que aparece no Reel.
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;

Console.OutputEncoding = Encoding.UTF8;

// Legenda:  I = Internet (início)   B = banco de dados   # = parede   . = livre
string[] semDefesas =
{
    "......I......",
    ".............",
    ".##..###..##.",
    ".............",
    "...#.....#...",   // firewall: sem WAF
    ".#....#....#.",   // login: sem MFA
    "....#...#....",   // app: acesso total
    ".............",   // rede do banco: aberta
    "......B......",
};
string[] emCamadas =
{
    "......I......",
    ".............",
    ".##..###..##.",
    ".............",
    "##.######.###",   // camada 1: WAF (firewall de aplicação web)
    "####.####.###",   // camada 2: MFA no login
    "#.#########.#",   // camada 3: menor privilégio
    "#######.#####",   // camada 4: criptografia + segmentação de rede
    "......B......",
};
// Repare: cada camada TEM furos (nenhuma defesa é perfeita), mas os furos
// de uma camada não se alinham com os da próxima. É isso que barra o invasor.

const int L = 9, C = 13; // linhas e colunas do mapa
const int LinhaInicio = 0, ColunaInicio = 6; // posição do I

string[] mapa = semDefesas; // mapa da rodada atual
int passos = 0;             // células examinadas pela busca

// Guardados pela busca para podermos desenhar o resultado depois:
HashSet<(int, int)> visto = [];
Dictionary<(int, int), (int, int)> veioDe = []; // célula -> de onde a busca chegou nela
(int, int)? banco = null;

// ---------------------------------------------------------------------
// Round 1
// ---------------------------------------------------------------------
Console.WriteLine("== Round 1: rede SEM defesas ==");
bool invadiu = Invadir(LinhaInicio, ColunaInicio);
Console.WriteLine($"sem defesas: {(invadiu ? "INVADIDO" : "PROTEGIDO")} em {passos} passos");
if (banco is (int, int) alvo)
{
    var caminho = Caminho(alvo);
    Console.WriteLine($"(caminho mais curto até o banco: {caminho.Count - 1} movimentos, marcado com *)");
    Desenhar(caminho);
}
Console.WriteLine();

// ---------------------------------------------------------------------
// Round 2
// ---------------------------------------------------------------------
Console.WriteLine("== Round 2: rede com defesa EM CAMADAS ==");
mapa = emCamadas;
passos = 0;
invadiu = Invadir(LinhaInicio, ColunaInicio);
Console.WriteLine(invadiu
    ? $"em camadas: INVADIDO em {passos} passos"
    : $"em camadas: PROTEGIDO, 0 caminhos até o banco ({passos} células exploradas)");
Console.WriteLine("(o = células que o invasor conseguiu alcançar)");
Desenhar([]);

// =====================================================================
// Invadir: busca em largura (BFS) a partir de (r0, c0).
// A BFS explora o mapa em "ondas": primeiro todas as células a 1 passo
// do início, depois todas a 2 passos, e assim por diante. Por isso, se
// existe caminho, ela sempre encontra o MAIS CURTO.
// =====================================================================
bool Invadir(int r0, int c0)
{
    Queue<(int, int)> fila = new([(r0, c0)]); // células a examinar (FIFO)
    visto = [(r0, c0)];                      // células que já entraram na fila
    veioDe = [];
    banco = null;

    while (fila.Count > 0)
    {
        var (r, c) = fila.Dequeue();
        passos++;

        if (mapa[r][c] == 'B') // chegou no banco de dados
        {
            banco = (r, c);
            return true;
        }

        foreach (var (nr, nc) in Vizinhos(r, c))
        {
            if (mapa[nr][nc] == '#') continue;  // parede: não passa
            if (!visto.Add((nr, nc))) continue; // já visitada: não repete
            veioDe[(nr, nc)] = (r, c);
            fila.Enqueue((nr, nc));
        }
    }

    // A fila esvaziou sem encontrar o B: não existe caminho.
    return false; // 0 caminhos até o banco
}

// Vizinhos dentro do mapa, na ordem: baixo, direita, esquerda, cima.
IEnumerable<(int, int)> Vizinhos(int r, int c)
{
    foreach (var (dr, dc) in new[] { (1, 0), (0, 1), (0, -1), (-1, 0) })
        if (r + dr >= 0 && r + dr < L && c + dc >= 0 && c + dc < C)
            yield return (r + dr, c + dc);
}

// Refaz o caminho do banco até o início seguindo "veioDe" de trás pra frente.
List<(int, int)> Caminho((int, int) fim)
{
    var caminho = new List<(int, int)> { fim };
    while (veioDe.TryGetValue(caminho[^1], out var anterior))
        caminho.Add(anterior);
    caminho.Reverse();
    return caminho;
}

// Desenha o mapa: * = caminho do invasor, o = célula alcançada (sem caminho).
void Desenhar(List<(int, int)> caminho)
{
    var noCaminho = caminho.ToHashSet();
    for (int r = 0; r < L; r++)
    {
        var linha = new StringBuilder("   ");
        for (int c = 0; c < C; c++)
        {
            char ch = mapa[r][c];
            if (ch == '.' && noCaminho.Contains((r, c))) ch = '*';
            else if (ch == '.' && caminho.Count == 0 && visto.Contains((r, c))) ch = 'o';
            linha.Append(ch).Append(' ');
        }
        Console.WriteLine(linha.ToString().TrimEnd());
    }
}
