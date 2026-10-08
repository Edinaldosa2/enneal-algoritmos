using System.Text;
using Enneal.Algoritmos.ComparacaoSegura;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class ComparacaoSeguraTests
{
    [Fact]
    public void Reel_CustosDoLacoQueSaiCedo_DependemDoPrefixo()
    {
        var r = DemoDoReel.Rodar();

        Assert.Equal([0, 3, 1, 5, 2, 6], r.Select(x => x.PrefixoIgual));
        Assert.Equal([1, 4, 2, 6, 3, 6], r.Select(x => x.CustoSaiCedo));
    }

    [Fact]
    public void Reel_TempoConstante_CustaSempre6()
    {
        var r = DemoDoReel.Rodar();

        Assert.All(r, x => Assert.Equal(6, x.CustoTempoConstante));
    }

    [Fact]
    public void Reel_AsDuasVersoesDaoAMesmaResposta_SoOUltimoEIgual()
    {
        var r = DemoDoReel.Rodar();

        Assert.Equal([false, false, false, false, false, true], r.Select(x => x.RespostaTempoConstante));
        Assert.All(r, x => Assert.Equal(x.RespostaTempoConstante, x.RespostaSaiCedo));
    }

    [Fact]
    public void Reel_ContaCadaComparacao_22SaindoCedoE36EmTempoConstante()
    {
        int inseguro = 0, seguro = 0;
        DemoDoReel.Comparar(DemoDoReel.Token, DemoDoReel.Candidatos, (versao, _, _, _, _) =>
        {
            if (versao == "I") inseguro++; else seguro++;
        });

        Assert.Equal(22, inseguro);
        Assert.Equal(36, seguro);
    }

    [Theory]
    [InlineData("7F3A9C", "7F3A9", 0)]       // tamanho diferente: false sem comparar nada
    [InlineData("", "", 0)]
    [InlineData("A", "B", 1)]
    public void TamanhosEBordas(string esperado, string recebido, int custoSaiCedo)
    {
        var contador = new ContadorDeComparacoes();
        bool ok = ComparacaoDeSegredos.IgualInseguro(esperado, recebido, contador);

        Assert.Equal(esperado == recebido, ok);
        Assert.Equal(esperado == recebido, ComparacaoDeSegredos.IgualSeguro(esperado, recebido));
        Assert.Equal(custoSaiCedo, contador.Comparacoes);
    }

    [Fact]
    public void Propriedade_AsTresVersoesConcordamComOIgualDoDotNet_EOCustoSegueOModelo()
    {
        var rnd = new Random(2026);
        const string Alfabeto = "0123456789ABCDEF";
        for (int t = 0; t < 2000; t++)
        {
            int n = rnd.Next(1, 12);
            string a = new([.. Enumerable.Range(0, n).Select(_ => Alfabeto[rnd.Next(Alfabeto.Length)])]);
            // metade das vezes, b começa igual a "a" por um prefixo sorteado
            int prefixo = rnd.Next(0, n + 1);
            string b = rnd.Next(2) == 0
                ? a[..prefixo] + new string([.. Enumerable.Range(0, n - prefixo).Select(_ => Alfabeto[rnd.Next(Alfabeto.Length)])])
                : new string([.. Enumerable.Range(0, n).Select(_ => Alfabeto[rnd.Next(Alfabeto.Length)])]);

            var cI = new ContadorDeComparacoes();
            var cS = new ContadorDeComparacoes();
            bool igual = string.Equals(a, b, StringComparison.Ordinal);

            Assert.Equal(igual, ComparacaoDeSegredos.IgualInseguro(a, b, cI));
            Assert.Equal(igual, ComparacaoDeSegredos.ModeloFixedTime(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b), cS));
            Assert.Equal(igual, ComparacaoDeSegredos.IgualSeguro(a, b));

            int p = ComparacaoDeSegredos.PrefixoIgual(a, b);
            Assert.Equal(Math.Min(p + 1, n), cI.Comparacoes); // sai cedo: vaza o prefixo
            Assert.Equal(n, cS.Comparacoes);                  // tempo constante: sempre o tamanho
        }
    }
}
