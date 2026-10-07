// =====================================================================
//  Rate limit x DDoS: token bucket por IP
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Simulação de brinquedo de uma DEFESA. Nada aqui gera tráfego de rede:
//  as "requisições" são só objetos numa lista, criados com semente fixa.
//
//  A simulação está em src/Enneal.Algoritmos.RateLimit/ (TokenBucket.cs e
//  SimulacaoRateLimit.cs). Números do Reel:
//    sem rate limit -> legítimos atendidos 22/84 (26%)
//    com rate limit -> legítimos atendidos 81/84 (96%), 886 bloqueadas (429)
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run                -> relatório das 2 rodadas
//    dotnet run -- --eventos   -> também imprime cada requisição, tick a tick
// =====================================================================

using System.Text;
using Enneal.Algoritmos.RateLimit;

Console.OutputEncoding = Encoding.UTF8;

bool mostrarEventos = args.Contains("--eventos");
Action<string>? eventos = mostrarEventos ? Console.WriteLine : null;

foreach (bool comLimite in new[] { false, true })
{
    var r = SimulacaoRateLimit.Rodar(comLimite, ConfiguracaoSimulacao.DoReel, eventos);

    Console.WriteLine($"{(comLimite ? "Com" : "Sem")} rate limit: requisições {r.Requisicoes} | " +
        $"200 ok {r.Atendidas} | 429 bloqueadas {r.Bloqueadas} | " +
        $"503 fila cheia {r.FilaCheia} | 408 timeout {r.Timeouts} | " +
        $"legítimos atendidos {r.LegitimasAtendidas}/{r.LegitimasTotal} ({r.PorcentagemLegitimasAtendidas}%) | " +
        $"flood atendido {r.FloodAtendido} | ticks {r.Ticks}");
}
