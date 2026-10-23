using System.Reflection;
using Enneal.Algoritmos.ValidacaoEntrada;
using Microsoft.Extensions.DependencyInjection;

namespace Enneal.Algoritmos.ValidacaoEntrada.Tests;

public class SaidaHtmlTests
{
    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("\"><svg onload=alert(1)>")]
    [InlineData("O'Brien & <3")]
    public void Certo_NaoDeixaNenhumSinalDeHtml(string ataque)
    {
        string html = SaidaHtml.ParagrafoCerto(ataque);
        string miolo = html["<p>".Length..^"</p>".Length];
        Assert.DoesNotContain('<', miolo);
        Assert.DoesNotContain('>', miolo);
        Assert.DoesNotContain('"', miolo);
        Assert.DoesNotContain('\'', miolo);
    }

    [Fact]
    public void Errado_ColaOScriptNoHtml() =>
        Assert.Equal("<p><script>alert(1)</script></p>", SaidaHtml.ParagrafoErrado("<script>alert(1)</script>"));
}

public class RepositorioTests
{
    [Theory]
    [InlineData("' OR '1'='1")]
    [InlineData("x' OR 1=1 --")]
    [InlineData("' UNION SELECT email FROM clientes --")]
    public void Parametrizado_NaoDevolveNada_ParaAtaque(string ataque)
    {
        using var repo = new RepositorioDeClientes();
        Assert.Empty(repo.BuscarPorNomeCerto(ataque));
    }

    [Fact]
    public void Concatenado_VazaTodos_Parametrizado_SoOExato()
    {
        using var repo = new RepositorioDeClientes();
        Assert.Equal(3, repo.BuscarPorNomeErrado("' OR '1'='1").Count);
        Assert.Equal(["Ana Souza"], repo.BuscarPorNomeCerto("Ana Souza"));
    }

    [Fact]
    public void Parametrizado_GravaAspasComoDado()
    {
        using var repo = new RepositorioDeClientes();
        repo.Inserir("O'Brien'); DROP TABLE clientes;--", "ob@exemplo.example");
        Assert.Single(repo.BuscarPorNomeCerto("O'Brien'); DROP TABLE clientes;--"));
        Assert.Equal(4, repo.BuscarPorNomeErrado("' OR '1'='1").Count); // a tabela continua lá
    }
}

public class InjecaoDeDependenciaTests
{
    [Fact]
    public void Container_EntregaOContrato_EOServicoUsaEle()
    {
        using var sp = new ServiceCollection().AddValidacaoDeEntrada()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var escopo = sp.CreateScope();
        Assert.IsType<ValidadorDeEntrada>(escopo.ServiceProvider.GetRequiredService<IValidadorDeEntrada>());
        Assert.NotNull(escopo.ServiceProvider.GetRequiredService<ServicoDeCadastro>());
    }

    /// <summary>Um validador falso: prova que o serviço depende só do contrato.</summary>
    private sealed class RecusaTudo : IValidadorDeEntrada
    {
        public ResultadoValidacao Validar(string? bruto, RegraDeCampo regra) => ResultadoValidacao.Falha(regra.Nome);
        public TipoDeDado DetectarTipo(string bruto) => TipoDeDado.Texto;
    }

    [Fact]
    public void Servico_TrocaAImplementacao_SemMudarNada()
    {
        using var repo = new RepositorioDeClientes();
        var r = new ServicoDeCadastro(new RecusaTudo(), repo)
            .Cadastrar(new("Ana", "ana@exemplo.example", "34", "1992-05-17"));
        Assert.False(r.Ok);
        Assert.Equal(["nome", "email", "idade", "nascimento"], r.Erros);
    }

    [Fact]
    public void Servico_NaoGrava_SeQualquerCampoFalhar()
    {
        using var repo = new RepositorioDeClientes();
        var servico = new ServicoDeCadastro(new ValidadorDeEntrada(), repo);
        Assert.False(servico.Cadastrar(new("Bruno", "bruno@exemplo.example", "34 OR 1=1", "1992-05-17")).Ok);
        Assert.Equal(3, repo.BuscarPorNomeErrado("' OR '1'='1").Count);
        Assert.True(servico.Cadastrar(new("Bruno", "bruno@exemplo.example", "34", "1992-05-17")).Ok);
        Assert.Equal(4, repo.BuscarPorNomeErrado("' OR '1'='1").Count);
    }
}

public class SaidaDoExemploTests
{
    [Fact]
    public void Exemplo_IgualAoArquivoEsperado()
    {
        string esperado = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "saida-esperada.txt"));
        var entrada = Assembly.Load("ValidacaoDeEntrada").EntryPoint!;
        var original = Console.Out;
        using var saida = new StringWriter();
        try
        {
            Console.SetOut(saida);
            entrada.Invoke(null, [Array.Empty<string>()]);
        }
        finally
        {
            Console.SetOut(original);
        }
        Assert.Equal(esperado.Replace("\r\n", "\n"), saida.ToString().Replace("\r\n", "\n"));
    }
}
