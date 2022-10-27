# LanCom
Command line utility for sharing text, files or folder over LAN.

## Usage
Show help:
```terminal
LanCom
```
Start receiving:
```terminal
LanCom receive
```
Creates server that will listen for connections.<br />
Server is automatically turned off after receiving all data from a client.<br />
Send text/file/folder:
```terminal
LanCom send <arg> <ip>
```
arg - path to file/folder or text<br />
ip - server IP

Save default values:
If ip is set, no ip in send is needed
You can add multiple shortcuts
```terminal
LanCom config ip:<Default IP> dir:<path to save files> add:<Shortcut name>-<IP> remove:<Shortcut name>
```
