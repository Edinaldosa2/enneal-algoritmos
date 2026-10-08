# Do F ao A+: um cabeçalho de segurança por vez

Um site **ASP.NET Core 8** (`loja.example`) começa com nota F e ganha um cabeçalho de segurança por fase (HTTPS, HSTS, nosniff, X-Frame-Options, Referrer-Policy, Permissions-Policy, cookies, COOP/COEP/CORP e CSP) até o **A+**.

> **Sem servidor e sem rede.** Cada fase monta o pipeline de verdade e o GET roda direto nele, em memória (`DefaultHttpContext`).

Notebook: [HTTP security headers: A to F grades + .NET fix](https://www.kaggle.com/code/edinaldos/http-security-headers-a-to-f-grades-net-fix)

Reel: (link em breve)

Para ver cada cabeçalho, cookie e ponto: `dotnet run -- --cabecalhos`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/cabecalhos-de-seguranca
dotnet run
```

## Saída esperada

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

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

A nota segue as regras do notebook (pontos e faixas no estilo do securityheaders.com); não é a nota oficial de nenhum serviço.

## Onde está o código

- Explicação completa, diagrama e **Como se defender**: [`docs/algoritmos/cabecalhos-de-seguranca.md`](../../docs/algoritmos/cabecalhos-de-seguranca.md)
- Código comentado: [`src/Enneal.Algoritmos.CabecalhosSeguranca`](../../src/Enneal.Algoritmos.CabecalhosSeguranca)
