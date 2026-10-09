[Português](README.md) | English


# Forgotten routes on your site: deny by default

Audit of the routes of **your own site** (`seu-site.example`) before deploy: of the 15 checklist routes, one leftover from deploy (`/.git/config`) answers 200 and exposes the repository. After the fix (deny by default and no directory listing), no sensitive route stays exposed.

> **Local and defensive audit.** The "server" is the app's route table, in memory, with fixed data. Nothing here generates network traffic.

Reel: (link coming soon)

To see each checked route: `dotnet run -- --rotas`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/rotas-esquecidas
dotnet run
```

## Expected output

```
Antes da correção: rotas conferidas 15 | 200 ok 6 | 302 1 | 403 1 | 404 7 | rotas sensíveis expostas 1 (/.git/config)
Depois da correção: rotas conferidas 15 | 200 ok 4 | 302 1 | 403 1 | 404 9 | rotas sensíveis expostas 0
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

## Where the code is

- Full explanation, diagram and **How to defend**: [`docs/algoritmos/rotas-esquecidas.md`](../../docs/algoritmos/rotas-esquecidas.md)
- Commented code: [`src/Enneal.Algoritmos.RotasEsquecidas`](../../src/Enneal.Algoritmos.RotasEsquecidas)
