# Duerst Charging

## Overview

This project implements a service for scheduling based charging for the KEBA P30 c-series charging station using
Modbus TCP and is written in .Net 8 / C# 12. It will *suspend* the charging station state during the given schedule
and enable it again outside of the prohibited charging times (see section "Configuration*).

It is tested on my Mac (Dev machine, Arm64) and is running in production on Raspberry Pi 3b (Arm32) but should run on
all other supported .Net platforms (Mac, Linux, Windows).

## Current status

**This software is in production usage at my home.**

The production system I use is a Raspberry Pi 3b with

## Dependencies

- .Net 8 / C# 12
- FluentModbus 5
- SeriLog 5

## Configuration

See the appsettings.json files for more configuration options.

``ChargingStationIpAddress`` (string)
The IP address of your charging station. Currently only fix IP addresses are supported.

``SimulationOnly`` (true, false)
If *true* the software does not write to the charging station but only read. It will write a log-entry if it would
change the state of the charging-station. This is good for testing. In production you like to have this setting to
*false*.

``ChargingProhibited`` (list of *ScheduleEntry*)
One or many entries which defines weekdays and times when charging is prohibited. For example you
monday to friday 7-20 and saturday morning as non-charging-times. This result in charging
during night-times, saturday afternoon and entire sunday.

```
"ChargingProhibited": [
      {
        "Weekday": "Monday",
        "StartTime": "07:00",
        "EndTime": "20:00"
      },
      {
        "Weekday": "Tuesday",
        "StartTime": "07:00",
        "EndTime": "20:00"
      },
      {
        "Weekday": "Wednesday",
        "StartTime": "07:00",
        "EndTime": "20:00"
      },
      {
        "Weekday": "Thursday",
        "StartTime": "07:00",
        "EndTime": "20:00"
      },
      {
        "Weekday": "Friday",
        "StartTime": "07:00",
        "EndTime": "20:00"
      },
      {
        "Weekday": "Saturday",
        "StartTime": "07:00",
        "EndTime": "13:00"
      }
    ]
```

## Build & Deployment

There is a handy bash script ``PublishAndDeploy.sh`` which you can use to build (publish)
and copy the binaries to your target machine (in my case the Paspberry Pi).

To build the binaries manually run the commend ``dotnet publish``. You will find the binaries
in *DuerstCharging/bin/Release/net8.0/publish*.

I'd recommend run the app as service on the system startup. How this is done depends on the operating sysstem
you use. See section *Links* on how to do it for example on Linux systems like Raspberry Pi's
*Paspberry Pi OS*.

## To-Do's / Ideas

- Add more automated unit-tests where appropriate
- Build and run it as Docker container
- Simplify installation and deployment as a daemon
- Scan for all charging stations in the network so dynamic IP addresses can be supported

## Links:

- [Deploy .NET apps on ARM single-board computers](https://learn.microsoft.com/en-us/dotnet/iot/deployment)
- [How to run a Linux program at startup](https://timleland.com/how-to-run-a-linux-program-on-startup/)
