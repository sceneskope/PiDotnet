#! /bin/bash


if [ $# -ne 3 ]
then
    echo "usage: remote_install.sh <pi name> <configuration file> <docker hub>"
    exit 1
fi

piName=$1
configurationFile=$2
dockerHub=$3

scp dashboard-data-pusher@.service install.sh $configurationFile ${piName}:

ssh -t $piName sudo ./install.sh `basename $configurationFile` $dockerHub
