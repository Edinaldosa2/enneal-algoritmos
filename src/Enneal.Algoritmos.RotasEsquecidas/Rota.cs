namespace Enneal.Algoritmos.RotasEsquecidas;

/// <summary>
/// O que o app responde num caminho.
/// </summary>
/// <param name="Status">Status HTTP (200, 302, 403 ou 404).</param>
/// <param name="Tipo">O que vem na resposta: "pagina", "json", "listagem", "git", "negado-padrao"...</param>
/// <param name="Sensivel">Se o conteúdo não deveria ser público (código-fonte, senhas, backups).</param>
public sealed record Rota(int Status, string Tipo, bool Sensivel);
