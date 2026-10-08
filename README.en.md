<p align="center">
  <img src="img/banner.png" alt="enneal-algoritmos: the complete code from Enneal's Reels in C# / .NET 8" width="100%">
</p>

<p align="center">
  <strong>BR</strong> <a href="README.md">Português</a> |
  <strong>US</strong> <a href="README.en.md">English</a>
</p>

<p align="center">
  <a href="https://github.com/Edinaldosa2/enneal-algoritmos/actions/workflows/ci.yml"><img src="https://github.com/Edinaldosa2/enneal-algoritmos/actions/workflows/ci.yml/badge.svg" alt="CI"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-green.svg" alt="MIT License"></a>
  <a href="https://dotnet.microsoft.com/download/dotnet/8.0"><img src="https://img.shields.io/badge/.NET-8-512BD4" alt=".NET 8"></a>
  <img src="https://img.shields.io/badge/C%23-12-239120" alt="C# 12">
  <img src="https://img.shields.io/badge/tests-xUnit-5E2750" alt="xUnit tests">
  <img src="https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-0078D6" alt="Windows, Linux and macOS">
  <a href="https://www.instagram.com/enneal.it/"><img src="https://img.shields.io/badge/Instagram-%40enneal.it-E4405F" alt="Instagram @enneal.it"></a>
  <a href="https://github.com/Edinaldosa2"><img src="https://img.shields.io/badge/author-Edinaldosa2-blue" alt="Author"></a>
</p>

# enneal-algoritmos

**The complete code of the algorithms shown in Enneal's Instagram Reels: commented, tested, and producing the exact
same numbers as the videos.**

Each algorithm has a **C# library** with the code explained line by line, a **sample program** you run with one
command, and **tests** that guarantee the numbers match the Reel. The code, comments and docs are in Portuguese
(the Reels are in Portuguese); identifiers are easy to follow and this page summarizes everything in English.

---

## Algorithms

| Algorithm | Topic | Numbers from the Reel | Sample |
|-----------|-------|-----------------------|--------|
| **N-Queens 8x8** | backtracking, recursion | 876 attempts, 105 backtracks | [`n-rainhas-8x8`](samples/n-rainhas-8x8) |
| **N-Queens 4x4 → 8x8** | backtracking, recursion | 26 / 15 / 171 / 42 / 876 attempts | [`n-rainhas-4x4-ate-8x8`](samples/n-rainhas-4x4-ate-8x8) |
| **Brute force vs protected login** | password security | 7,392 attempts unprotected; locked after 5 | [`forca-bruta-senha`](samples/forca-bruta-senha) |
| **Intruder path** | breadth-first search, defense in depth | 93 steps with no defenses; 0 paths with 4 layers | [`caminho-do-invasor`](samples/caminho-do-invasor) |
| **Sorting race** | Bubble vs Quick vs Merge Sort | 1st Quick (185), 2nd Merge (235), 3rd Bubble (536) | [`corrida-de-ordenacoes`](samples/corrida-de-ordenacoes) |
| **Rate limit vs DDoS** | per-IP token bucket | legitimate requests served: 26% → 96%; 886 rejected (429) | [`rate-limit-token-bucket`](samples/rate-limit-token-bucket) |
| **Dijkstra vs A\*** | shortest path (how GPS finds a route) | cost 18; 89 vs 27 nodes explored (70% fewer) | [`dijkstra-a-estrela`](samples/dijkstra-a-estrela) |
| **Linear vs binary search** | searching a sorted array | 1,024 items: up to 1,024 vs up to 11; 1 million: 1,000,000 vs 20 | [`busca-linear-vs-binaria`](samples/busca-linear-vs-binaria) |
| **Tower of Hanoi** | recursion, 2ⁿ − 1 moves | 3 disks: 7; 4: 15; 6: 63; 64 disks: 18,446,744,073,709,551,615 | [`torre-de-hanoi`](samples/torre-de-hanoi) |

### Defensive security

| Sample | Topic | Numbers from the Reel | Sample |
|--------|-------|-----------------------|--------|
| **HTTPS grade** | DNS → TCP → TLS → certificate → HTTP, A+ to F grade | 6 cases in a local lab: no grade, C, T, M, T, A+ | [`nota-do-https`](samples/nota-do-https) |
| **Security headers** | HSTS, CSP, nosniff... in ASP.NET Core 8 | F (0/160) → A+ (160/160) in 10 steps | [`cabecalhos-de-seguranca`](samples/cabecalhos-de-seguranca) |
| **Malicious URL or not?** | 30 lexical features + tree model (ONNX) behind a minimal API | 4 of 6 URLs flagged; the "clean" phishing gets through; empty URL → 400 | [`url-maliciosa`](samples/url-maliciosa) |
| **High CVSS ≠ exploited** | CVSS vs EPSS vs KEV triage | exploited CVEs at #9, #10, #24 → #1, #2, #3; P1 3, P2 5 | [`triagem-cvss-epss-kev`](samples/triagem-cvss-epss-kev) |
| **Forgotten routes** | auditing your own routes, deny by default | 15 checked: 1 exposed (`/.git/config`) → 0 | [`rotas-esquecidas`](samples/rotas-esquecidas) |
| **IDOR: ownership check** | broken access control (OWASP A01) | 4 other people's orders leaked → 0 (403) | [`idor-checagem-de-dono`](samples/idor-checagem-de-dono) |
| **Constant-time comparison** | `==` vs `FixedTimeEquals` (cost model) | costs 1, 4, 2, 6, 3, 6 → 6 for all; same answer in 6/6 | [`comparacao-tempo-constante`](samples/comparacao-tempo-constante) |
| **Validating JWT the right way** | JwtBearer in ASP.NET Core 8: loose vs strict | bad tokens accepted: loose 3 of 4, strict 0 of 4 | [`jwt-validacao`](samples/jwt-validacao) |

Detailed pages (diagram, complexity, the code from the Reel, exercises) live in [`docs/algoritmos/`](docs/algoritmos).

---

## Quick start

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (free; Windows, Linux or macOS).
2. Clone the repo and run the sample for the Reel you saw (one folder per Reel, including `samples/caminho-do-invasor`):

```bash
git clone https://github.com/Edinaldosa2/enneal-algoritmos.git
cd enneal-algoritmos
dotnet run --project samples/n-rainhas-8x8
dotnet run --project samples/n-rainhas-4x4-ate-8x8
dotnet run --project samples/forca-bruta-senha
dotnet run --project samples/caminho-do-invasor
dotnet run --project samples/corrida-de-ordenacoes
dotnet run --project samples/rate-limit-token-bucket
dotnet run --project samples/dijkstra-a-estrela
dotnet run --project samples/busca-linear-vs-binaria
dotnet run --project samples/torre-de-hanoi
dotnet run --project samples/nota-do-https
dotnet run --project samples/cabecalhos-de-seguranca
dotnet run --project samples/url-maliciosa
dotnet run --project samples/triagem-cvss-epss-kev
dotnet run --project samples/rotas-esquecidas
dotnet run --project samples/idor-checagem-de-dono
dotnet run --project samples/comparacao-tempo-constante
dotnet run --project samples/jwt-validacao
```

3. Run the tests:

```bash
dotnet test
```

## Using the libraries

```csharp
using Enneal.Algoritmos.NRainhas;
using Enneal.Algoritmos.MenorCaminho;

var queens = ResolvedorNRainhas.Resolver(8);
Console.WriteLine($"{queens.Tentativas} attempts, {queens.Voltas} backtracks"); // 876, 105

var route = BuscaDeRota.AEstrela(MapaDaCidade.DoReel);
Console.WriteLine($"cost {route.Custo}, {route.NosExplorados} nodes");          // 18, 27
```

---

## Repository layout

```
enneal-algoritmos/
├── src/        Libraries: the pure algorithms, commented (no Console)
├── samples/    One console program per Reel, with saida-esperada.txt (expected output)
├── tests/      Enneal.Algoritmos.Tests (xUnit)
├── docs/       Architecture, security rules, roadmap, one page per algorithm
├── img/        Banner
├── util/       Verification and GitHub setup scripts
└── .github/    CI, issue and pull request templates
```

## Security topics are defensive

The security samples teach **defense**: toy simulations, audits of your own system and local labs. Nothing leaves the
machine: no internet access, no real passwords, no third-party systems. The only sockets (the TLS lab, the URL
minimal API and the two JWT APIs) listen on `127.0.0.1` inside the same process. Tokens and keys are demo values. Hosts are fictional (`.example`, `.invalid`, RFC 5737
documentation IPs). The brute-force library only accepts a 4-digit PIN and only "attacks" a `LoginProtegido` created
in the same process. The CVE queue uses public NVD, FIRST EPSS and CISA KEV data, and the URL model is the one
exported by the notebook (trained on a CC0 dataset); sources are credited next to the files. See
[docs/seguranca-didatica.md](docs/seguranca-didatica.md) and [SECURITY.md](SECURITY.md).

## Requirements

| Component | Version |
|-----------|---------|
| SDK | .NET 8 or newer |
| OS | Windows, Linux or macOS |

No external packages in the libraries or samples, with one exception: the JWT sample uses Microsoft's official
`Microsoft.AspNetCore.Authentication.JwtBearer` package (configuring it correctly is the point of that Reel). The
headers, URL and JWT samples use ASP.NET Core 8, which ships with the .NET 8 SDK (`FrameworkReference`). Tests use
xUnit.

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) (Portuguese).

## License

MIT. See [LICENSE](LICENSE).

## Enneal

- Instagram: [@enneal.it](https://www.instagram.com/enneal.it/)
- Website: [enneal.com.br](https://enneal.com.br)
- Author: [Edinaldosa2](https://github.com/Edinaldosa2)
