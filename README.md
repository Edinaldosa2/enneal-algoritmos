# enneal-algoritmos

Código **completo** dos algoritmos mostrados nos Reels da **Enneal** ([@enneal.it](https://www.instagram.com/enneal.it/)).
Cada pasta é um programa de console em **C# / .NET 8** que você roda com um comando, com o código comentado
em português, linha por linha, e os **mesmos números** que aparecem no vídeo.

Veio de um Reel e comentou pedindo o código? É aqui.

## Algoritmos

| Algoritmo | Tema | Pasta | Números do Reel |
|-----------|------|-------|-----------------|
| N-Rainhas 8x8 | backtracking, recursão | [`n-rainhas/basico`](n-rainhas/basico) | 876 tentativas, 105 voltas |
| N-Rainhas de 4x4 até 8x8 | backtracking, recursão | [`n-rainhas/4x4-ate-8x8`](n-rainhas/4x4-ate-8x8) | 26 / 15 / 171 / 42 / 876 tentativas |
| Força bruta x login protegido | segurança de senhas | [`forca-bruta-senha`](forca-bruta-senha) | 7.392 tentativas sem proteção; bloqueado após 5 com proteção |
| Caminho do invasor | busca em largura (BFS), defesa em camadas | [`caminho-do-invasor`](caminho-do-invasor) | 93 passos sem defesas; 0 caminhos com 4 camadas |
| Corrida de ordenações | Bubble x Quick x Merge Sort | [`corrida-de-ordenacoes`](corrida-de-ordenacoes) | 1º Quick (185), 2º Merge (235), 3º Bubble (536) |
| Rate limit x DDoS | token bucket por IP, disponibilidade | [`rate-limit-token-bucket`](rate-limit-token-bucket) | legítimos atendidos: 26% sem limite, 96% com token bucket; 886 bloqueadas (429) |

Novos algoritmos entram aqui conforme os Reels forem saindo.

Cada pasta tem um `README.md` explicando o algoritmo, a complexidade, como rodar e a saída esperada. Os temas
de segurança têm também uma seção **Como se defender**.

## Como rodar

1. Instale o [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis, Windows, Linux ou macOS).
2. Baixe o repositório (no GitHub, **Code > Download ZIP**, ou `git clone`).
3. Entre na pasta do algoritmo e rode:

```bash
cd corrida-de-ordenacoes
dotnet run
```

Também dá para rodar da raiz, apontando o projeto:

```bash
dotnet run --project n-rainhas/basico
dotnet run --project n-rainhas/4x4-ate-8x8
dotnet run --project forca-bruta-senha
dotnet run --project caminho-do-invasor
dotnet run --project corrida-de-ordenacoes
dotnet run --project rate-limit-token-bucket
```

Para compilar tudo de uma vez (ou abrir no Visual Studio / Rider / VS Code), use a solução:

```bash
dotnet build enneal-algoritmos.sln
```

## Estrutura

```
enneal-algoritmos/
├── enneal-algoritmos.sln       solução com todos os projetos
├── Directory.Build.props       configurações comuns a todos os projetos
├── n-rainhas/
│   ├── basico/                 N-Rainhas 8x8
│   └── 4x4-ate-8x8/            N-Rainhas de 4x4 até 8x8
├── forca-bruta-senha/
├── caminho-do-invasor/
├── corrida-de-ordenacoes/
└── rate-limit-token-bucket/
```

Cada projeto é independente: um `.csproj`, um `Program.cs` e um `README.md`. Pode copiar a pasta e estudar
sozinha.

## Sobre os temas de segurança

Os exemplos de segurança são **simulações didáticas**: o "alvo" é sempre uma variável ou um mapa do próprio
programa, sem rede, sistemas de terceiros ou dados reais. O objetivo é mostrar **por que** as defesas funcionam.

## Enneal

- Instagram: [@enneal.it](https://www.instagram.com/enneal.it/)
- Site: [enneal.com.br](https://enneal.com.br)

Gostou? Deixe uma estrela no repositório e mande o Reel para quem está aprendendo a programar.

## Licença

[MIT](LICENSE). Pode usar, estudar, modificar e compartilhar, mantendo o aviso de copyright.
