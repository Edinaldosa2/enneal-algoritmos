# Fila de demonstração (dados públicos)

`remediation_queue_demo.csv` é a saída `outputs/remediation_queue_demo.csv` do notebook
[High CVSS ≠ Exploited: CVSS, EPSS & KEV](https://www.kaggle.com/code/edinaldos/high-cvss-exploited-cvss-epss-kev-vulnerabil),
copiada sem mudança: 400 CVEs reais, sorteadas entre as publicadas desde 2023.

| Coluna | Fonte |
|--------|-------|
| `cve_id`, `base_score`, `cwe` | [NVD](https://nvd.nist.gov/) (National Vulnerability Database, NIST) |
| `epss_score` | [EPSS](https://www.first.org/epss/) da FIRST.org |
| `kev`, `vendorproject`, `product`, uso em ransomware (coluna `why`) | [Known Exploited Vulnerabilities Catalog](https://www.cisa.gov/known-exploited-vulnerabilities-catalog) da CISA, catálogo 2026.10.02 |
| `tier`, `risk`, `due_date`, `why` | calculadas pelo notebook (a mesma triagem de `Triagem.cs`) |
| `asset` | **simulada** no notebook: o serviço fictício onde a CVE "apareceu" (não é inventário de ninguém) |

Atribuição: "This product uses the NVD API but is not endorsed or certified by the NVD." EPSS: Jacobs, J.,
Romanosky, S., et al., *Exploit Prediction Scoring System*, FIRST.org (https://www.first.org/epss/). KEV: CISA.

Os valores de EPSS e da KEV mudam todo dia; estes são os da execução do notebook (outubro de 2026, KEV
2026.10.02) e servem só para estudar a triagem. Para decidir o que corrigir no seu ambiente, use os dados atuais.
