[![Build](https://github.com/SmartHomeOS/ZWaveDotNet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/SmartHomeOS/ZWaveDotNet/actions/workflows/dotnet.yml)
[![Version](https://img.shields.io/nuget/v/ZWaveDotNet.svg)](https://www.nuget.org/packages/ZWaveDotNet)
# ZWaveDotNet
An implementation of ZWave Plus using the 2024a public specification. 

### Features:
* Support is included for 70+ command classes. [Full and partial command class support is listed here](SupportedCommandClasses.md).
* Support for security (V0 and V2) and message encapsulation (CRC16, MultiChannel, MultiCommand, Transport and Supervision)
* Support for 8/12/16 bit node IDs (4000+ nodes per controller) including ZWave LR
* Support for extended command classes (16-bit)
* Support for broadcast messaging
* Support for ZWave Plus and Device Type v2
* Node database import/export
* SmartStart inclusion and SmartStart for ZWave Long Range (LR) devices (including SmartStart by QR Code)

#### Getting Started:
* See our [Examples Page](Examples.md)

#### Not Supported:
* Secure Multicast is not fully implemented

#### Other Projects
* Check out my other projects for [HomeKit](https://github.com/SmartHomeOS/HomeKitDotNet) and [Matter](https://github.com/SmartHomeOS/MatterDotNet).

Support is always appreciated:<br/><a href="https://www.buymeacoffee.com/jdomnitz" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-red.png" alt="Buy Me A Pizza" style="height: 60px !important;width: 217px !important;" ></a>

Testers, Tickets, Feedback and PRs are welcome.