namespace Enneal.Algoritmos.ComparacaoSegura;

/// <summary>
/// O "custo" do modelo: conta quantas comparações de caractere uma função fez.
/// </summary>
/// <remarks>
/// É um <b>modelo didático e determinístico</b>, não uma medição de tempo: cada comparação de caractere vale 1.
/// Assim a diferença entre as duas versões aparece igual em qualquer computador, sem ruído.
/// </remarks>
public sealed class ContadorDeComparacoes
{
    private readonly Action<int, char, char>? _aoComparar;

    /// <summary>
    /// Cria um contador zerado.
    /// </summary>
    /// <param name="aoComparar">Opcional: chamado a cada comparação com (posição, esperado, recebido).</param>
    public ContadorDeComparacoes(Action<int, char, char>? aoComparar = null) => _aoComparar = aoComparar;

    /// <summary>Quantas comparações de caractere foram feitas até agora.</summary>
    public int Comparacoes { get; private set; }

    /// <summary>
    /// Registra uma comparação.
    /// </summary>
    /// <param name="posicao">Posição comparada (0 = primeiro caractere).</param>
    /// <param name="esperado">Caractere do segredo.</param>
    /// <param name="recebido">Caractere recebido.</param>
    public void Contar(int posicao, char esperado, char recebido)
    {
        Comparacoes++;
        _aoComparar?.Invoke(posicao, esperado, recebido);
    }
}
