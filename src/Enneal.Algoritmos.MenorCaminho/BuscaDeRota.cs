namespace Enneal.Algoritmos.MenorCaminho;

/// <summary>
/// Resultado de uma busca de rota.
/// </summary>
/// <param name="Custo">Custo da rota mais barata (-1 se não houver rota).</param>
/// <param name="NosExplorados">Esquinas exploradas até chegar no destino (o contador do Reel).</param>
/// <param name="Rota">A rota, da partida até o destino (vazia se não houver).</param>
/// <param name="Explorados">As esquinas exploradas.</param>
public sealed record ResultadoRota(
    int Custo,
    int NosExplorados,
    IReadOnlyList<Esquina> Rota,
    IReadOnlySet<Esquina> Explorados)
{
    /// <summary>Se existe rota entre a partida e o destino.</summary>
    public bool Encontrada => Custo >= 0;
}

/// <summary>
/// "Como o GPS acha a rota": Dijkstra x A* (A-estrela) num mapa em grade com pesos.
/// </summary>
/// <remarks>
/// <para>
/// As duas buscas usam uma fila de prioridade e sempre exploram primeiro a esquina mais
/// "promissora". A ÚNICA diferença é a prioridade:
/// </para>
/// <list type="bullet">
/// <item><b>Dijkstra:</b> prioridade = custo conhecido até a esquina. Espalha-se em anel para todos os lados.</item>
/// <item><b>A*:</b> prioridade = custo + H, onde H é a distância Manhattan até o destino.
/// Como H nunca passa do custo real (heurística admissível), a rota continua a mais barata,
/// mas a busca vai direto para o destino.</item>
/// </list>
/// <para>
/// Tempo O((V + E) log V) com fila de prioridade binária; memória O(V).
/// </para>
/// </remarks>
public static class BuscaDeRota
{
    // Ordem dos vizinhos: direita, baixo, esquerda, cima. Com empates na fila, a ordem
    // muda quais nós são explorados; por isso é a mesma do Reel.
    private static readonly (int Dl, int Dc)[] Direcoes = [(0, 1), (1, 0), (0, -1), (-1, 0)];

    /// <summary>
    /// Rota mais barata de <c>A</c> até <c>B</c> com o algoritmo de Dijkstra.
    /// </summary>
    /// <param name="mapa">Linhas do mapa, todas do mesmo tamanho, com um <c>A</c> e um <c>B</c>.</param>
    /// <returns>Custo, nós explorados e a rota.</returns>
    public static ResultadoRota Dijkstra(IReadOnlyList<string> mapa) => Buscar(mapa, usarHeuristica: false);

    /// <summary>
    /// Rota mais barata de <c>A</c> até <c>B</c> com o algoritmo A* (heurística Manhattan).
    /// </summary>
    /// <param name="mapa">Linhas do mapa, todas do mesmo tamanho, com um <c>A</c> e um <c>B</c>.</param>
    /// <returns>Custo, nós explorados e a rota.</returns>
    public static ResultadoRota AEstrela(IReadOnlyList<string> mapa) => Buscar(mapa, usarHeuristica: true);

    /// <summary>
    /// Porcentagem (arredondada) de nós que a segunda busca explorou a menos que a primeira.
    /// No Reel: Dijkstra 89, A* 27, ou seja, 70% menos.
    /// </summary>
    /// <param name="nosPrimeira">Nós explorados pela primeira busca (maior que zero).</param>
    /// <param name="nosSegunda">Nós explorados pela segunda busca.</param>
    /// <returns>A redução em porcentagem.</returns>
    public static int PorcentagemAMenos(int nosPrimeira, int nosSegunda)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(nosPrimeira, 1);
        return (100 * (nosPrimeira - nosSegunda) + nosPrimeira / 2) / nosPrimeira;
    }

    private static ResultadoRota Buscar(IReadOnlyList<string> mapa, bool usarHeuristica)
    {
        var (partida, destino) = ValidarEAcharPontas(mapa);
        int linhas = mapa.Count, colunas = mapa[0].Length;

        // custo[e] = menor custo conhecido da partida até a esquina e (começa "infinito").
        var custo = new Dictionary<Esquina, int>();
        for (int l = 0; l < linhas; l++)
            for (int c = 0; c < colunas; c++)
                custo[new Esquina(l, c)] = int.MaxValue;

        var feito = new HashSet<Esquina>();                // esquinas já exploradas (custo final)
        var fila = new PriorityQueue<Esquina, int>();      // sai sempre a de menor prioridade

        // H(e): distância Manhattan até o destino (quantas quadras, ignorando prédios e trânsito).
        int H(Esquina e) => Math.Abs(e.Linha - destino.Linha) + Math.Abs(e.Coluna - destino.Coluna);

        fila.Enqueue(partida, 0);
        custo[partida] = 0;

        while (fila.TryDequeue(out var u, out _))
        {
            if (!feito.Add(u)) continue; // já explorada (uma versão mais cara dela ficou na fila)

            if (u == destino)
                return new ResultadoRota(custo[destino], feito.Count, RefazerRota(mapa, partida, destino, custo), feito);

            foreach (var v in Vizinhos(mapa, u))
            {
                int novo = custo[u] + MapaDaCidade.Peso(mapa[v.Linha][v.Coluna]);
                if (novo >= custo[v]) continue; // já existe caminho igual ou mais barato até v

                custo[v] = novo;
                // A única linha que muda entre os dois algoritmos:
                fila.Enqueue(v, usarHeuristica ? novo + H(v) : novo);
            }
        }

        return new ResultadoRota(-1, feito.Count, [], feito); // destino inalcançável
    }

    private static IEnumerable<Esquina> Vizinhos(IReadOnlyList<string> mapa, Esquina u)
    {
        foreach (var (dl, dc) in Direcoes)
        {
            int l = u.Linha + dl, c = u.Coluna + dc;
            if (l >= 0 && l < mapa.Count && c >= 0 && c < mapa[0].Length && MapaDaCidade.Transitavel(mapa[l][c]))
                yield return new Esquina(l, c);
        }
    }

    // Refaz a rota de trás para frente: de cada esquina, volta para o vizinho cujo
    // custo + o peso do passo dá exatamente o custo atual.
    private static List<Esquina> RefazerRota(
        IReadOnlyList<string> mapa, Esquina partida, Esquina destino, Dictionary<Esquina, int> custo)
    {
        var rota = new List<Esquina> { destino };
        while (rota[^1] != partida)
        {
            var p = rota[^1];
            int pesoDoPasso = MapaDaCidade.Peso(mapa[p.Linha][p.Coluna]);
            rota.Add(Vizinhos(mapa, p).First(v => custo[v] != int.MaxValue && custo[v] + pesoDoPasso == custo[p]));
        }
        rota.Reverse();
        return rota;
    }

    private static (Esquina Partida, Esquina Destino) ValidarEAcharPontas(IReadOnlyList<string> mapa)
    {
        ArgumentNullException.ThrowIfNull(mapa);
        if (mapa.Count == 0 || mapa[0].Length == 0)
            throw new ArgumentException("O mapa não pode ser vazio.", nameof(mapa));
        if (mapa.Any(linha => linha.Length != mapa[0].Length))
            throw new ArgumentException("Todas as linhas do mapa precisam ter o mesmo tamanho.", nameof(mapa));

        Esquina? partida = null, destino = null;
        for (int l = 0; l < mapa.Count; l++)
        {
            for (int c = 0; c < mapa[l].Length; c++)
            {
                if (mapa[l][c] == 'A')
                    partida = partida is null ? new Esquina(l, c) : throw new ArgumentException("O mapa tem mais de um 'A'.", nameof(mapa));
                else if (mapa[l][c] == 'B')
                    destino = destino is null ? new Esquina(l, c) : throw new ArgumentException("O mapa tem mais de um 'B'.", nameof(mapa));
            }
        }

        if (partida is null || destino is null)
            throw new ArgumentException("O mapa precisa ter um 'A' (partida) e um 'B' (destino).", nameof(mapa));
        return (partida.Value, destino.Value);
    }
}
