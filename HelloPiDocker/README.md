# HelloPiDocker

Very simple Hello world for running under docker on a PI.

## Install

### On the Build machine (Windows only currently)
Make sure you have the correct SDK and Runtime installed (see [README.md](../README.md)) Also make sure you have Docker for Windows installed.

    buildPi.ps1 <docker hub name>

That will build the docker file and push to docker hub.

### On the Raspberry PI
Make sure docker is installed and running. 

    sudo apt install docker.io

Run the image

    sudo docker run --rm <docker hub name>/hello-pi-docker

