# Duerst Charging

## Overview

This project implements a service for scheduling based charging for the KEBA P30 c-series charging station using
Modbus TCP and is written in .Net 8 / C# 12. It will *suspend* the charging station state during the given schedule
and enable it again outside of the prohibited charging times (see section "Configuration*).

You can also use the KEBA smartphone app to control the suspended / enabled state of our charging station manually.
This is useful if you need to charge NOW no matter if the schedule does prohibit it now. Means you can overrule
my application using the KEBA smartphone app. You also use the smartphone app to monitor/verify the state of your
charging station.

> **Note:**
>
>My .Net app does read and change the charging stations *charging state* just as the smartphone app from KEBA
> does. This means that you have use both of them together and they are compatible and work side-by-side.

## Current status

### This software is in production usage at my own ~~~~home.

The production system I use is a Raspberry Pi 3b (ARM 32bit) with latest *Raspberry Pi OS* (Debian-based)

It is tested on my Mac (Dev machine, Arm64) and is running in production on Raspberry Pi 3b (Arm32) but should run on
all other supported .Net platforms (Mac, Linux, Windows - 32/64 bit).

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

To simplify this I added a little bash script called ``StartProd.sh`` which starts the .Net app
with environment set to *Production*. On my Raspberry Pi I registered this as system service like written
[here](https://timleland.com/how-to-run-a-linux-program-on-startup/) and the service configuration I've added to
*Deploy/duerstcharging.service*.

## Logs / Monitoring

The software will write logs to logfiles in ./logs/ (rolling logfile per month) and for convenience to the
standard console. This way you can also check if the software started, stopped and what if did when it runs in the
background as a service.

Another tool is the KEBA smartphone-app where you can see the state of your wallbox or the web-interface available
on the wallbox IP address.

## To-Do's / Ideas

- Add more automated unit-tests where appropriate
- Build and run it as Docker container
- Simplify installation and deployment as a daemon
- Scan for all charging stations in the network so dynamic IP addresses can be supported

## Links:

- [Deploy .NET apps on ARM single-board computers](https://learn.microsoft.com/en-us/dotnet/iot/deployment)
- [How to run a Linux program at startup](https://timleland.com/how-to-run-a-linux-program-on-startup/)
