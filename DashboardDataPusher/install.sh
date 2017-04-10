#! /bin/bash

if [ $# -ne 2 ]
then
    echo "usage: install.sh <configuration.json> <docker hub name>"
    exit 1
fi

configuration=$1
dockerHub=$2

if [ ! -f $configuration ]
then
    echo "Cannot find dashboard configuration file: $configuration"
    exit 1
fi

cp $configuration /root/configuration.json
cp dashboard-data-pusher@.service /etc/systemd/system/
systemctl daemon-reload
systemctl enable dashboard-data-pusher@$dockerHub.service
systemctl start dashboard-data-pusher@$dockerHub.service