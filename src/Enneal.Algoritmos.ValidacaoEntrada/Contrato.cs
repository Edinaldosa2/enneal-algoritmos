namespace Enneal.Algoritmos.ValidacaoEntrada;

/// <summary>Os tipos de dado que a validação conhece. Qualquer outro é recusado.</summary>
public enum TipoDeDado
{
    /// <summary>Número inteiro (<see cref="int"/>), só dígitos e sinal opcional.</summary>
    Inteiro,
    /// <summary>Número decimal (<see cref="decimal"/>) com ponto, sem expoente.</summary>
    Decimal,
    /// <summary>Endereço de e-mail simples, sem nome de exibição.</summary>
    Email,
    /// <summary>Data no formato ISO 8601 <c>aaaa-MM-dd</c>.</summary>
    Data,
    /// <summary>Texto livre: letras, números, pontuação, símbolos e espaço.</summary>
    Texto,
}

/// <summary>A regra de um campo: o tipo esperado, o tamanho máximo e (para números) a faixa.</summary>
/// <param name="Nome">Nome do campo (aparece na mensagem de erro).</param>
/// <param name="Tipo">Tipo esperado.</param>
/// <param name="TamanhoMaximo">Máximo de caracteres depois da normalização.</param>
/// <param name="Minimo">Menor valor aceito (só números).</param>
/// <param name="Maximo">Maior valor aceito (só números).</param>
public sealed record RegraDeCampo(string Nome, TipoDeDado Tipo, int TamanhoMaximo, decimal? Minimo = null, decimal? Maximo = null);

/// <summary>Resultado de uma validação: ou o valor já convertido para o tipo, ou o motivo da recusa.</summary>
/// <param name="Valido">Se a entrada foi aceita.</param>
/// <param name="Valor">O valor convertido (<see cref="int"/>, <see cref="decimal"/>, <see cref="DateOnly"/> ou <see cref="string"/>).</param>
/// <param name="Erro">O motivo da recusa (nulo quando válido).</param>
public sealed record ResultadoValidacao(bool Valido, object? Valor, string? Erro)
{
    /// <summary>Entrada aceita.</summary>
    public static ResultadoValidacao Ok(object valor) => new(true, valor, null);

    /// <summary>Entrada recusada.</summary>
    public static ResultadoValidacao Falha(string erro) => new(false, null, erro);
}

/// <summary>
/// O contrato da validação. Quem precisa validar recebe esta interface pelo construtor (injeção de dependência)
/// e não sabe qual implementação está por trás: dá para trocar ou testar sem mudar o serviço.
/// </summary>
public interface IValidadorDeEntrada
{
    /// <summary>Normaliza e valida <paramref name="bruto"/> contra a <paramref name="regra"/>.</summary>
    ResultadoValidacao Validar(string? bruto, RegraDeCampo regra);

    /// <summary>Descobre o tipo mais específico em que o texto se encaixa (inteiro, decimal, data, e-mail ou texto).</summary>
    TipoDeDado DetectarTipo(string bruto);
}
