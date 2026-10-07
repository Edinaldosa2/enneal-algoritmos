# Rate limit x DDoS: token bucket por IP

Uma enxurrada de requisições (como num ataque **DDoS**) lota a fila do servidor e quem é legítimo fica sem
resposta. Um **rate limit** com **token bucket** ("balde de fichas") por IP recusa o excesso logo na entrada,
e a fila volta a ter lugar para os usuários de verdade.

> **Simulação de brinquedo, para ensinar defesa.** Nada aqui gera tráfego de rede: as "requisições" são só
> objetos numa lista, criados com semente fixa (2026). Não há ferramenta de ataque, IP real nem alvo real.

Reel: (link em breve)

## O cenário

| Parâmetro | Valor |
|-----------|-------|
| Capacidade do servidor | 10 requisições por tick |
| Fila de espera | 24 lugares; quem espera mais de 1 tick leva timeout |
| Usuários legítimos | 6, cada um manda 1 pedido por tick com 40% de chance |
| Enxurrada | a partir do tick 4, 12 IPs mandando 2 a 4 pedidos por tick cada |
| Token bucket (rodada 2) | 3 fichas por IP, recarga de 0,5 ficha por tick |

Respostas possíveis: **200** (atendida), **429** (passou da cota do IP), **503** (fila cheia) e **408** (timeout).

## Como funciona o token bucket

1. Cada IP tem um balde que começa com **3 fichas** e ganha **0,5 ficha por tick**, até no máximo 3.
2. Cada requisição gasta **1 ficha**.
3. Sem ficha, a requisição volta na hora com **429** e **nem entra na fila**.

Assim um usuário normal pode fazer pequenas rajadas (até 3 pedidos seguidos), mas ninguém passa, no longo
prazo, de 1 pedido a cada 2 ticks. O custo por requisição é **O(1)** em tempo, e a memória é **O(número de
IPs)** (um balde por IP).

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd rate-limit-token-bucket
dotnet run
```

Para ver cada requisição, tick a tick (`R1`/`R2` = rodada, tick, id, IP, `L` legítimo ou `F` flood, destino e
fichas restantes no balde):

```bash
dotnet run -- --eventos
```

## Saída esperada

```
Sem rate limit: requisições 1173 | 200 ok 202 | 429 bloqueadas 0 | 503 fila cheia 851 | 408 timeout 120 | legítimos atendidos 22/84 (26%) | flood atendido 180 | ticks 36
Com rate limit: requisições 1173 | 200 ok 268 | 429 bloqueadas 886 | 503 fila cheia 14 | 408 timeout 5 | legítimos atendidos 81/84 (96%) | flood atendido 187 | ticks 34
```

- **Sem rate limit:** só **26%** dos pedidos legítimos são atendidos (22 de 84).
- **Com rate limit:** **96%** (81 de 84), com **886 requisições bloqueadas com 429** antes de chegar na fila.

Repare que o rate limit **não zera** o flood: cada IP da enxurrada ainda consegue a sua cota, por isso o
servidor continua ocupado. O que muda é que o excesso volta no portão, a fila não lota e sobra lugar para quem
é legítimo.

## Como se defender

- **Rate limit por cliente** (IP, usuário ou chave de API) nas rotas públicas, principalmente login, cadastro,
  busca e APIs. No ASP.NET Core já existe pronto: `builder.Services.AddRateLimiter(...)` com
  `TokenBucketRateLimiter` ou `FixedWindowRateLimiter`.
- **Responda 429 com `Retry-After`**, para clientes bem-comportados saberem quando tentar de novo.
- **Corte cedo:** quanto mais na borda o excesso for barrado (CDN, proxy reverso, WAF), menos ele custa para a
  aplicação e para o banco.
- **Ataques grandes e distribuídos** (milhares de IPs) passam de qualquer limite por IP: use proteção contra
  DDoS do provedor ou de uma CDN, que absorve o volume antes de chegar no seu servidor.
- **Filas e timeouts limitados:** fila infinita só transforma sobrecarga em lentidão para todo mundo.
- **Monitore** a taxa de 429, 503 e o tempo de resposta, e tenha alertas para picos fora do normal.
