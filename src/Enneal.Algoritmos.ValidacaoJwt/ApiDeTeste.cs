using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Enneal.Algoritmos.ValidacaoJwt;

/// <summary>
/// A resposta da API para um token.
/// </summary>
/// <param name="Status">O status HTTP (200, 401 ou 403).</param>
/// <param name="Motivo">"ok", "-" ou o <c>error_description</c> do <c>WWW-Authenticate</c> do JwtBearer.</param>
public sealed record RespostaDaApi(int Status, string Motivo);

/// <summary>
/// Uma API ASP.NET Core 8 de teste com <c>GET /admin</c> protegido por JwtBearer + <c>RequireRole("admin")</c>.
/// Sobe só em <c>127.0.0.1</c>, numa porta livre.
/// </summary>
public sealed class ApiDeTeste : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _http;

    private ApiDeTeste(WebApplication app)
    {
        _app = app;
        _http = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
    }

    /// <summary>
    /// Sobe a API com os parâmetros de validação informados.
    /// </summary>
    /// <param name="parametros">Frouxos ou certos (veja <see cref="ValidacaoDeTokens"/>).</param>
    /// <returns>A API rodando em 127.0.0.1.</returns>
    public static async Task<ApiDeTeste> SubirAsync(TokenValidationParameters parametros)
    {
        ArgumentNullException.ThrowIfNull(parametros);

        var b = WebApplication.CreateSlimBuilder();
        b.Logging.ClearProviders();
        b.WebHost.UseUrls("http://127.0.0.1:0");   // só local, porta livre
        var esquema = JwtBearerDefaults.AuthenticationScheme;
        parametros.RoleClaimType = "role";
        b.Services.AddAuthentication(esquema).AddJwtBearer(o =>
        {
            o.MapInboundClaims = false;
            o.TokenValidationParameters = parametros;
        });
        b.Services.AddAuthorization();

        var app = b.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/admin", () => "painel admin")
           .RequireAuthorization(pol => pol.RequireRole("admin"));
        await app.StartAsync();
        return new ApiDeTeste(app);
    }

    /// <summary>
    /// <c>GET /admin</c> com o token no cabeçalho <c>Authorization: Bearer</c>.
    /// </summary>
    /// <param name="token">O token enviado.</param>
    /// <returns>O status e o motivo.</returns>
    public async Task<RespostaDaApi> PedirAsync(string token)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "/admin");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var r = await _http.SendAsync(req);
        string motivo = r.Headers.WwwAuthenticate.Select(h => h.Parameter).FirstOrDefault() ?? "-";
        var m = Regex.Match(motivo, "error_description=\"([^\"]*)\"");
        return new RespostaDaApi((int)r.StatusCode,
            m.Success ? m.Groups[1].Value.Split(';')[0] : r.IsSuccessStatusCode ? "ok" : "-");
    }

    /// <summary>Para a API e libera a porta.</summary>
    public async ValueTask DisposeAsync()
    {
        _http.Dispose();
        await _app.DisposeAsync();
    }
}
