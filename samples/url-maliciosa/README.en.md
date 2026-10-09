[Português](README.md) | English


# Malicious URL or not?

Six example URLs go through a classifier that reads **only the URL text**: 30 lexical features and the tree model (LightGBM) trained in the notebook, served by an ASP.NET Core 8 minimal API (`POST /score`). Four are flagged; URL 6, a "clean" phishing, gets through.

> **Localhost only.** The API listens on `127.0.0.1` on a free port and never fetches the received URL. The URLs use reserved documentation names and IPs (RFC 2606 and RFC 5737).

Notebook: [Malicious URL Classifier → ONNX → ASP.NET Core 8](https://www.kaggle.com/code/edinaldos/malicious-url-classifier-onnx-asp-net-core-8)


Reel: (link coming soon)

To see the 30 features of each URL and the API JSON: `dotnet run -- --pistas`

## How to run

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/url-maliciosa
dotnet run
```

## Expected output

```
URL 1 (normal): p=0.165815 -> ok (acertou)
URL 2 (phishing): p=0.981784 -> maliciosa (acertou)
URL 3 (phishing): p=0.980968 -> maliciosa (acertou)
URL 4 (malware): p=0.999031 -> maliciosa (acertou)
URL 5 (defacement): p=0.849362 -> maliciosa (acertou)
URL 6 (phishing): p=0.107498 -> ok (errou)
Resumo: 6 URLs, 4 maliciosas pelo modelo (limiar 0.768444); URL vazia -> 400
```

This file is also in [`saida-esperada.txt`](saida-esperada.txt), and the tests check that the output stays
identical.

The model is the notebook's `url_model.onnx`, unchanged, evaluated in pure C# (no ONNX Runtime and no NuGet package).

## Where the code is

- Full explanation, diagram and **How to defend**: [`docs/algoritmos/url-maliciosa.md`](../../docs/algoritmos/url-maliciosa.md)
- Commented code: [`src/Enneal.Algoritmos.UrlMaliciosa`](../../src/Enneal.Algoritmos.UrlMaliciosa)
