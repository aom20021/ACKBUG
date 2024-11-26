//Recibir invite
//Enviar 100 trying
//Enviar 200 ok
//Recibir ACK
//Recibir BYE
//Enviar 200 ok

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Org.BouncyCastle.Crypto.Macs;
using SIPSorcery.SIP;
using SIPSorcery.SIP.App;

class UAS
{
    SIPTransport transport;

    UASInviteTransaction tx;
    public delegate Task EventHandlerDelegate(SIPEndPoint localSIPEndPoint, SIPEndPoint remoteEndPoint, SIPTransaction transaction, SIPRequest sipRequest);
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
        if (sipResponse.StatusCode == 100)
        {
            return;
        }
        else if (sipResponse.StatusCode < 200)
        {
            var res = await tx.SendProvisionalResponse(response);
        }
        else if (sipResponse.StatusCode < 300)
        {
            response = tx.GetOkResponse(sipResponse.Header.ContentType, System.Text.Encoding.UTF8.GetString(sipResponse.BodyBuffer ?? Array.Empty<byte>()));
            response.StatusCode = sipResponse.StatusCode;
            tx.SendFinalResponse(response);
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
            onMsg.Invoke(localSIPEndPoint, remoteEndPoint, tx, sipRequest);
            tx.OnAckReceived += async (loc, rem, tx, req) => {
                onMsg.Invoke(loc, rem, tx, req);
                return SocketError.Success;
            };
        }
        else
        {
            Console.Write(sipRequest);
            throw new NotImplementedException();
        }
        return;
    }
}
