using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;

namespace Common.Logging
{
    public class LoggingDelegatingHanddler : DelegatingHandler
    {

        private readonly ILogger<LoggingDelegatingHanddler> _logger;

        public LoggingDelegatingHanddler(ILogger<LoggingDelegatingHanddler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(requestMessage, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Succesufull response from {url}", response.RequestMessage.RequestUri);
                }
                else
                {
                    _logger.LogWarning("Received a non-success statuscode {StatusCode} from {Url}", (int)response.StatusCode, response.RequestMessage.RequestUri);
                }


                return response;
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionRefused)
            {
                var hostWithPort = requestMessage.RequestUri.IsDefaultPort ? requestMessage.RequestUri.DnsSafeHost : $"{requestMessage.RequestUri.DnsSafeHost}:{requestMessage.RequestUri.Port}";

                _logger.LogCritical(ex, "Unable to connect to {Host}. Please check the configuration to ensurance the correct URL", hostWithPort);
            }

            return new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadGateway,
                RequestMessage = requestMessage
            };

        }

    }
}