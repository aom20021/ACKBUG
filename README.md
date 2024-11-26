## Description

This repo is an example of a bug that happens with the SIPSorcery library when receiving ACKs that confirm transactions in a B2B scenario, the event of the `UASInviteTransaction` class called `OnAckReceived` does not work as expected, it shows 2 behaviors, sometimes it acts correctly, receiving the ACK and invoking the event, but there are times that this ACK is not received without changes on the code or the setup of the User Agents.

### Usage

For testing we used 2 instances of [pjsua](https://www.pjsip.org/pjsua.htm), initialized with the following options
```
pjsua --no-tcp --local-port 5061 --null-audio

pjsua --no-tcp --local-port 5062 --null-audio --outbound sip:192.168.2.216
```

Being the outbound parameter of the second instance (the one that is going to call) the ip of the running program

### Output when it works

![image](https://github.com/user-attachments/assets/82d2f430-f9b6-49e2-bdb0-650a80c976b1)

### Output when the event is not triggered

![image](https://github.com/user-attachments/assets/849bc1ad-73a4-43bf-86b8-e94a9a469ecc)
