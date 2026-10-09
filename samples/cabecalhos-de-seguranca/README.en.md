[Português](README.md) | English


# From F to A+: one security header at a time

An **ASP.NET Core 8** site (`loja.example`) starts with grade F and gains one security header per step (HTTPS, HSTS, nosniff, X-Frame-Options, Referrer-Policy, Permissions-Policy, cookies, COOP/COEP/CORP and CSP) up to **A+**.

> **No server and no network.** Each step builds the real pipeline and the GET runs straight through it, in memory (`DefaultHttpContext`).

Notebook: [HTTP security headers: A to F grades + .NET fix](https://www.kaggle.com/code/edinaldos/http-security-headers-a-to-f-grades-net-fix)


Reel: (link coming soon)

To see each header, cookie and score point: `dotnet run -- --cabecalhos`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/cabecalhos-de-seguranca
dotnet run
```

## Expected output

```
Fase 0: nada, só HTTP | 0/160 | nota F | 1 aviso(s)
Fase 1: HTTPS | 30/160 | nota E | 2 aviso(s)
Fase 2: HSTS | 55/160 | nota D | 2 aviso(s)
Fase 3: X-Content-Type-Options | 75/160 | nota D | 2 aviso(s)
Fase 4: X-Frame-Options | 95/160 | nota C | 2 aviso(s)
Fase 5: Referrer-Policy | 115/160 | nota B | 2 aviso(s)
Fase 6: Permissions-Policy | 135/160 | nota A | 2 aviso(s)
Fase 7: cookies Secure/HttpOnly/SameSite | 135/160 | nota A
Fase 8: COOP/COEP/CORP | 135/160 | nota A
Fase 9: CSP com 'unsafe-inline' | 160/160 | nota A (teto: csp_unsafe_inline)
Fase 10: CSP com nonce + 'strict-dynamic' | 160/160 | nota A+
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

The grade follows the notebook rules (points and bands in the style of securityheaders.com); it is not the official grade from any service.

## Where the code is

- Full explanation, diagram and **How to defend**: [`docs/algoritmos/cabecalhos-de-seguranca.md`](../../docs/algoritmos/cabecalhos-de-seguranca.md)
- Commented code: [`src/Enneal.Algoritmos.CabecalhosSeguranca`](../../src/Enneal.Algoritmos.CabecalhosSeguranca)
