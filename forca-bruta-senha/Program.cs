// =====================================================================
//  Força bruta x login protegido (simulação didática)
//  Enneal · @enneal.it · https://enneal.com.br
//
//  IMPORTANTE: isto é uma SIMULAÇÃO de brinquedo para ensinar DEFESA.
//  O "alvo" é uma variável deste próprio programa. Não há rede, arquivos,
//  sistemas de terceiros nem senhas reais. Não é ferramenta de ataque.
//
//  Round 1: um PIN de 4 dígitos sem nenhuma proteção. Um laço simples
//           testa 0000, 0001, 0002... até acertar.
//  Round 2: o MESMO laço contra um login que guarda só o hash da senha
//           (com salt) e bloqueia a conta depois de 5 erros.
//
//  Os números impressos são os mesmos do Reel:
//    Round 1 -> 7392 tentativas até abrir o PIN 7391
//    Round 2 -> bloqueado após 5 tentativas (15 min)
//               no mesmo PIN: 1478 bloqueios = 15.4 dias de espera
//
//  Como rodar (dentro desta pasta, com o .NET 8 SDK):  dotnet run
// =====================================================================

using System.Security.Cryptography;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

// =====================================================================
// Round 1: PIN de 4 dígitos, sem proteção
// =====================================================================
Console.WriteLine("== Round 1: PIN de 4 dígitos, SEM proteção ==");

string pin = "7391"; // o PIN "secreto" da demo (está aqui mesmo, na memória)
int tentativas = 0;

// Existem só 10.000 PINs possíveis (0000 a 9999). Testar todos é rápido.
for (int n = 0; n <= 9999; n++)
{
    tentativas++;
    string palpite = n.ToString("D4"); // "D4" completa com zeros: 42 -> "0042"
    if (palpite == pin)
    {
        Console.WriteLine("ABERTO!");
        break;
    }
}
Console.WriteLine(tentativas); // 7392
Console.WriteLine($"(o PIN {pin} caiu na tentativa {tentativas} de 10000 possíveis)");
Console.WriteLine();

// =====================================================================
// Round 2: o mesmo laço contra um login protegido
// =====================================================================
Console.WriteLine("== Round 2: login protegido (hash + bloqueio) ==");

const int MaxFalhas = 5;        // erros permitidos antes de bloquear
const int MinutosBloqueio = 15; // tempo de bloqueio da conta

// Defesa 1: o servidor NUNCA guarda a senha em texto. Ele guarda um salt
// aleatório e o hash (lento, de propósito) da senha com esse salt.
// Aqui usamos PBKDF2, que já vem no .NET. Em produção também são ótimas
// opções o Argon2id ou o bcrypt (ex.: pacote BCrypt.Net-Next).
const int Iteracoes = 100_000;
byte[] salt = RandomNumberGenerator.GetBytes(16);
byte[] hashSalvo = GerarHash(pin, salt);

// Defesa 2: contar as falhas no SERVIDOR e bloquear a conta por um tempo.
int falhas = 0;
bool bloqueado = false;

int pedidos = 0;
for (int n = 0; n <= 9999 && !bloqueado; n++)
{
    pedidos++;
    if (Entrar(n.ToString("D4")))
    {
        Console.WriteLine("ABERTO!");
        break;
    }
}
Console.WriteLine($"bloqueado após {falhas} tentativas ({MinutosBloqueio} min)");
Console.WriteLine($"(o {pedidos}º pedido já encontrou a conta bloqueada)");
Console.WriteLine();

// Quanto o bloqueio atrasa o MESMO PIN do Round 1?
// A cada 5 erros, 15 minutos de espera. Para chegar na tentativa 7392,
// o atacante erra 7391 vezes -> 7391 / 5 = 1478 bloqueios completos.
int bloqueios = (tentativas - 1) / MaxFalhas;
double dias = bloqueios * MinutosBloqueio / 60.0 / 24.0;
Console.WriteLine($"mesmo PIN: {bloqueios} bloqueios = {dias:0.0} dias");

// ---------------------------------------------------------------------
// Entrar(senha): o "login" da demo.
// ---------------------------------------------------------------------
bool Entrar(string senha)
{
    // Conta bloqueada: recusa SEM nem olhar a senha.
    if (falhas >= MaxFalhas)
    {
        bloqueado = true; // num sistema real: bloquear por MinutosBloqueio
        return false;
    }

    // Calcula o hash da senha digitada com o mesmo salt e compara com o
    // hash guardado. FixedTimeEquals leva sempre o mesmo tempo, para a
    // comparação não "vazar" quantos bytes estavam certos.
    byte[] hashDigitado = GerarHash(senha, salt);
    if (CryptographicOperations.FixedTimeEquals(hashDigitado, hashSalvo)) return true;

    falhas++;
    return false;
}

// Hash lento da senha com salt (PBKDF2 com SHA-256).
byte[] GerarHash(string senha, byte[] saltDaConta) =>
    Rfc2898DeriveBytes.Pbkdf2(senha, saltDaConta, Iteracoes, HashAlgorithmName.SHA256, 32);
