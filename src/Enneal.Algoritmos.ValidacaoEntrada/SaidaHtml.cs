using System.Text.Encodings.Web;

namespace Enneal.Algoritmos.ValidacaoEntrada;

/// <summary>
/// Validar a entrada não basta contra XSS: um nome como <c>O'Brien &lt;3</c> é texto legítimo. A defesa é
/// codificar na SAÍDA, conforme o contexto (aqui, corpo HTML), com o <see cref="HtmlEncoder"/> do .NET.
/// </summary>
public static class SaidaHtml
{
    /// <summary>ERRADO: cola o texto no HTML como veio. Um <c>&lt;script&gt;</c> vira código no navegador.</summary>
    public static string ParagrafoErrado(string texto) => $"<p>{texto}</p>";

    /// <summary>CERTO: codifica o texto para o contexto de corpo HTML antes de montar a página.</summary>
    public static string ParagrafoCerto(string texto) => $"<p>{HtmlEncoder.Default.Encode(texto)}</p>";
}
