using System.Net.Http.Headers;

namespace frontend.Services;

public class AuthenticatedHttpMessageHandler : DelegatingHandler
{
    private readonly SessionService _sessionService;

    public AuthenticatedHttpMessageHandler(SessionService sessionService)
    {
        _sessionService = sessionService;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Agregar el token de autenticación si está disponible
        if (_sessionService.IsLoggedIn && !string.IsNullOrEmpty(_sessionService.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessionService.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
