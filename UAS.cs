//Recibir invite
//Enviar 100 trying
//Enviar 200 ok
//Recibir ACK
//Recibir BYE
//Enviar 200 ok

using System.Collections.Concurrent;
using System.Net;
using Org.BouncyCastle.Crypto.Macs;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;

class UAS
{
    SIPTransport transport;

    UASInviteTransaction tx;
    public delegate Task EventHandlerDelegate(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest);
    public event EventHandlerDelegate onMsg;


    public UAS(SIPTransport transport)
    {
        this.transport = transport;
    }

    public void handleErr(SIPEndPoint localSIPEndPoint, SIPEndPoint remotePoint, string message, SIPValidationFieldsEnum errorField, string rawMessage)
    {
        throw new NotImplementedException();
    }

    public async Task ResponseHandler(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction transaction, SIPResponse sipResponse)
    {  
        Console.WriteLine($"Sending response to {remoteEndPoint}"); 
        var response = SIPResponse.GetResponse(tx.TransactionRequest, (SIPResponseStatusCodesEnum) sipResponse.StatusCode, sipResponse.ReasonPhrase);
        if (sipResponse.StatusCode < 200)
        {
            var res = await tx.SendProvisionalResponse(response);
        }
    }

    public async Task RequestHandler(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPRequest sipRequest)
    {
        if (sipRequest.Method == SIPMethodsEnum.INVITE)
        {
            Console.Write(sipRequest);
            tx = new UASInviteTransaction(transport, sipRequest, null);
            var response = SIPResponse.GetResponse(tx.TransactionRequest, SIPResponseStatusCodesEnum.Trying, "Trying");
            var res = await tx.SendProvisionalResponse(response);
            onMsg.Invoke(localSIPEndPoint, remoteEndPoint, sipRequest);
        }
        else
        {
            Console.Write(sipRequest);
            throw new NotImplementedException();
        }
        return;
    }
}
