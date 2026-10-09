[Português](README.md) | English


# Tower of Hanoi (recursion)

Move the tower from peg A to peg C, one disk at a time, never placing a larger disk on a smaller one:
3 disks = 7 moves, 4 = 15, 6 = 63. With 64 disks: 18,446,744,073,709,551,615 moves.

Reel: (link coming soon)

To see each move: `dotnet run -- --movimentos`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/torre-de-hanoi
dotnet run
```

## Expected output

```
3 discos: 7 movimentos = 2^3 - 1 = 7
4 discos: 15 movimentos = 2^4 - 1 = 15
6 discos: 63 movimentos = 2^6 - 1 = 63
64 discos = 18.446.744.073.709.551.615 movimentos
a 1 movimento por segundo: 584.542.046.090 anos (≈ 585 bilhões de anos)
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

With `--movimentos`, before each summary line above the moves of that round appear, in the format
`H<disks> <move number> <disk> <from> <to>`. The 7 moves with 3 disks:


```
H3 1 1 A C
H3 2 2 A B
H3 3 1 C B
H3 4 3 A C
H3 5 1 B A
H3 6 2 B C
H3 7 1 A C
```

## Where the algorithm is

- Full explanation, recursion diagram and complexity: [`docs/algoritmos/torre-de-hanoi.md`](../../docs/algoritmos/torre-de-hanoi.md)
- Commented code: [`src/Enneal.Algoritmos.TorreHanoi`](../../src/Enneal.Algoritmos.TorreHanoi)
