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

if [[ (-z $replSet || -z $dbPort) && (-z $replSetFile || -z $dbPortFile) ]]; then
    echo "Insufficient parameters provided."
    exit 1
fi

if [[ -z $dbPort || -z $replSet ]]; then
    dbPort=$(cat $dbPortFile)
    replSet=$(cat $replSetFile)
fi

if [[ -z $tlsCertificateKeyFile || -z $tlsCAFile ]]; then
    tlsCertificateKeyFile=/security/app.pem
    tlsCAFile=/security/ca.pem
fi

mongod --replSet $replSet --bind_ip "0.0.0.0" --port $dbPort --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile
