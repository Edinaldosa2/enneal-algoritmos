# Corrida de ordenações: Bubble x Quick x Merge

Três algoritmos de ordenação clássicos apostam corrida no **mesmo vetor** de 28 números embaralhados.
Quem precisar de **menos passos** para deixar tudo em ordem vence.

Reel: (link em breve)

## A regra da corrida

**1 passo = 1 comparação, 1 troca ou 1 escrita no vetor.** Todos os algoritmos usam só três operações
(`Menor`, `Troca` e `Escreve`), que contam os passos, então a disputa é justa.

O vetor é `1..28` embaralhado uma vez com semente fixa (2026) e escrito no código, para todo mundo rodar com
o mesmo vetor do vídeo.

## Os competidores

| Algoritmo | Ideia | Tempo (melhor / médio / pior) | Memória extra |
|-----------|-------|-------------------------------|---------------|
| **Bubble Sort** | compara vizinhos e troca os fora de ordem; o maior "borbulha" para o fim a cada volta | O(n) / O(n²) / O(n²) | O(1) |
| **Quick Sort** | escolhe um pivô, põe os menores à esquerda e os maiores à direita, e repete em cada lado | O(n log n) / O(n log n) / O(n²) | O(log n) (recursão) |
| **Merge Sort** | divide ao meio, ordena cada metade e intercala as duas | O(n log n) / O(n log n) / O(n log n) | O(n) |

O Quick Sort usado aqui tem o último elemento como pivô (partição de Lomuto). O Merge Sort é estável: números
iguais mantêm a ordem original.

## Como rodar

Precisa do [.NET 8 SDK](https://dotnet.microsoft.com/download) (grátis).

```bash
cd corrida-de-ordenacoes
dotnet run
```

## Saída esperada

```
Vetor inicial: 13 9 7 3 25 6 19 22 27 5 12 2 10 1 23 15 16 14 18 24 20 8 28 21 26 17 11 4

Bubble: comparações 375 | trocas 161 | escritas 0 | passos 536 | ordenado True
Quick: comparações 116 | trocas 69 | escritas 0 | passos 185 | ordenado True
Merge: comparações 99 | trocas 0 | escritas 136 | passos 235 | ordenado True

Chegada: 1º Quick (185) · 2º Merge (235) · 3º Bubble (536)
```

O Merge faz menos comparações (99) que o Quick (116), mas gasta mais escritas copiando de volta do vetor
auxiliar. Com vetores maiores, a diferença entre os O(n log n) e o O(n²) do Bubble fica enorme.

## Para praticar

- Teste com o vetor **já ordenado** e com o vetor **ao contrário**: veja o Bubble melhorar e o Quick com pivô no
  fim piorar.
- Troque o pivô do Quick pelo elemento do meio e compare os passos.
