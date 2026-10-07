namespace Enneal.Algoritmos.CaminhoInvasor;

/// <summary>
/// Resultado da busca do invasor.
/// </summary>
/// <param name="Invadido">Se a busca chegou no banco de dados (<c>B</c>).</param>
/// <param name="Passos">Células tiradas da fila e examinadas (o contador do Reel).</param>
/// <param name="Caminho">Caminho mais curto do início até o banco (vazio se não houver).</param>
/// <param name="Alcancadas">Todas as células que a busca conseguiu alcançar.</param>
public sealed record ResultadoInvasao(
    bool Invadido,
    int Passos,
    IReadOnlyList<Celula> Caminho,
    IReadOnlySet<Celula> Alcancadas);

/// <summary>
/// "Caminho do invasor": uma busca em largura (BFS) num mapa abstrato, para ensinar
/// defesa em camadas. Nada de rede, IPs ou alvos reais: o mapa é só uma lista de textos.
/// </summary>
/// <remarks>
/// A BFS explora o mapa em "ondas": primeiro tudo a 1 passo do início, depois tudo a
/// 2 passos, e assim por diante. Por isso, se existe caminho, ela encontra o MAIS CURTO.
/// Tempo O(V + E) e memória O(V), onde V é o número de células e E o de ligações.
/// </remarks>
public static class BuscaInvasor
{
    // Ordem dos vizinhos: baixo, direita, esquerda, cima.
    // A ordem muda quantos passos a busca leva até o banco, então é a mesma do Reel.
    private static readonly (int Dl, int Dc)[] Direcoes = [(1, 0), (0, 1), (0, -1), (-1, 0)];

    /// <summary>
    /// Procura um caminho do <c>I</c> até o <c>B</c> andando para cima, baixo, esquerda e
    /// direita, sem atravessar paredes (<c>#</c>).
    /// </summary>
    /// <param name="mapa">Linhas do mapa, todas do mesmo tamanho, com exatamente um <c>I</c>.</param>
    /// <returns>Se invadiu, quantos passos levou, o caminho e as células alcançadas.</returns>
    /// <exception cref="ArgumentException">Se o mapa for vazio, irregular ou não tiver um único <c>I</c>.</exception>
    public static ResultadoInvasao Invadir(IReadOnlyList<string> mapa)
    {
        var inicio = ValidarEAcharInicio(mapa);
        int linhas = mapa.Count, colunas = mapa[0].Length;

        Queue<Celula> fila = new([inicio]);      // células a examinar (primeiro a entrar, primeiro a sair)
        HashSet<Celula> visto = [inicio];        // células que já entraram na fila
        Dictionary<Celula, Celula> veioDe = [];  // célula -> de onde a busca chegou nela
        int passos = 0;

        while (fila.Count > 0)
        {
            var atual = fila.Dequeue();
            passos++;

            if (mapa[atual.Linha][atual.Coluna] == 'B') // chegou no banco de dados
                return new ResultadoInvasao(true, passos, RefazerCaminho(atual, veioDe), visto);

            foreach (var (dl, dc) in Direcoes)
            {
                var vizinho = new Celula(atual.Linha + dl, atual.Coluna + dc);
                if (vizinho.Linha < 0 || vizinho.Linha >= linhas ||
                    vizinho.Coluna < 0 || vizinho.Coluna >= colunas) continue; // fora do mapa
                if (mapa[vizinho.Linha][vizinho.Coluna] == '#') continue;      // parede: não passa
                if (!visto.Add(vizinho)) continue;                             // já visitada: não repete

                veioDe[vizinho] = atual;
                fila.Enqueue(vizinho);
            }
        }

        // A fila esvaziou sem encontrar o B: não existe caminho até o banco.
        return new ResultadoInvasao(false, passos, [], visto);
    }

    // Refaz o caminho do banco até o início seguindo "veioDe" de trás para frente.
    private static List<Celula> RefazerCaminho(Celula fim, Dictionary<Celula, Celula> veioDe)
    {
        var caminho = new List<Celula> { fim };
        while (veioDe.TryGetValue(caminho[^1], out var anterior))
            caminho.Add(anterior);
        caminho.Reverse();
        return caminho;
    }

    private static Celula ValidarEAcharInicio(IReadOnlyList<string> mapa)
    {
        ArgumentNullException.ThrowIfNull(mapa);
        if (mapa.Count == 0 || mapa[0].Length == 0)
            throw new ArgumentException("O mapa não pode ser vazio.", nameof(mapa));
        if (mapa.Any(linha => linha.Length != mapa[0].Length))
            throw new ArgumentException("Todas as linhas do mapa precisam ter o mesmo tamanho.", nameof(mapa));

        var inicios = new List<Celula>();
        for (int l = 0; l < mapa.Count; l++)
            for (int c = 0; c < mapa[l].Length; c++)
                if (mapa[l][c] == 'I') inicios.Add(new Celula(l, c));

        if (inicios.Count != 1)
            throw new ArgumentException("O mapa precisa ter exatamente um 'I' (início).", nameof(mapa));
        return inicios[0];
    }
}
