[Português](README.md) | English


# Linear search vs binary search

Same sorted array of 1,024 items: linear up to 1,024 comparisons, binary up to 11. With 1 million: 1,000,000 vs 20.

Reel: (link coming soon)

To see each comparison: `dotnet run -- --eventos`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/busca-linear-vs-binaria
dotnet run
```

## Expected output

```
Rodada 1: alvo 19 | linear 7 comparações (posição 6) | binária 10 comparações (posição 6)
Rodada 2: alvo 1565 | linear 613 comparações (posição 612) | binária 10 comparações (posição 612)
Rodada 3: alvo 2562 | linear 1024 comparações (posição 1023) | binária 11 comparações (posição 1023)
1024 itens: linear até 1024 | binária até 11 (média 9.01 achando cada item)
1000000 itens: linear até 1000000 | binária até 20 (média 18.95 achando cada item)
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

## Where the algorithm is

- Full explanation, diagram and complexity: [`docs/algoritmos/busca-linear-vs-binaria.md`](../../docs/algoritmos/busca-linear-vs-binaria.md)
- Commented code: [`src/Enneal.Algoritmos.Busca`](../../src/Enneal.Algoritmos.Busca)
