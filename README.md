# Background System Reporter
## Introduction
This is the code for a background app created using Worker Service | C#. It is a framework for building apps that run in the background on Windows. My app sends logs about the system (CPU, memory, and OS), processes, and the network (ports, adapters, and connections).
Unfortunately, the app was developed in a rush, so there’s a bit of “spaghetti code” here.

## About Code
The program contains several classes:
- Program - this is the heart of the entire application, which starts all components and injects services via DI.
- Worker - this class handles logging and processes the information retrieved about the PC. Information about the PC is obtained using the following libraries and frameworks: WMI, System Diagnostic, and System.Net.NetInformation. This is where the code gets a bit confusing.
- INetReporter and IPrcessReporter - these are the interfaces responsible for retrieving the list of logs.
- NetReporter and ProcessReporter - these classes are responsible for writing the generated logs to a Notepad file.

## Information Display:
The information is output via hard-coded paths. These can be found in the Program class under the following variables: *systemLogPath, processLogPath, and networkLogPath*.
In addition, some information is displayed only once. For example, system information and other static data.
Information such as processes (and their properties) and network information is updated every 5 seconds.
