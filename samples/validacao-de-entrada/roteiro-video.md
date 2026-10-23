# Roteiro narrado: validação de entrada em C# (3 a 4 minutos)

Formato: vídeo horizontal de 2 a 5 minutos para YouTube, com corte vertical para IG e FB. Narração em português,
calma, com pausas nas vírgulas. Tela: editor com o código deste repositório e o terminal rodando `dotnet run` e
`dotnet test`. Sem emoji na tela. Cada cena indica o arquivo e as linhas a destacar.

Duração estimada: cerca de 640 palavras de narração, perto de 4 minutos num ritmo de 160 palavras por minuto. Para
ficar perto de 3 min 30 s, corte as duas últimas frases da cena 3 (o link de confirmação) e as duas últimas da cena 5
(Razor e `Html.Raw`).

Regras do texto: nenhum número que não saia do código ou da saída do programa; nenhuma promessa de segurança total.
A frase-chave é "validação reduz o risco, não substitui as outras defesas".

---

## Cena 1. O gancho (0:00 a 0:15)

**Tela:** o formulário com o campo idade preenchido com `34 OR 1=1`. Corte para o terminal: `recusado: idade`.

**Narração:**
> Todo campo de formulário é texto, até você provar o contrário. Hoje, em C#, eu vou mostrar como provar, campo por
> campo, e por que isso ainda não basta contra XSS e SQL injection.

## Cena 2. A arquitetura: o contrato (0:15 a 0:45)

**Tela:** `Contrato.cs`, destacar `interface IValidadorDeEntrada` com `Validar` e `DetectarTipo`. Depois
`RegistroNoDi.cs`, a linha `AddSingleton<IValidadorDeEntrada, ValidadorDeEntrada>()`. Depois o construtor de
`ServicoDeCadastro`.

**Narração:**
> Primeiro, a arquitetura. A regra de validação mora atrás de um contrato, a interface IValidadorDeEntrada. Ela
> registra no container de injeção de dependência do .NET, e o serviço de cadastro recebe essa interface pelo
> construtor. O serviço não dá new no validador, e nem sabe qual implementação está por trás. Na prática, isso
> deixa a regra num lugar só, e nos testes eu troco o validador por um falso sem mudar uma linha do serviço.

## Cena 3. Detectar o tipo sem regex (0:45 a 1:20)

**Tela:** `ValidadorDeEntrada.cs`, os métodos `TentarInteiro`, `TentarDecimal`, `TentarData`, `TentarEmail`, com
`NumberStyles` e `CultureInfo.InvariantCulture` em destaque. Terminal: o bloco "Detectar o tipo".

**Narração:**
> Agora, o tipo. A função DetectarTipo tenta, em ordem, inteiro, decimal, data e e-mail, e o que sobra é texto.
> Tudo com TryParse, com cultura invariante, e com o NumberStyles fechado: só sinal, e no decimal, o ponto.
> Por isso, um e nove, que o double aceitaria como expoente, não vira número. Zero x um F também não. E dois mil e
> vinte e seis, fevereiro, trinta, não é data. No e-mail, eu uso o MailAddress do próprio .NET, e exijo que o
> resultado seja exatamente o que foi digitado. Assim, um e-mail com nome de exibição, entre aspas, é recusado.
> E lembre: formato certo não prova que a caixa existe. Para isso, mande um link de confirmação.

## Cena 4. A ordem que importa (1:20 a 2:05)

**Tela:** o método `Validar`, numerado de 1 a 5. Destacar `FatorTetoBruto`, `Normalize(NormalizationForm.FormKC)` e
`SoCaracteresPermitidos`. Terminal: o bloco "Cadastro".

**Narração:**
> A ordem dos passos é o que faz isso funcionar. Primeiro, um limite bruto: o que passa de quatro vezes o tamanho do
> campo é recusado antes de qualquer processamento. Segundo, a normalização Unicode, na forma NFKC. Ela transforma
> o sinal de menor de largura total no menor comum, e o três e quatro de largura total em trinta e quatro.
> Se eu validasse antes de normalizar, o disfarce passaria. Terceiro, o limite de novo, porque normalizar pode
> aumentar o texto. Quarto, uma allowlist de caracteres: letras, números, pontuação, símbolos e o espaço comum.
> Fica de fora o caractere invisível que inverte a direção do texto, o byte nulo e a quebra de linha. E o apóstrofo
> de O'Brien continua aceito, porque nome com apóstrofo é nome legítimo. Por último, o tipo e a faixa: idade de
> zero a cento e trinta. Converter para inteiro prova o tipo, não que o valor faz sentido.

## Cena 5. Por que validar não basta: XSS (2:05 a 2:40)

**Tela:** `SaidaHtml.cs`, `ParagrafoErrado` e `ParagrafoCerto` lado a lado. Terminal: o bloco "XSS".

**Narração:**
> Aqui está a pegadinha. O texto menor, script, maior, passa na validação de texto, e deve passar, porque é só
> texto. O perigo não está em guardar, está em colar isso no HTML. No jeito errado, a página recebe uma tag script.
> No jeito certo, o HtmlEncoder do .NET transforma os sinais em entidades, e o navegador mostra o texto, sem
> executar nada. A defesa contra XSS é codificar na saída, conforme o contexto. No Razor, isso já é o padrão. O
> risco volta quando alguém usa Html.Raw.

## Cena 6. SQL injection: concatenado x parametrizado (2:40 a 3:15)

**Tela:** `RepositorioDeClientes.cs`, `BuscarPorNomeErrado` e `BuscarPorNomeCerto`. Terminal: o bloco "SQLi", com
`4 linhas` e `0 linhas`.

**Narração:**
> Mesma ideia no banco. Eu busco pelo nome aspas, OR, um igual a um. Na versão concatenada, o texto vira parte do
> SQL, e a consulta devolve as quatro linhas da tabela. Na versão parametrizada, o SQL é fixo, o nome vai como
> parâmetro, e o banco trata aquilo só como dado. Resultado: zero linhas. Validação ajuda, mas quem protege a
> consulta é o parâmetro.

## Cena 7. Os testes (3:15 a 3:35)

**Tela:** terminal com `dotnet test`, mostrando os 82 testes do projeto de validação passando. Rolar rapidamente
pelos `InlineData` maliciosos de `ValidadorDeEntradaTests.cs`.

**Narração:**
> E cada um desses ataques virou teste. SQL na idade, NaN, e-mail com quebra de linha, data impossível, caractere
> invisível, union select. São oitenta e dois testes xUnit neste projeto, e todos passam.

## Cena 8. Fechamento (3:35 a 3:50)

**Tela:** o README do exemplo, com a lista de fontes, e o link do repositório.

**Narração:**
> Resumindo: contrato no DI, allowlist, normalizar antes de validar, tipo com TryParse, encoding na saída e
> consulta parametrizada. Validação reduz o risco, não substitui as outras defesas. O código completo, com os
> testes e as fontes da OWASP e da Microsoft, está no repositório enneal-algoritmos, link na descrição.

---

## Notas de produção

- Os números falados (4 linhas, 0 linhas, 82 testes, 0 a 130, 4 vezes o limite) saem do código e da
  [`saida-esperada.txt`](saida-esperada.txt). Se o código mudar, rode de novo e atualize a narração.
- Gravar o terminal com a saída real de `dotnet run` e `dotnet test`; não montar a saída à mão.
- Corte vertical para IG e FB: cenas 1, 4, 5 e 6 (cerca de 2 minutos).
