# Como contribuir

Obrigado pelo interesse! Este repositório guarda o código dos Reels da Enneal ([@enneal.it](https://www.instagram.com/enneal.it/)),
então a prioridade é **código fácil de estudar** e **números idênticos aos dos vídeos**.

## Jeitos de ajudar

- **Achou um erro** (no código, num comentário ou na documentação)? Abra uma issue com o modelo **Erro**.
- **Ficou com dúvida** sobre um algoritmo? Abra uma issue: a dúvida de um é a de muitos.
- **Tem ideia de Reel/algoritmo?** Use o modelo **Ideia de algoritmo**.
- **Quer mandar código?** Siga os passos abaixo e abra um pull request.

## Preparando o ambiente

1. Instale o [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Clone o repositório e rode a verificação completa:

```bash
dotnet build enneal-algoritmos.sln -warnaserror
dotnet test
```

Ou use os scripts de [`util/`](util): `pwsh util/verificar.ps1` (Windows) ou `bash util/verificar.sh` (Linux/macOS).

## Regras do código

- **Português** nos nomes, comentários e documentação (é o idioma dos Reels).
- **Comentários para quem está aprendendo:** explique o *porquê* de cada passo, não só o *quê*.
  Todo membro público das bibliotecas precisa de comentário `///` (o build falha sem ele).
- **Bibliotecas sem `Console`:** o algoritmo devolve resultados; quem imprime é o exemplo em `samples/`.
- **Zero avisos:** o CI compila com `-warnaserror`.
- **Números do Reel são sagrados:** se uma mudança alterar qualquer número mostrado num vídeo, ela não entra.
  Os testes e o `saida-esperada.txt` de cada exemplo protegem isso.
- **Sem pacotes externos** nas bibliotecas e nos exemplos. O ASP.NET Core pode ser usado com
  `<FrameworkReference Include="Microsoft.AspNetCore.App" />`, porque ele já vem no .NET 8 SDK.
- **Dados e modelos só pequenos e com licença clara** (até poucos MB), com a fonte num `README.md` ao lado do
  arquivo e embutidos na DLL (`EmbeddedResource`).
- **Segurança só defensiva:** leia [docs/seguranca-didatica.md](docs/seguranca-didatica.md). Nada de ferramentas de
  ataque, alvos reais ou tráfego para fora da máquina: laboratórios só em `127.0.0.1` e com hosts fictícios
  (`.example`, `.invalid`).

## Adicionando um algoritmo

Cada algoritmo segue o mesmo padrão (veja `Enneal.Algoritmos.Busca` como modelo):

1. **Biblioteca** em `src/Enneal.Algoritmos.<Nome>/`
   - `Enneal.Algoritmos.<Nome>.csproj` (só `RootNamespace`, `AssemblyName` e `Description`; o resto vem do
     `Directory.Build.props`);
   - o algoritmo, com `record`s de resultado e comentários `///`.
2. **Exemplo** em `samples/<nome-em-kebab-case>/`
   - `.csproj` com `ProjectReference` para a biblioteca;
   - `Program.cs` imprimindo os mesmos números do Reel;
   - `saida-esperada.txt` (gere com `dotnet run > saida-esperada.txt`) e `README.md`.
3. **Testes** em `tests/Enneal.Algoritmos.Tests/`
   - `Algoritmos/<Nome>Tests.cs` com os números exatos do Reel e pelo menos uma propriedade do algoritmo;
   - referência para a biblioteca e para o exemplo no `.csproj` de testes;
   - uma linha `[InlineData]` em `Exemplos/SaidaDosExemplosTests.cs`.
4. **Solução:** `dotnet sln enneal-algoritmos.sln add --solution-folder src src/Enneal.Algoritmos.<Nome>/*.csproj`
   (e o mesmo para `samples`).
5. **Documentação:** `docs/algoritmos/<nome>.md` (o que faz, diagrama, código do Reel, complexidade, números do Reel,
   uso da biblioteca, exercícios e, se for de segurança, **Como se defender**), uma linha na tabela do `README.md` e
   do `README.en.md`, confira que o laço do `.github/workflows/ci.yml` roda o novo exemplo (ele percorre todas as pastas de `samples/`) e uma entrada no `CHANGELOG.md`.

## Pull requests

- Um assunto por pull request.
- Descreva o que mudou e por quê (o modelo de PR ajuda).
- Confirme que `dotnet build -warnaserror` e `dotnet test` passam.
- Mensagens de commit em português, curtas e no imperativo (ex.: `Adiciona exemplo da Torre de Hanói`).

## Conduta

Seja gentil e paciente: muita gente aqui está começando. Críticas ao código, nunca às pessoas.
