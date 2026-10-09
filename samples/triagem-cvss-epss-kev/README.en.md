[Português](README.md) | English


# High CVSS is not exploited: triage with CVSS × EPSS × KEV

The same queue of **400 public CVEs** ordered three ways: highest CVSS first (the habit), CVSS + EPSS and triage with CISA's KEV. The 3 already-exploited CVEs move from #9, #10 and #24 to #1, #2 and #3.

Notebook: [High CVSS ≠ Exploited: CVSS, EPSS & KEV](https://www.kaggle.com/code/edinaldos/high-cvss-exploited-cvss-epss-kev-vulnerabil)


Reel: (link coming soon)

To see each CVE (signals, risk, tier and position in the 3 orders): `dotnet run -- --fila`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/triagem-cvss-epss-kev
dotnet run
```

## Expected output

```
1) CVSS primeiro: exploradas (KEV) em #9, #10, #24
2) + EPSS: exploradas em #1, #2, #19
3) + KEV: exploradas em #1, #2, #3 | P1 3, P2 5, P3 197, P4 195
Topo do 'CVSS >= 9 primeiro': 31 | P1+P2: 8 (3 exploradas)
Conferência com o notebook: risco 400/400, faixa 400/400, ordem da fila igual
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

The queue comes from the notebook, with public data from NVD, FIRST (EPSS) and CISA (KEV). Sources and attribution in
[`src/Enneal.Algoritmos.TriagemVulnerabilidades/dados/README.md`](../../src/Enneal.Algoritmos.TriagemVulnerabilidades/dados/README.md).
The last line checks the count against the columns the notebook computed; if anything diverges, the program exits with code 1.

## Where the code is

- Full explanation, diagram and **How to defend**: [`docs/algoritmos/triagem-cvss-epss-kev.md`](../../docs/algoritmos/triagem-cvss-epss-kev.md)
- Commented code: [`src/Enneal.Algoritmos.TriagemVulnerabilidades`](../../src/Enneal.Algoritmos.TriagemVulnerabilidades)
