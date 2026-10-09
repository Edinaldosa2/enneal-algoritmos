[Português](README.md) | English

# URL classifier model

`url_model.onnx` and `model_meta.json` are the files the notebook
[Malicious URL Classifier → ONNX → ASP.NET Core 8](https://www.kaggle.com/code/edinaldos/malicious-url-classifier-onnx-asp-net-core-8)
exported, copied unchanged.

| Item | Value |
|------|-------|
| Model | LightGBM converted to ONNX (`TreeEnsembleClassifier` ONNX-ML operator) |
| Input | 30 lexical URL features (`float32`), version `url-lexical-v2` |
| Threshold | 0.768444 (about 1% false positive on validation) |
| Size | 984,419 bytes |
| SHA-256 | `e5598e59cc2cceaeb6c4246c6cc19eed7b1ecf0a9683a0d1e7fcc5f8bdf50e14` |
| Trained on | [Malicious URLs dataset](https://www.kaggle.com/datasets/sid321axn/malicious-urls-dataset) (Kaggle, CC0: public domain), URLs normalized and split by host |

The model does not contain URLs, only tree thresholds and leaf weights. Both files are embedded in the DLL
(`EmbeddedResource`) and read by `ModeloDeArvores` and `MetadadosDoModelo`.

It is for studying the topic. To protect real users, combine it with maintained block lists and retrain with current
data.
