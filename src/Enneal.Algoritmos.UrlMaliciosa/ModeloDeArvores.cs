using System.Text.Json;

namespace Enneal.Algoritmos.UrlMaliciosa;

/// <summary>
/// Um conjunto de árvores de decisão (o LightGBM do notebook, exportado para ONNX como
/// <c>TreeEnsembleClassifier</c>), avaliado em C# puro: sem ONNX Runtime e sem pacote nenhum.
/// </summary>
/// <remarks>
/// <para>Cada árvore faz perguntas do tipo "a pista 18 é ≤ 2,5?" e termina numa folha com um peso. O modelo
/// soma os pesos das folhas de todas as árvores e passa a soma pela função logística, que vira uma
/// probabilidade entre 0 e 1.</para>
/// <para>O arquivo <c>url_model.onnx</c> é o do notebook, sem mudança. Este leitor entende só o que esse modelo
/// usa (nós <c>BRANCH_LEQ</c> e <c>LEAF</c>, saída <c>LOGISTIC</c>, 2 classes); qualquer outra coisa dá erro.</para>
/// </remarks>
public sealed class ModeloDeArvores
{
    private readonly int[] _pista;       // qual pista o nó pergunta
    private readonly float[] _limite;    // vai para "sim" se pista <= limite
    private readonly int[] _sim;         // índice do próximo nó se sim
    private readonly int[] _nao;         // índice do próximo nó se não
    private readonly bool[] _faltaVaiSim; // valor ausente (NaN) vai para "sim"?
    private readonly bool[] _folha;
    private readonly float[] _peso;      // peso da folha
    private readonly int[] _raizes;      // o primeiro nó de cada árvore

    private ModeloDeArvores(int[] pista, float[] limite, int[] sim, int[] nao, bool[] faltaVaiSim, bool[] folha,
        float[] peso, int[] raizes, int pistas)
    {
        (_pista, _limite, _sim, _nao, _faltaVaiSim, _folha, _peso, _raizes) = (pista, limite, sim, nao, faltaVaiSim, folha, peso, raizes);
        QuantidadeDePistas = pistas;
    }

    /// <summary>Quantas árvores o modelo tem (212 no modelo do Reel).</summary>
    public int Arvores => _raizes.Length;

    /// <summary>Quantos nós (perguntas + folhas) o modelo tem.</summary>
    public int Nos => _folha.Length;

    /// <summary>Quantas pistas o modelo espera (a maior pista usada + 1).</summary>
    public int QuantidadeDePistas { get; }

    /// <summary>O modelo do notebook, que vai dentro da DLL.</summary>
    public static ModeloDeArvores DoReel { get; } = CarregarEmbutido();

    /// <summary>
    /// Lê um arquivo .onnx com um <c>TreeEnsembleClassifier</c> binário.
    /// </summary>
    /// <param name="onnx">O conteúdo do arquivo.</param>
    /// <returns>O modelo pronto para avaliar.</returns>
    public static ModeloDeArvores Carregar(byte[] onnx)
    {
        ArgumentNullException.ThrowIfNull(onnx);
        var atributos = AtributosDoEnsemble(onnx);

        string Texto(string nome) => atributos.TryGetValue(nome, out var a) && a.Texto is not null
            ? a.Texto : throw new InvalidDataException($"Falta o atributo {nome}.");
        List<long> Inteiros(string nome) => atributos.TryGetValue(nome, out var a) ? a.Inteiros
            : throw new InvalidDataException($"Falta o atributo {nome}.");
        List<float> Reais(string nome) => atributos.TryGetValue(nome, out var a) ? a.Reais
            : throw new InvalidDataException($"Falta o atributo {nome}.");
        List<string> Textos(string nome) => atributos.TryGetValue(nome, out var a) ? a.Textos
            : throw new InvalidDataException($"Falta o atributo {nome}.");

        if (Texto("post_transform") != "LOGISTIC") throw new NotSupportedException("Só a saída LOGISTIC é suportada.");
        if (Inteiros("classlabels_int64s").Count != 2) throw new NotSupportedException("Só modelos de 2 classes.");
        if (atributos.ContainsKey("base_values")) throw new NotSupportedException("base_values não é suportado.");

        var arvore = Inteiros("nodes_treeids");
        var no = Inteiros("nodes_nodeids");
        var pista = Inteiros("nodes_featureids");
        var limite = Reais("nodes_values");
        var modo = Textos("nodes_modes");
        var sim = Inteiros("nodes_truenodeids");
        var nao = Inteiros("nodes_falsenodeids");
        var falta = atributos.TryGetValue("nodes_missing_value_tracks_true", out var f) ? f.Inteiros : [];
        int total = arvore.Count;

        // Cada nó vira um índice num vetor só; (árvore, nó) -> índice.
        var indice = new Dictionary<(long, long), int>(total);
        for (int i = 0; i < total; i++) indice[(arvore[i], no[i])] = i;

        var rPista = new int[total];
        var rLimite = new float[total];
        var rSim = new int[total];
        var rNao = new int[total];
        var rFalta = new bool[total];
        var rFolha = new bool[total];
        var rPeso = new float[total];
        for (int i = 0; i < total; i++)
        {
            rFolha[i] = modo[i] switch
            {
                "LEAF" => true,
                "BRANCH_LEQ" => false,
                _ => throw new NotSupportedException($"Nó do tipo {modo[i]} não é suportado."),
            };
            if (rFolha[i]) continue;
            rPista[i] = checked((int)pista[i]);
            rLimite[i] = limite[i];
            rSim[i] = indice[(arvore[i], sim[i])];
            rNao[i] = indice[(arvore[i], nao[i])];
            rFalta[i] = falta.Count > i && falta[i] != 0;
        }

        // Pesos das folhas (num modelo binário, todos são da mesma classe).
        var pArvore = Inteiros("class_treeids");
        var pNo = Inteiros("class_nodeids");
        var pClasse = Inteiros("class_ids");
        var pPeso = Reais("class_weights");
        if (pClasse.Distinct().Count() != 1) throw new NotSupportedException("Só o caso binário (uma classe nos pesos).");
        for (int i = 0; i < pPeso.Count; i++) rPeso[indice[(pArvore[i], pNo[i])]] += pPeso[i];

        int[] raizes = [.. arvore.Distinct().Order().Select(t => indice[(t, 0)])];
        int pistas = (int)pista.Where((_, i) => !rFolha[i]).DefaultIfEmpty(-1).Max() + 1;
        return new ModeloDeArvores(rPista, rLimite, rSim, rNao, rFalta, rFolha, rPeso, raizes, pistas);
    }

    /// <summary>
    /// A probabilidade de a URL ser maliciosa, a partir das pistas.
    /// </summary>
    /// <param name="pistas">O vetor de <see cref="PistasDaUrl.Extrair"/>.</param>
    /// <returns>Probabilidade de 0 a 1.</returns>
    public float Probabilidade(IReadOnlyList<float> pistas)
    {
        ArgumentNullException.ThrowIfNull(pistas);
        if (pistas.Count < QuantidadeDePistas)
            throw new ArgumentException($"O modelo espera {QuantidadeDePistas} pistas.", nameof(pistas));

        double soma = 0;
        foreach (int raiz in _raizes)
        {
            int i = raiz;
            while (!_folha[i])
            {
                float x = pistas[_pista[i]];
                bool vaiSim = x <= _limite[i] || (float.IsNaN(x) && _faltaVaiSim[i]);
                i = vaiSim ? _sim[i] : _nao[i];
            }
            soma += _peso[i];
        }
        return (float)(1.0 / (1.0 + Math.Exp(-soma))); // função logística: soma -> probabilidade
    }

    private static ModeloDeArvores CarregarEmbutido()
    {
        using var fluxo = typeof(ModeloDeArvores).Assembly.GetManifestResourceStream("url_model.onnx")
            ?? throw new InvalidOperationException("O modelo não está embutido na DLL.");
        using var memoria = new MemoryStream();
        fluxo.CopyTo(memoria);
        return Carregar(memoria.ToArray());
    }

    // ---------- leitura do ONNX: ModelProto.graph (7) -> GraphProto.node (1) -> NodeProto.attribute (5) ----------

    private sealed class Atributo
    {
        public string? Texto;
        public List<long> Inteiros { get; } = [];
        public List<float> Reais { get; } = [];
        public List<string> Textos { get; } = [];
    }

    private static Dictionary<string, Atributo> AtributosDoEnsemble(byte[] onnx)
    {
        var modelo = new LeitorProtobuf(onnx, 0, onnx.Length);
        while (!modelo.Acabou)
        {
            var (campo, tipo) = modelo.Etiqueta();
            if (campo != 7 || tipo != 2) { modelo.Pular(tipo); continue; }

            var grafo = modelo.Bloco();
            while (!grafo.Acabou)
            {
                var (cg, tg) = grafo.Etiqueta();
                if (cg != 1 || tg != 2) { grafo.Pular(tg); continue; }
                var atributos = LerNo(grafo.Bloco(), out string tipoDoNo);
                if (tipoDoNo == "TreeEnsembleClassifier") return atributos;
            }
        }
        throw new InvalidDataException("O arquivo não tem um TreeEnsembleClassifier.");
    }

    private static Dictionary<string, Atributo> LerNo(LeitorProtobuf no, out string tipoDoNo)
    {
        tipoDoNo = "";
        var atributos = new Dictionary<string, Atributo>();
        while (!no.Acabou)
        {
            var (campo, tipo) = no.Etiqueta();
            if (campo == 4 && tipo == 2) tipoDoNo = no.Texto();                  // op_type
            else if (campo == 5 && tipo == 2) LerAtributo(no.Bloco(), atributos); // attribute
            else no.Pular(tipo);
        }
        return atributos;
    }

    // AttributeProto: name (1), f (2), i (3), s (4), floats (7), ints (8), strings (9). Listas podem vir "packed".
    private static void LerAtributo(LeitorProtobuf a, Dictionary<string, Atributo> destino)
    {
        string nome = "";
        var atr = new Atributo();
        while (!a.Acabou)
        {
            var (campo, tipo) = a.Etiqueta();
            switch (campo, tipo)
            {
                case (1, 2): nome = a.Texto(); break;
                case (2, 5): atr.Reais.Add(a.Float32()); break;
                case (3, 0): atr.Inteiros.Add((long)a.Varint()); break;
                case (4, 2): atr.Texto = a.Texto(); break;
                case (7, 5): atr.Reais.Add(a.Float32()); break;
                case (7, 2):
                    var reais = a.Bloco();
                    while (!reais.Acabou) atr.Reais.Add(reais.Float32());
                    break;
                case (8, 0): atr.Inteiros.Add((long)a.Varint()); break;
                case (8, 2):
                    var inteiros = a.Bloco();
                    while (!inteiros.Acabou) atr.Inteiros.Add((long)inteiros.Varint());
                    break;
                case (9, 2): atr.Textos.Add(a.Texto()); break;
                default: a.Pular(tipo); break;
            }
        }
        destino[nome] = atr;
    }
}

/// <summary>
/// Os metadados que o notebook gravou junto com o modelo (<c>model_meta.json</c>).
/// </summary>
/// <param name="VersaoDasPistas">Versão do extrator usado no treino.</param>
/// <param name="NomesDasPistas">Nomes das pistas, na ordem.</param>
/// <param name="Limiar">Limiar escolhido no notebook para 1% de falso positivo na validação.</param>
public sealed record MetadadosDoModelo(string VersaoDasPistas, IReadOnlyList<string> NomesDasPistas, float Limiar)
{
    /// <summary>Os metadados do modelo do Reel, embutidos na DLL.</summary>
    public static MetadadosDoModelo DoReel { get; } = CarregarEmbutido();

    private static MetadadosDoModelo CarregarEmbutido()
    {
        using var fluxo = typeof(MetadadosDoModelo).Assembly.GetManifestResourceStream("model_meta.json")
            ?? throw new InvalidOperationException("model_meta.json não está embutido na DLL.");
        using var doc = JsonDocument.Parse(fluxo);
        var raiz = doc.RootElement;
        return new MetadadosDoModelo(
            raiz.GetProperty("featureVersion").GetString()!,
            [.. raiz.GetProperty("featureNames").EnumerateArray().Select(e => e.GetString()!)],
            (float)raiz.GetProperty("threshold").GetDouble());
    }
}
