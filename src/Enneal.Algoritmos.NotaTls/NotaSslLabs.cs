using System.Security.Authentication;

namespace Enneal.Algoritmos.NotaTls;

/// <summary>
/// A nota do HTTPS.
/// </summary>
/// <param name="Letra">A+, A, A-, B, C, D, E, F, ou T (certificado sem confiança) e M (nome não bate).</param>
/// <param name="Pontos">Pontuação de 0 a 100 (protocolo, chave e cifra).</param>
/// <param name="Tetos">Os tetos aplicados, na ordem em que o cálculo os conferiu.</param>
public sealed record NotaHttps(string Letra, double Pontos, IReadOnlyList<string> Tetos);

/// <summary>
/// A nota no estilo do <i>SSL Server Rating Guide</i> do SSL Labs (versão 2009r): pontos por protocolo, chave e
/// cifra, uma letra pela faixa de pontos e depois os <b>tetos</b> (regras que limitam a nota máxima).
/// Não é a nota oficial do SSL Labs; é a mesma conta do notebook do Kaggle, para estudar.
/// </summary>
public static class NotaSslLabs
{
    private static readonly string[] OrdemDasLetras = ["A+", "A", "A-", "B", "C", "D", "E", "F"];

    /// <summary>
    /// Calcula a nota de um servidor, sabendo onde a conexão parou.
    /// </summary>
    /// <param name="s">Como o servidor se comporta.</param>
    /// <param name="q">Onde a conexão parou (só importa se parou no certificado).</param>
    /// <returns>Letra, pontos e tetos aplicados.</returns>
    public static NotaHttps Calcular(ServidorDeTeste s, ResultadoConexao q)
    {
        ArgumentNullException.ThrowIfNull(s);
        ArgumentNullException.ThrowIfNull(q);

        // Pontos: 30% protocolo, 30% chave, 40% cifra (cada um é a média do melhor e do pior caso).
        double p = (Protocolo(s.Melhor) + Protocolo(s.Pior)) / 2;
        double k = Chave(s.ChaveBits);
        double c = (Cifra(s.CifraMax) + Cifra(s.CifraMin)) / 2;
        double pontos = 0.3 * p + 0.3 * k + 0.4 * c;

        var tetos = new List<string>();
        string n = Letra(pontos);
#pragma warning disable SYSLIB0039 // comparar com TLS 1.2 não usa TLS 1.0, só o valor do enum
        if (s.Pior < SslProtocols.Tls12) n = Teto(n, "B", tetos);    // ainda aceita TLS 1.0 ou 1.1
        if (s.Melhor < SslProtocols.Tls12) n = Teto(n, "C", tetos);  // nem fala TLS 1.2
#pragma warning restore SYSLIB0039
        if (!s.Sigilo || !s.Aead) n = Teto(n, "B", tetos);           // sem sigilo futuro ou sem AEAD
        if (s.Melhor < SslProtocols.Tls13) n = Teto(n, "A-", tetos); // sem TLS 1.3
        if (s.HstsDias == 0) n = Teto(n, "A-", tetos);               // sem HSTS
        if (n == "A" && s.HstsDias >= 180) n = "A+";                 // A com HSTS longo vira A+

        // Certificado ruim derruba tudo: M = nome não bate, T = sem confiança (expirado ou autoassinado).
        if (q.Camada == "Certificado")
            n = q.Motivo.StartsWith("nome", StringComparison.Ordinal) ? "M" : "T";

        return new NotaHttps(n, pontos, tetos);
    }

    /// <summary>
    /// A nota que o servidor teria se o certificado fosse confiável (o que aparece entre parênteses no Reel).
    /// </summary>
    /// <param name="s">Como o servidor se comporta.</param>
    /// <returns>A nota ignorando o problema de certificado.</returns>
    public static NotaHttps SemContarOCertificado(ServidorDeTeste s) => Calcular(s, new ResultadoConexao("HTTP", "", Quebrou: false));

    /// <summary>Pontos do protocolo: TLS 1.0 = 90, TLS 1.1 = 95, TLS 1.2 ou mais = 100.</summary>
    /// <param name="protocolo">Versão de TLS.</param>
#pragma warning disable SYSLIB0039
    public static double Protocolo(SslProtocols protocolo) => protocolo switch
    {
        SslProtocols.Tls => 90,
        SslProtocols.Tls11 => 95,
        _ => 100,
    };
#pragma warning restore SYSLIB0039

    /// <summary>
    /// Pontos da chave: menos de 512 bits = 20, menos de 1024 = 40, menos de 2048 = 80, menos de 4096 = 90.
    /// Como a conta do notebook limita a chave em 3072 bits, o máximo na prática é 90.
    /// </summary>
    /// <param name="bits">Tamanho da chave RSA.</param>
    public static double Chave(int bits) => Math.Min(bits, 3072) switch
    {
        < 512 => 20,
        < 1024 => 40,
        < 2048 => 80,
        < 4096 => 90,
        _ => 100,
    };

    /// <summary>Pontos da cifra: 0 bits = 0, menos de 128 = 20, menos de 256 = 80, 256 ou mais = 100.</summary>
    /// <param name="bits">Bits da cifra.</param>
    public static double Cifra(int bits) => bits switch
    {
        0 => 0,
        < 128 => 20,
        < 256 => 80,
        _ => 100,
    };

    /// <summary>Letra pela faixa de pontos: 80 = A, 65 = B, 50 = C, 35 = D, 20 = E, abaixo = F.</summary>
    /// <param name="pontos">Pontuação de 0 a 100.</param>
    public static string Letra(double pontos) =>
        pontos >= 80 ? "A" : pontos >= 65 ? "B" : pontos >= 50 ? "C" : pontos >= 35 ? "D" : pontos >= 20 ? "E" : "F";

    // Aplica um teto: a nota fica com a PIOR entre a atual e o teto. Registra o teto conferido.
    private static string Teto(string nota, string teto, List<string> tetos)
    {
        tetos.Add(teto);
        return Array.IndexOf(OrdemDasLetras, teto) > Array.IndexOf(OrdemDasLetras, nota) ? teto : nota;
    }
}
