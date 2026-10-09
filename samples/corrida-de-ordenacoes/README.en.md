[Português](README.md) | English


# Sorting race

Bubble vs Quick vs Merge on the same array of 28 numbers: 1st Quick (185), 2nd Merge (235), 3rd Bubble (536).

Reel: (link coming soon)

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/corrida-de-ordenacoes
dotnet run
```

## Expected output

```
Vetor inicial: 13 9 7 3 25 6 19 22 27 5 12 2 10 1 23 15 16 14 18 24 20 8 28 21 26 17 11 4

Bubble: comparações 375 | trocas 161 | escritas 0 | passos 536 | ordenado True
Quick: comparações 116 | trocas 69 | escritas 0 | passos 185 | ordenado True
Merge: comparações 99 | trocas 0 | escritas 136 | passos 235 | ordenado True

Chegada: 1º Quick (185) · 2º Merge (235) · 3º Bubble (536)
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

## Where the algorithm is

- Full explanation, diagram and complexity: [`docs/algoritmos/corrida-de-ordenacoes.md`](../../docs/algoritmos/corrida-de-ordenacoes.md)
- Commented code: [`src/Enneal.Algoritmos.Ordenacoes`](../../src/Enneal.Algoritmos.Ordenacoes)
