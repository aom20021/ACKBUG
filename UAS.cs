using System.Net.Sockets;
using SIPSorcery.SIP;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

class UAS
{
    private static Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

    private SIPTransport _transport;

    public UASInviteTransaction? InviteTransaction;

    public delegate Task OnRequestDelegate(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction transaction, SIPRequest sipRequest);
    public event OnRequestDelegate? onRequest;

    public UAS(SIPTransport transport, SerilogLoggerFactory loggerFactory)
    {
        _transport = transport;
        _logger = loggerFactory.CreateLogger<UAS>();
    }

    public async Task RequestHandler(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
    {
        _logger.LogDebug($"Received request: {sipRequest.Method}");
        _logger.LogTrace($"{sipRequest}");
        if (sipRequest.Method == SIPMethodsEnum.INVITE)
        {
            _logger.LogDebug($"Initializing transaction");
            InviteTransaction = new UASInviteTransaction(_transport, sipRequest, null);

            _logger.LogDebug($"Sending automatic Trying response");
            var response = SIPResponse.GetResponse(InviteTransaction.TransactionRequest, SIPResponseStatusCodesEnum.Trying, "Trying");
            var res = await InviteTransaction.SendProvisionalResponse(response);

            InviteTransaction.OnAckReceived += (loc, rem, tx, req) => {
                _logger.LogDebug($"Received ACK");
                onRequest!.Invoke(loc, rem, tx, req);
                return Task.FromResult(SocketError.Success);
            };

            await onRequest!.Invoke(localSIPEndPoint, remoteEndPoint, InviteTransaction, sipRequest);
        }
    }

    public async Task ResponseHandler(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction transaction, SIPResponse sipResponse)
    {  
        _logger.LogDebug($"Received response: {sipResponse.StatusCode} {sipResponse.ReasonPhrase}");
        _logger.LogTrace($"{sipResponse}");
        var response = SIPResponse.GetResponse(InviteTransaction?.TransactionRequest, (SIPResponseStatusCodesEnum) sipResponse.StatusCode, sipResponse.ReasonPhrase);
        if (sipResponse.StatusCode == 100)
        {
            _logger.LogDebug($"Ignoring Trying response");
            return;
        }
        else if (sipResponse.StatusCode < 200)
        {
            _logger.LogDebug($"Sending provisional response");
            _logger.LogTrace($"{response}");
            var res = await InviteTransaction!.SendProvisionalResponse(response);
        }
        else if (sipResponse.StatusCode < 300)
        {
            response = InviteTransaction!.GetOkResponse(sipResponse.Header.ContentType, System.Text.Encoding.UTF8.GetString(sipResponse.BodyBuffer ?? Array.Empty<byte>()));
            response.StatusCode = sipResponse.StatusCode;

            _logger.LogDebug($"Sending Final OK response");
            _logger.LogTrace($"{response}");
            InviteTransaction.SendFinalResponse(response);
        }
    }
}
