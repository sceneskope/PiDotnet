# PiDotnet

Various Raspberry PI projects running on dotnet.

All these projects are designed to run in Docker, but could of
course be run standalone. 

## Setup
First of all follow the [dogfooding](https://github.com/dotnet/corefx/blob/master/Documentation/project-docs/dogfooding.md) on the corefx github site. This will get you setup with the SDK and a runtime. 

This project is setup to use SDK version *2.0.0-preview1-005743* with
runtime version *2.0.0-preview1-001915-00*

The base docker images (sceneskope/armv7-dotnet) are built using docker files from [dotnet-core-pi](https://github.com/sceneskope/dotnet-core-pi)

## Projects
* [DashboardDataPusher](DashboardDataPusher/README.md) shows how to push data into a PowerBI dataset.

