# Força bruta x login protegido (simulação didática)

**Força bruta** é testar todas as senhas possíveis até acertar. Contra um PIN de 4 dígitos sem proteção isso
é trivial. Este programa mostra o ataque e, principalmente, **como um login bem feito o torna inviável**.

> **Simulação de brinquedo, para ensinar defesa.** O "alvo" é uma variável do próprio programa. Não há rede,
> arquivos, sistemas de terceiros nem senhas reais. Não é ferramenta de ataque.

Reel: (link em breve)

## O que o programa faz

- **Round 1, sem proteção:** um laço testa `0000`, `0001`, `0002`... e abre o PIN `7391` em **7.392 tentativas**.
  São só 10.000 combinações, então um computador faz isso num piscar de olhos.
- **Round 2, login protegido:** o **mesmo laço** contra uma função `Entrar()` que:
  - guarda só o **hash** da senha, com **salt** aleatório e um algoritmo **lento** de propósito (PBKDF2,
    que já vem no .NET);
  - compara os hashes em **tempo constante** (`CryptographicOperations.FixedTimeEquals`);
  - conta as falhas e **bloqueia a conta por 15 minutos depois de 5 erros**.

  Resultado: **bloqueado após 5 tentativas**. Para chegar no mesmo PIN do Round 1, o atacante precisaria de
  1.478 bloqueios, ou seja, **15,4 dias** de espera, e isso para um PIN de só 4 dígitos.

## Complexidade

- **Força bruta:** **O(Kᴸ)** tentativas no pior caso, onde K é o tamanho do alfabeto e L o tamanho da senha.
  PIN de 4 dígitos: 10⁴ = 10.000. Senha de 12 caracteres com letras, números e símbolos (~95): 95¹² ≈ 5,4 x 10²³.
- **Com bloqueio:** o tempo deixa de depender da velocidade do computador e passa a depender do relógio:
  cada 5 erros custam 15 minutos.

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd forca-bruta-senha
dotnet run
```

## Saída esperada

```
== Round 1: PIN de 4 dígitos, SEM proteção ==
ABERTO!
7392
(o PIN 7391 caiu na tentativa 7392 de 10000 possíveis)

== Round 2: login protegido (hash + bloqueio) ==
bloqueado após 5 tentativas (15 min)
(o 6º pedido já encontrou a conta bloqueada)

mesmo PIN: 1478 bloqueios = 15.4 dias
```

## Como se defender

- **Limite de tentativas** no servidor: bloqueio temporário ou espera crescente (1 s, 2 s, 4 s...) depois de
  algumas falhas, por conta **e** por origem. Prefira bloqueio temporário a permanente, para um atacante não
  conseguir trancar a conta dos seus usuários de propósito.
- **Nunca guarde senha em texto.** Guarde só o hash, com salt único por usuário e um algoritmo lento feito
  para senhas: **Argon2id**, **bcrypt** ou **PBKDF2** com muitas iterações. Hash rápido (MD5, SHA-1, SHA-256
  puro) não serve para senha.
- **Senhas longas** (frases-senha) valem mais que senhas "complicadas" e curtas. Cada caractere a mais
  multiplica o trabalho do atacante.
- **Autenticação em dois fatores (MFA):** mesmo que a senha vaze, só ela não basta.
- **Monitore e alerte:** muitas falhas seguidas, vindas de vários lugares, é sinal de ataque.
- **Não reaproveite senhas** entre sites: um vazamento em um lugar vira acesso em outro.
