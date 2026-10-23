[Português](README.md) | English

# Input validation in C#: the contract in DI, the right type, and XSS and SQL injection done wrong vs right

A sign-up form receives 7 inputs, 6 of them malicious or invalid. The service does not know how to validate: it
receives the `IValidadorDeEntrada` contract through dependency injection. The validator detects and checks the type
with `TryParse` (no regex), normalizes Unicode, limits length and only accepts characters from an allowlist. Then the
same text is pasted into HTML (wrong) and encoded with `HtmlEncoder` (right), and the same search is concatenated into
SQL (wrong, 4 rows leaked) and parameterized (right, 0 rows).

> **Local and defensive test.** The customers are fictional, the database is an in-memory SQLite created by the
> program itself, and nothing leaves the machine. Validation reduces risk, it does not remove it: it does not replace
> parameterized queries, output encoding or permission checks.

Video: (link coming soon) · Narrated script (Portuguese): [`roteiro-video.md`](roteiro-video.md)

## How to run

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) (free).

```bash
cd samples/validacao-de-entrada
dotnet run
```

The expected output (in Portuguese, the program's language) is in [`saida-esperada.txt`](saida-esperada.txt) and in
the [Portuguese README](README.md#saída-esperada); the tests check it character by character. The saved id is 4
because the database starts with 3 fictional customers; the concatenated search returns 4 rows because Ana has
already been saved by then.

## The architecture, in 5 files

| File | Role |
|------|------|
| [`Contrato.cs`](../../src/Enneal.Algoritmos.ValidacaoEntrada/Contrato.cs) | `IValidadorDeEntrada`, `TipoDeDado`, `RegraDeCampo`, `ResultadoValidacao` |
| [`ValidadorDeEntrada.cs`](../../src/Enneal.Algoritmos.ValidacaoEntrada/ValidadorDeEntrada.cs) | the implementation: length limit, NFKC, allowlist, `TryParse` per type, `DetectarTipo` |
| [`ServicoDeCadastro.cs`](../../src/Enneal.Algoritmos.ValidacaoEntrada/ServicoDeCadastro.cs) | receives the contract through the constructor; saves only if every field passes |
| [`RepositorioDeClientes.cs`](../../src/Enneal.Algoritmos.ValidacaoEntrada/RepositorioDeClientes.cs) | the same search concatenated (wrong) and parameterized (right) |
| [`SaidaHtml.cs`](../../src/Enneal.Algoritmos.ValidacaoEntrada/SaidaHtml.cs) | the same text pasted (wrong) and encoded with `HtmlEncoder` (right) |
| [`RegistroNoDi.cs`](../../src/Enneal.Algoritmos.ValidacaoEntrada/RegistroNoDi.cs) | `AddValidacaoDeEntrada()`: registers the contract and its consumers |

## Why each step

| Step | What the code does | Why |
|------|--------------------|-----|
| Contract in DI | the service receives `IValidadorDeEntrada`, not `new ValidadorDeEntrada()` | the rule lives in one place; in the tests, a fake validator proves the service depends only on the contract |
| Raw limit | rejects anything over 4x the field maximum before normalizing | OWASP asks for size limits **before** processing input, so no resources are spent on what will be rejected |
| NFKC normalization | `"＜script＞"` becomes `"<script>"`, `"３４"` becomes `"34"`, composed and decomposed `é` become equal | validating the disguise and using the canonical form later leaves the check behind. OWASP notes that compatibility normalization can erase distinctions: here it is the chosen policy for these fields |
| Limit after | the length applies to the normalized text | normalization can make text longer (the test uses a character that becomes 18) |
| Character allowlist | letters, marks, digits, punctuation, symbols and the plain space | OWASP prefers defining what is accepted over listing every attack. Out go `U+202E` (reverses text), zero-width space, null byte and line breaks. The apostrophe in `O'Brien` stays |
| Type via `TryParse` | `int`/`decimal` with restricted `NumberStyles` and invariant culture; exact `yyyy-MM-dd` date; email via `MailAddress.TryCreate` | a maintained parser instead of fragile regex: `1e9`, `0x1F`, `1,000`, `2026-02-30` and `"Davi" <davi@...>` are rejected |
| Range | age from 0 to 130 | converting to `int` only proves the type, not that the value makes sense |
| Output encoding | `HtmlEncoder.Default.Encode` | valid text may contain `<`: the XSS defense is encoding **on output**, per context |
| Parameterized SQL | `WHERE nome = $nome` + `AddWithValue` | with a parameter, input is always data and never code; validation alone does not protect the query |

> **Why `InvariantGlobalization` is off here.** The rest of the repository uses invariant mode, and in that mode
> .NET has no normalization data: `Normalize` returns the text unchanged. The validator checks this in its
> constructor and **refuses to be created** (fail closed) instead of validating without normalizing. That is why this
> sample and its test project turn invariant mode off.

## Tests

[`tests/Enneal.Algoritmos.ValidacaoEntrada.Tests`](../../tests/Enneal.Algoritmos.ValidacaoEntrada.Tests): 82 xUnit
tests with the malicious cases (SQL in the age field, `1e2`, `NaN`, email with a display name and with a line break,
impossible date, `U+202E`, null byte, lone surrogate, text that grows when normalized, `<img onerror>`,
`' UNION SELECT`), swapping the validator for a fake, and this sample's output.

```bash
dotnet test
```

## Sources

- OWASP, [Input Validation Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html): allowlists, limits before processing, maintained parsers, Unicode, email and pitfalls
- OWASP, [Cross Site Scripting Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html) and [SQL Injection Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html)
- OWASP Top 10 2021, [A03 Injection](https://owasp.org/Top10/A03_2021-Injection/)
- Microsoft, [Dependency injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- Microsoft, [Prevent Cross-Site Scripting (XSS) in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cross-site-scripting) and [`HtmlEncoder`](https://learn.microsoft.com/en-us/dotnet/api/system.text.encodings.web.htmlencoder)
- Microsoft, [Microsoft.Data.Sqlite parameters](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/parameters)
- Microsoft, [`String.Normalize`](https://learn.microsoft.com/en-us/dotnet/api/system.string.normalize) and [`MailAddress.TryCreate`](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.mailaddress.trycreate)
- .NET runtime, [Globalization invariant mode](https://github.com/dotnet/runtime/blob/main/docs/design/features/globalization-invariant-mode.md) (*String normalization* section)
- Unicode, [UAX #15: Unicode Normalization Forms](https://unicode.org/reports/tr15/)
