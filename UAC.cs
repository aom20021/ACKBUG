//Recibir invite
//Enviar 100 trying
//Enviar 200 ok
//Recibir ACK
//Recibir BYE
//Enviar 200 ok

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;
class UAC
{
    SIPTransport transport;

    UACInviteTransaction tx;

    public UAC(SIPTransport transport)
    {
        this.transport = transport;
    }

    public delegate Task OnResponseDelegate(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction transaction, SIPResponse sipRequest);
    public event OnResponseDelegate onResponse;



    public async Task sendMsg(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction transaction, SIPRequest sipRequest)
    {
        if (sipRequest.Method == SIPMethodsEnum.INVITE)
        {
            var newDestination = sipRequest.URI;
            Console.WriteLine($"{newDestination}");
            var outboundRequest = SIPRequest.GetRequest(SIPMethodsEnum.INVITE, newDestination);
            outboundRequest.URI = newDestination;
            outboundRequest.BodyBuffer = sipRequest.BodyBuffer;
            var finalHeaders = sipRequest.Header.Copy();
            finalHeaders.Vias = outboundRequest.Header.Vias;
            finalHeaders.From = outboundRequest.Header.From;
            finalHeaders.To = outboundRequest.Header.To;
            finalHeaders.Contact = new List<SIPContactHeader>() {SIPContactHeader.GetDefaultSIPContactHeader(outboundRequest.URI.Scheme)};
            finalHeaders.Routes = null;
            outboundRequest.Header = finalHeaders;
            tx = new UACInviteTransaction(transport, outboundRequest, null, true);
            Console.WriteLine($"Sending request to {remoteEndPoint}");
            tx.SendInviteRequest();
            tx.UACInviteTransactionInformationResponseReceived += async (loc, rem, tx, resp) =>
            {
                await onResponse.Invoke(loc, rem, tx, resp);
                return SocketError.Success;
            };
            tx.UACInviteTransactionFinalResponseReceived += async (loc, rem, tx, resp) =>
            {
                await onResponse.Invoke(loc, rem, tx, resp);
                return SocketError.Success;
            };
        }
        else if (sipRequest.Method == SIPMethodsEnum.ACK)
        {
            Console.WriteLine("ACK received");
            tx.AckAnswer(tx.TransactionFinalResponse, tx.TransactionFinalResponse.Body, tx.TransactionFinalResponse.Header.ContentType);
        }
    }
}
