[Português](README.md) | English


# Dijkstra vs A*

How GPS finds a route: both searches find a route of cost 18; Dijkstra explores 89 nodes, A* only 27 (70% fewer).

Reel: (link coming soon)

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/dijkstra-a-estrela
dotnet run
```

## Expected output

```
Dijkstra: rota de custo 18, 89 nós explorados
A*: rota de custo 18, 27 nós explorados
A* explorou 70% menos
rota Dijkstra: 4,0 4,1 4,2 4,3 4,4 4,5 3,5 3,6 3,7 3,8 3,9 3,10 4,10 4,11 4,12 4,13 4,14 4,15 4,16
rota A*: 4,0 4,1 4,2 4,3 4,4 4,5 3,5 3,6 3,7 3,8 3,9 3,10 4,10 4,11 4,12 4,13 4,14 4,15 4,16

Legenda: * = rota   o = esquina explorada   m = trânsito   T = engarrafamento   # = prédio   P = praça

Dijkstra (89 nós explorados):
   o o o o o o o o m o o o o o o . .
   o # # o # # # o # o # # # o # # .
   o # # o # # # o o o # # # o # # .
   o # # o o * * * * * * o o o # # .
   A * * * * * T T T T * * * * * * B
   o # # o m m m m m m o o o o # # .
   o # # o P P P o o o # # # o # # .
   o # # o P P P o # o # # # . # # .
   o o o o o o o o m o o o . . . . .

A* (27 nós explorados):
   . . . . . . . . m . . . . . . . .
   . # # . # # # . # . # # # . # # .
   . # # . # # # . . . # # # . # # .
   o # # o o * * * * * * o o o # # .
   A * * * * * T T T T * * * * * * B
   o # # o m m m m m m . . . . # # .
   . # # . P P P . . . # # # . # # .
   . # # . P P P . # . # # # . # # .
   . . . . . . . . m . . . . . . . .
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

## Where the algorithm is

- Full explanation, diagram and complexity: [`docs/algoritmos/dijkstra-a-estrela.md`](../../docs/algoritmos/dijkstra-a-estrela.md)
- Commented code: [`src/Enneal.Algoritmos.MenorCaminho`](../../src/Enneal.Algoritmos.MenorCaminho)
