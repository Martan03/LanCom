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
Send text/file/folder:
```terminal
LanCom send text "This is a example" <IP of computer that's receiving>
LanCom send file example.txt <IP of a computer that's receiving>
LanCom send dir MusicFolder <IP of a computer that's receiving>
```
Save default values:
If ip is set, no ip in send is needed
You can add multiple shortcuts
```terminal
LanCom config ip:<Default IP> dir:<path to save files> add:<Shortcut name>-<IP> remove:<Shortcut name>
```
