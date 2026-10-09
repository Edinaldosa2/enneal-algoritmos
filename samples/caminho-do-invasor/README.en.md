[Português](README.md) | English


# Intruder path

Breadth-first search on an abstract map: with no defenses, the intruder reaches the bank in 93 steps; with 4 layers, 0 paths.

> Educational defense-in-depth demo: no network, IPs or real targets.

Reel: (link coming soon)

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/caminho-do-invasor
dotnet run
```

## Expected output

```
== Round 1: rede SEM defesas ==
sem defesas: INVADIDO em 93 passos
(caminho mais curto até o banco: 12 movimentos, marcado com *)
   . . . . . . I . . . . . .
   . . . . . . * * * . . . .
   . # # . . # # # * . # # .
   . . . . . . . . * . . . .
   . . . # . . . . * # . . .
   . # . . . . # * * . . # .
   . . . . # . . * # . . . .
   . . . . . . . * . . . . .
   . . . . . . B * . . . . .

== Round 2: rede com defesa EM CAMADAS ==
em camadas: PROTEGIDO, 0 caminhos até o banco (48 células exploradas)
(o = células que o invasor conseguiu alcançar)
   o o o o o o I o o o o o o
   o o o o o o o o o o o o o
   o # # o o # # # o o # # o
   o o o o o o o o o o o o o
   # # o # # # # # # o # # #
   # # # # . # # # # o # # #
   # . # # # # # # # # # . #
   # # # # # # # . # # # # #
   . . . . . . B . . . . . .
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

## Where the algorithm is

- Full explanation, diagram and complexity: [`docs/algoritmos/caminho-do-invasor.md`](../../docs/algoritmos/caminho-do-invasor.md)
- Commented code: [`src/Enneal.Algoritmos.CaminhoInvasor`](../../src/Enneal.Algoritmos.CaminhoInvasor)
