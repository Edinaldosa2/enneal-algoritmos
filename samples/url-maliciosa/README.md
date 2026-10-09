Português | [English](README.en.md)

# URL maliciosa ou não?

Seis URLs de exemplo passam por um classificador que lê **só o texto da URL**: 30 pistas léxicas e o modelo de árvores (LightGBM) treinado no notebook, servido por uma minimal API do ASP.NET Core 8 (`POST /score`). Quatro são marcadas; a URL 6, um phishing "limpo", passa.

> **Só localhost.** A API sobe em `127.0.0.1` numa porta livre e nunca acessa a URL recebida. As URLs usam nomes e IPs reservados para documentação (RFC 2606 e RFC 5737).

Notebook: [Malicious URL Classifier → ONNX → ASP.NET Core 8](https://www.kaggle.com/code/edinaldos/malicious-url-classifier-onnx-asp-net-core-8)

Reel: (link em breve)

Para ver as 30 pistas de cada URL e o JSON da API: `dotnet run -- --pistas`

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/url-maliciosa
dotnet run
```

## Saída esperada

```
URL 1 (normal): p=0.165815 -> ok (acertou)
URL 2 (phishing): p=0.981784 -> maliciosa (acertou)
URL 3 (phishing): p=0.980968 -> maliciosa (acertou)
URL 4 (malware): p=0.999031 -> maliciosa (acertou)
URL 5 (defacement): p=0.849362 -> maliciosa (acertou)
URL 6 (phishing): p=0.107498 -> ok (errou)
Resumo: 6 URLs, 4 maliciosas pelo modelo (limiar 0.768444); URL vazia -> 400
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

O modelo é o `url_model.onnx` do notebook, sem mudança, avaliado em C# puro (sem ONNX Runtime e sem pacote NuGet).

## Onde está o código

- Explicação completa, diagrama e **Como se defender**: [`docs/algoritmos/url-maliciosa.md`](../../docs/algoritmos/url-maliciosa.md)
- Código comentado: [`src/Enneal.Algoritmos.UrlMaliciosa`](../../src/Enneal.Algoritmos.UrlMaliciosa)
