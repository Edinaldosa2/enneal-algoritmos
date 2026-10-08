using System.Security.Cryptography;
using System.Text;

namespace Enneal.Algoritmos.ComparacaoSegura;

/// <summary>
/// As duas formas de conferir um segredo (token de API, assinatura HMAC, código de reset) do Reel.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IgualInseguro"/> é o laço que sai no primeiro caractere diferente: quanto mais letras certas no começo,
/// mais ele demora. Esse "quanto demora" vaza o prefixo certo. O <c>==</c> de <see cref="string"/> também para no
/// primeiro trecho diferente (é otimizado, mas não é tempo constante).
/// </para>
/// <para>
/// <see cref="IgualSeguro"/> usa <see cref="CryptographicOperations.FixedTimeEquals"/>: percorre todos os bytes,
/// acumulando a diferença, sem <c>if</c> e sem saída cedo. O custo é sempre o mesmo.
/// </para>
/// </remarks>
public static class ComparacaoDeSegredos
{
    /// <summary>
    /// ERRADO para segredo: compara caractere por caractere e sai no primeiro diferente.
    /// </summary>
    /// <param name="esperado">O segredo.</param>
    /// <param name="recebido">O que chegou na requisição.</param>
    /// <param name="contador">Opcional: conta as comparações (o custo do modelo).</param>
    /// <returns><see langword="true"/> se forem iguais.</returns>
    public static bool IgualInseguro(string esperado, string recebido, ContadorDeComparacoes? contador = null)
    {
        ArgumentNullException.ThrowIfNull(esperado);
        ArgumentNullException.ThrowIfNull(recebido);

        if (esperado.Length != recebido.Length) return false;
        for (int i = 0; i < esperado.Length; i++)
        {
            contador?.Contar(i, esperado[i], recebido[i]);
            if (esperado[i] != recebido[i])
                return false;           // sai no 1º diferente: o custo depende do prefixo certo
        }
        return true;
    }

    /// <summary>
    /// CERTO: compara os bytes em tempo constante com <see cref="CryptographicOperations.FixedTimeEquals"/>.
    /// </summary>
    /// <param name="esperado">O segredo.</param>
    /// <param name="recebido">O que chegou na requisição.</param>
    /// <returns><see langword="true"/> se forem iguais.</returns>
    /// <remarks>
    /// Tamanhos diferentes devolvem <see langword="false"/> na hora: o tamanho não é segredo. Se for, compare um
    /// HMAC ou hash de tamanho fixo dos dois lados.
    /// </remarks>
    public static bool IgualSeguro(string esperado, string recebido)
    {
        ArgumentNullException.ThrowIfNull(esperado);
        ArgumentNullException.ThrowIfNull(recebido);

        byte[] a = Encoding.UTF8.GetBytes(esperado);
        byte[] b = Encoding.UTF8.GetBytes(recebido);
        return CryptographicOperations.FixedTimeEquals(a, b);
    }

    /// <summary>
    /// O que o <see cref="CryptographicOperations.FixedTimeEquals"/> faz por dentro (modelo, com contador).
    /// </summary>
    /// <param name="a">Bytes do segredo.</param>
    /// <param name="b">Bytes recebidos.</param>
    /// <param name="contador">Opcional: conta as comparações (sempre o tamanho inteiro).</param>
    /// <returns><see langword="true"/> se forem iguais.</returns>
    public static bool ModeloFixedTime(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, ContadorDeComparacoes? contador = null)
    {
        if (a.Length != b.Length) return false; // o tamanho não é segredo
        int dif = 0;
        for (int i = 0; i < a.Length; i++)
        {
            contador?.Contar(i, (char)a[i], (char)b[i]);
            dif |= a[i] - b[i];   // sem if, sem saída cedo
        }
        return dif == 0;
    }

    /// <summary>
    /// Quantos caracteres do começo batem (o que o laço que sai cedo deixa vazar).
    /// </summary>
    /// <param name="esperado">O segredo.</param>
    /// <param name="recebido">O candidato.</param>
    /// <returns>O tamanho do prefixo igual.</returns>
    public static int PrefixoIgual(string esperado, string recebido)
    {
        ArgumentNullException.ThrowIfNull(esperado);
        ArgumentNullException.ThrowIfNull(recebido);
        return esperado.Zip(recebido).TakeWhile(p => p.First == p.Second).Count();
    }
}
