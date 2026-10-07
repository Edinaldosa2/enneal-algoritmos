# Changelog

Todas as mudanças importantes deste repositório ficam registradas aqui.

O formato segue o [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e as versões seguem o
[Versionamento Semântico](https://semver.org/lang/pt-BR/).

## 1.2.0 - 2026-10-07

### Adicionado

- Novo algoritmo: **Torre de Hanói** (recursão). Biblioteca `Enneal.Algoritmos.TorreHanoi` com `ResolvedorHanoi`
  (a recursão do Reel), `Pinos` (três pilhas que recusam um disco maior sobre um menor) e `ContaDeHanoi`
  (2ⁿ − 1 movimentos e os anos a 1 movimento por segundo, com `UInt128`).
- Exemplo `samples/torre-de-hanoi`: 3 discos = 7 movimentos, 4 = 15, 6 = 63 e 64 discos =
  18.446.744.073.709.551.615 movimentos (≈ 584.542.046.090 anos); `--movimentos` imprime cada movimento.
- Testes da Torre de Hanói: números do Reel, 2ⁿ − 1 e a recorrência T(n) = 2 · T(n − 1) + 1, nenhum disco maior
  sobre um menor, quantas vezes cada disco se move e a saída do exemplo.
- Página `docs/algoritmos/torre-de-hanoi.md` com o diagrama da recursão e a árvore de chamadas com 3 discos.

## 1.1.0 - 2026-10-07

### Adicionado

- Bibliotecas por algoritmo em `src/` (`Enneal.Algoritmos.*`), sem `Console`, devolvendo resultados e contadores,
  com comentários `///` em português em todos os membros públicos.
- Exemplos de console em `samples/`, um por Reel, cada um com `README.md` e `saida-esperada.txt`.
- Projeto de testes `Enneal.Algoritmos.Tests` (xUnit): números exatos dos Reels, propriedades dos algoritmos,
  defesas do login e do token bucket, e saída completa de cada exemplo.
- Novo algoritmo: **Dijkstra x A\*** (como o GPS acha a rota).
- Novo algoritmo: **Busca linear x busca binária**.
- `ResolvedorNRainhas.ContarSolucoes` e `SolucaoValida`.
- `LoginProtegido` com bloqueio que vence sozinho depois do tempo configurado (relógio injetável para testes).
- Documentação: `docs/arquitetura.md`, `docs/seguranca-didatica.md`, `docs/roteiro.md` e uma página por algoritmo
  com diagrama, complexidade, código do Reel e exercícios.
- README em inglês (`README.en.md`), banner e selos.
- GitHub Actions: compila sem avisos, roda os testes e todos os exemplos.
- `CONTRIBUTING.md`, `SECURITY.md`, modelos de issue e de pull request, scripts em `util/`.
- `Directory.Packages.props` (versões centrais de pacotes) e `global.json`.

### Alterado

- Estrutura de pastas: `src/`, `samples/`, `tests/` e `docs/` no lugar de uma pasta por algoritmo na raiz.
- Força bruta: hash PBKDF2 com 600.000 iterações (valor recomendado pela OWASP).

### Mantido

- Todos os números e saídas dos Reels são idênticos aos da versão 1.0.0.

## 1.0.0 - 2026-10-07

### Adicionado

- Primeira versão: N-Rainhas (8x8 e de 4x4 até 8x8), força bruta x login protegido, caminho do invasor,
  corrida de ordenações e rate limit com token bucket, cada um como projeto de console .NET 8 com README.
