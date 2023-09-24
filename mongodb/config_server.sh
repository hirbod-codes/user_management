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

mongod --configsvr --replSet $replSet --bind_ip "0.0.0.0" --port $dbPort --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile /security/app.pem --tlsCAFile /security/ca.pem --tlsClusterFile /security/member.pem --tlsClusterCAFile /security/ca.pem &

echo "\n\nWaiting...................................................................................\n\n"
sleep 60s
echo "\n\nWaited...................................................................................\n\n"

echo $(mongo --tls --tlsCertificateKeyFile /security/member.pem --tlsCAFile /security/ca.pem --tlsAllowInvalidHostnames --eval 'rs.status()')
status=$(mongo --tls --tlsCertificateKeyFile /security/member.pem --tlsCAFile /security/ca.pem --tlsAllowInvalidHostnames --quiet --eval 'rs.status().ok')
echo "status------------------------------------->$status"

if [[ $status != "1" ]]; then
    mongo --tls --tlsCertificateKeyFile /security/member.pem --tlsCAFile /security/ca.pem --tlsAllowInvalidHostnames --eval "
        rs.initiate(
            { 
                _id: \"$replSet\",
                configsvr: true,
                members: [
                    { _id: 0, host: \"$member0:$dbPort\" }, 
                    { _id: 1, host: \"$member1:$dbPort\" }, 
                    { _id: 2, host: \"$member2:$dbPort\" }
                ] 
            }
        );
    "
    echo $(mongo --tls --tlsCertificateKeyFile /security/member.pem --tlsCAFile /security/ca.pem --tlsAllowInvalidHostnames --eval 'rs.status()')
    echo "The replication initialized successfully......................................................................................."
else
    echo "The replication already initialized......................................................................................."
fi

fg %1
