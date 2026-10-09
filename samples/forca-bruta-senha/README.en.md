[Português](README.md) | English


# Brute force vs protected login

Didactic simulation: PIN 7391 falls in 7,392 attempts with no protection; with hash + lockout, the account locks after 5 failures.

> Toy simulation to teach defense: the target is a PIN in the program's own memory.

Reel: (link coming soon)

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/forca-bruta-senha
dotnet run
```

## Expected output

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

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

## Where the algorithm is

- Full explanation, diagram and complexity: [`docs/algoritmos/forca-bruta-senha.md`](../../docs/algoritmos/forca-bruta-senha.md)
- Commented code: [`src/Enneal.Algoritmos.ForcaBruta`](../../src/Enneal.Algoritmos.ForcaBruta)
