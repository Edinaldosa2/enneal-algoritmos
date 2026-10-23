using Microsoft.Data.Sqlite;

namespace Enneal.Algoritmos.ValidacaoEntrada;

/// <summary>
/// Uma tabela de clientes fictícios num banco SQLite em memória (some quando o objeto é descartado).
/// Mostra a mesma busca escrita do jeito errado (texto concatenado no SQL) e do jeito certo (parâmetro).
/// </summary>
public sealed class RepositorioDeClientes : IDisposable
{
    private readonly SqliteConnection _conexao;

    /// <summary>Cria o banco em memória com os 3 clientes fictícios do exemplo.</summary>
    public RepositorioDeClientes()
    {
        _conexao = new SqliteConnection("Data Source=:memory:");
        _conexao.Open();
        using var cmd = _conexao.CreateCommand();
        cmd.CommandText =
            "CREATE TABLE clientes (id INTEGER PRIMARY KEY, nome TEXT NOT NULL, email TEXT NOT NULL);" +
            "INSERT INTO clientes (nome, email) VALUES " +
            "('Ana Souza', 'ana@exemplo.example'), ('Bruno Lima', 'bruno@exemplo.example'), ('Carla Dias', 'carla@exemplo.example');";
        cmd.ExecuteNonQuery();
    }

    /// <summary>ERRADO: o texto do usuário entra DENTRO do SQL. <c>' OR '1'='1</c> muda a consulta e devolve todos.</summary>
    public IReadOnlyList<string> BuscarPorNomeErrado(string nome)
    {
        using var cmd = _conexao.CreateCommand();
        cmd.CommandText = "SELECT nome FROM clientes WHERE nome = '" + nome + "'";
        return Ler(cmd);
    }

    /// <summary>CERTO: o SQL é fixo e o texto vai como parâmetro; o banco o trata só como dado, nunca como código.</summary>
    public IReadOnlyList<string> BuscarPorNomeCerto(string nome)
    {
        using var cmd = _conexao.CreateCommand();
        cmd.CommandText = "SELECT nome FROM clientes WHERE nome = $nome";
        cmd.Parameters.AddWithValue("$nome", nome);
        return Ler(cmd);
    }

    /// <summary>Insere um cliente (sempre com parâmetros) e devolve o id gerado.</summary>
    public long Inserir(string nome, string email)
    {
        using var cmd = _conexao.CreateCommand();
        cmd.CommandText = "INSERT INTO clientes (nome, email) VALUES ($nome, $email); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$nome", nome);
        cmd.Parameters.AddWithValue("$email", email);
        return (long)cmd.ExecuteScalar()!;
    }

    private static List<string> Ler(SqliteCommand cmd)
    {
        var nomes = new List<string>();
        using var leitor = cmd.ExecuteReader();
        while (leitor.Read()) nomes.Add(leitor.GetString(0));
        return nomes;
    }

    /// <inheritdoc />
    public void Dispose() => _conexao.Dispose();
}
