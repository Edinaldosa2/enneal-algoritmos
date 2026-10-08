using Enneal.Algoritmos.ControleDeAcesso;

namespace Enneal.Algoritmos.Tests.Algoritmos;

public class ControleDeAcessoTests
{
    private static readonly int[] IdsDoReel = [41, 42, 43, 44, 45];

    [Fact]
    public void SemChecagemDeDono_TemOsNumerosDoReel()
    {
        var t = TesteDeAcesso.Rodar(ApiDePedidos.DoReel(), IdsDoReel, TesteDeAcesso.UsuariaDoReel, checarDono: false);

        Assert.Equal(new ResultadoTesteDeAcesso(5, 5, 0, 4), t);
    }

    [Fact]
    public void ComChecagemDeDono_TemOsNumerosDoReel()
    {
        var t = TesteDeAcesso.Rodar(ApiDePedidos.DoReel(), IdsDoReel, TesteDeAcesso.UsuariaDoReel, checarDono: true);

        Assert.Equal(new ResultadoTesteDeAcesso(5, 1, 4, 0), t);
    }

    [Theory]
    [InlineData(41)]
    [InlineData(42)]
    [InlineData(43)]
    [InlineData(44)]
    [InlineData(45)]
    public void ComChecagem_NenhumUsuarioVePedidoDeOutro(int usuario)
    {
        var api = ApiDePedidos.DoReel();
        foreach (var p in ApiDePedidos.PedidosDoReel)
        {
            var r = api.Buscar(p.Id, usuario, checarDono: true);
            if (p.Dono == usuario)
            {
                Assert.Equal(200, r.Status);
                Assert.Equal(p, r.Pedido);
            }
            else
            {
                Assert.Equal(403, r.Status);
                Assert.Null(r.Pedido); // negado = nenhum dado na resposta
            }
        }
    }

    [Fact]
    public void PedidoInexistente_404SemDados_ComOuSemChecagem()
    {
        var api = ApiDePedidos.DoReel();

        foreach (bool checar in new[] { false, true })
        {
            var r = api.Buscar(999, 41, checar);
            Assert.Equal(404, r.Status);
            Assert.Null(r.Pedido);
        }
    }

    [Fact]
    public void SemChecagem_EntregaQualquerPedido_EhOErro()
    {
        var r = ApiDePedidos.DoReel().Buscar(42, usuarioDaSessao: 41, checarDono: false);

        Assert.Equal(200, r.Status);
        Assert.Equal(42, r.Pedido!.Dono); // o pedido do Bruno para a sessão da Ana: IDOR
    }

    [Fact]
    public void Callback_RecebeCadaRequisicaoEMarcaOsVazamentos()
    {
        var vazados = new List<int>();
        TesteDeAcesso.Rodar(ApiDePedidos.DoReel(), IdsDoReel, 41, false, (id, _, vazou) => { if (vazou) vazados.Add(id); });

        Assert.Equal([42, 43, 44, 45], vazados);
    }

    [Fact]
    public void DadosDoReel_SaoFicticiosEComDocumentoMascarado()
    {
        Assert.All(ApiDePedidos.PedidosDoReel, p => Assert.StartsWith("***.***.", p.Documento));
        Assert.Equal(5, ApiDePedidos.PedidosDoReel.Select(p => p.Dono).Distinct().Count());
    }
}
