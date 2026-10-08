# Validando JWT do jeito certo

Os mesmos 6 tokens HS256 em duas APIs **ASP.NET Core 8** com JwtBearer: uma com a validação frouxa (aceita token sem assinatura e não confere audience nem validade) e outra com a certa (algoritmo fixo, assinatura obrigatória, chave forte, emissor, audience e validade). Tokens ruins aceitos: **frouxa 3 de 4, certa 0 de 4**.

> **Só localhost, valores de brinquedo.** As duas APIs sobem em `127.0.0.1` numa porta livre. Hosts fictícios (`seu-site.example`) e chave de demonstração (pública de propósito). Os tokens "ruins" são os testes negativos que a sua API precisa recusar.

Reel: (link em breve)

Para ver os tokens, as partes decodificadas e o motivo de cada 401: `dotnet run -- --tokens`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/jwt-validacao
dotnet run
```

## Saída esperada

```
Chave fraca "segredo123": recusada (80 bits, IDX10653: mínimo 128)
Token 0 original (cliente): API frouxa 403 | API certa 403
Token 1 role editado + alg none: API frouxa 200 | API certa 401
Token 2 role editado, assinatura antiga: API frouxa 401 | API certa 401
Token 3 admin de outra API (aud): API frouxa 200 | API certa 401
Token 4 admin vencido (exp): API frouxa 200 | API certa 401
Token 5 admin de verdade: API frouxa 200 | API certa 200
Resumo: tokens ruins aceitos (200): API frouxa 3 de 4, API certa 0 de 4
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

Os tokens são emitidos num instante fixo (08/10/2026 12:00 UTC) para saírem byte a byte iguais em qualquer dia. Por isso, **só no teste**, a API certa confere a validade nesse mesmo relógio, com a mesma regra do `ValidateLifetime`. Em produção, deixe o padrão (o relógio do servidor).

Na primeira vez, o `dotnet run` baixa o pacote oficial `Microsoft.AspNetCore.Authentication.JwtBearer` do NuGet.

## Onde está o código

- Explicação completa, diagrama e **Como se defender**: [`docs/algoritmos/jwt-validacao.md`](../../docs/algoritmos/jwt-validacao.md)
- Código comentado: [`src/Enneal.Algoritmos.ValidacaoJwt`](../../src/Enneal.Algoritmos.ValidacaoJwt)
