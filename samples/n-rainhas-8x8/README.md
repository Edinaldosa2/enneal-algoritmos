# N-Rainhas 8x8

Resolve o clássico 8x8 com backtracking e desenha o tabuleiro: 876 tentativas e 105 voltas até a primeira solução.

Reel: (link em breve)

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/n-rainhas-8x8
dotnet run
```

## Saída esperada

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

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

## Onde está o algoritmo

- Explicação completa, diagrama e complexidade: [`docs/algoritmos/n-rainhas.md`](../../docs/algoritmos/n-rainhas.md)
- Código comentado: [`src/Enneal.Algoritmos.NRainhas`](../../src/Enneal.Algoritmos.NRainhas)
