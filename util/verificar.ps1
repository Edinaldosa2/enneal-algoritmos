# Verificação completa (Windows/PowerShell): compila sem avisos, roda os testes
# e confere a saída de cada exemplo com o saida-esperada.txt.
# Uso, na raiz do repositório:  powershell -ExecutionPolicy Bypass -File util\verificar.ps1
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host '== Compilando (sem avisos) =='
dotnet build enneal-algoritmos.sln -c Release -warnaserror -nologo -v q
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host '== Testando =='
dotnet test enneal-algoritmos.sln -c Release --no-build -nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host '== Conferindo a saída dos exemplos =='
$falhas = 0
foreach ($pasta in Get-ChildItem samples -Directory) {
    $arquivo = Join-Path $pasta.FullName 'saida-esperada.txt'
    if (-not (Test-Path $arquivo)) { continue }
    $esperado = (Get-Content $arquivo -Raw -Encoding UTF8) -replace "`r`n", "`n"
    $saida = ((dotnet run -c Release --no-build --project $pasta.FullName) -join "`n") + "`n"
    if ($saida -eq $esperado) {
        Write-Host "  ok     $($pasta.Name)"
    } else {
        Write-Host "  DIFERE $($pasta.Name)"
        $falhas++
    }
}

if ($falhas -ne 0) {
    Write-Host "$falhas exemplo(s) com saída diferente do esperado."
    exit 1
}
Write-Host 'Tudo certo.'
