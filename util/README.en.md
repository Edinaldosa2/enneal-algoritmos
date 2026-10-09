[Português](README.md) | English

# util — maintenance scripts

| Script | What it does |
|--------|--------------|
| `verificar.ps1` | Windows: builds with no warnings, runs the tests and checks each sample's output |
| `verificar.sh` | the same, for Linux, macOS or Git Bash |
| `configurar-github.ps1` | sets the repository description, homepage and topics on GitHub and turns on private vulnerability reporting (needs GitHub CLI signed in) |

## Full check

```powershell
powershell -ExecutionPolicy Bypass -File util\verificar.ps1
```

```bash
bash util/verificar.sh
```

## Checklist before each commit

- [ ] `util\verificar.ps1` (or `util/verificar.sh`) ends with **Tudo certo.**
- [ ] No `bin/`, `obj/` or `TestResults/` in `git status`
- [ ] If you changed a Reel number on purpose: update `saida-esperada.txt`, tests, docs and READMEs together
- [ ] New version recorded in `CHANGELOG.md`

## Publishing a version

```powershell
git tag -a v1.3.0 -m "enneal-algoritmos 1.3.0"
git push origin main
git push origin v1.3.0
```

Then, optionally, create a GitHub Release from the tag.
