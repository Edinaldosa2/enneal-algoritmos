[Português](README.md) | English


# What is the HTTPS grade of your site?

An HTTPS connection goes through **DNS → TCP → TLS → Certificate → HTTP**. In 6 cases, the checker shows where it breaks (with a real `SslStream` and X509 validation) and gives the grade from A+ to F in the style of the SSL Labs guide.

> **Local lab.** Each case is a TLS server on `127.0.0.1` with a certificate generated on the spot. Names are fictional (`.invalid` and `.example`, inspired by badssl.com test cases). Nothing leaves the machine.

Notebook: [HTTPS Certificate & TLS Health Monitor](https://www.kaggle.com/code/edinaldos/https-tls-health-monitor-grades-net-worker)


Reel: (link coming soon)

To see each layer and the grade ceilings: `dotnet run -- --camadas`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/nota-do-https
dotnet run
```

## Expected output

```
Caso 1: does-not-exist.invalid | quebra no DNS: nome não existe | sem nota
Caso 2: tls-v1-0.lab.example | quebra no TLS: versão recusada | nota C | 90.0 pontos
Caso 3: expired.lab.example | quebra no Certificado: expirado | nota T (sem a confiança: A-) | 93.0 pontos
Caso 4: wrong.host.lab.example | quebra no Certificado: nome não bate com o site | nota M (sem a confiança: A-) | 93.0 pontos
Caso 5: self-signed.lab.example | quebra no Certificado: autoassinado | nota T (sem a confiança: A-) | 93.0 pontos
Caso 6: seu-site.example | chegou no HTTP: 200 · HSTS 365 dias | nota A+ | 93.0 pontos
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

The chain is validated at the notebook scan date (04/10/2026 11:40:25 UTC), so the output is the same on any day.

## Where the code is

- Full explanation, diagram and **How to defend**: [`docs/algoritmos/nota-do-https.md`](../../docs/algoritmos/nota-do-https.md)
- Commented code: [`src/Enneal.Algoritmos.NotaTls`](../../src/Enneal.Algoritmos.NotaTls)
