namespace Enneal.Algoritmos.ControleDeAcesso;

/// <summary>
/// O resultado de um teste de acesso: o que aconteceu com cada id pedido.
/// </summary>
/// <param name="Requisicoes">Quantas requisições o teste fez.</param>
/// <param name="Ok">Quantas voltaram 200.</param>
/// <param name="Negadas">Quantas voltaram 403 ou 404.</param>
/// <param name="Vazadas">Quantas voltaram 200 com o pedido de OUTRA pessoa (o vazamento).</param>
public sealed record ResultadoTesteDeAcesso(int Requisicoes, int Ok, int Negadas, int Vazadas);

/// <summary>
/// O teste automatizado que pega IDOR: logado como um cliente, pede os pedidos dos vizinhos e conta quantos vazam.
/// Rode um teste assim na SUA API (em ambiente de teste), nunca na de outra pessoa.
/// </summary>
public static class TesteDeAcesso
{
    /// <summary>Id da usuária logada no Reel (Ana, cliente #41).</summary>
    public const int UsuariaDoReel = 41;

    /// <summary>
    /// Pede cada id de <paramref name="ids"/> logado como <paramref name="usuarioDaSessao"/>.
    /// No Reel (ids 41 a 45, logada como 41): sem checagem, 5 respostas 200 e 4 vazadas;
    /// com checagem, 1 resposta 200 e 4 respostas 403, nenhuma vazada.
    /// </summary>
    /// <param name="api">A API testada.</param>
    /// <param name="ids">Os ids pedidos.</param>
    /// <param name="usuarioDaSessao">Quem está logado.</param>
    /// <param name="checarDono">Se a API faz a checagem de dono.</param>
    /// <param name="aoResponder">Opcional: chamado a cada resposta com (id, resposta, vazou).</param>
    /// <returns>As contagens do teste.</returns>
    public static ResultadoTesteDeAcesso Rodar(ApiDePedidos api, IEnumerable<int> ids, int usuarioDaSessao,
        bool checarDono, Action<int, RespostaApi, bool>? aoResponder = null)
    {
        ArgumentNullException.ThrowIfNull(api);
        ArgumentNullException.ThrowIfNull(ids);

        int total = 0, ok = 0, negadas = 0, vazadas = 0;
        foreach (int id in ids)
        {
            var r = api.Buscar(id, usuarioDaSessao, checarDono);
            bool vazou = r.Status == 200 && r.Pedido!.Dono != usuarioDaSessao; // 200 com dado de outra pessoa
            total++;
            if (r.Status == 200) ok++; else negadas++;
            if (vazou) vazadas++;
            aoResponder?.Invoke(id, r, vazou);
        }
        return new ResultadoTesteDeAcesso(total, ok, negadas, vazadas);
    }
}
