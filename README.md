[![Build](https://github.com/SmartHomeOS/ZWaveDotNet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/SmartHomeOS/ZWaveDotNet/actions/workflows/dotnet.yml)
[![Version](https://img.shields.io/nuget/v/ZWaveDotNet.svg)](https://www.nuget.org/packages/ZWaveDotNet)
# ZWaveDotNet
An implementation of ZWave Plus using the 2023b public specification. 

### Features:
* Support is included for 60+ command classes. [Full and partial command class support is listed here](SupportedCommandClasses.md).
* Support for security (V0 and V2) and message encapsulation (CRC16, MultiChannel, MultiCommand, Transport and Supervision)
* Support for 8/12/16 bit node IDs (4000+ nodes per controller) including ZWave LR
* Support for extended command classes (16-bit)
* Support for broadcast messaging
* Support for ZWave Plus and Device Type v2
* Node database import/export
* SmartStart inclusion and SmartStart for ZWave long range (LR) devices (including SmartStart by QR Code)

#### Getting Started:
* See our [Examples Page](Examples.md)

#### Work in progress:
* Multicast is not yet exposed
* Security2 multicast is not implemented
* Transport CC is receive only and not fully implemented (Very few devices use this)
* Node interviews are only partially implemented

Testers, Tickets, Feedback and PRs are welcome.