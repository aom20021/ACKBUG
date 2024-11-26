using System.Net.Sockets;
using SIPSorcery.SIP;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

class UAC
{
    private static Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

    private SIPTransport _transport;

    public UACInviteTransaction? InviteTransaction;

    public delegate Task OnResponseDelegate(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction transaction, SIPResponse sipRequest);
    public event OnResponseDelegate? onResponse;

    public UAC(SIPTransport transport, SerilogLoggerFactory loggerFactory)
    {
        _transport = transport;
        _logger = loggerFactory.CreateLogger<UAC>();
    }

    public Task RequestHandler(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction transaction, SIPRequest sipRequest)
    {
        _logger.LogDebug($"Received request: {sipRequest.Method}");
        _logger.LogTrace($"{sipRequest}");
        if (sipRequest.Method == SIPMethodsEnum.INVITE)
        {
            _logger.LogDebug($"Setting up outbound request");
            var outboundRequest = SIPRequest.GetRequest(SIPMethodsEnum.INVITE, sipRequest.URI);
            outboundRequest.URI = sipRequest.URI;
            outboundRequest.BodyBuffer = sipRequest.BodyBuffer;

            _logger.LogDebug($"Setting up headers");
            var finalHeaders = sipRequest.Header.Copy();
            finalHeaders.Vias = outboundRequest.Header.Vias;
            finalHeaders.From = outboundRequest.Header.From;
            finalHeaders.To = outboundRequest.Header.To;
            finalHeaders.Contact = new List<SIPContactHeader>() {SIPContactHeader.GetDefaultSIPContactHeader(outboundRequest.URI.Scheme)};
            finalHeaders.Routes = null;
            outboundRequest.Header = finalHeaders;

            _logger.LogDebug($"Initializing transaction");
            InviteTransaction = new UACInviteTransaction(_transport, outboundRequest, null, true);

            _logger.LogDebug($"Sending INVITE request");
            InviteTransaction.SendInviteRequest();

            InviteTransaction.UACInviteTransactionInformationResponseReceived += (loc, rem, tx, resp) =>
            {
                _logger.LogDebug($"Received Information response");
                onResponse!.Invoke(loc, rem, tx, resp);
                return Task.FromResult(SocketError.Success);
            };
            InviteTransaction.UACInviteTransactionFinalResponseReceived += (loc, rem, tx, resp) =>
            {
                _logger.LogDebug($"Received Final response");
                onResponse!.Invoke(loc, rem, tx, resp);
                return Task.FromResult(SocketError.Success);
            };
        }
        else if (sipRequest.Method == SIPMethodsEnum.ACK)
        {
            _logger.LogDebug($"Sending ACK request");
            InviteTransaction!.AckAnswer(InviteTransaction.TransactionFinalResponse, InviteTransaction.TransactionFinalResponse.Body, InviteTransaction.TransactionFinalResponse.Header.ContentType);
        }
        return Task.CompletedTask;
    }
}
