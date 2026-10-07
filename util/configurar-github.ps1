# Configura a página do repositório no GitHub: descrição, site, tópicos e o
# relato privado de vulnerabilidades (usado pelo SECURITY.md).
# Precisa do GitHub CLI (https://cli.github.com) logado: gh auth login
# Uso:  powershell -ExecutionPolicy Bypass -File util\configurar-github.ps1
$ErrorActionPreference = 'Stop'
$repo = 'Edinaldosa2/enneal-algoritmos'

gh repo edit $repo `
    --description 'O código completo dos algoritmos dos Reels da Enneal (@enneal.it): C# / .NET 8, comentado em português e testado.' `
    --homepage 'https://enneal.com.br' `
    --add-topic algoritmos `
    --add-topic csharp `
    --add-topic dotnet `
    --add-topic estrutura-de-dados `
    --add-topic backtracking `
    --add-topic dijkstra `
    --add-topic a-star `
    --add-topic bfs `
    --add-topic sorting-algorithms `
    --add-topic binary-search `
    --add-topic rate-limiting `
    --add-topic seguranca `
    --add-topic educacao `
    --add-topic xunit
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Liga o "Report a vulnerability" (aba Security).
gh api -X PUT "repos/$repo/private-vulnerability-reporting"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host 'Repositório configurado.'
