# N-Rainhas (backtracking)

Como colocar **N rainhas** num tabuleiro **N x N** sem que nenhuma ataque outra (mesma linha, coluna ou
diagonal)? O programa testa casa por casa e, quando chega num beco sem saída, **volta** e tenta outra
opção. Essa técnica se chama **backtracking** ("tentar e voltar").

Reel: (link em breve)

Esta pasta tem duas versões:

| Pasta | O que faz |
|-------|-----------|
| [`basico/`](basico/) | resolve o clássico **8x8** e desenha o tabuleiro |
| [`4x4-ate-8x8/`](4x4-ate-8x8/) | resolve **4x4, 5x5, 6x6, 7x7 e 8x8** em sequência e compara o esforço |

## Como funciona

1. Coloca **uma rainha por linha**, de cima para baixo (assim nunca há duas na mesma linha).
2. Em cada linha, testa as colunas da esquerda para a direita com `Seguro(linha, coluna)`.
3. A casa é segura se nenhuma rainha de cima está na **mesma coluna** (`q == c`) nem na **mesma diagonal**
   (`Abs(q - c) == l - i`: andou tantas colunas quanto andou linhas).
4. Se é segura, coloca a rainha e chama `Resolver(linha + 1)` (recursão).
5. Se nenhuma coluna serve, tira a rainha da linha anterior (**volta**) e continua de onde parou.

O tabuleiro inteiro cabe num vetor: `rainhas[linha] = coluna`.

## Complexidade

- **Tempo:** no pior caso cresce de forma exponencial, na ordem de **O(N!)**, porque cada linha tem menos
  opções que a anterior. Na prática o backtracking corta a maioria dos ramos cedo, por isso o 8x8 sai com
  só 876 tentativas, quando existem 4.426.165.368 jeitos de pôr 8 rainhas em 64 casas.
- **Memória:** **O(N)** (o vetor `rainhas` e a pilha da recursão).

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd n-rainhas/basico
dotnet run
```

```bash
cd n-rainhas/4x4-ate-8x8
dotnet run
```

Ou, da raiz do repositório: `dotnet run --project n-rainhas/basico` e `dotnet run --project n-rainhas/4x4-ate-8x8`.

## Saída esperada

`basico/` (8x8):

```
Solução (coluna da rainha em cada linha, de cima para baixo):
0 4 7 5 2 6 1 3

tentativas 876 | voltas 105
```

`4x4-ate-8x8/` (primeira solução de cada tabuleiro):

| Tabuleiro | 1ª solução | Tentativas | Voltas |
|-----------|------------|-----------:|-------:|
| 4x4 | 1 3 0 2 | 26 | 4 |
| 5x5 | 0 2 4 1 3 | 15 | 0 |
| 6x6 | 1 3 5 0 2 4 | 171 | 25 |
| 7x7 | 0 2 4 6 1 3 5 | 42 | 2 |
| 8x8 | 0 4 7 5 2 6 1 3 | 876 | 105 |

Tabuleiro maior nem sempre é mais difícil: o 5x5 sai sem nenhuma volta, e o 6x6 dá mais trabalho que o 7x7.

- **Tentativas** = quantas casas foram testadas (chamadas de `Seguro`).
- **Voltas** = quantas vezes uma rainha foi retirada (backtrack).

## Para praticar

- Troque `const int N = 8;` no `basico/Program.cs` por outros valores (o 2x2 e o 3x3 não têm solução).
- Em vez de parar na primeira, conte **todas** as soluções do 8x8 (são 92).
