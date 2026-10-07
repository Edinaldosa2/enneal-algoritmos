# N-Rainhas de 4x4 até 8x8

Resolve 4x4, 5x5, 6x6, 7x7 e 8x8 em sequência e compara o esforço de cada tabuleiro.

Reel: (link em breve)

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd samples/n-rainhas-4x4-ate-8x8
dotnet run
```

## Saída esperada

```
4x4: 1 3 0 2 | tentativas 26 | voltas 4
   . R . .
   . . . R
   R . . .
   . . R .

5x5: 0 2 4 1 3 | tentativas 15 | voltas 0
   R . . . .
   . . R . .
   . . . . R
   . R . . .
   . . . R .

6x6: 1 3 5 0 2 4 | tentativas 171 | voltas 25
   . R . . . .
   . . . R . .
   . . . . . R
   R . . . . .
   . . R . . .
   . . . . R .

7x7: 0 2 4 6 1 3 5 | tentativas 42 | voltas 2
   R . . . . . .
   . . R . . . .
   . . . . R . .
   . . . . . . R
   . R . . . . .
   . . . R . . .
   . . . . . R .

8x8: 0 4 7 5 2 6 1 3 | tentativas 876 | voltas 105
   R . . . . . . .
   . . . . R . . .
   . . . . . . . R
   . . . . . R . .
   . . R . . . . .
   . . . . . . R .
   . R . . . . . .
   . . . R . . . .

Do mais fácil ao mais difícil (tentativas até a 1ª solução):
  5x5:   15 tentativas,   0 voltas
  4x4:   26 tentativas,   4 voltas
  7x7:   42 tentativas,   2 voltas
  6x6:  171 tentativas,  25 voltas
  8x8:  876 tentativas, 105 voltas
```

Este arquivo também está em [`saida-esperada.txt`](saida-esperada.txt), e os testes conferem que a saída continua
idêntica.

## Onde está o algoritmo

- Explicação completa, diagrama e complexidade: [`docs/algoritmos/n-rainhas.md`](../../docs/algoritmos/n-rainhas.md)
- Código comentado: [`src/Enneal.Algoritmos.NRainhas`](../../src/Enneal.Algoritmos.NRainhas)
