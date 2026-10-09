[Português](README.md) | English


# Constant-time comparison

Comparing a secret (API token, HMAC signature, reset code) with `==` or with a loop that **exits on the first different character** makes the cost depend on how many leading characters are correct. The fix is one line from .NET: `CryptographicOperations.FixedTimeEquals`, which always costs the same.

> **Didactic model.** The "cost" is the number of character comparisons of each version: a deterministic model that does not measure wall-clock time. The token (`7F3A9C`) and the 6 candidates are toys.

Reel: (link coming soon)

To see each character comparison: `dotnet run -- --comparacoes`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/comparacao-tempo-constante
dotnet run
```

## Expected output

```
Candidato 1 B04E21: prefixo igual 0 | sai cedo: custo 1 -> false | FixedTimeEquals: custo 6 -> false
Candidato 2 7F3D58: prefixo igual 3 | sai cedo: custo 4 -> false | FixedTimeEquals: custo 6 -> false
Candidato 3 7A0B6E: prefixo igual 1 | sai cedo: custo 2 -> false | FixedTimeEquals: custo 6 -> false
Candidato 4 7F3A92: prefixo igual 5 | sai cedo: custo 6 -> false | FixedTimeEquals: custo 6 -> false
Candidato 5 7F1C07: prefixo igual 2 | sai cedo: custo 3 -> false | FixedTimeEquals: custo 6 -> false
Candidato 6 7F3A9C: prefixo igual 6 | sai cedo: custo 6 -> true | FixedTimeEquals: custo 6 -> true
Resumo: sai cedo [1, 4, 2, 6, 3, 6] | tempo constante [6, 6, 6, 6, 6, 6] | mesma resposta em 6/6
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

Each line checks that the model and the loop answer the same as real `FixedTimeEquals`; if not, the program stops.

## Where the code is

- Full explanation, diagram and **How to defend**: [`docs/algoritmos/comparacao-tempo-constante.md`](../../docs/algoritmos/comparacao-tempo-constante.md)
- Commented code: [`src/Enneal.Algoritmos.ComparacaoSegura`](../../src/Enneal.Algoritmos.ComparacaoSegura)
