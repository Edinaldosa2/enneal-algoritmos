using System.Globalization;

namespace Enneal.Algoritmos.RateLimit;

/// <summary>
/// Parâmetros da simulação. Os valores padrão são os do Reel.
/// </summary>
/// <param name="Ticks">Duração do tráfego, em ticks (unidades de tempo).</param>
/// <param name="InicioFlood">Tick em que a enxurrada começa.</param>
/// <param name="Legitimos">Quantidade de usuários legítimos.</param>
/// <param name="IpsFlood">Quantidade de IPs da enxurrada.</param>
/// <param name="Cpu">Requisições que o servidor atende por tick.</param>
/// <param name="TamanhoFila">Lugares na fila de espera.</param>
/// <param name="Prazo">Espera máxima (em ticks) antes do timeout.</param>
/// <param name="CapacidadeBalde">Fichas do token bucket de cada IP.</param>
/// <param name="RecargaPorTick">Fichas recarregadas por tick.</param>
/// <param name="Semente">Semente do gerador pseudoaleatório (mesma semente = mesmo tráfego).</param>
public sealed record ConfiguracaoSimulacao(
    int Ticks = 34,
    int InicioFlood = 4,
    int Legitimos = 6,
    int IpsFlood = 12,
    int Cpu = 10,
    int TamanhoFila = 24,
    int Prazo = 1,
    int CapacidadeBalde = 3,
    double RecargaPorTick = 0.5,
    uint Semente = 2026)
{
    /// <summary>A configuração usada no Reel.</summary>
    public static ConfiguracaoSimulacao DoReel { get; } = new();
}

/// <summary>
/// Totais de uma rodada da simulação.
/// </summary>
/// <param name="ComLimite">Se o rate limit estava ligado.</param>
/// <param name="Requisicoes">Total de requisições recebidas.</param>
/// <param name="Atendidas">Respondidas com 200.</param>
/// <param name="Bloqueadas">Recusadas com 429 pelo rate limit.</param>
/// <param name="FilaCheia">Recusadas com 503 (fila cheia).</param>
/// <param name="Timeouts">Respondidas com 408 (esperaram demais).</param>
/// <param name="LegitimasAtendidas">Requisições legítimas respondidas com 200.</param>
/// <param name="LegitimasTotal">Total de requisições legítimas.</param>
/// <param name="FloodAtendido">Requisições da enxurrada respondidas com 200.</param>
/// <param name="Ticks">Ticks até a fila esvaziar.</param>
public sealed record ResultadoRodada(
    bool ComLimite,
    int Requisicoes,
    int Atendidas,
    int Bloqueadas,
    int FilaCheia,
    int Timeouts,
    int LegitimasAtendidas,
    int LegitimasTotal,
    int FloodAtendido,
    int Ticks)
{
    /// <summary>Porcentagem (arredondada) de requisições legítimas atendidas.</summary>
    public int PorcentagemLegitimasAtendidas =>
        LegitimasTotal == 0 ? 0 : (200 * LegitimasAtendidas + LegitimasTotal) / (2 * LegitimasTotal);
}

/// <summary>
/// Simulação de brinquedo de uma DEFESA: a fila de um servidor sob uma enxurrada de
/// requisições, sem e com rate limit (token bucket por IP).
/// </summary>
/// <remarks>
/// Nada aqui gera tráfego de rede: as "requisições" são objetos numa lista, criados com
/// semente fixa. Com a configuração do Reel: sem rate limit, 26% dos pedidos legítimos são
/// atendidos; com token bucket, 96%, e 886 requisições levam 429.
/// </remarks>
public static class SimulacaoRateLimit
{
    /// <summary>
    /// Roda uma rodada completa: o tráfego chega por <see cref="ConfiguracaoSimulacao.Ticks"/>
    /// ticks e a simulação continua até a fila esvaziar.
    /// </summary>
    /// <param name="comLimite">Liga o token bucket por IP.</param>
    /// <param name="configuracao">Parâmetros (padrão: os do Reel).</param>
    /// <param name="aoRegistrarEvento">
    /// Opcional: recebe uma linha de texto para cada evento (chegada e atendimento), no formato
    /// <c>R1 tick req id ip L|F destino fichas</c> e <c>R1 tick serve id status</c>.
    /// </param>
    /// <returns>Os totais da rodada.</returns>
    public static ResultadoRodada Rodar(
        bool comLimite,
        ConfiguracaoSimulacao? configuracao = null,
        Action<string>? aoRegistrarEvento = null)
    {
        var simulacao = new Simulacao(configuracao ?? ConfiguracaoSimulacao.DoReel, comLimite, aoRegistrarEvento);
        return simulacao.Rodar();
    }

    private sealed class Simulacao(ConfiguracaoSimulacao cfg, bool comLimite, Action<string>? registrar)
    {
        private readonly Queue<Requisicao> _fila = new();          // fila de espera do servidor
        private readonly Dictionary<int, TokenBucket> _balde = []; // um balde de fichas por IP
        private uint _semente = cfg.Semente;                       // estado do gerador pseudoaleatório
        private int _ids;                                          // numeração das requisições
        private int _t;                                            // relógio (tick atual)

        public ResultadoRodada Rodar()
        {
            for (int ip = 0; ip < cfg.Legitimos + cfg.IpsFlood; ip++)
                _balde[ip] = new TokenBucket(cfg.CapacidadeBalde, cfg.RecargaPorTick);

            string rodada = comLimite ? "R2" : "R1";
            var todas = new List<Requisicao>();

            // Roda enquanto houver tráfego chegando ou gente esperando na fila.
            for (_t = 0; _t < cfg.Ticks || _fila.Count > 0; _t++)
            {
                // 1) Chegam as requisições deste tick.
                foreach (var r in Trafego(_t))
                {
                    todas.Add(r);
                    Chegou(r);
                    if (registrar is not null)
                    {
                        string destino = r.Status == StatusRequisicao.NaFila ? "fila" : Codigo(r.Status);
                        registrar(string.Create(CultureInfo.InvariantCulture,
                            $"{rodada} {_t} req {r.Id} {r.Ip} {(r.Legitima ? "L" : "F")} {destino} {_balde[r.Ip].Fichas:0.0}"));
                    }
                }

                // 2) O servidor atende o que der.
                var antes = _fila.ToArray();
                Atender();
                if (registrar is not null)
                    foreach (var r in antes.Where(r => r.Status != StatusRequisicao.NaFila))
                        registrar(string.Create(CultureInfo.InvariantCulture, $"{rodada} {_t} serve {r.Id} {Codigo(r.Status)}"));
            }

            var legitimas = todas.Where(r => r.Legitima).ToList();
            return new ResultadoRodada(
                ComLimite: comLimite,
                Requisicoes: todas.Count,
                Atendidas: todas.Count(r => r.Status == StatusRequisicao.Ok),
                Bloqueadas: todas.Count(r => r.Status == StatusRequisicao.Bloqueada),
                FilaCheia: todas.Count(r => r.Status == StatusRequisicao.FilaCheia),
                Timeouts: todas.Count(r => r.Status == StatusRequisicao.Timeout),
                LegitimasAtendidas: legitimas.Count(r => r.Status == StatusRequisicao.Ok),
                LegitimasTotal: legitimas.Count,
                FloodAtendido: todas.Count(r => !r.Legitima && r.Status == StatusRequisicao.Ok),
                Ticks: _t);
        }

        // Gerador congruencial linear: com a mesma semente, sempre a mesma sequência.
        private uint Rand()
        {
            _semente = _semente * 1664525u + 1013904223u;
            return _semente >> 8;
        }

        // Requisições que chegam neste tick:
        //   - cada usuário legítimo manda 1 pedido com 40% de chance;
        //   - a partir do InicioFlood, cada IP da enxurrada manda 2 a 4 pedidos.
        private List<Requisicao> Trafego(int tick)
        {
            var novas = new List<Requisicao>();
            if (tick >= cfg.Ticks) return novas;

            for (int u = 0; u < cfg.Legitimos; u++)
                if (Rand() % 10 < 4)
                    novas.Add(new Requisicao(_ids++, u, true, tick));

            if (tick >= cfg.InicioFlood)
                for (int f = 0; f < cfg.IpsFlood; f++)
                    for (uint k = 2 + Rand() % 3; k > 0; k--)
                        novas.Add(new Requisicao(_ids++, cfg.Legitimos + f, false, tick));

            // Embaralha a ordem de chegada dentro do tick (Fisher-Yates).
            for (int i = novas.Count - 1; i > 0; i--)
            {
                int j = (int)(Rand() % (uint)(i + 1));
                (novas[i], novas[j]) = (novas[j], novas[i]);
            }
            return novas;
        }

        // O "portão" do servidor decide o destino da requisição.
        private void Chegou(Requisicao r)
        {
            if (comLimite && !_balde[r.Ip].TentarConsumir(_t))
                r.Status = StatusRequisicao.Bloqueada;  // 429: passou da cota do IP
            else if (_fila.Count < cfg.TamanhoFila)
                _fila.Enqueue(r);                       // entra na fila
            else
                r.Status = StatusRequisicao.FilaCheia;  // 503: fila cheia
        }

        // O servidor processa até Cpu requisições por tick.
        // Quem esperou mais que o Prazo já desistiu: 408.
        private void Atender()
        {
            for (int i = 0; i < cfg.Cpu; i++)
                if (_fila.TryDequeue(out var r))
                    r.Status = _t - r.Chegada > cfg.Prazo
                        ? StatusRequisicao.Timeout  // 408: esperou demais
                        : StatusRequisicao.Ok;      // 200: atendida
        }

        private static string Codigo(StatusRequisicao status) =>
            ((int)status).ToString(CultureInfo.InvariantCulture);
    }
}
