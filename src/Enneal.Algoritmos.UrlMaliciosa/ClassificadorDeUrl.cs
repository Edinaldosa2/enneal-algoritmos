namespace Enneal.Algoritmos.UrlMaliciosa;

/// <summary>
/// O resultado da classificação de uma URL.
/// </summary>
/// <param name="Probabilidade">P(maliciosa), de 0 a 1.</param>
/// <param name="Maliciosa">Se a probabilidade passou do limiar.</param>
/// <param name="Pistas">As 30 pistas usadas.</param>
public sealed record AvaliacaoUrl(float Probabilidade, bool Maliciosa, IReadOnlyList<float> Pistas)
{
    /// <summary>"maliciosa" ou "ok", como no Reel.</summary>
    public string Veredito => Maliciosa ? "maliciosa" : "ok";
}

/// <summary>
/// O classificador do Reel: pistas léxicas + modelo de árvores + limiar.
/// Ele só LÊ o texto da URL: nunca acessa o endereço recebido.
/// </summary>
/// <remarks>
/// Um classificador de texto é uma camada a mais, não a única: erra (a URL 6 do Reel passa) e não substitui
/// listas de bloqueio atualizadas, filtro de e-mail, navegador atualizado e treinamento das pessoas.
/// </remarks>
public static class ClassificadorDeUrl
{
    /// <summary>Limiar do notebook (1% de falso positivo na validação): acima dele, "maliciosa".</summary>
    public const float Limiar = 0.768444f;

    /// <summary>Tamanho máximo aceito pela API do exemplo.</summary>
    public const int TamanhoMaximo = 2048;

    /// <summary>Se a URL pode ser avaliada (não vazia e com até <see cref="TamanhoMaximo"/> caracteres). Se não, a API responde 400.</summary>
    /// <param name="url">A URL recebida.</param>
    public static bool Aceita(string? url) => !string.IsNullOrWhiteSpace(url) && url.Length <= TamanhoMaximo;

    /// <summary>
    /// Classifica uma URL com o modelo do Reel.
    /// No Reel: 6 URLs de exemplo, 4 marcadas como maliciosas; a URL 6 (phishing) passa com 0,107.
    /// </summary>
    /// <param name="url">A URL, como texto.</param>
    /// <returns>Probabilidade, veredito e pistas.</returns>
    public static AvaliacaoUrl Avaliar(string url) => Avaliar(url, ModeloDeArvores.DoReel, Limiar);

    /// <summary>
    /// Classifica uma URL com outro modelo ou outro limiar.
    /// </summary>
    /// <param name="url">A URL, como texto.</param>
    /// <param name="modelo">O modelo de árvores.</param>
    /// <param name="limiar">Probabilidade a partir da qual a URL é marcada.</param>
    /// <returns>Probabilidade, veredito e pistas.</returns>
    public static AvaliacaoUrl Avaliar(string url, ModeloDeArvores modelo, float limiar)
    {
        ArgumentNullException.ThrowIfNull(modelo);
        float[] pistas = PistasDaUrl.Extrair(url); // só lê o texto
        float p = modelo.Probabilidade(pistas);
        return new AvaliacaoUrl(p, p >= limiar, pistas);
    }
}
