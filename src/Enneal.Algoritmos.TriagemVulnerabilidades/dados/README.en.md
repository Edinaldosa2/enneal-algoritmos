[Português](README.md) | English

# Demo queue (public data)

`remediation_queue_demo.csv` is the `outputs/remediation_queue_demo.csv` output from the notebook
[High CVSS ≠ Exploited: CVSS, EPSS & KEV](https://www.kaggle.com/code/edinaldos/high-cvss-exploited-cvss-epss-kev-vulnerabil),
copied unchanged: 400 real CVEs, sampled among those published since 2023.

| Column | Source |
|--------|--------|
| `cve_id`, `base_score`, `cwe` | [NVD](https://nvd.nist.gov/) (National Vulnerability Database, NIST) |
| `epss_score` | [EPSS](https://www.first.org/epss/) from FIRST.org |
| `kev`, `vendorproject`, `product`, ransomware use (column `why`) | CISA [Known Exploited Vulnerabilities Catalog](https://www.cisa.gov/known-exploited-vulnerabilities-catalog), catalog 2026.10.02 |
| `tier`, `risk`, `due_date`, `why` | computed by the notebook (the same triage as `Triagem.cs`) |
| `asset` | **simulated** in the notebook: the fictional service where the CVE "showed up" (not anyone's inventory) |

Attribution: "This product uses the NVD API but is not endorsed or certified by the NVD." EPSS: Jacobs, J.,
Romanosky, S., et al., *Exploit Prediction Scoring System*, FIRST.org (https://www.first.org/epss/). KEV: CISA.

EPSS and KEV values change every day; these are from the notebook run (October 2026, KEV 2026.10.02) and are only for
studying the triage. To decide what to fix in your environment, use current data.
