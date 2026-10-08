# Rotas esquecidas no seu site: negar por padrão

Auditoria das rotas do **seu próprio site** (`seu-site.example`) antes do deploy: das 15 rotas do checklist, uma sobra do deploy (`/.git/config`) responde 200 e expõe o repositório. Depois da correção (negar por padrão e sem listagem de diretório), nenhuma rota sensível fica exposta.

> **Auditoria local e defensiva.** O "servidor" é a tabela de rotas do app, em memória, com dados fixos. Nada aqui gera tráfego de rede.

Reel: (link em breve)

Para ver cada rota conferida: `dotnet run -- --rotas`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/rotas-esquecidas
dotnet run
```

## Saída esperada

```
Antes da correção: rotas conferidas 15 | 200 ok 6 | 302 1 | 403 1 | 404 7 | rotas sensíveis expostas 1 (/.git/config)
Depois da correção: rotas conferidas 15 | 200 ok 4 | 302 1 | 403 1 | 404 9 | rotas sensíveis expostas 0
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

## Onde está o código

- Explicação completa, diagrama e **Como se defender**: [`docs/algoritmos/rotas-esquecidas.md`](../../docs/algoritmos/rotas-esquecidas.md)
- Código comentado: [`src/Enneal.Algoritmos.RotasEsquecidas`](../../src/Enneal.Algoritmos.RotasEsquecidas)
