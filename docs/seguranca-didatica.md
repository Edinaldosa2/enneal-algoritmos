# Regras da segurança didática

## Princípio

**Ensinar por que as defesas funcionam, sem entregar ferramenta de ataque.**

Os exemplos de segurança deste repositório (força bruta, caminho do invasor e rate limit) são **simulações de
brinquedo**. Eles existem para mostrar, com números, o efeito de cada defesa.

## Regras

| Regra | Como é cumprida |
|-------|-----------------|
| Nada sai do processo | nenhum exemplo abre conexão de rede, lê arquivos de senha ou acessa outro sistema |
| Alvo de brinquedo | o "alvo" é sempre uma variável, um mapa em texto ou uma lista de objetos do próprio programa |
| Escopo mínimo | a força bruta só aceita PIN de **4 dígitos** e só "ataca" um `LoginProtegido` criado no mesmo processo |
| Defesa em primeiro lugar | todo exemplo de segurança termina mostrando a defesa funcionando e tem a seção **Como se defender** |
| Sem técnicas ofensivas | não há exploração de vulnerabilidade, quebra de hash real, varredura ou geração de tráfego |
| Números reproduzíveis | sementes fixas: qualquer pessoa confere os mesmos números do Reel |

## O que cada exemplo ensina

| Exemplo | Ataque (simulado) | Defesa demonstrada |
|---------|-------------------|--------------------|
| [Força bruta x login protegido](algoritmos/forca-bruta-senha.md) | testar 0000 a 9999 num PIN na memória | hash lento com salt, comparação em tempo constante, bloqueio por falhas |
| [Caminho do invasor](algoritmos/caminho-do-invasor.md) | busca em largura num mapa abstrato | defesa em camadas: WAF, MFA, menor privilégio, segmentação |
| [Rate limit x DDoS](algoritmos/rate-limit-token-bucket.md) | enxurrada de objetos numa fila simulada | token bucket por cliente, 429 na entrada |

## Contribuições

Pull requests com conteúdo ofensivo (ferramentas de ataque, alvos reais, quebra de senhas de verdade, exploração de
vulnerabilidades) **não serão aceitos**. Veja também [SECURITY.md](../SECURITY.md).

**Use o que aprender aqui só em sistemas seus ou com autorização por escrito.**
