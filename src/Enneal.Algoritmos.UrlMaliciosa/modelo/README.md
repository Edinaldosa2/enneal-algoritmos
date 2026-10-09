Português | [English](README.en.md)

# Modelo do classificador de URL

`url_model.onnx` e `model_meta.json` são os arquivos que o notebook
[Malicious URL Classifier → ONNX → ASP.NET Core 8](https://www.kaggle.com/code/edinaldos/malicious-url-classifier-onnx-asp-net-core-8)
exportou, copiados sem mudança.

| Item | Valor |
|------|-------|
| Modelo | LightGBM convertido para ONNX (operador `TreeEnsembleClassifier` do ONNX-ML) |
| Entrada | 30 pistas léxicas da URL (`float32`), versão `url-lexical-v2` |
| Limiar | 0,768444 (cerca de 1% de falso positivo na validação) |
| Tamanho | 984.419 bytes |
| SHA-256 | `e5598e59cc2cceaeb6c4246c6cc19eed7b1ecf0a9683a0d1e7fcc5f8bdf50e14` |
| Treinado em | [Malicious URLs dataset](https://www.kaggle.com/datasets/sid321axn/malicious-urls-dataset) (Kaggle, licença CC0: domínio público), URLs normalizadas e separadas por host |

O modelo não contém URLs, só os limites das árvores e os pesos das folhas. Os dois arquivos vão embutidos na DLL
(`EmbeddedResource`) e são lidos por `ModeloDeArvores` e `MetadadosDoModelo`.

Ele serve para estudar o assunto. Para proteger usuários de verdade, combine com listas de bloqueio mantidas e
re-treine com dados atuais.
