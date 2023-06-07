[![Build](https://github.com/jdomnitz/ZWaveDotNet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jdomnitz/ZWaveDotNet/actions/workflows/dotnet.yml)
# ZWaveDotNet
An implementation of ZWave Plus using the 2022b public specification. 

**Not yet ready for use - star/follow for our first release.** 
##### Features:
* Partial support is included for 121 command classes. Detailed [command class support is listed here](SupportedCommandClasses.md).
* Support for Security (V0 and V2) and message encapsulation (CRC16, Transport, MultiChannel, MultiCommand and Supervision)
* Support for 8/12/16 bit Node IDs (4000+ nodes) including ZWave LR
* Support for 16-bit Command Classes
* Support for broadcast messaging
* Support for ZWave Plus
* Node database import/export

##### Work in progress:
* Security2 is working in Unauth-mode singlecast only
* Transport CC is unimplemented
* Multicast is not yet exposed
* ZWave LR inclusion is not yet exposed
* Node interviews are only partially supported

Testers, Tickets, Feedback and PRs are welcome.