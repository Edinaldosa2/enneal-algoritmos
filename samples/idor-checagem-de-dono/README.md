Português | [English](README.en.md)

# Controle de acesso quebrado (IDOR): a checagem de dono

Logada como a cliente #41, o teste da **sua própria API** pede `/api/pedidos/41` a `45`. Sem checagem de dono, a API entrega 4 pedidos de outras pessoas; com a checagem, responde **403** sem nenhum dado.

> **Teste local e defensivo.** A "API" é uma função C# e os pedidos estão em memória, com nomes e documentos fictícios (já mascarados). Nada aqui gera tráfego de rede.

Reel: (link em breve)

Para ver cada requisição: `dotnet run -- --requisicoes`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/idor-checagem-de-dono
dotnet run
```

## Saída esperada

```
Sem checagem de dono: requisições 5 | 200 ok 5 | 403 negado 0 | pedidos de outras pessoas vazados 4
Com checagem de dono: requisições 5 | 200 ok 1 | 403 negado 4 | pedidos de outras pessoas vazados 0
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

## Onde está o código

- Explicação completa, diagrama e **Como se defender**: [`docs/algoritmos/idor-checagem-de-dono.md`](../../docs/algoritmos/idor-checagem-de-dono.md)
- Código comentado: [`src/Enneal.Algoritmos.ControleDeAcesso`](../../src/Enneal.Algoritmos.ControleDeAcesso)
