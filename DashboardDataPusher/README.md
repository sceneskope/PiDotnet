# DashboardDataPusher

Example using dotnet core on the PI to push data into a PowerBI dataset.

This uses the PowerBI Nuget package from [powerbi](https://github.com/sceneskope/powerbi). In order to use this, you need a device code for PowerBI as described by [powerbi](https://github.com/sceneskope/powerbi)

This example creates two datasets, one updated every 5 seconds with a snapshot of data, the other updating once a minute showing some overall information.

The project is designed to be built on a windows PC, but could of course be built on a Mac/Linux box. It then runs as a service on a Raspberry PI running Ubuntu 16.04

## Install

### On the Build machine (Windows only currently)
Make sure you have the correct SDK and Runtime installed (see [README.md](../README.md)) Also make sure you have Docker for Windows installed.

    buildPi.ps1 <docker hub name>

That will build the docker file and push to docker hub.

### On the Raspberry PI
Make sure docker is installed and running. 

    sudo apt install docker.io

Get a configuration file for PowerBI as described in [powerbi](https://github.com/sceneskope/powerbi).

Install the service as

    sudo ./install.sh <configuration file name> <docker hub name>

This can all be run from a bash shell on the build machine (bash for windows is great!)

    ./remote_install.sh <pi name> <configuration file name> <docker hub name>





