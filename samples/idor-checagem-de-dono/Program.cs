// =====================================================================
//  Controle de acesso quebrado (IDOR) na SUA API
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Logada como a cliente #41, o teste pede /api/pedidos/41 a 45.
//  Sem checagem de dono, a API entrega o pedido de outras pessoas.
//  Com a checagem, responde 403 e nenhum dado.
//
//  Teste local: a "API" é uma função e os pedidos estão em memória
//  (dados fictícios). Nada aqui gera tráfego de rede.
//
//  A checagem está em src/Enneal.Algoritmos.ControleDeAcesso/ApiDePedidos.cs.
//  Números do Reel:
//    sem checagem -> 5 respostas 200, 4 pedidos de outras pessoas vazados
//    com checagem -> 1 resposta 200, 4 respostas 403, 0 vazados
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run                    -> o resumo dos 2 testes
//    dotnet run -- --requisicoes   -> também cada requisição
// =====================================================================

using System.Text;
using Enneal.Algoritmos.ControleDeAcesso;

Console.OutputEncoding = Encoding.UTF8;

bool mostrarRequisicoes = args.Contains("--requisicoes");
var api = ApiDePedidos.DoReel();
int logada = TesteDeAcesso.UsuariaDoReel; // a sessão é da Ana (#41)
int[] ids = [41, 42, 43, 44, 45];

foreach (bool checarDono in new[] { false, true })
{
    string rodada = checarDono ? "R2" : "R1";

    // Com --requisicoes: rodada, id, status, dono e nome (se entregou) e a situação.
    Action<int, RespostaApi, bool>? aoResponder = mostrarRequisicoes
        ? (id, r, vazou) =>
        {
            string situacao = r.Status == 403 ? "negado" : vazou ? "vazou" : "proprio";
            string dado = r.Pedido is null ? "-" : $"dono={r.Pedido.Dono} {r.Pedido.Nome}";
            Console.WriteLine($"{rodada} {id} {r.Status} {dado} {situacao}");
        }
        : null;

    var t = TesteDeAcesso.Rodar(api, ids, logada, checarDono, aoResponder);

    string titulo = checarDono ? "Com checagem de dono" : "Sem checagem de dono";
    Console.WriteLine($"{titulo}: requisições {t.Requisicoes} | 200 ok {t.Ok} | 403 negado {t.Negadas}" +
                      $" | pedidos de outras pessoas vazados {t.Vazadas}");
}
