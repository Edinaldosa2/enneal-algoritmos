# Torre de Hanói (recursão)

Levar a torre do pino A para o C, um disco por vez, sem nunca pôr um disco maior sobre um menor:
3 discos = 7 movimentos, 4 = 15, 6 = 63. Com 64 discos: 18.446.744.073.709.551.615 movimentos.

Reel: (link em breve)

Para ver cada movimento: `dotnet run -- --movimentos`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/torre-de-hanoi
dotnet run
```

## Saída esperada

```
3 discos: 7 movimentos = 2^3 - 1 = 7
4 discos: 15 movimentos = 2^4 - 1 = 15
6 discos: 63 movimentos = 2^6 - 1 = 63
64 discos = 18.446.744.073.709.551.615 movimentos
a 1 movimento por segundo: 584.542.046.090 anos (≈ 585 bilhões de anos)
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

Com `--movimentos`, antes de cada linha acima aparecem os movimentos da rodada, no formato
`H<discos> <nº do movimento> <disco> <de> <para>`. Os 7 movimentos com 3 discos:

```
H3 1 1 A C
H3 2 2 A B
H3 3 1 C B
H3 4 3 A C
H3 5 1 B A
H3 6 2 B C
H3 7 1 A C
```

## Onde está o algoritmo

- Explicação completa, diagrama da recursão e complexidade: [`docs/algoritmos/torre-de-hanoi.md`](../../docs/algoritmos/torre-de-hanoi.md)
- Código comentado: [`src/Enneal.Algoritmos.TorreHanoi`](../../src/Enneal.Algoritmos.TorreHanoi)
