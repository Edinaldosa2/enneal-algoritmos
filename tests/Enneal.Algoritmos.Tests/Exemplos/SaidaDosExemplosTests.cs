using System.Reflection;

namespace Enneal.Algoritmos.Tests.Exemplos;

// Estes testes mexem no Console (global), então rodam um de cada vez.
[CollectionDefinition(nameof(ConsoleSequencial), DisableParallelization = true)]
public sealed class ConsoleSequencial;

/// <summary>
/// Roda cada programa de samples/ e compara a saída, caractere por caractere,
/// com o arquivo saida-esperada.txt da pasta dele. Se alguém mudar um número
/// dos Reels sem querer, este teste avisa.
/// </summary>
[Collection(nameof(ConsoleSequencial))]
public class SaidaDosExemplosTests
{
    [Theory]
    [InlineData("NRainhas", "n-rainhas-8x8")]
    [InlineData("NRainhasLongo", "n-rainhas-4x4-ate-8x8")]
    [InlineData("ForcaBrutaSenha", "forca-bruta-senha")]
    [InlineData("CaminhoDoInvasor", "caminho-do-invasor")]
    [InlineData("CorridaDeOrdenacoes", "corrida-de-ordenacoes")]
    [InlineData("RateLimitTokenBucket", "rate-limit-token-bucket")]
    [InlineData("DijkstraAEstrela", "dijkstra-a-estrela")]
    [InlineData("BuscaLinearVsBinaria", "busca-linear-vs-binaria")]
    [InlineData("TorreDeHanoi", "torre-de-hanoi")]
    [InlineData("IdorChecagemDeDono", "idor-checagem-de-dono")]
    [InlineData("RotasEsquecidas", "rotas-esquecidas")]
    [InlineData("TriagemCvssEpssKev", "triagem-cvss-epss-kev")]
    [InlineData("NotaDoHttps", "nota-do-https")]
    [InlineData("CabecalhosDeSeguranca", "cabecalhos-de-seguranca")]
    [InlineData("UrlMaliciosa", "url-maliciosa")]
    public void SaidaDoExemplo_IgualAoArquivoEsperado(string assembly, string pasta)
    {
        string esperado = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "SaidasEsperadas", pasta, "saida-esperada.txt"));

        string saida = RodarExemplo(assembly);

        Assert.Equal(Normalizar(esperado), Normalizar(saida));
    }

    [Fact]
    public void RateLimit_ComEventos_ImprimeUmaLinhaPorEventoEDepoisORelatorio()
    {
        string saida = RodarExemplo("RateLimitTokenBucket", "--eventos");
        string[] linhas = Normalizar(saida).TrimEnd('\n').Split('\n');

        Assert.Equal(2943, linhas.Length);
        Assert.StartsWith("R1 0 req ", linhas[0]);
        Assert.StartsWith("Com rate limit: ", linhas[^1]);
    }

    [Fact]
    public void Busca_ComEventos_ImprimeCadaComparacaoDasTresRodadas()
    {
        string saida = RodarExemplo("BuscaLinearVsBinaria", "--eventos");
        string[] linhas = Normalizar(saida).TrimEnd('\n').Split('\n');

        // 7 + 10 + 613 + 10 + 1024 + 11 comparações, mais as 5 linhas do relatório.
        Assert.Equal(1675 + 5, linhas.Length);
        Assert.Equal("R1 L 1 5 19", linhas[0]);
    }

    [Fact]
    public void TorreDeHanoi_ComMovimentos_ImprimeCadaMovimentoDasTresRodadas()
    {
        string saida = RodarExemplo("TorreDeHanoi", "--movimentos");
        string[] linhas = Normalizar(saida).TrimEnd('\n').Split('\n');

        // 7 + 15 + 63 movimentos, mais as 5 linhas do relatório (uma por rodada logo depois dos movimentos dela).
        Assert.Equal(85 + 5, linhas.Length);
        Assert.Equal("H3 1 1 A C", linhas[0]);
        Assert.Equal("3 discos: 7 movimentos = 2^3 - 1 = 7", linhas[7]);
        Assert.StartsWith("a 1 movimento por segundo: ", linhas[^1]);
    }

    [Theory]
    [InlineData("IdorChecagemDeDono", "--requisicoes", 10 + 2, "R1 41 200 dono=41 Ana Souza proprio")]
    [InlineData("RotasEsquecidas", "--rotas", 30 + 2, "R1 0 / 200 pagina nao")]
    [InlineData("TriagemCvssEpssKev", "--fila", 400 + 5, "Q CVE-2024-50623 cvss=9.8 epss=0.98607 kev=1 ransomware=1 mistura=0.988457 risco=100.0 faixa=P1 cvss#=10 epss#=1 fila#=1")]
    [InlineData("NotaDoHttps", "--camadas", 32, "T 1 DNS quebra | nome não existe")]
    [InlineData("CabecalhosDeSeguranca", "--cabecalhos", 90, "F 0 200 http://loja.example/ https nao")]
    [InlineData("UrlMaliciosa", "--pistas", 6 * 5 + 1, "U 1 https://www.example.com/produtos/tenis-corrida")]
    public void ExemplosDeSeguranca_ComDetalhes(string assembly, string opcao, int linhasEsperadas, string primeira)
    {
        string[] linhas = Normalizar(RodarExemplo(assembly, opcao)).TrimEnd('\n').Split('\n');

        Assert.Equal(linhasEsperadas, linhas.Length);
        Assert.Equal(primeira, linhas[0]);
    }

    private static string RodarExemplo(string assembly, params string[] argumentos)
    {
        var entrada = Assembly.Load(assembly).EntryPoint
            ?? throw new InvalidOperationException($"{assembly} não tem ponto de entrada.");

        var original = Console.Out;
        using var saida = new StringWriter();
        try
        {
            Console.SetOut(saida);
            entrada.Invoke(null, [argumentos]);
        }
        finally
        {
            Console.SetOut(original);
        }
        return saida.ToString();
    }

    // O git pode trocar \n por \r\n no Windows: compara sem ligar para isso.
    private static string Normalizar(string texto) => texto.Replace("\r\n", "\n", StringComparison.Ordinal);
}
