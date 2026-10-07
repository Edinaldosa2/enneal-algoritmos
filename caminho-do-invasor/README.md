# Caminho do invasor (busca em largura e defesa em camadas)

Um "invasor" sai da **Internet** e procura **qualquer caminho** até o **banco de dados** num mapa de rede,
usando **busca em largura (BFS)**. Na primeira rodada a rede não tem defesas. Na segunda, entram quatro
camadas, cada uma **com furos**, e mesmo assim o banco fica protegido. É a ideia de **defesa em camadas**.

> Demo educativa: o "invasor" é só uma busca num mapa abstrato do próprio programa. Nada de rede, IPs,
> ferramentas ou alvos reais.

Reel: (link em breve)

## O mapa

```
I = Internet (início)   B = banco de dados   # = parede   . = livre
```

| Rodada | O que acontece |
|--------|----------------|
| Sem defesas | o invasor chega no banco em **93 passos** |
| Em camadas (WAF, MFA no login, menor privilégio, criptografia + segmentação) | **0 caminhos até o banco** (48 células exploradas) |

Na segunda rodada o invasor ainda passa por furos do WAF e do MFA, e isso é de propósito: **nenhuma camada
sozinha é perfeita**. O que barra o ataque é que os furos de uma camada não se alinham com os da seguinte.

## Como funciona a BFS

1. Coloca o início numa **fila** e marca como visto.
2. Tira a primeira célula da fila (cada uma conta como **1 passo**). Se for o banco, achou.
3. Põe na fila os vizinhos (baixo, direita, esquerda, cima) que não são parede e ainda não foram vistos.
4. Repete até achar o banco ou a fila esvaziar (aí não existe caminho).

Como explora em "ondas" (tudo a 1 passo, depois tudo a 2 passos...), a BFS sempre encontra o caminho
**mais curto**. O programa refaz esse caminho e desenha no mapa com `*`.

## Complexidade

- **Tempo:** **O(V + E)**, onde V é o número de células e E o de ligações entre vizinhas. Cada célula entra na
  fila no máximo uma vez. Neste mapa 9 x 13 são no máximo 117 células.
- **Memória:** **O(V)** para a fila e o conjunto de vistos.

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd caminho-do-invasor
dotnet run
```

## Saída esperada

```
== Round 1: rede SEM defesas ==
sem defesas: INVADIDO em 93 passos
(caminho mais curto até o banco: 12 movimentos, marcado com *)
   . . . . . . I . . . . . .
   . . . . . . * * * . . . .
   . # # . . # # # * . # # .
   . . . . . . . . * . . . .
   . . . # . . . . * # . . .
   . # . . . . # * * . . # .
   . . . . # . . * # . . . .
   . . . . . . . * . . . . .
   . . . . . . B * . . . . .

== Round 2: rede com defesa EM CAMADAS ==
em camadas: PROTEGIDO, 0 caminhos até o banco (48 células exploradas)
(o = células que o invasor conseguiu alcançar)
   o o o o o o I o o o o o o
   o o o o o o o o o o o o o
   o # # o o # # # o o # # o
   o o o o o o o o o o o o o
   # # o # # # # # # o # # #
   # # # # . # # # # o # # #
   # . # # # # # # # # # . #
   # # # # # # # . # # # # #
   . . . . . . B . . . . . .
```

## Como se defender

- **Defesa em camadas:** não aposte numa barreira só. Combine firewall de aplicação (WAF), autenticação forte,
  permissões mínimas e segmentação de rede, para que a falha de uma camada não exponha tudo.
- **MFA** em todos os acessos administrativos e remotos.
- **Menor privilégio:** cada usuário, serviço e aplicação acessa só o que precisa. A aplicação não deve
  conectar no banco com uma conta de administrador.
- **Segmentação:** o banco de dados não deve ser alcançável direto da Internet, só pelas máquinas que
  realmente precisam dele.
- **Criptografia** em trânsito (TLS) e em repouso, para dados roubados não serem legíveis.
- **Monitoramento:** registre e revise acessos para perceber movimentação estranha entre as camadas.
- **Teste os seus furos:** revisões de configuração e testes de intrusão autorizados mostram onde as camadas
  se alinham antes que alguém descubra.
