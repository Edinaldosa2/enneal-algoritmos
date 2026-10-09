[Português](README.md) | English


# Broken access control (IDOR): the ownership check

Logged in as customer #41, the test of **your own API** requests `/api/pedidos/41` through `45`. Without an ownership check, the API returns 4 other people's orders; with the check, it answers **403** with no data.

> **Local and defensive test.** The "API" is a C# function and the orders sit in memory, with fictional names and documents (already masked). Nothing here generates network traffic.

Reel: (link coming soon)

To see each request: `dotnet run -- --requisicoes`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/idor-checagem-de-dono
dotnet run
```

## Expected output

```
Sem checagem de dono: requisições 5 | 200 ok 5 | 403 negado 0 | pedidos de outras pessoas vazados 4
Com checagem de dono: requisições 5 | 200 ok 1 | 403 negado 4 | pedidos de outras pessoas vazados 0
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

## Where the code is

- Full explanation, diagram and **How to defend**: [`docs/algoritmos/idor-checagem-de-dono.md`](../../docs/algoritmos/idor-checagem-de-dono.md)
- Commented code: [`src/Enneal.Algoritmos.ControleDeAcesso`](../../src/Enneal.Algoritmos.ControleDeAcesso)
