# Kaggle: Hunting PHP Webshell Patterns in Upload Payloads and Access Logs (PHP2Shell context)

Notebook defensivo em inglês, roda no Kaggle sem internet (só Python, re e pandas).
- Contexto: CVE-2026-109618/109619. A correção de verdade é atualizar o PHP, e o notebook não reproduz as falhas.
- Todos os dados são sintéticos, escritos à mão e identificados como tal. Os IPs são de documentação (RFC 5737).
- Seções: 1) amostras de uploads e de logs; 2) heurísticas de upload: extensão, tag PHP, eval/system, superglobais, ofuscação, poliglota imagem+PHP, .htaccess; 3) regras de log: PHP em pasta de uploads, parâmetro de comando, path traversal; 4) matriz de confusão só no conjunto sintético, com aviso de que não mede a precisão real; 5) boas práticas e fontes oficiais (CERT-EU, CERT-FR, CERT Polska, NCSC-NL).
- Rodapé: enneal.com.br e o LinkedIn do Edinaldo.
Status: executado localmente sem erros e NÃO publicado.
