#!/bin/bash

while [ $# -gt 0 ]; do
    if [[ $1 == "--"* ]]; then
        if [[ -n $2 && $2 != "-"* ]]; then
            v="${1/--/}"
            declare $v="$2"
        else
            v="${1/--/}"
            declare $v="true"
        fi
    elif [[ $1 == "-"* ]]; then
        if [[ -n $2 && $2 != "-"* ]]; then
            v="${1/-/}"
            declare $v="$2"
        else
            v="${1/-/}"
            declare $v="true"
        fi
    fi

    shift
done

if [[ $help == "true" || $h == "true" ]]; then
    echo 'Following arguments are expected:
    --projectRootDirectory value                --> required
    '
    exit
fi

sudo docker build --tag ghcr.io/hirbod-codes/user_management:latest -f $projectRootDirectory/Dockerfile.production $projectRootDirectory
sleep 1s

sudo docker build --tag ghcr.io/hirbod-codes/user_management_mongodb:latest -f $projectRootDirectory/Dockerfile.mongos.production $projectRootDirectory
sleep 1s

sudo docker build --tag ghcr.io/hirbod-codes/user_management_config_server1:latest -f $projectRootDirectory/Dockerfile.config.production $projectRootDirectory
sleep 1s
sudo docker build --tag ghcr.io/hirbod-codes/user_management_config_server2:latest -f $projectRootDirectory/Dockerfile.config2.production $projectRootDirectory
sleep 1s
sudo docker build --tag ghcr.io/hirbod-codes/user_management_config_server3:latest -f $projectRootDirectory/Dockerfile.config3.production $projectRootDirectory
sleep 1s

sudo docker build --tag ghcr.io/hirbod-codes/user_management_shard_server1:latest -f $projectRootDirectory/Dockerfile.shard.production $projectRootDirectory
sleep 1s
sudo docker build --tag ghcr.io/hirbod-codes/user_management_shard_server2:latest -f $projectRootDirectory/Dockerfile.shard2.production $projectRootDirectory
sleep 1s
sudo docker build --tag ghcr.io/hirbod-codes/user_management_shard_server3:latest -f $projectRootDirectory/Dockerfile.shard3.production $projectRootDirectory
sleep 1s
