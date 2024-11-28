using System.Net;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

class B2B
{
    private static Microsoft.Extensions.Logging.ILogger _logger = NullLogger.Instance;

    public SIPB2BUserAgent userAgent;
    public SIPTransport transport;

    public B2B()
    {
        SerilogLoggerFactory factory = AddConsoleLogger(LogEventLevel.Debug);

        SIPSorcery.LogFactory.Set(factory);
        _logger = factory.CreateLogger<Program>();

        _logger.LogDebug("Setting up transport");
        transport = new SIPTransport();

        transport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(IPAddress.Any, 5060)));
        transport.AddSIPChannel(new SIPTCPChannel(new IPEndPoint(IPAddress.Any, 5060)));

        _logger.LogDebug("Binding handlers");
        transport.SIPTransportRequestReceived += OnRequest;
        transport.SIPBadRequestInTraceEvent += BadTrace;
        transport.SIPBadResponseInTraceEvent += BadTrace;
    }

    private async Task OnRequest(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
    {
        _logger.LogDebug($"Received request: {sipRequest.Method}");
        _logger.LogTrace($"{sipRequest}");
        if (sipRequest.Method == SIPMethodsEnum.INVITE)
        {
            _logger.LogDebug($"Initializing transaction");
            UASInviteTransaction inviteTransaction = new UASInviteTransaction(transport, sipRequest, null);
            _logger.LogDebug($"Initializing B2B user agent");
            userAgent = new SIPB2BUserAgent(transport, null, inviteTransaction, null);
            SIPCallDescriptor callDescriptor = new SIPCallDescriptor(sipRequest.URI.ToString(), sipRequest.Body);
            callDescriptor.CallId = sipRequest.Header.CallId;
            _logger.LogDebug($"Calling...");
            userAgent.Call(callDescriptor);
        }
    }

    private static void BadTrace(SIPEndPoint localSIPEndPoint, SIPEndPoint remotePoint, string message, SIPValidationFieldsEnum errorField, string rawMessage)
    {
        _logger.LogError($"Bad request in {localSIPEndPoint} from {remotePoint}, {message}, field: {errorField}");
        _logger.LogDebug($"{rawMessage}");
    }

    private static SerilogLoggerFactory AddConsoleLogger(LogEventLevel logLevel = LogEventLevel.Debug)
    {
        var serilogLogger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Is(logLevel)
            .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        return new SerilogLoggerFactory(serilogLogger);
    }
}
