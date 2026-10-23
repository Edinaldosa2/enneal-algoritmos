// =====================================================================
//  Validação de entrada em C#: o contrato no DI, o tipo certo,
//  e o errado x certo de XSS e SQL injection
//  Enneal · @enneal.it · https://enneal.com.br
//
//  1. O container de DI entrega o IValidadorDeEntrada ao serviço.
//  2. DetectarTipo: inteiro, decimal, data, e-mail ou texto, com TryParse.
//  3. Entradas maliciosas contra o formulário de cadastro.
//  4. XSS: o mesmo nome colado no HTML x codificado com HtmlEncoder.
//  5. SQLi: a mesma busca concatenada x parametrizada (SQLite em memória).
//
//  Tudo local: dados fictícios, banco em memória, nenhum acesso à rede.
//  O código está em src/Enneal.Algoritmos.ValidacaoEntrada.
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;
using Enneal.Algoritmos.ValidacaoEntrada;
using Microsoft.Extensions.DependencyInjection;

Console.OutputEncoding = Encoding.UTF8;

// 1. Injeção de dependência pelo contrato.
var services = new ServiceCollection().AddValidacaoDeEntrada();
using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
using var escopo = provider.CreateScope();
var validador = escopo.ServiceProvider.GetRequiredService<IValidadorDeEntrada>();
var cadastro = escopo.ServiceProvider.GetRequiredService<ServicoDeCadastro>();
var repositorio = escopo.ServiceProvider.GetRequiredService<RepositorioDeClientes>();
Console.WriteLine($"DI: IValidadorDeEntrada -> {validador.GetType().Name}");
Console.WriteLine();

// 2. Detecção de tipo.
Console.WriteLine("Detectar o tipo:");
foreach (string entrada in new[] { "42", "-7", "19.90", "2026-10-23", "ana@exemplo.example", "1e9", "0x1F", "2026-02-30", "Olá, mundo" })
    Console.WriteLine($"  {entrada,-22} -> {validador.DetectarTipo(entrada)}");
Console.WriteLine();

// 3. O formulário recebe entradas maliciosas.
Console.WriteLine("Cadastro:");
var formularios = new (string Caso, FormularioCadastro Form)[]
{
    ("válido", new("Ana Souza", "ana2@exemplo.example", "34", "1992-05-17")),
    ("idade com SQL", new("Bruno", "bruno2@exemplo.example", "34 OR 1=1", "1992-05-17")),
    ("idade negativa", new("Carla", "carla2@exemplo.example", "-1", "1992-05-17")),
    ("e-mail com nome", new("Davi", "\"Davi\" <davi@exemplo.example>", "20", "2006-01-01")),
    ("data impossível", new("Eva", "eva@exemplo.example", "20", "2006-02-30")),
    ("caractere invisível", new("Fábio\u202Etxt.exe", "fabio@exemplo.example", "20", "2006-01-01")),
    ("nome gigante", new(new string('A', 5000), "gil@exemplo.example", "20", "2006-01-01")),
};
foreach (var (caso, form) in formularios)
{
    var r = cadastro.Cadastrar(form);
    Console.WriteLine(r.Ok ? $"  {caso,-20} -> gravado (id {r.Id})" : $"  {caso,-20} -> recusado: {string.Join("; ", r.Erros)}");
}
Console.WriteLine();

// 4. XSS: validação não basta, a defesa é codificar na saída.
string nome = "<script>alert(1)</script>";
var nomeValido = validador.Validar(nome, ServicoDeCadastro.Nome);
Console.WriteLine($"XSS: \"{nome}\" passa na validação de texto? {(nomeValido.Valido ? "sim" : "não")}");
Console.WriteLine($"  errado: {SaidaHtml.ParagrafoErrado(nome)}");
Console.WriteLine($"  certo:  {SaidaHtml.ParagrafoCerto(nome)}");
string disfarce = "\uFF1Cscript\uFF1E";
Console.WriteLine($"  disfarce \"{disfarce}\" depois do NFKC: \"{validador.Validar(disfarce, ServicoDeCadastro.Nome).Valor}\"");
Console.WriteLine();

// 5. SQL injection: concatenação x parâmetro.
string ataque = "' OR '1'='1";
Console.WriteLine($"SQLi: buscar o nome \"{ataque}\"");
Console.WriteLine($"  errado (concatenado):   {repositorio.BuscarPorNomeErrado(ataque).Count} linhas");
Console.WriteLine($"  certo  (parametrizado): {repositorio.BuscarPorNomeCerto(ataque).Count} linhas");
