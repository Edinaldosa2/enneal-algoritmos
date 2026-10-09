Português | [English](README.en.md)

# Corrida de ordenações

Bubble x Quick x Merge no mesmo vetor de 28 números: 1º Quick (185), 2º Merge (235), 3º Bubble (536).

Reel: (link em breve)

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/corrida-de-ordenacoes
dotnet run
```

## Saída esperada

```
Vetor inicial: 13 9 7 3 25 6 19 22 27 5 12 2 10 1 23 15 16 14 18 24 20 8 28 21 26 17 11 4

Bubble: comparações 375 | trocas 161 | escritas 0 | passos 536 | ordenado True
Quick: comparações 116 | trocas 69 | escritas 0 | passos 185 | ordenado True
Merge: comparações 99 | trocas 0 | escritas 136 | passos 235 | ordenado True

Chegada: 1º Quick (185) · 2º Merge (235) · 3º Bubble (536)
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

## Onde está o algoritmo

- Explicação completa, diagrama e complexidade: [`docs/algoritmos/corrida-de-ordenacoes.md`](../../docs/algoritmos/corrida-de-ordenacoes.md)
- Código comentado: [`src/Enneal.Algoritmos.Ordenacoes`](../../src/Enneal.Algoritmos.Ordenacoes)
