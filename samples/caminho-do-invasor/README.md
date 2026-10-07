# Caminho do invasor

Busca em largura num mapa abstrato: sem defesas, o invasor chega no banco em 93 passos; com 4 camadas, 0 caminhos.

Reel: (link em breve)

> Demo educativa de defesa em camadas: nada de rede, IPs ou alvos reais.

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/caminho-do-invasor
dotnet run
```

## Saída esperada

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

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

## Onde está o algoritmo

- Explicação completa, diagrama e complexidade: [`docs/algoritmos/caminho-do-invasor.md`](../../docs/algoritmos/caminho-do-invasor.md)
- Código comentado: [`src/Enneal.Algoritmos.CaminhoInvasor`](../../src/Enneal.Algoritmos.CaminhoInvasor)
