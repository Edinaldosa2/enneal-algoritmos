namespace Enneal.Algoritmos.ControleDeAcesso;

/// <summary>
/// Um pedido da loja de exemplo. Todos os nomes e documentos são fictícios (e os documentos já vêm mascarados).
/// </summary>
/// <param name="Id">Número do pedido, o que aparece na URL (<c>/api/pedidos/42</c>).</param>
/// <param name="Dono">Id do cliente dono do pedido.</param>
/// <param name="Nome">Nome do cliente.</param>
/// <param name="Documento">Documento do cliente, mascarado.</param>
/// <param name="Centavos">Total do pedido, em centavos.</param>
public sealed record Pedido(int Id, int Dono, string Nome, string Documento, int Centavos);

/// <summary>
/// A resposta da API: o status HTTP e, só quando o status é 200, o pedido.
/// </summary>
/// <param name="Status">200 (entregue), 403 (não é seu) ou 404 (não existe).</param>
/// <param name="Pedido">O pedido entregue, ou <see langword="null"/> quando a API negou ou não achou.</param>
public sealed record RespostaApi(int Status, Pedido? Pedido);
