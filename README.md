[![Build](https://github.com/jdomnitz/ZWaveDotNet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jdomnitz/ZWaveDotNet/actions/workflows/dotnet.yml)
# ZWaveDotNet
An implementation of ZWave Plus using the 2022b public specification. 

Features:
* Partial support is included for 122 command classes.  Detailed [command class support is listed here](SupportedCommandClasses.md).
* Support for Security (V0 and V2) and message encapsulation (CRC16, Transport, MultiChannel, MultiCommand and Supervision)
* Support for 8/12/16 bit Node IDs (4000+ nodes) including ZWave LR
* Support for 16-bit Command Classes

Current Status:
* Not yet ready for use - follow for our first release. 
* Security2 is working in Unauth-mode singlecast only.
* ZWave LR is not yet exposed.
* Transport CC is unimplemented.

Testers, Tickets, Feedback and PRs are welcome.