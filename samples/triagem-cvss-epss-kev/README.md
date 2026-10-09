Português | [English](README.en.md)

# CVSS alto não é explorada: triagem com CVSS x EPSS x KEV

A mesma fila de **400 CVEs públicas** ordenada de 3 jeitos: maior CVSS primeiro (o hábito), CVSS + EPSS e a triagem com a KEV da CISA. As 3 CVEs já exploradas saem de #9, #10 e #24 para #1, #2 e #3.

Notebook: [High CVSS ≠ Exploited: CVSS, EPSS & KEV](https://www.kaggle.com/code/edinaldos/high-cvss-exploited-cvss-epss-kev-vulnerabil)

Reel: (link em breve)

Para ver cada CVE (sinais, risco, faixa e posição nas 3 ordens): `dotnet run -- --fila`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/triagem-cvss-epss-kev
dotnet run
```

## Saída esperada

```
1) CVSS primeiro: exploradas (KEV) em #9, #10, #24
2) + EPSS: exploradas em #1, #2, #19
3) + KEV: exploradas em #1, #2, #3 | P1 3, P2 5, P3 197, P4 195
Topo do 'CVSS >= 9 primeiro': 31 | P1+P2: 8 (3 exploradas)
Conferência com o notebook: risco 400/400, faixa 400/400, ordem da fila igual
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

A fila vem do notebook, com dados públicos da NVD, da FIRST (EPSS) e da CISA (KEV). Fontes e atribuição em
[`src/Enneal.Algoritmos.TriagemVulnerabilidades/dados/README.md`](../../src/Enneal.Algoritmos.TriagemVulnerabilidades/dados/README.md).
A última linha confere a conta com as colunas que o notebook calculou; se algo divergir, o programa termina com código 1.

## Onde está o código

- Explicação completa, diagrama e **Como se defender**: [`docs/algoritmos/triagem-cvss-epss-kev.md`](../../docs/algoritmos/triagem-cvss-epss-kev.md)
- Código comentado: [`src/Enneal.Algoritmos.TriagemVulnerabilidades`](../../src/Enneal.Algoritmos.TriagemVulnerabilidades)
