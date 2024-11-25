using System.Net;
using SIPSorcery.SIP;

class Program
{
    
    public static void Main()
    {
        SIPTransport transport = new SIPTransport();

        transport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(IPAddress.Any, 5060)));
        transport.AddSIPChannel(new SIPTCPChannel(new IPEndPoint(IPAddress.Any, 5060)));

        var uas = new UAS(transport);
        var uac = new UAC(transport);

        transport.SIPTransportRequestReceived += uas.RequestHandler;
        transport.SIPBadRequestInTraceEvent += uas.handleErr;
        transport.SIPBadResponseInTraceEvent += uas.handleErr;

        uas.onMsg += uac.sendMsg;
        uac.onResponse += uas.ResponseHandler;
        Console.ReadLine();
    }
}
