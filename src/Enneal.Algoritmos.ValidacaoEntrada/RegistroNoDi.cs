using Microsoft.Extensions.DependencyInjection;

namespace Enneal.Algoritmos.ValidacaoEntrada;

/// <summary>Registro no container de injeção de dependência do .NET.</summary>
public static class RegistroNoDi
{
    /// <summary>
    /// Registra o contrato <see cref="IValidadorDeEntrada"/> com a implementação padrão (sem estado, então
    /// singleton), o repositório e o serviço que consome o contrato.
    /// </summary>
    public static IServiceCollection AddValidacaoDeEntrada(this IServiceCollection services)
    {
        services.AddSingleton<IValidadorDeEntrada, ValidadorDeEntrada>();
        services.AddScoped<RepositorioDeClientes>();
        services.AddScoped<ServicoDeCadastro>();
        return services;
    }
}
