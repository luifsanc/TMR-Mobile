using Grpc.Net.Client;
using tmr_shared.Protos;

namespace tmr_mobile.Services;

public class GrpcAuthService
{
    private readonly GrpcChannel _channel;
    private readonly tmr_shared.Protos.AuthService.AuthServiceClient _client;

    public GrpcAuthService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://dev.api.tmr2.dokploy.integritysolutions.com.ec")
        };

        _channel = GrpcChannel.ForAddress("https://dev.api.tmr2.dokploy.integritysolutions.com.ec", new GrpcChannelOptions
        {
            HttpClient = httpClient
        });
        _client = new tmr_shared.Protos.AuthService.AuthServiceClient(_channel);
    }

    public async Task<AuthResponse> LoginAsync(string user, string password)
    {
        var request = new LoginRequest { User = user, Password = password };
        return await _client.LoginAsync(request);
    }
}
