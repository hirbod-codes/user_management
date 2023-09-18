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

sudo docker build --tag ghcr.io/hirbod-codes/user_management:latest -f $projectRootDirectory/Dockerfile.development $projectRootDirectory
sleep 1s
