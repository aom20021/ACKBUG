using System.Net;
using SIPSorcery.SIP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

class Program
{
    private static Microsoft.Extensions.Logging.ILogger _logger = NullLogger.Instance;

    public static void Main()
    {
        SerilogLoggerFactory factory = AddConsoleLogger(LogEventLevel.Debug);

        SIPSorcery.LogFactory.Set(factory);
        _logger = factory.CreateLogger<Program>();

        _logger.LogDebug("Setting up transport");

        SIPTransport transport = new SIPTransport();

        transport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(IPAddress.Any, 5060)));
        transport.AddSIPChannel(new SIPTCPChannel(new IPEndPoint(IPAddress.Any, 5060)));

        var uas = new UAS(transport, factory);
        var uac = new UAC(transport, factory);

        _logger.LogDebug("Binding handlers");

        transport.SIPTransportRequestReceived += uas.RequestHandler;
        transport.SIPBadRequestInTraceEvent += BadTrace;
        transport.SIPBadResponseInTraceEvent += BadTrace;

        uas.onRequest += uac.RequestHandler;
        uac.onResponse += uas.ResponseHandler;

        Console.ReadLine();
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

    private static void BadTrace(SIPEndPoint localSIPEndPoint, SIPEndPoint remotePoint, string message, SIPValidationFieldsEnum errorField, string rawMessage)
    {
        _logger.LogError($"Bad request in {localSIPEndPoint} from {remotePoint}, {message}, field: {errorField}");
        _logger.LogDebug($"{rawMessage}");
    }
}
