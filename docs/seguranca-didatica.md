# Regras da segurança didática

## Princípio

**Ensinar por que as defesas funcionam, sem entregar ferramenta de ataque.**

Os exemplos de segurança deste repositório são de três tipos, todos para mostrar, com números, o efeito de cada
defesa:

- **simulações de brinquedo** (força bruta, caminho do invasor, rate limit);
- **auditorias do seu próprio sistema** (rotas esquecidas, checagem de dono, triagem de vulnerabilidades);
- **laboratórios locais** com as peças de verdade do .NET (TLS, cabeçalhos do ASP.NET Core, minimal API da URL,
  JwtBearer), e um **modelo de custo** determinístico para a comparação em tempo constante.

## Regras

| Regra | Como é cumprida |
|-------|-----------------|
| Nada sai da máquina | nenhum exemplo acessa a internet, lê arquivos de senha ou acessa outro sistema. Os únicos sockets são o laboratório TLS, a minimal API da URL e as duas APIs do JWT, que escutam em `127.0.0.1`, numa porta livre, dentro do próprio processo, e fecham no fim |
| Alvo de brinquedo | o "alvo" é sempre uma variável, um mapa em texto, uma tabela de rotas, uma lista de objetos ou um servidor criado pelo próprio programa |
| Nomes fictícios | hosts `.example` e `.invalid` (RFC 2606), IPs de documentação `198.51.100.0/24` e `203.0.113.0/24` (RFC 5737), pessoas e documentos inventados e já mascarados |
| Escopo mínimo | a força bruta só aceita PIN de **4 dígitos** e só "ataca" um `LoginProtegido` criado no mesmo processo; o teste de acesso e a auditoria de rotas só conhecem a API e o site do próprio exemplo |
| Defesa em primeiro lugar | todo exemplo de segurança termina mostrando a defesa funcionando e tem a seção **Como se defender** |
| Sem técnicas ofensivas | não há exploração de vulnerabilidade, quebra de hash real, varredura de terceiros, lista de palavras para descobrir rotas ou geração de tráfego |
| Dados públicos com fonte | a fila de CVEs usa dados públicos da NVD, da FIRST (EPSS) e da CISA (KEV); o modelo da URL foi treinado num dataset CC0. A fonte fica ao lado de cada arquivo |
| Segredos de brinquedo | o token da comparação (`7F3A9C`) e a chave do JWT (SHA-256 de uma frase fixa) são de demonstração, públicos de propósito; os tokens "ruins" do JWT são testes negativos da própria API do exemplo |
| Números reproduzíveis | sementes e datas fixas (o laboratório TLS valida na data do scan do notebook; os tokens JWT usam um relógio fixo só no teste): qualquer pessoa confere os mesmos números do Reel |

## O que cada exemplo ensina

| Exemplo | Problema (simulado ou auditado) | Defesa demonstrada |
|---------|---------------------------------|--------------------|
| [Força bruta x login protegido](algoritmos/forca-bruta-senha.md) | testar 0000 a 9999 num PIN na memória | hash lento com salt, comparação em tempo constante, bloqueio por falhas |
| [Caminho do invasor](algoritmos/caminho-do-invasor.md) | busca em largura num mapa abstrato | defesa em camadas: WAF, MFA, menor privilégio, segmentação |
| [Rate limit x DDoS](algoritmos/rate-limit-token-bucket.md) | enxurrada de objetos numa fila simulada | token bucket por cliente, 429 na entrada |
| [Nota do HTTPS](algoritmos/nota-do-https.md) | TLS antigo, certificado vencido, com nome errado ou autoassinado (servidores do laboratório) | só TLS 1.2/1.3, renovação automática, nome certo no certificado, HSTS |
| [Cabeçalhos de segurança](algoritmos/cabecalhos-de-seguranca.md) | site sem cabeçalhos (nota F) | HTTPS, HSTS, nosniff, X-Frame-Options, Referrer/Permissions-Policy, cookies seguros, CSP com nonce |
| [URL maliciosa ou não?](algoritmos/url-maliciosa.md) | URLs de exemplo com sinais de phishing e malware (só o texto) | classificador como uma camada a mais, validação de entrada, nunca acessar a URL recebida |
| [CVSS alto ≠ explorada](algoritmos/triagem-cvss-epss-kev.md) | fila de correção ordenada só pelo CVSS | triagem com EPSS e KEV, prazos por faixa |
| [Rotas esquecidas](algoritmos/rotas-esquecidas.md) | `/.git/config` respondendo 200 no seu site | publicar só o build, negar por padrão, sem listagem de diretório, auditoria no CI |
| [IDOR: checagem de dono](algoritmos/idor-checagem-de-dono.md) | API que entrega o pedido de outra pessoa | checagem de dono no servidor (403), usuário vindo da sessão, teste de acesso no CI |
| [Comparação em tempo constante](algoritmos/comparacao-tempo-constante.md) | laço que sai cedo e vaza o prefixo certo (modelo de custo) | `CryptographicOperations.FixedTimeEquals`, hash para senhas, limite de tentativas |
| [JWT do jeito certo](algoritmos/jwt-validacao.md) | API frouxa aceitando token sem assinatura, de outra API ou vencido | algoritmo fixo, assinatura obrigatória, chave forte, emissor, audience, validade curta |

## Contribuições

Pull requests com conteúdo ofensivo (ferramentas de ataque, alvos reais, quebra de senhas de verdade, exploração de
vulnerabilidades, varredura de sistemas de terceiros) **não serão aceitos**. Veja também [SECURITY.md](../SECURITY.md).

**Use o que aprender aqui só em sistemas seus ou com autorização por escrito.**
