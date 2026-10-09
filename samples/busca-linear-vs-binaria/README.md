Português | [English](README.en.md)

# Busca linear x busca binária

Mesmo vetor ordenado de 1.024 itens: linear até 1.024 comparações, binária até 11. Com 1 milhão: 1.000.000 x 20.

Reel: (link em breve)

Para ver cada comparação: `dotnet run -- --eventos`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/busca-linear-vs-binaria
dotnet run
```

## Saída esperada

```
Rodada 1: alvo 19 | linear 7 comparações (posição 6) | binária 10 comparações (posição 6)
Rodada 2: alvo 1565 | linear 613 comparações (posição 612) | binária 10 comparações (posição 612)
Rodada 3: alvo 2562 | linear 1024 comparações (posição 1023) | binária 11 comparações (posição 1023)
1024 itens: linear até 1024 | binária até 11 (média 9.01 achando cada item)
1000000 itens: linear até 1000000 | binária até 20 (média 18.95 achando cada item)
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

## Onde está o algoritmo

- Explicação completa, diagrama e complexidade: [`docs/algoritmos/busca-linear-vs-binaria.md`](../../docs/algoritmos/busca-linear-vs-binaria.md)
- Código comentado: [`src/Enneal.Algoritmos.Busca`](../../src/Enneal.Algoritmos.Busca)
