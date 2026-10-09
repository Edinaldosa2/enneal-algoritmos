Português | [English](README.en.md)

# Qual a nota do HTTPS do seu site?

Uma conexão HTTPS passa por **DNS → TCP → TLS → Certificado → HTTP**. Em 6 casos, o verificador mostra onde ela quebra (com `SslStream` e validação X509 de verdade) e dá a nota de A+ a F no estilo do guia do SSL Labs.

> **Laboratório local.** Cada caso é um servidor TLS em `127.0.0.1` com um certificado gerado na hora. Os nomes são fictícios (`.invalid` e `.example`, inspirados nos casos de teste do badssl.com). Nada sai da máquina.

Notebook: [HTTPS Certificate & TLS Health Monitor](https://www.kaggle.com/code/edinaldos/https-tls-health-monitor-grades-net-worker)

Reel: (link em breve)

Para ver cada camada e os tetos da nota: `dotnet run -- --camadas`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/nota-do-https
dotnet run
```

## Saída esperada

```
Caso 1: does-not-exist.invalid | quebra no DNS: nome não existe | sem nota
Caso 2: tls-v1-0.lab.example | quebra no TLS: versão recusada | nota C | 90.0 pontos
Caso 3: expired.lab.example | quebra no Certificado: expirado | nota T (sem a confiança: A-) | 93.0 pontos
Caso 4: wrong.host.lab.example | quebra no Certificado: nome não bate com o site | nota M (sem a confiança: A-) | 93.0 pontos
Caso 5: self-signed.lab.example | quebra no Certificado: autoassinado | nota T (sem a confiança: A-) | 93.0 pontos
Caso 6: seu-site.example | chegou no HTTP: 200 · HSTS 365 dias | nota A+ | 93.0 pontos
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

A cadeia é validada na data do scan do notebook (04/10/2026 11:40:25 UTC), então a saída é a mesma em qualquer dia.

## Onde está o código

- Explicação completa, diagrama e **Como se defender**: [`docs/algoritmos/nota-do-https.md`](../../docs/algoritmos/nota-do-https.md)
- Código comentado: [`src/Enneal.Algoritmos.NotaTls`](../../src/Enneal.Algoritmos.NotaTls)
