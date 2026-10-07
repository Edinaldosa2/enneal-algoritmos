using Enneal.Algoritmos.MenorCaminho;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class MenorCaminhoTests
{
    private static readonly string RotaDoReel =
        "4,0 4,1 4,2 4,3 4,4 4,5 3,5 3,6 3,7 3,8 3,9 3,10 4,10 4,11 4,12 4,13 4,14 4,15 4,16";

    [Fact]
    public void Dijkstra_Custo18Com89NosExplorados()
    {
        var r = BuscaDeRota.Dijkstra(MapaDaCidade.DoReel);

        Assert.Equal(18, r.Custo);
        Assert.Equal(89, r.NosExplorados);
        Assert.Equal(RotaDoReel, string.Join(" ", r.Rota));
    }

    [Fact]
    public void AEstrela_MesmoCusto18ComSo27NosExplorados()
    {
        var r = BuscaDeRota.AEstrela(MapaDaCidade.DoReel);

        Assert.Equal(18, r.Custo);
        Assert.Equal(27, r.NosExplorados);
        Assert.Equal(RotaDoReel, string.Join(" ", r.Rota));
    }

    [Fact]
    public void AEstrela_Explorou70PorCentoMenos()
    {
        Assert.Equal(70, BuscaDeRota.PorcentagemAMenos(89, 27));
    }

    [Fact]
    public void ARota_DesviaDoEngarrafamentoEOCustoBateComOsPesos()
    {
        var mapa = MapaDaCidade.DoReel;
        var r = BuscaDeRota.Dijkstra(mapa);

        Assert.DoesNotContain(r.Rota, e => mapa[e.Linha][e.Coluna] == 'T');
        int custo = r.Rota.Skip(1).Sum(e => MapaDaCidade.Peso(mapa[e.Linha][e.Coluna]));
        Assert.Equal(r.Custo, custo);
    }

    public static TheoryData<int> Sementes() => new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    [Theory]
    [MemberData(nameof(Sementes))]
    public void EmMapasAleatorios_AEstrelaAchaOMesmoCustoQueDijkstraEExploraNoMaximoIgual(int semente)
    {
        var mapa = MapaAleatorio(semente, linhas: 12, colunas: 20);

        var dijkstra = BuscaDeRota.Dijkstra(mapa);
        var aEstrela = BuscaDeRota.AEstrela(mapa);

        Assert.Equal(dijkstra.Custo, aEstrela.Custo);
        Assert.True(aEstrela.NosExplorados <= dijkstra.NosExplorados);
    }

    [Fact]
    public void SemRota_DevolveCustoMenosUm()
    {
        var r = BuscaDeRota.Dijkstra(["A#B"]);

        Assert.False(r.Encontrada);
        Assert.Equal(-1, r.Custo);
        Assert.Empty(r.Rota);
    }

    [Theory]
    [InlineData("..B")]        // sem partida
    [InlineData("A.A|..B")]    // duas partidas
    [InlineData("A..|B")]      // linhas de tamanhos diferentes
    public void MapaInvalido_LancaExcecao(string linhas)
    {
        string[] mapa = linhas.Split('|'); // '|' separa as linhas do mapa
        Assert.Throws<ArgumentException>(() => BuscaDeRota.Dijkstra(mapa));
    }

    // Mapa com ruas, trânsito, engarrafamento e prédios; A no canto de cima, B no de baixo.
    private static string[] MapaAleatorio(int semente, int linhas, int colunas)
    {
        var aleatorio = new Random(semente);
        var grade = new char[linhas][];
        for (int l = 0; l < linhas; l++)
        {
            grade[l] = new char[colunas];
            for (int c = 0; c < colunas; c++)
            {
                int x = aleatorio.Next(100);
                grade[l][c] = x < 15 ? '#' : x < 25 ? 'm' : x < 30 ? 'T' : '.';
            }
        }
        grade[0][0] = 'A';
        grade[linhas - 1][colunas - 1] = 'B';
        return grade.Select(linha => new string(linha)).ToArray();
    }
}
