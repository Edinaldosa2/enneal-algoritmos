// =====================================================================
//  Comparação em tempo constante
//  Enneal · @enneal.it · https://enneal.com.br
//
//  Por que comparar segredo (token de API, assinatura HMAC, código de
//  reset) com "==" ou com um laço que sai no primeiro caractere diferente
//  é bug, e o conserto de 1 linha do .NET:
//  CryptographicOperations.FixedTimeEquals.
//
//  O "custo" é um MODELO didático e determinístico: quantas comparações
//  de caractere cada versão faz. Não mede tempo de nada. Token e
//  candidatos são de brinquedo.
//
//  As comparações estão em src/Enneal.Algoritmos.ComparacaoSegura.
//  Números do Reel:
//    sai cedo        -> custos 1, 4, 2, 6, 3, 6 (mais alto = mais letras certas no começo)
//    tempo constante -> 6 em todos; mesma resposta em 6/6
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):
//    dotnet run                    -> o resumo por candidato
//    dotnet run -- --comparacoes   -> também cada comparação de caractere
// =====================================================================

using System.Text;
using Enneal.Algoritmos.ComparacaoSegura;

Console.OutputEncoding = Encoding.UTF8;

bool mostrarComparacoes = args.Contains("--comparacoes");

// Com --comparacoes: "I" = laço que sai cedo, "S" = modelo do FixedTimeEquals.
var resultados = DemoDoReel.Comparar(DemoDoReel.Token, DemoDoReel.Candidatos,
    mostrarComparacoes ? (versao, c, i, e, r) => Console.WriteLine($"{versao} {c} {i} {e} {r} {(e == r ? "igual" : "dif")}") : null);

int iguais = 0;
for (int c = 0; c < resultados.Count; c++)
{
    var r = resultados[c];
    if (r.RespostaSaiCedo == r.RespostaTempoConstante) iguais++;
    Console.WriteLine($"Candidato {c + 1} {r.Candidato}: prefixo igual {r.PrefixoIgual} | sai cedo: custo {r.CustoSaiCedo}"
        + $" -> {Texto(r.RespostaSaiCedo)} | FixedTimeEquals: custo {r.CustoTempoConstante} -> {Texto(r.RespostaTempoConstante)}");
}
Console.WriteLine($"Resumo: sai cedo [{string.Join(", ", resultados.Select(r => r.CustoSaiCedo))}] | tempo constante"
    + $" [{string.Join(", ", resultados.Select(r => r.CustoTempoConstante))}] | mesma resposta em {iguais}/{resultados.Count}");

static string Texto(bool b) => b ? "true" : "false";
