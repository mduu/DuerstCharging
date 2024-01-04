#!/bin/bash
# This script starts the Charging Manger in Production Mode

cd /opt/duerstcharging
/home/admin/.dotnet/dotnet DuerstCharging.dll --environment Production 