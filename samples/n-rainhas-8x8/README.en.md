[Português](README.md) | English


# N-Queens 8x8

Solves the classic 8x8 with backtracking and draws the board: 876 attempts and 105 backtracks until the first solution.

Reel: (link coming soon)

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/n-rainhas-8x8
dotnet run
```

## Expected output

```
N-Rainhas 8x8 (backtracking)

Solução (coluna da rainha em cada linha, de cima para baixo):
0 4 7 5 2 6 1 3

tentativas 876 | voltas 105

    0 1 2 3 4 5 6 7
 0  R . . . . . . .
 1  . . . . R . . .
 2  . . . . . . . R
 3  . . . . . R . .
 4  . . R . . . . .
 5  . . . . . . R .
 6  . R . . . . . .
 7  . . . R . . . .
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

## Where the algorithm is

- Full explanation, diagram and complexity: [`docs/algoritmos/n-rainhas.md`](../../docs/algoritmos/n-rainhas.md)
- Commented code: [`src/Enneal.Algoritmos.NRainhas`](../../src/Enneal.Algoritmos.NRainhas)
