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

if [[ -z $dbPort && -z $dbPortFile ]]; then
    echo "Insufficient parameters provided."
    exit
fi

if [[ -z $dbPort ]]; then
    dbPort=$(cat $dbPortFile)
fi

if [[ -z $tlsClusterFile || -z $tlsCertificateKeyFile || -z $tlsCAFile || -z $tlsClusterCAFile ]]; then
    tlsCertificateKeyFile=/security/app.pem
    tlsCAFile=/security/ca.pem
    tlsClusterFile=/security/member.pem
    tlsClusterCAFile=/security/ca.pem
fi

echo $tlsCertificateKeyFile
echo $tlsCAFile
echo $tlsClusterFile
echo $tlsClusterCAFile

mongod --shardsvr --replSet user_management_shardReplicaSet --port $dbPort --bind_ip "0.0.0.0" --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsClusterFile $tlsClusterFile --tlsCAFile $tlsCAFile --tlsClusterCAFile $tlsClusterCAFile
