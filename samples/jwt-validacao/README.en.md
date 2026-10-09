[Português](README.md) | English


# Validating JWT the right way

The same 6 HS256 tokens on two **ASP.NET Core 8** APIs with JwtBearer: one with loose validation (accepts unsigned tokens and does not check audience or expiry) and one with the strict setup (fixed algorithm, required signature, strong key, issuer, audience and lifetime). Bad tokens accepted: **loose 3 of 4, strict 0 of 4**.

> **Localhost only, toy values.** Both APIs listen on `127.0.0.1` on a free port. Fictional hosts (`seu-site.example`) and a demo key (public on purpose). The "bad" tokens are the negative tests your API must reject.

Reel: (link coming soon)

To see the tokens, decoded parts and the reason for each 401: `dotnet run -- --tokens`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/jwt-validacao
dotnet run
```

## Expected output

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

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

Tokens are issued at a fixed instant (08/10/2026 12:00 UTC) so they come out byte-identical on any day. That is why, **only in the test**, the strict API checks lifetime against that same clock, with the same `ValidateLifetime` rule. In production, leave the default (the server clock).

The first `dotnet run` downloads the official `Microsoft.AspNetCore.Authentication.JwtBearer` package from NuGet.

## Where the code is

- Full explanation, diagram and **How to defend**: [`docs/algoritmos/jwt-validacao.md`](../../docs/algoritmos/jwt-validacao.md)
- Commented code: [`src/Enneal.Algoritmos.ValidacaoJwt`](../../src/Enneal.Algoritmos.ValidacaoJwt)
