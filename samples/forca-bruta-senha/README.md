# Força bruta x login protegido

Simulação didática: o PIN 7391 cai em 7.392 tentativas sem proteção; com hash + bloqueio, a conta trava após 5 erros.

Reel: (link em breve)

> Simulação de brinquedo para ensinar defesa: o alvo é um PIN na memória do próprio programa.

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/forca-bruta-senha
dotnet run
```

## Saída esperada

```
== Round 1: PIN de 4 dígitos, SEM proteção ==
ABERTO!
7392
(o PIN 7391 caiu na tentativa 7392 de 10000 possíveis)

== Round 2: login protegido (hash + bloqueio) ==
bloqueado após 5 tentativas (15 min)
(o 6º pedido já encontrou a conta bloqueada)

mesmo PIN: 1478 bloqueios = 15.4 dias
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

## Onde está o algoritmo

- Explicação completa, diagrama e complexidade: [`docs/algoritmos/forca-bruta-senha.md`](../../docs/algoritmos/forca-bruta-senha.md)
- Código comentado: [`src/Enneal.Algoritmos.ForcaBruta`](../../src/Enneal.Algoritmos.ForcaBruta)
