using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace Enneal.Algoritmos.ValidacaoEntrada;

/// <summary>
/// Implementação padrão de <see cref="IValidadorDeEntrada"/>. A ordem segue o OWASP Input Validation Cheat Sheet:
/// limite bruto, normalização (canonicalização), limite depois de normalizar, caracteres permitidos (allowlist)
/// e por fim a validação semântica do tipo, sempre com <c>TryParse</c> e cultura invariante. Sem regex.
/// </summary>
public sealed class ValidadorDeEntrada : IValidadorDeEntrada
{
    /// <summary>
    /// Teto do texto bruto, antes de normalizar: o TamanhoMaximo da regra vezes este fator.
    /// Evita gastar CPU normalizando uma entrada gigante que seria recusada de qualquer jeito.
    /// </summary>
    public const int FatorTetoBruto = 4;

    /// <summary>
    /// Falha fechada: no modo de globalização invariante (<c>InvariantGlobalization</c>), o .NET não tem os dados
    /// do Unicode e o <c>Normalize(FormKC)</c> devolve o texto sem mudar nada, em silêncio. Em vez de validar sem
    /// normalizar, o validador se recusa a nascer.
    /// </summary>
    /// <exception cref="InvalidOperationException">Se a normalização Unicode não estiver disponível.</exception>
    public ValidadorDeEntrada()
    {
        if ("\uFF1C".Normalize(NormalizationForm.FormKC) != "<")
            throw new InvalidOperationException(
                "Normalização Unicode indisponível (InvariantGlobalization ligado?). Desligue no projeto que roda o validador.");
    }

    /// <inheritdoc />
    public ResultadoValidacao Validar(string? bruto, RegraDeCampo regra)
    {
        ArgumentNullException.ThrowIfNull(regra);

        if (string.IsNullOrWhiteSpace(bruto))
            return ResultadoValidacao.Falha($"{regra.Nome}: vazio");

        // 1. Limite bruto: recusa cedo o que é grande demais.
        if (bruto.Length > regra.TamanhoMaximo * FatorTetoBruto)
            return ResultadoValidacao.Falha($"{regra.Nome}: grande demais");

        // 2. Canonicalização: NFKC transforma variantes "disfarçadas" (como o "＜" de largura total) na forma comum.
        //    Validar ANTES de normalizar deixaria o disfarce passar e virar "<" depois.
        if (!TentarNormalizar(bruto, out bruto))
            return ResultadoValidacao.Falha($"{regra.Nome}: Unicode inválido");
        string texto = bruto.Trim();

        // 3. Limite depois de normalizar (a normalização pode aumentar o texto).
        if (texto.Length > regra.TamanhoMaximo)
            return ResultadoValidacao.Falha($"{regra.Nome}: mais de {regra.TamanhoMaximo} caracteres");

        // 4. Allowlist de caracteres: só o que o texto livre pode ter. Controles, caracteres invisíveis de formato
        //    (como o U+202E, que inverte a direção do texto) e substitutos soltos são recusados.
        if (!SoCaracteresPermitidos(texto))
            return ResultadoValidacao.Falha($"{regra.Nome}: caractere não permitido");

        // 5. Validação semântica, por tipo.
        return regra.Tipo switch
        {
            TipoDeDado.Inteiro => ValidarInteiro(texto, regra),
            TipoDeDado.Decimal => ValidarDecimal(texto, regra),
            TipoDeDado.Data => TentarData(texto, out var d) ? ResultadoValidacao.Ok(d) : ResultadoValidacao.Falha($"{regra.Nome}: data inválida (use aaaa-MM-dd)"),
            TipoDeDado.Email => TentarEmail(texto, out var e) ? ResultadoValidacao.Ok(e) : ResultadoValidacao.Falha($"{regra.Nome}: e-mail inválido"),
            TipoDeDado.Texto => ResultadoValidacao.Ok(texto),
            _ => ResultadoValidacao.Falha($"{regra.Nome}: tipo desconhecido"),
        };
    }

    /// <inheritdoc />
    public TipoDeDado DetectarTipo(string bruto)
    {
        ArgumentNullException.ThrowIfNull(bruto);
        string texto = bruto.Trim();
        if (TentarInteiro(texto, out _)) return TipoDeDado.Inteiro;
        if (TentarDecimal(texto, out _)) return TipoDeDado.Decimal;
        if (TentarData(texto, out _)) return TipoDeDado.Data;
        if (TentarEmail(texto, out _)) return TipoDeDado.Email;
        return TipoDeDado.Texto;
    }

    // Só dígitos e sinal: sem espaço interno, sem separador de milhar, sem hexadecimal.
    private const NumberStyles EstiloInteiro = NumberStyles.AllowLeadingSign;

    // Sinal e ponto decimal: sem expoente ("1e9"), sem "NaN"/"Infinity" (decimal nem tem), sem milhar.
    private const NumberStyles EstiloDecimal = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    private static bool TentarInteiro(string s, out int valor) =>
        int.TryParse(s, EstiloInteiro, CultureInfo.InvariantCulture, out valor) && SoAscii(s);

    private static bool TentarDecimal(string s, out decimal valor) =>
        decimal.TryParse(s, EstiloDecimal, CultureInfo.InvariantCulture, out valor) && SoAscii(s);

    private static bool TentarData(string s, out DateOnly valor) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out valor);

    /// <summary>
    /// E-mail sem regex: <see cref="MailAddress.TryCreate(string?, out MailAddress?)"/> faz o parse e o resultado
    /// precisa ser exatamente o texto digitado (recusa nome de exibição, como <c>"Ana" &lt;a@b.c&gt;</c>), ASCII,
    /// até 254 caracteres e com ponto no domínio. A prova final de um e-mail continua sendo mandar um link de
    /// confirmação para ele.
    /// </summary>
    private static bool TentarEmail(string s, out string valor)
    {
        valor = "";
        if (s.Length > 254 || !SoAscii(s) || s.Contains(' ')) return false;
        if (!MailAddress.TryCreate(s, out var m) || m.Address != s || m.DisplayName.Length != 0) return false;
        string dominio = m.Host;
        if (!dominio.Contains('.') || dominio.StartsWith('.') || dominio.EndsWith('.') || dominio.Contains("..")) return false;
        valor = m.Address;
        return true;
    }

    private static ResultadoValidacao ValidarInteiro(string s, RegraDeCampo regra)
    {
        if (!TentarInteiro(s, out int v)) return ResultadoValidacao.Falha($"{regra.Nome}: não é um inteiro");
        return DentroDaFaixa(v, regra) ? ResultadoValidacao.Ok(v) : ResultadoValidacao.Falha($"{regra.Nome}: fora da faixa");
    }

    private static ResultadoValidacao ValidarDecimal(string s, RegraDeCampo regra)
    {
        if (!TentarDecimal(s, out decimal v)) return ResultadoValidacao.Falha($"{regra.Nome}: não é um decimal");
        return DentroDaFaixa(v, regra) ? ResultadoValidacao.Ok(v) : ResultadoValidacao.Falha($"{regra.Nome}: fora da faixa");
    }

    private static bool DentroDaFaixa(decimal v, RegraDeCampo r) =>
        (r.Minimo is null || v >= r.Minimo) && (r.Maximo is null || v <= r.Maximo);

    private static bool TentarNormalizar(string s, out string normalizado)
    {
        try
        {
            normalizado = s.Normalize(NormalizationForm.FormKC);
            return true;
        }
        catch (ArgumentException)
        {
            // Substituto (surrogate) solto: não é Unicode válido.
            normalizado = "";
            return false;
        }
    }

    private static bool SoAscii(string s)
    {
        foreach (char c in s)
            if (c > 127) return false;
        return true;
    }

    /// <summary>A allowlist: letras, marcas, números, pontuação, símbolos e o espaço comum. Mais nada.</summary>
    internal static bool SoCaracteresPermitidos(string s)
    {
        foreach (char c in s)
        {
            switch (char.GetUnicodeCategory(c))
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.DashPunctuation:
                case UnicodeCategory.OpenPunctuation:
                case UnicodeCategory.ClosePunctuation:
                case UnicodeCategory.InitialQuotePunctuation:
                case UnicodeCategory.FinalQuotePunctuation:
                case UnicodeCategory.OtherPunctuation:
                case UnicodeCategory.MathSymbol:
                case UnicodeCategory.CurrencySymbol:
                case UnicodeCategory.ModifierSymbol:
                    break;
                case UnicodeCategory.SpaceSeparator when c == ' ':
                    break;
                default:
                    return false; // controle, formato (invisível), substituto solto, não atribuído, outros espaços...
            }
        }
        return true;
    }
}
