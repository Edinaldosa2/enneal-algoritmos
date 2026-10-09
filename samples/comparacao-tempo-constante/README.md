Português | [English](README.en.md)

# Comparação em tempo constante

Comparar segredo (token de API, assinatura HMAC, código de reset) com `==` ou com um laço que **sai no primeiro caractere diferente** deixa o custo depender de quantas letras do começo estão certas. O conserto é uma linha do .NET: `CryptographicOperations.FixedTimeEquals`, que custa sempre o mesmo.

> **Modelo didático.** O "custo" é o número de comparações de caractere de cada versão: um modelo determinístico, que não mede tempo de nada. O token (`7F3A9C`) e os 6 candidatos são de brinquedo.

Reel: (link em breve)

Para ver cada comparação de caractere: `dotnet run -- --comparacoes`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/comparacao-tempo-constante
dotnet run
```

## Saída esperada

```
Candidato 1 B04E21: prefixo igual 0 | sai cedo: custo 1 -> false | FixedTimeEquals: custo 6 -> false
Candidato 2 7F3D58: prefixo igual 3 | sai cedo: custo 4 -> false | FixedTimeEquals: custo 6 -> false
Candidato 3 7A0B6E: prefixo igual 1 | sai cedo: custo 2 -> false | FixedTimeEquals: custo 6 -> false
Candidato 4 7F3A92: prefixo igual 5 | sai cedo: custo 6 -> false | FixedTimeEquals: custo 6 -> false
Candidato 5 7F1C07: prefixo igual 2 | sai cedo: custo 3 -> false | FixedTimeEquals: custo 6 -> false
Candidato 6 7F3A9C: prefixo igual 6 | sai cedo: custo 6 -> true | FixedTimeEquals: custo 6 -> true
Resumo: sai cedo [1, 4, 2, 6, 3, 6] | tempo constante [6, 6, 6, 6, 6, 6] | mesma resposta em 6/6
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

Cada linha confere que o modelo e o laço respondem igual ao `FixedTimeEquals` de verdade; se não, o programa para.

## Onde está o código

- Explicação completa, diagrama e **Como se defender**: [`docs/algoritmos/comparacao-tempo-constante.md`](../../docs/algoritmos/comparacao-tempo-constante.md)
- Código comentado: [`src/Enneal.Algoritmos.ComparacaoSegura`](../../src/Enneal.Algoritmos.ComparacaoSegura)
