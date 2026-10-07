using System.Security.Cryptography;

namespace Enneal.Algoritmos.ForcaBruta;

/// <summary>
/// Resultado de uma tentativa de login.
/// </summary>
public enum ResultadoLogin
{
    /// <summary>Senha correta: entrou.</summary>
    Aceito,

    /// <summary>Senha errada: a falha foi contada.</summary>
    SenhaErrada,

    /// <summary>Conta bloqueada: recusado sem nem olhar a senha.</summary>
    ContaBloqueada,
}

/// <summary>
/// Um login de DEMONSTRAÇÃO com as defesas básicas contra força bruta:
/// guarda só o hash da senha (PBKDF2 com salt), compara em tempo constante e
/// bloqueia a conta por um tempo depois de várias falhas seguidas.
/// </summary>
/// <remarks>
/// Tudo fica na memória deste processo: é material de estudo, não um sistema de
/// autenticação pronto. Em produção, use a solução de identidade da sua plataforma
/// (por exemplo ASP.NET Core Identity), que já faz isso e muito mais.
/// </remarks>
public sealed class LoginProtegido
{
    /// <summary>
    /// Iterações padrão do PBKDF2-SHA256 (valor recomendado pela OWASP).
    /// Quanto mais iterações, mais caro fica testar cada senha.
    /// </summary>
    public const int IteracoesPadrao = 600_000;

    private readonly byte[] _salt;
    private readonly byte[] _hashSalvo;
    private readonly int _iteracoes;
    private readonly TimeProvider _relogio;
    private DateTimeOffset _bloqueadoAte = DateTimeOffset.MinValue;

    /// <summary>
    /// Cria o login a partir da senha. A senha em texto NÃO é guardada:
    /// só um salt aleatório e o hash.
    /// </summary>
    /// <param name="senha">Senha da conta.</param>
    /// <param name="maxFalhas">Falhas seguidas permitidas antes do bloqueio (padrão 5).</param>
    /// <param name="tempoBloqueio">Duração do bloqueio (padrão 15 minutos).</param>
    /// <param name="iteracoes">Iterações do PBKDF2 (padrão <see cref="IteracoesPadrao"/>).</param>
    /// <param name="relogio">Relógio usado no bloqueio (padrão: relógio do sistema; útil para testes).</param>
    public LoginProtegido(
        string senha,
        int maxFalhas = 5,
        TimeSpan? tempoBloqueio = null,
        int iteracoes = IteracoesPadrao,
        TimeProvider? relogio = null)
    {
        ArgumentNullException.ThrowIfNull(senha);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxFalhas, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(iteracoes, 1);

        MaxFalhas = maxFalhas;
        TempoBloqueio = tempoBloqueio ?? TimeSpan.FromMinutes(15);
        _iteracoes = iteracoes;
        _relogio = relogio ?? TimeProvider.System;

        // Defesa 1: nunca guardar a senha. Guarda um salt aleatório (diferente para
        // cada conta) e o hash LENTO da senha com esse salt.
        _salt = RandomNumberGenerator.GetBytes(16);
        _hashSalvo = GerarHash(senha);
    }

    /// <summary>Falhas seguidas permitidas antes do bloqueio.</summary>
    public int MaxFalhas { get; }

    /// <summary>Quanto tempo a conta fica bloqueada.</summary>
    public TimeSpan TempoBloqueio { get; }

    /// <summary>Falhas seguidas desde o último acerto ou desbloqueio.</summary>
    public int Falhas { get; private set; }

    /// <summary>Se a conta está bloqueada agora.</summary>
    public bool Bloqueada => Falhas >= MaxFalhas && _relogio.GetUtcNow() < _bloqueadoAte;

    /// <summary>
    /// Tenta entrar com uma senha.
    /// </summary>
    /// <param name="senha">Senha digitada.</param>
    /// <returns>Aceito, senha errada ou conta bloqueada.</returns>
    public ResultadoLogin Tentar(string senha)
    {
        ArgumentNullException.ThrowIfNull(senha);

        // Defesa 2: conta bloqueada recusa SEM nem olhar a senha.
        if (Falhas >= MaxFalhas)
        {
            if (_relogio.GetUtcNow() < _bloqueadoAte) return ResultadoLogin.ContaBloqueada;
            Falhas = 0; // o bloqueio venceu: a contagem recomeça
        }

        // Calcula o hash da senha digitada com o mesmo salt e compara com o guardado.
        // FixedTimeEquals leva sempre o mesmo tempo, para a comparação não "vazar"
        // quantos bytes estavam certos.
        if (CryptographicOperations.FixedTimeEquals(GerarHash(senha), _hashSalvo))
        {
            Falhas = 0;
            return ResultadoLogin.Aceito;
        }

        Falhas++;
        if (Falhas >= MaxFalhas)
            _bloqueadoAte = _relogio.GetUtcNow() + TempoBloqueio; // começa o bloqueio
        return ResultadoLogin.SenhaErrada;
    }

    /// <summary>
    /// Atalho para <see cref="Tentar"/>: <see langword="true"/> só quando a senha foi aceita.
    /// </summary>
    /// <param name="senha">Senha digitada.</param>
    /// <returns><see langword="true"/> se entrou.</returns>
    public bool Entrar(string senha) => Tentar(senha) == ResultadoLogin.Aceito;

    // Hash lento da senha com salt (PBKDF2 com SHA-256, 32 bytes).
    private byte[] GerarHash(string senha) =>
        Rfc2898DeriveBytes.Pbkdf2(senha, _salt, _iteracoes, HashAlgorithmName.SHA256, 32);
}
