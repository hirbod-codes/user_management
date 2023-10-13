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

# Enable job control
set -m

if [[ -z $dbPort && -z $dbPortFile ]]; then
    echo "Insufficient parameters provided."
    exit 1
fi

if [[ -z $replSet ]]; then
    echo "Insufficient parameters provided."
    exit 1
fi

if [[ -z $replSet || -z $member0 || -z $member1 || -z $member2 ]]; then
    echo "Insufficient parameters provided."
    exit 1
fi

if [[ -z $dbPort ]]; then
    dbPort="$(cat $dbPortFile)"
fi

if [[ -z $tlsClusterFile || -z $tlsCertificateKeyFile || -z $tlsCAFile ]]; then
    tlsCertificateKeyFile=/security/app.pem
    tlsCAFile=/security/ca.pem
    tlsClusterFile=/security/member.pem
fi

echo "\n\nWaiting...................................................................................\n\n"
sleep 100s
echo "\n\nWaited...................................................................................\n\n"

mongod --shardsvr --replSet $replSet --bind_ip "0.0.0.0" --port $dbPort --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsClusterFile $tlsClusterFile &

echo "\n\nWaiting...................................................................................\n\n"
sleep 60s
echo "\n\nWaited...................................................................................\n\n"

echo mongo --tls --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsAllowInvalidHostnames --quiet --eval 'rs.status()'
status=$(mongo --tls --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsAllowInvalidHostnames --quiet --eval 'rs.status()')
echo "status------------------------------------->$status"

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

if [[ $? -ne 0 ]]; then exit $?; fi

echo "The replication initialized successfully......................................................................................."

fg
