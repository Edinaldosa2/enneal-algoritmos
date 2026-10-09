Português | [English](README.en.md)

# Rate limit x DDoS

Fila de servidor sob enxurrada: sem limite, 26% dos pedidos legítimos atendidos; com token bucket, 96% (886 bloqueadas com 429).

Reel: (link em breve)

> Simulação de brinquedo: as requisições são objetos numa lista, nada vai para a rede.

Para ver cada requisição, tick a tick: `dotnet run -- --eventos`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/rate-limit-token-bucket
dotnet run
```

## Saída esperada

```
Sem rate limit: requisições 1173 | 200 ok 202 | 429 bloqueadas 0 | 503 fila cheia 851 | 408 timeout 120 | legítimos atendidos 22/84 (26%) | flood atendido 180 | ticks 36
Com rate limit: requisições 1173 | 200 ok 268 | 429 bloqueadas 886 | 503 fila cheia 14 | 408 timeout 5 | legítimos atendidos 81/84 (96%) | flood atendido 187 | ticks 34
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

## Onde está o algoritmo

- Explicação completa, diagrama e complexidade: [`docs/algoritmos/rate-limit-token-bucket.md`](../../docs/algoritmos/rate-limit-token-bucket.md)
- Código comentado: [`src/Enneal.Algoritmos.RateLimit`](../../src/Enneal.Algoritmos.RateLimit)
