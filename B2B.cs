using System.Net;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;

class B2B
{
    public SIPB2BUserAgent userAgent;
    public SIPTransport transport;

    public B2B()
    {
        transport = new SIPTransport();

        transport.AddSIPChannel(new SIPUDPChannel(new IPEndPoint(IPAddress.Any, 5060)));
        transport.AddSIPChannel(new SIPTCPChannel(new IPEndPoint(IPAddress.Any, 5060)));

        transport.SIPTransportRequestReceived += OnRequest;
    }

    private async Task OnRequest(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
    {
        if (sipRequest.Method == SIPMethodsEnum.INVITE)
        {
            UASInviteTransaction inviteTransaction = new UASInviteTransaction(transport, sipRequest, null);
            userAgent = new SIPB2BUserAgent(transport, null, inviteTransaction, null);
            SIPCallDescriptor callDescriptor = new SIPCallDescriptor(sipRequest.URI.ToString(), sipRequest.Body);
            callDescriptor.CallId = sipRequest.Header.CallId;
            userAgent.Call(callDescriptor);
            
        }
    }

}
