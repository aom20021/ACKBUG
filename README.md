# MRE-ACK-Ignore-Behavior

## Description

This repo is an example of a bug that happens with the SIPSorcery library when
receiving ACKs that confirm transactions in a B2B scenario, the event of the
`UASInviteTransaction` class called `OnAckReceived` does not work as expected,
it shows 2 behaviors, sometimes it acts correctly, receiving the ACK and
invoking the event, but there are times that this ACK is not received without
changes on the code or the setup of the User Agents.

### Usage

Project is run with:

```bash
dotnet run
```

For testing we used 2 instances of [pjsua](https://www.pjsip.org/pjsua.htm),
initialized with the following options:

```bash
pjsua --no-tcp --local-port <caller_port> --outbound sip:<b2b_ip>

pjsua --no-tcp --local-port <callee_port>
```

Being the outbound parameter of the caller instance the ip of the running
program.

## Examples

### Output when the event is triggered

![image](https://github.com/user-attachments/assets/82d2f430-f9b6-49e2-bdb0-650a80c976b1)

### Output when the event is not triggered

![image](https://github.com/user-attachments/assets/849bc1ad-73a4-43bf-86b8-e94a9a469ecc)

### Log Example

```log
[11:48:05 DBG] Program: Setting up transport
[11:48:05 DBG] sipsorcery: CreateBoundSocket attempting to create and bind socket(s) on 0.0.0.0:5060 using protocol Udp.
[11:48:05 DBG] sipsorcery: CreateBoundSocket successfully bound on 0.0.0.0:5060.
[11:48:05 INF] sipsorcery: SIP UDP Channel created for udp:0.0.0.0:5060.
[11:48:05 INF] sipsorcery: SIP TCP Channel created for tcp:0.0.0.0:5060.
[11:48:05 DBG] Program: Binding handlers
[11:48:05 DBG] sipsorcery: SIP TCP Channel socket on tcp:0.0.0.0:5060 accept connections thread started.
[11:48:13 DBG] UAS: Received request: INVITE
[11:48:13 DBG] UAS: Initializing transaction
[11:48:13 DBG] UAS: Sending automatic Trying response
[11:48:13 DBG] UAC: Received request: INVITE
[11:48:13 DBG] UAC: Setting up outbound request
[11:48:13 DBG] UAC: Setting up headers
[11:48:13 DBG] UAC: Initializing transaction
[11:48:13 DBG] UAC: Sending INVITE request
[11:48:13 DBG] UAC: Received Information response
[11:48:13 DBG] UAS: Received response: 100 Trying
[11:48:13 DBG] UAS: Ignoring Trying response
[11:48:15 DBG] UAC: Received Final response
[11:48:15 DBG] UAS: Received response: 200 OK
[11:48:15 DBG] UAS: Sending Final OK response
[11:48:15 ERR] Program: Bad request in udp:0.0.0.0:5060 from udp:192.168.1.160:5050, ACK received on Confirmed transaction, ignoring., field: Request
[11:48:15 DBG] Program:
[11:48:16 WRN] sipsorcery: An ACK retransmit was required but there was no stored ACK request to send.
[11:48:16 ERR] Program: Bad request in udp:0.0.0.0:5060 from udp:192.168.1.160:5050, ACK received on Confirmed transaction, ignoring., field: Request
[11:48:16 DBG] Program:
[11:48:17 WRN] sipsorcery: An ACK retransmit was required but there was no stored ACK request to send.
[11:48:17 ERR] Program: Bad request in udp:0.0.0.0:5060 from udp:192.168.1.160:5050, ACK received on Confirmed transaction, ignoring., field: Request
[11:48:17 DBG] Program:
[11:48:19 WRN] sipsorcery: An ACK retransmit was required but there was no stored ACK request to send.
[11:48:19 ERR] Program: Bad request in udp:0.0.0.0:5060 from udp:192.168.1.160:5050, ACK received on Confirmed transaction, ignoring., field: Request
[11:48:19 DBG] Program:
[11:48:22 DBG] UAS: Received request: BYE
[11:48:22 DBG] UAS: Received request: BYE
[11:48:22 DBG] UAS: Received request: BYE
```
