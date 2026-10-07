#!/usr/bin/env bash
# Verificação completa (Linux/macOS/Git Bash): compila sem avisos, roda os testes
# e confere a saída de cada exemplo com o saida-esperada.txt.
# Uso, na raiz do repositório:  bash util/verificar.sh
set -euo pipefail
cd "$(dirname "$0")/.."

echo "== Compilando (sem avisos) =="
dotnet build enneal-algoritmos.sln -c Release -warnaserror -nologo -v q

echo "== Testando =="
dotnet test enneal-algoritmos.sln -c Release --no-build -nologo

echo "== Conferindo a saída dos exemplos =="
falhas=0
for pasta in samples/*/; do
  esperado="${pasta}saida-esperada.txt"
  [ -f "$esperado" ] || continue
  if diff <(dotnet run -c Release --no-build --project "$pasta" | tr -d '\r') <(tr -d '\r' < "$esperado") > /dev/null; then
    echo "  ok     $pasta"
  else
    echo "  DIFERE $pasta"
    falhas=$((falhas + 1))
  fi
done

if [ "$falhas" -ne 0 ]; then
  echo "$falhas exemplo(s) com saída diferente do esperado."
  exit 1
fi
echo "Tudo certo."
