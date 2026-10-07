// =====================================================================
//  Força bruta x login protegido (simulação didática)
//  Enneal · @enneal.it · https://enneal.com.br
//
//  IMPORTANTE: isto é uma SIMULAÇÃO de brinquedo para ensinar DEFESA.
//  O "alvo" é um PIN que está na memória deste próprio programa. Não há
//  rede, arquivos, sistemas de terceiros nem senhas reais.
//
//  Round 1: PIN de 4 dígitos sem proteção. Um laço testa 0000, 0001...
//  Round 2: o MESMO laço contra um login que guarda só o hash da senha
//           (PBKDF2 com salt) e bloqueia a conta depois de 5 erros.
//
//  O código está em src/Enneal.Algoritmos.ForcaBruta/ (LoginProtegido.cs
//  e SimulacaoForcaBruta.cs). Números do Reel:
//    Round 1 -> 7392 tentativas até abrir o PIN 7391
//    Round 2 -> bloqueado após 5 tentativas (15 min)
//               no mesmo PIN: 1478 bloqueios = 15.4 dias de espera
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Text;
using Enneal.Algoritmos.ForcaBruta;

Console.OutputEncoding = Encoding.UTF8;

const string Pin = "7391";      // o PIN "secreto" da demo
const int MaxFalhas = 5;        // erros permitidos antes de bloquear
const int MinutosBloqueio = 15; // tempo de bloqueio da conta

// ---- Round 1: sem proteção ----
Console.WriteLine("== Round 1: PIN de 4 dígitos, SEM proteção ==");
int tentativas = SimulacaoForcaBruta.TentativasSemProtecao(Pin);
Console.WriteLine("ABERTO!");
Console.WriteLine(tentativas); // 7392
Console.WriteLine($"(o PIN {Pin} caiu na tentativa {tentativas} de {SimulacaoForcaBruta.TotalDePins} possíveis)");
Console.WriteLine();

// ---- Round 2: login protegido ----
Console.WriteLine("== Round 2: login protegido (hash + bloqueio) ==");
var login = new LoginProtegido(Pin, MaxFalhas, TimeSpan.FromMinutes(MinutosBloqueio));
var round2 = SimulacaoForcaBruta.TentarContraLogin(login);
if (round2.Aberto) Console.WriteLine("ABERTO!");
Console.WriteLine($"bloqueado após {round2.Falhas} tentativas ({MinutosBloqueio} min)");
Console.WriteLine($"(o {round2.Pedidos}º pedido já encontrou a conta bloqueada)");
Console.WriteLine();

// ---- Quanto o bloqueio atrasa o MESMO PIN do Round 1? ----
// A cada 5 erros, 15 minutos de espera: 7391 erros / 5 = 1478 bloqueios.
var custo = SimulacaoForcaBruta.CalcularCustoDoBloqueio(tentativas, MaxFalhas, MinutosBloqueio);
Console.WriteLine($"mesmo PIN: {custo.Bloqueios} bloqueios = {custo.Dias:0.0} dias");
