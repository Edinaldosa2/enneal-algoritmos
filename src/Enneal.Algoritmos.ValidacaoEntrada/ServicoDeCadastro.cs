namespace Enneal.Algoritmos.ValidacaoEntrada;

/// <summary>Os campos crus de um formulário de cadastro, como chegam da requisição.</summary>
public sealed record FormularioCadastro(string? Nome, string? Email, string? Idade, string? Nascimento);

/// <summary>Resultado do cadastro: o id gerado ou a lista de erros (nada é gravado se houver erro).</summary>
public sealed record ResultadoCadastro(long? Id, IReadOnlyList<string> Erros)
{
    /// <summary>Se o cadastro foi gravado.</summary>
    public bool Ok => Id is not null;
}

/// <summary>
/// O serviço só conhece o CONTRATO <see cref="IValidadorDeEntrada"/>, recebido pelo construtor. Ele valida
/// todos os campos, junta os erros e só grava (com consulta parametrizada) quando tudo passou.
/// </summary>
public sealed class ServicoDeCadastro(IValidadorDeEntrada validador, RepositorioDeClientes repositorio)
{
    /// <summary>Regras dos campos do formulário.</summary>
    public static readonly RegraDeCampo Nome = new("nome", TipoDeDado.Texto, 60);
    /// <summary>Regra do e-mail.</summary>
    public static readonly RegraDeCampo Email = new("email", TipoDeDado.Email, 254);
    /// <summary>Regra da idade (0 a 130).</summary>
    public static readonly RegraDeCampo Idade = new("idade", TipoDeDado.Inteiro, 3, 0, 130);
    /// <summary>Regra da data de nascimento.</summary>
    public static readonly RegraDeCampo Nascimento = new("nascimento", TipoDeDado.Data, 10);

    /// <summary>Valida o formulário inteiro e grava só se tudo for válido.</summary>
    public ResultadoCadastro Cadastrar(FormularioCadastro f)
    {
        ArgumentNullException.ThrowIfNull(f);
        var resultados = new[]
        {
            validador.Validar(f.Nome, Nome),
            validador.Validar(f.Email, Email),
            validador.Validar(f.Idade, Idade),
            validador.Validar(f.Nascimento, Nascimento),
        };

        var erros = resultados.Where(r => !r.Valido).Select(r => r.Erro!).ToList();
        if (erros.Count > 0) return new ResultadoCadastro(null, erros);

        long id = repositorio.Inserir((string)resultados[0].Valor!, (string)resultados[1].Valor!);
        return new ResultadoCadastro(id, []);
    }
}
