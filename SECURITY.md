# Política de segurança

## Sobre o conteúdo deste repositório

Este é um repositório **educativo**. Os exemplos de segurança são simulações de brinquedo, auditorias do próprio
sistema e laboratórios locais que mostram por que as defesas funcionam:

- nada sai da máquina: nenhum exemplo acessa a internet, lê arquivos de senha ou acessa outro sistema; os únicos
  sockets (laboratório TLS e minimal API da URL) escutam em `127.0.0.1`, dentro do próprio processo;
- o "alvo" é sempre uma variável, um mapa em texto, uma tabela de rotas, uma lista de objetos ou um servidor criado
  pelo próprio programa, com nomes fictícios (`.example`, `.invalid`);
- a força bruta só aceita um PIN de 4 dígitos e só "ataca" um login criado no mesmo processo.

O `LoginProtegido` é material de estudo. Em produção, use a solução de identidade da sua plataforma (por exemplo
ASP.NET Core Identity) e um rate limiter mantido (por exemplo o middleware de rate limiting do ASP.NET Core).

**Use o que aprender aqui só em sistemas seus ou com autorização por escrito.**

Detalhes em [docs/seguranca-didatica.md](docs/seguranca-didatica.md).

## Versões com suporte

| Versão | Suporte |
|--------|---------|
| 1.3.x | sim |
| < 1.3 | não |

## Como relatar uma vulnerabilidade

Se encontrar um problema de segurança no código (por exemplo, um exemplo que acessa algo fora do processo ou uma
defesa explicada de forma errada):

1. **Não abra uma issue pública.**
2. Use o relato privado do GitHub: aba **Security > Report a vulnerability** deste repositório.
3. Descreva o problema, o arquivo e, se possível, como reproduzir.

A resposta inicial sai em até 7 dias. Correções são publicadas numa nova versão e registradas no
[CHANGELOG](CHANGELOG.md).

## O que não aceitamos

Contribuições com ferramentas de ataque, alvos reais, quebra de senhas de verdade, exploração de vulnerabilidades ou
geração de tráfego de rede não serão aceitas.
