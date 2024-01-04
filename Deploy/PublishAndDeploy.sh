#!/bin/bash
# Script that build and publish the release. Finally the copy job to copy the files over to
# the Raspberry Pi is initiated.

dotnet publish ..

scp -r ../DuerstCharging/bin/Release/net8.0/publish/* admin@192.168.1.13:/opt/duerstcharging