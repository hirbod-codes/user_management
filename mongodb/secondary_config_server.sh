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

if [[ (-z $dbPort || -z $replSet) && (-z $dbPortFile || -z $replSet) ]]; then
    echo "Insufficient parameters provided."
    exit
fi

if [[ -z $dbPort ]]; then
    dbPort="$(cat $dbPortFile)"
fi

if [[ -z $tlsClusterFile || -z $tlsCertificateKeyFile || -z $tlsCAFile || -z $tlsClusterCAFile ]]; then
    tlsCertificateKeyFile=/security/app.pem
    tlsCAFile=/security/ca.pem
    tlsClusterFile=/security/member.pem
    tlsClusterCAFile=/security/ca.pem
fi

mongod --configsvr --replSet $replSet --bind_ip "0.0.0.0" --port $dbPort --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsClusterFile $tlsClusterFile --tlsCAFile $tlsCAFile --tlsClusterCAFile $tlsClusterCAFile
