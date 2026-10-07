namespace Enneal.Algoritmos.ForcaBruta;

/// <summary>
/// Resultado do laço de força bruta contra o <see cref="LoginProtegido"/>.
/// </summary>
/// <param name="Aberto">Se alguma tentativa foi aceita.</param>
/// <param name="Pedidos">Quantos pedidos de login foram feitos (o último pode ter encontrado a conta bloqueada).</param>
/// <param name="Falhas">Falhas contadas pelo login até parar.</param>
/// <param name="Bloqueado">Se o laço parou porque a conta foi bloqueada.</param>
public sealed record ResultadoComBloqueio(bool Aberto, int Pedidos, int Falhas, bool Bloqueado);

/// <summary>
/// Quanto o bloqueio atrasa quem tenta adivinhar.
/// </summary>
/// <param name="Bloqueios">Bloqueios completos até chegar na senha.</param>
/// <param name="Dias">Tempo total de espera, em dias.</param>
public sealed record CustoDoBloqueio(int Bloqueios, double Dias);

/// <summary>
/// Simulação DIDÁTICA de força bruta contra um PIN de 4 dígitos que está na memória
/// deste próprio processo. Serve para mostrar por que as defesas funcionam:
/// não acessa rede, arquivos, hashes de terceiros nem senhas reais.
/// </summary>
public static class SimulacaoForcaBruta
{
    /// <summary>Quantos PINs de 4 dígitos existem (0000 a 9999).</summary>
    public const int TotalDePins = 10_000;

    /// <summary>
    /// Round 1, SEM proteção: testa 0000, 0001, 0002... comparando direto com o PIN.
    /// </summary>
    /// <param name="pinDaDemo">O PIN "secreto" da demonstração (4 dígitos).</param>
    /// <returns>Em qual tentativa o PIN foi aberto (o Reel usa 7391: 7392 tentativas).</returns>
    /// <exception cref="ArgumentException">Se o PIN não tiver exatamente 4 dígitos.</exception>
    public static int TentativasSemProtecao(string pinDaDemo)
    {
        ValidarPin(pinDaDemo);

        int tentativas = 0;
        for (int n = 0; n < TotalDePins; n++)
        {
            tentativas++;
            string palpite = n.ToString("D4"); // "D4" completa com zeros: 42 -> "0042"
            if (palpite == pinDaDemo) break;
        }
        return tentativas;
    }

    /// <summary>
    /// Round 2: o MESMO laço, agora contra um <see cref="LoginProtegido"/>.
    /// Para assim que a conta é bloqueada (ou se, por sorte, acertar antes).
    /// </summary>
    /// <param name="login">Login de demonstração, criado neste processo.</param>
    /// <returns>Pedidos feitos, falhas contadas e se terminou bloqueado.</returns>
    public static ResultadoComBloqueio TentarContraLogin(LoginProtegido login)
    {
        ArgumentNullException.ThrowIfNull(login);

        bool bloqueado = false;
        int pedidos = 0;
        for (int n = 0; n < TotalDePins && !bloqueado; n++)
        {
            pedidos++;
            var resultado = login.Tentar(n.ToString("D4"));
            if (resultado == ResultadoLogin.Aceito)
                return new ResultadoComBloqueio(true, pedidos, login.Falhas, false);
            if (resultado == ResultadoLogin.ContaBloqueada)
                bloqueado = true;
        }
        return new ResultadoComBloqueio(false, pedidos, login.Falhas, bloqueado);
    }

    /// <summary>
    /// Quanto tempo de espera o bloqueio impõe para chegar na tentativa
    /// <paramref name="tentativas"/>: a cada <paramref name="maxFalhas"/> erros,
    /// <paramref name="minutosBloqueio"/> minutos parados.
    /// </summary>
    /// <param name="tentativas">Em qual tentativa a senha seria acertada.</param>
    /// <param name="maxFalhas">Erros permitidos antes de cada bloqueio.</param>
    /// <param name="minutosBloqueio">Minutos de cada bloqueio.</param>
    /// <returns>Bloqueios completos e dias de espera (7392 tentativas: 1478 bloqueios, ~15,4 dias).</returns>
    public static CustoDoBloqueio CalcularCustoDoBloqueio(int tentativas, int maxFalhas, int minutosBloqueio)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(tentativas, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxFalhas, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(minutosBloqueio);

        // Para acertar na tentativa T, erra-se T - 1 vezes: (T - 1) / maxFalhas bloqueios completos.
        int bloqueios = (tentativas - 1) / maxFalhas;
        double dias = bloqueios * minutosBloqueio / 60.0 / 24.0;
        return new CustoDoBloqueio(bloqueios, dias);
    }

    private static void ValidarPin(string pin)
    {
        ArgumentNullException.ThrowIfNull(pin);
        if (pin.Length != 4 || !pin.All(char.IsAsciiDigit))
            throw new ArgumentException("A demonstração usa só PINs de exatamente 4 dígitos.", nameof(pin));
    }
}
