# util — scripts de manutenção

| Script | O que faz |
|--------|-----------|
| `verificar.ps1` | Windows: compila sem avisos, roda os testes e confere a saída de cada exemplo |
| `verificar.sh` | o mesmo, para Linux, macOS ou Git Bash |
| `configurar-github.ps1` | define descrição, site e tópicos do repositório no GitHub e liga o relato privado de vulnerabilidades (precisa do GitHub CLI logado) |

## Verificação completa

```powershell
powershell -ExecutionPolicy Bypass -File util\verificar.ps1
```

```bash
bash util/verificar.sh
```

## Checklist antes de cada commit

- [ ] `util\verificar.ps1` (ou `util/verificar.sh`) termina com **Tudo certo.**
- [ ] Nenhum `bin/`, `obj/` ou `TestResults/` no `git status`
- [ ] Se mudou um número de Reel de propósito: `saida-esperada.txt`, testes, docs e READMEs atualizados juntos
- [ ] Nova versão registrada no `CHANGELOG.md`

## Publicando uma versão

```powershell
git tag -a v1.2.0 -m "enneal-algoritmos 1.2.0"
git push origin main
git push origin v1.2.0
```

Depois, opcionalmente, crie um Release no GitHub a partir da tag.
