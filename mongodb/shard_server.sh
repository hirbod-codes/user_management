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

# Enable job controll
set -m

if [[ (-z $dbPort || -z $replSet || -z $member0 || -z $member1 || -z $member2) && (-z $dbPortFile || -z $replSetFile || -z $member0File || -z $member1File || -z $member2File) ]]; then
    echo "Insufficient parameters provided."
    exit
fi

if [[ -z $dbPort || -z $replSet || -z $member0 || -z $member1 || -z $member2 ]]; then
    dbPort=$(cat $dbPortFile)
    replSet=$(cat $replSetFile)
    member0=$(cat $member0File)
    member1=$(cat $member1File)
    member2=$(cat $member2File)
fi

if [[ -z $tlsClusterFile || -z $tlsCertificateKeyFile || -z $tlsCAFile || -z $tlsClusterCAFile ]]; then
    tlsCertificateKeyFile=/security/app.pem
    tlsCAFile=/security/ca.pem
    tlsClusterFile=/security/member.pem
    tlsClusterCAFile=/security/ca.pem
fi

echo "\n\nWaiting...................................................................................\n\n"
sleep 80s
echo "\n\nWaited...................................................................................\n\n"

mongod --shardsvr --replSet $replSet --bind_ip "0.0.0.0" --port $dbPort --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsClusterFile $tlsClusterFile --tlsClusterCAFile $tlsClusterCAFile &

echo "\n\nWaiting...................................................................................\n\n"
sleep 60s
echo "\n\nWaited...................................................................................\n\n"

echo mongo --tls --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsAllowInvalidHostnames --quiet --eval 'rs.status()'
status=$(mongo --tls --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsAllowInvalidHostnames --quiet --eval 'rs.status()')
echo "status------------------------------------->$status"

if [[ $status != "1" ]]; then
    mongo --tls --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsAllowInvalidHostnames --eval "
        rs.initiate(
            { 
                _id: \"$replSet\", 
                members: [
                    { _id: 0, host: \"$member0:$dbPort\" }, 
                    { _id: 1, host: \"$member1:$dbPort\" }, 
                    { _id: 2, host: \"$member2:$dbPort\" }
                ] 
            }
        );
        rs.status();
    "
    echo "The replication initialized successfully......................................................................................."
else
    echo "The replication already initialized......................................................................................."
fi

fg %1
