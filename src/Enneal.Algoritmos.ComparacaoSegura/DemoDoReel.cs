using System.Text;

namespace Enneal.Algoritmos.ComparacaoSegura;

/// <summary>
/// O resultado de um candidato nas duas comparações.
/// </summary>
/// <param name="Candidato">O texto comparado com o token.</param>
/// <param name="PrefixoIgual">Quantos caracteres do começo batem.</param>
/// <param name="CustoSaiCedo">Comparações do laço que sai cedo (prefixo + 1, ou o tamanho se todas baterem).</param>
/// <param name="RespostaSaiCedo">O que o laço que sai cedo respondeu.</param>
/// <param name="CustoTempoConstante">Comparações do modelo do FixedTimeEquals (sempre o tamanho).</param>
/// <param name="RespostaTempoConstante">O que o FixedTimeEquals de verdade respondeu.</param>
public sealed record ResultadoCandidato(string Candidato, int PrefixoIgual, int CustoSaiCedo, bool RespostaSaiCedo,
    int CustoTempoConstante, bool RespostaTempoConstante);

/// <summary>
/// O token de brinquedo e os 6 candidatos do Reel.
/// </summary>
public static class DemoDoReel
{
    /// <summary>O token de brinquedo (6 caracteres). Não é segredo de ninguém.</summary>
    public const string Token = "7F3A9C";

    /// <summary>Os 6 candidatos do Reel; só o último é igual ao token.</summary>
    public static IReadOnlyList<string> Candidatos { get; } = ["B04E21", "7F3D58", "7A0B6E", "7F3A92", "7F1C07", "7F3A9C"];

    /// <summary>
    /// Compara cada candidato com o token nas duas versões e confere que o modelo responde igual ao
    /// <see cref="System.Security.Cryptography.CryptographicOperations.FixedTimeEquals"/> real.
    /// No Reel: custos que saem cedo 1, 4, 2, 6, 3, 6; em tempo constante, 6 em todos; mesma resposta em 6/6.
    /// </summary>
    /// <param name="token">O segredo.</param>
    /// <param name="candidatos">Os candidatos.</param>
    /// <param name="aoComparar">Opcional: chamado a cada comparação com (versão "I" ou "S", candidato, posição, esperado, recebido).</param>
    /// <returns>Um resultado por candidato.</returns>
    /// <exception cref="InvalidOperationException">Se o modelo ou o laço discordarem do FixedTimeEquals.</exception>
    public static IReadOnlyList<ResultadoCandidato> Comparar(string token, IReadOnlyList<string> candidatos,
        Action<string, int, int, char, char>? aoComparar = null)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(candidatos);

        var resultados = new List<ResultadoCandidato>();
        for (int c = 0; c < candidatos.Count; c++)
        {
            string r = candidatos[c];
            int indice = c;
            var inseguro = new ContadorDeComparacoes(aoComparar is null ? null : (i, e, v) => aoComparar("I", indice, i, e, v));
            bool okI = ComparacaoDeSegredos.IgualInseguro(token, r, inseguro);

            var seguro = new ContadorDeComparacoes(aoComparar is null ? null : (i, e, v) => aoComparar("S", indice, i, e, v));
            bool okM = ComparacaoDeSegredos.ModeloFixedTime(Encoding.UTF8.GetBytes(token), Encoding.UTF8.GetBytes(r), seguro);
            bool okS = ComparacaoDeSegredos.IgualSeguro(token, r);   // o FixedTimeEquals de verdade

            if (okM != okS || okI != okS) throw new InvalidOperationException("O modelo diverge do FixedTimeEquals.");
            resultados.Add(new ResultadoCandidato(r, ComparacaoDeSegredos.PrefixoIgual(token, r),
                inseguro.Comparacoes, okI, seguro.Comparacoes, okS));
        }
        return resultados;
    }

    /// <summary>A comparação dos 6 candidatos do Reel.</summary>
    public static IReadOnlyList<ResultadoCandidato> Rodar() => Comparar(Token, Candidatos);
}
