using Enneal.Algoritmos.Ordenacoes;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class OrdenacoesTests
{
    [Fact]
    public void Corrida_TemOsNumerosDoReel()
    {
        var r = CorridaDeOrdenacoes.Correr(CorridaDeOrdenacoes.VetorDoReel);

        Assert.Equal(("Bubble", 375, 161, 0, 536), (r[0].Nome, r[0].Comparacoes, r[0].Trocas, r[0].Escritas, r[0].Passos));
        Assert.Equal(("Quick", 116, 69, 0, 185), (r[1].Nome, r[1].Comparacoes, r[1].Trocas, r[1].Escritas, r[1].Passos));
        Assert.Equal(("Merge", 99, 0, 136, 235), (r[2].Nome, r[2].Comparacoes, r[2].Trocas, r[2].Escritas, r[2].Passos));
        Assert.All(r, x => Assert.True(x.Ordenado));
    }

    [Fact]
    public void Podio_QuickMergeBubble()
    {
        var podio = CorridaDeOrdenacoes.Podio(CorridaDeOrdenacoes.Correr(CorridaDeOrdenacoes.VetorDoReel));

        Assert.Equal(["Quick", "Merge", "Bubble"], podio.Select(r => r.Nome));
        Assert.Equal([185, 235, 536], podio.Select(r => r.Passos));
    }

    [Fact]
    public void VetorDoReel_EhUmaPermutacaoDe1a28()
    {
        Assert.Equal(Enumerable.Range(1, 28), CorridaDeOrdenacoes.VetorDoReel.Order());
    }

    [Fact]
    public void Corrida_NaoAlteraOVetorDePartida()
    {
        int[] vetor = [3, 1, 2];
        CorridaDeOrdenacoes.Correr(vetor);
        Assert.Equal([3, 1, 2], vetor);
    }

    public static TheoryData<string, int[]> VetoresAleatorios()
    {
        // Semente fixa: os testes são sempre os mesmos.
        var aleatorio = new Random(2026);
        var dados = new TheoryData<string, int[]>();
        int[] tamanhos = [0, 1, 2, 3, 7, 16, 28, 50, 101, 256];
        foreach (int n in tamanhos)
        {
            dados.Add($"aleatorio-{n}", Enumerable.Range(0, n).Select(_ => aleatorio.Next(-50, 50)).ToArray());
        }
        dados.Add("ja-ordenado", Enumerable.Range(1, 40).ToArray());
        dados.Add("ao-contrario", Enumerable.Range(1, 40).Reverse().ToArray());
        dados.Add("todos-iguais", Enumerable.Repeat(7, 30).ToArray());
        return dados;
    }

    [Theory]
    [MemberData(nameof(VetoresAleatorios))]
    public void OsTresAlgoritmos_OrdenamIgualAoArraySort(string caso, int[] vetor)
    {
        Assert.NotNull(caso);
        int[] esperado = vetor.Order().ToArray();

        int[] bubble = (int[])vetor.Clone();
        int[] quick = (int[])vetor.Clone();
        int[] merge = (int[])vetor.Clone();
        Ordenacao.BubbleSort(bubble);
        Ordenacao.QuickSort(quick);
        Ordenacao.MergeSort(merge);

        Assert.Equal(esperado, bubble);
        Assert.Equal(esperado, quick);
        Assert.Equal(esperado, merge);
    }

    [Theory]
    [MemberData(nameof(VetoresAleatorios))]
    public void BubbleSort_FazUmaTrocaPorInversao(string caso, int[] vetor)
    {
        // Propriedade clássica: cada troca de vizinhos desfaz exatamente uma inversão
        // (par i < j com v[i] > v[j]). O vetor do Reel tem 161 inversões = 161 trocas.
        Assert.NotNull(caso);
        long inversoes = 0;
        for (int i = 0; i < vetor.Length; i++)
            for (int j = i + 1; j < vetor.Length; j++)
                if (vetor[i] > vetor[j]) inversoes++;

        var passos = Ordenacao.BubbleSort((int[])vetor.Clone());

        Assert.Equal(inversoes, passos.Trocas);
    }

    [Theory]
    [MemberData(nameof(VetoresAleatorios))]
    public void MergeSort_EscreveCadaPosicaoUmaVezPorNivel(string caso, int[] vetor)
    {
        // O Merge não troca: só escreve. E nunca compara mais que n * ceil(log2 n) vezes.
        Assert.NotNull(caso);
        var passos = Ordenacao.MergeSort((int[])vetor.Clone());

        Assert.Equal(0, passos.Trocas);
        int n = vetor.Length;
        int niveis = n <= 1 ? 0 : (int)Math.Ceiling(Math.Log2(n));
        Assert.True(passos.Comparacoes <= n * niveis);
        Assert.True(passos.Escritas <= n * niveis);
    }

    [Fact]
    public void BubbleSort_VetorJaOrdenado_UmaVoltaSoSemTrocas()
    {
        var passos = Ordenacao.BubbleSort(Enumerable.Range(1, 28).ToArray());

        Assert.Equal(27, passos.Comparacoes);
        Assert.Equal(0, passos.Trocas);
    }

    [Fact]
    public void EstaOrdenado_AceitaRepetidos()
    {
        Assert.True(CorridaDeOrdenacoes.EstaOrdenado([1, 2, 2, 3]));
        Assert.False(CorridaDeOrdenacoes.EstaOrdenado([2, 1]));
        Assert.True(CorridaDeOrdenacoes.EstaOrdenado([]));
    }
}
