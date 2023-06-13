[![Build](https://github.com/SmartHomeOS/ZWaveDotNet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/SmartHomeOS/ZWaveDotNet/actions/workflows/dotnet.yml)
# ZWaveDotNet
An implementation of ZWave Plus using the 2022b public specification. 

**Not yet ready for use - star/follow for our first release.** 
#### Features:
* Partial support is included for 121 command classes. [Full and partial command class support is listed here](SupportedCommandClasses.md).
* Support for Security (V0 and V2) and message encapsulation (CRC16, MultiChannel, MultiCommand, Transport and Supervision)
* Support for 8/12/16 bit Node IDs (4000+ nodes) including ZWave LR
* Support for 16-bit Command Classes
* Support for broadcast messaging
* Support for ZWave Plus and DeviceTypev2
* Node database import/export
* SmartStart and SmartStart for ZWave LR Devices

#### Work in progress:
* Multicast is not yet exposed
* Security2 multicast is not implemented
* Transport CC is receive only and not fully implemented
* Node interviews are only partially implemented

Testers, Tickets, Feedback and PRs are welcome.