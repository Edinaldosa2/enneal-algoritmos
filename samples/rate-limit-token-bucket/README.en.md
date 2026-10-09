[Português](README.md) | English


# Rate limit vs DDoS

Server queue under a flood: with no limit, 26% of legitimate requests served; with a token bucket, 96% (886 blocked with 429).

> Toy simulation: the requests are objects in a list; nothing goes to the network.

Reel: (link coming soon)

To see each request, tick by tick: `dotnet run -- --eventos`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/rate-limit-token-bucket
dotnet run
```

## Expected output

```
Sem rate limit: requisições 1173 | 200 ok 202 | 429 bloqueadas 0 | 503 fila cheia 851 | 408 timeout 120 | legítimos atendidos 22/84 (26%) | flood atendido 180 | ticks 36
Com rate limit: requisições 1173 | 200 ok 268 | 429 bloqueadas 886 | 503 fila cheia 14 | 408 timeout 5 | legítimos atendidos 81/84 (96%) | flood atendido 187 | ticks 34
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

## Where the algorithm is

- Full explanation, diagram and complexity: [`docs/algoritmos/rate-limit-token-bucket.md`](../../docs/algoritmos/rate-limit-token-bucket.md)
- Commented code: [`src/Enneal.Algoritmos.RateLimit`](../../src/Enneal.Algoritmos.RateLimit)
