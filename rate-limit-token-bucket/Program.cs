// =====================================================================
//  Rate limit x DDoS: token bucket por IP
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Simulação de brinquedo de uma DEFESA. Nada aqui gera tráfego de rede:
//  as "requisições" são só objetos numa lista, criados com semente fixa.
//  Não há ferramenta de ataque, IP real nem alvo real.
//
//  O cenário: um servidor atende 10 requisições por "tick" (uma unidade
//  de tempo) e tem uma fila de espera de 24 lugares. Chegam pedidos de
//  6 usuários legítimos e, a partir do tick 4, uma enxurrada (flood) de
//  12 IPs mandando 2 a 4 pedidos por tick cada um.
//
//  Rodada 1, SEM rate limit: a fila lota, quase todo usuário legítimo
//    fica sem resposta (503 fila cheia ou 408 timeout).
//  Rodada 2, COM rate limit (token bucket por IP): o MESMO tráfego, mas
//    quem passa da sua cota leva 429 na hora e nem entra na fila.
//
//  Números esperados (os mesmos do Reel):
//    sem rate limit -> legítimos atendidos 22/84 (26%)
//    com rate limit -> legítimos atendidos 81/84 (96%), 886 bloqueadas (429)
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run                -> relatório das 2 rodadas
//    dotnet run -- --eventos   -> também imprime cada requisição, tick a tick
// =====================================================================

using System.Text;
using static System.Math;

Console.OutputEncoding = Encoding.UTF8;

// ---- Parâmetros da simulação ----
const int Ticks = 34;     // duração do tráfego (em ticks)
const int InicioFlood = 4; // tick em que a enxurrada começa
const int Legitimos = 6;   // usuários legítimos (IPs 0 a 5)
const int IpsFlood = 12;   // IPs da enxurrada (IPs 6 a 17)
const int Cpu = 10;        // requisições que o servidor atende por tick
const int TamanhoFila = 24; // lugares na fila de espera
const int Prazo = 1;       // espera máxima (em ticks) antes do timeout

// ---- Parâmetros do token bucket (balde de fichas) ----
const int Capacidade = 3;    // cada IP começa com 3 fichas e nunca passa disso
const double Recarga = 0.5;  // ganha meia ficha por tick (= 1 pedido a cada 2 ticks)

bool mostrarEventos = args.Contains("--eventos");

var fila = new Queue<Req>();                 // fila de espera do servidor
var balde = new Dictionary<int, TokenBucket>(); // um balde de fichas por IP
bool limite = false; // rate limit ligado nesta rodada?
int t = 0;           // relógio (tick atual)
int ids = 0;         // numeração das requisições
uint semente = 0;    // estado do gerador de números pseudoaleatórios

foreach (bool comLimite in new[] { false, true })
{
    // Cada rodada começa do zero e com a MESMA semente: o tráfego é idêntico.
    limite = comLimite;
    semente = 2026;
    ids = 0;
    fila.Clear();
    balde.Clear();
    for (int ip = 0; ip < Legitimos + IpsFlood; ip++)
        balde[ip] = new TokenBucket(Capacidade, Recarga);

    string rodada = comLimite ? "R2" : "R1";
    var todas = new List<Req>();

    // Roda enquanto houver tráfego chegando ou gente esperando na fila.
    for (t = 0; t < Ticks || fila.Count > 0; t++)
    {
        // 1) Chegam as requisições deste tick.
        foreach (var r in Trafego(t))
        {
            todas.Add(r);
            Chegou(r);
            if (mostrarEventos)
                Console.WriteLine($"{rodada} {t} req {r.Id} {r.Ip} {(r.Legit ? "L" : "F")} " +
                    $"{(r.Status == 0 ? "fila" : r.Status.ToString())} {balde[r.Ip].Tokens:0.0}");
        }

        // 2) O servidor atende o que der.
        var antes = fila.ToArray();
        Atender();
        if (mostrarEventos)
            foreach (var r in antes.Where(r => r.Status != 0))
                Console.WriteLine($"{rodada} {t} serve {r.Id} {r.Status}");
    }

    // 3) Relatório da rodada.
    var leg = todas.Where(r => r.Legit).ToList();
    int ok = leg.Count(r => r.Status == 200);
    int porcento = (200 * ok + leg.Count) / (2 * leg.Count); // porcentagem arredondada
    Console.WriteLine($"{(comLimite ? "Com" : "Sem")} rate limit: requisições {todas.Count} | " +
        $"200 ok {todas.Count(r => r.Status == 200)} | 429 bloqueadas {todas.Count(r => r.Status == 429)} | " +
        $"503 fila cheia {todas.Count(r => r.Status == 503)} | 408 timeout {todas.Count(r => r.Status == 408)} | " +
        $"legítimos atendidos {ok}/{leg.Count} ({porcento}%) | " +
        $"flood atendido {todas.Count(r => !r.Legit && r.Status == 200)} | ticks {t}");
}

// ---------------------------------------------------------------------
// Gerador pseudoaleatório simples (congruencial linear). Com a mesma
// semente, gera sempre a mesma sequência: a simulação é reproduzível.
// ---------------------------------------------------------------------
uint Rand()
{
    semente = semente * 1664525u + 1013904223u;
    return semente >> 8;
}

// ---------------------------------------------------------------------
// Trafego(tick): cria as requisições que chegam neste tick.
//   - cada usuário legítimo manda 1 pedido com 40% de chance;
//   - a partir do InicioFlood, cada IP do flood manda 2 a 4 pedidos.
// ---------------------------------------------------------------------
List<Req> Trafego(int tick)
{
    var novas = new List<Req>();
    if (tick >= Ticks) return novas;

    for (int u = 0; u < Legitimos; u++)
        if (Rand() % 10 < 4)
            novas.Add(new Req(ids++, u, true, tick));

    if (tick >= InicioFlood)
        for (int f = 0; f < IpsFlood; f++)
            for (uint k = 2 + Rand() % 3; k > 0; k--)
                novas.Add(new Req(ids++, Legitimos + f, false, tick));

    // Embaralha a ordem de chegada dentro do tick (Fisher-Yates).
    for (int i = novas.Count - 1; i > 0; i--)
    {
        int j = (int)(Rand() % (uint)(i + 1));
        (novas[i], novas[j]) = (novas[j], novas[i]);
    }
    return novas;
}

// ---------------------------------------------------------------------
// Chegou(r): o "portão" do servidor decide o destino da requisição.
// ---------------------------------------------------------------------
void Chegou(Req r)
{
    if (limite && !balde[r.Ip].Pode(t))
        r.Status = 429;      // 429 Too Many Requests: passou da cota do IP
    else if (fila.Count < TamanhoFila)
        fila.Enqueue(r);     // entra na fila (Status 0 = esperando)
    else
        r.Status = 503;      // 503 Service Unavailable: fila cheia
}

// ---------------------------------------------------------------------
// Atender(): o servidor processa até Cpu requisições da fila neste tick.
// Quem esperou mais que o Prazo já desistiu: 408 (timeout).
// ---------------------------------------------------------------------
void Atender()
{
    for (int i = 0; i < Cpu; i++)
        if (fila.TryDequeue(out var r))
            r.Status = t - r.Inicio > Prazo
                ? 408   // 408 Request Timeout: esperou demais
                : 200;  // 200 OK: atendida
}

// Uma requisição: quem mandou (Ip), se é legítima, quando chegou e o resultado.
class Req(int id, int ip, bool legit, int inicio)
{
    public int Id = id, Ip = ip, Inicio = inicio, Status = 0;
    public bool Legit = legit;
}

// ---------------------------------------------------------------------
// Token bucket ("balde de fichas"):
//   - o balde começa cheio (cap fichas) e recarrega 'taxa' fichas por tick,
//     sem nunca passar da capacidade;
//   - cada requisição gasta 1 ficha; sem ficha, é recusada.
// Assim cada IP pode fazer pequenas rajadas (até cap), mas no longo prazo
// fica limitado à taxa de recarga.
// ---------------------------------------------------------------------
class TokenBucket(int cap, double taxa)
{
    public double Tokens = cap; // fichas disponíveis agora
    int antes = 0;              // tick da última recarga

    public bool Pode(int t)
    {
        Tokens = Min(cap, Tokens + (t - antes) * taxa); // recarrega pelo tempo que passou
        antes = t;
        if (Tokens < 1) return false; // balde vazio: recusa
        Tokens--;                     // gasta uma ficha
        return true;
    }
}
