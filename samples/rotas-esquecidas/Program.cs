// =====================================================================
//  Rotas esquecidas no SEU site
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Auditoria das rotas do seu próprio site (seu-site.example) antes do
//  deploy: 15 caminhos conferidos, antes e depois da correção.
//  A correção: negar por padrão (só as rotas liberadas existem), tirar as
//  sobras do deploy e desligar a listagem de diretório.
//
//  Nada aqui gera tráfego de rede: o "servidor" é a tabela de rotas do
//  app, em memória, com dados fixos.
//
//  A auditoria está em src/Enneal.Algoritmos.RotasEsquecidas.
//  Números do Reel:
//    antes  -> 200: 6 | 302: 1 | 403: 1 | 404: 7 | expostas: 1 (/.git/config)
//    depois -> 200: 4 | 302: 1 | 403: 1 | 404: 9 | expostas: 0
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run              -> o resumo das 2 auditorias
//    dotnet run -- --rotas   -> também cada rota conferida
// =====================================================================

using System.Text;
using Enneal.Algoritmos.RotasEsquecidas;

Console.OutputEncoding = Encoding.UTF8;

bool mostrarRotas = args.Contains("--rotas");
var site = SiteDeExemplo.DoReel();
var checklist = AuditoriaDeRotas.ChecklistDoReel;

foreach (bool corrigido in new[] { false, true })
{
    string rodada = corrigido ? "R2" : "R1";

    // Com --rotas: rodada, índice, caminho, status, tipo e se ficou exposta.
    Action<int, string, Rota, bool>? aoConferir = mostrarRotas
        ? (i, caminho, r, exposta) => Console.WriteLine($"{rodada} {i} {caminho} {r.Status} {r.Tipo} {(exposta ? "sim" : "nao")}")
        : null;

    var a = AuditoriaDeRotas.Auditar(site, checklist, corrigido, aoConferir);

    string titulo = corrigido ? "Depois da correção" : "Antes da correção";
    Console.WriteLine($"{titulo}: rotas conferidas {a.Conferidas} | 200 ok {a.Contar(200)} | 302 {a.Contar(302)}" +
                      $" | 403 {a.Contar(403)} | 404 {a.Contar(404)} | rotas sensíveis expostas {a.Expostas.Count}" +
                      (a.Expostas.Count > 0 ? $" ({string.Join(", ", a.Expostas)})" : ""));
}
