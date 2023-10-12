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

if [[ (-z $dbAdminUsername || -z $dbPassword || -z $dbUsername || -z $dbName || -z $replSet || -z $member0 || -z $member1 || -z $member2 || -z $dbPort) && (-z $replSet || -z $member0 || -z $member1 || -z $member2 || -z $dbAdminUsernameFile || -z $dbPasswordFile || -z $dbUsernameFile || -z $dbNameFile || -z $dbPortFile) ]]; then
    echo "Insufficient parameters provided."
    exit 1
fi

if [[ -z $dbAdminUsername || -z $dbPassword || -z $dbUsername || -z $dbName || -z $dbPort ]]; then
    dbAdminUsername="$(cat $dbAdminUsernameFile)"
    dbPassword="$(cat $dbPasswordFile)"
    dbUsername="$(cat $dbUsernameFile)"
    dbName="$(cat $dbNameFile)"
    dbPort="$(cat $dbPortFile)"
fi

if [[ -z $tlsCAFile || -z $tlsCertificateKeyFile || -z $tlsClusterFile || -z $tlsClusterCAFile ]]; then
    tlsCertificateKeyFile=/security/app.pem
    tlsClusterFile=/security/member.pem
    tlsClusterCAFile=/security/ca.pem
    tlsCAFile=/security/ca.pem
fi

mongod --replSet $replSet --bind_ip "0.0.0.0" --port $dbPort --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsClusterFile $tlsClusterFile --tlsClusterCAFile $tlsClusterCAFile &

echo "\n\nWaiting...................................................................................\n\n"
sleep 40s
echo "\n\nWaited...................................................................................\n\n"

mongo --tls --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsAllowInvalidHostnames --eval "
rs.status()

rs.initiate(
    { 
        _id: \"$replSet\",
        members: [
            { _id: 0, host: \"$member0:$dbPort\" }, 
            { _id: 1, host: \"$member1:$dbPort\" }, 
            { _id: 2, host: \"$member2:$dbPort\" }
        ] 
    }
)

rs.status()"

echo "\n\nWaiting...................................................................................\n\n"
sleep 60s
echo "\n\nWaited...................................................................................\n\n"

echo "
use admin

db.createUser({ user: '$dbAdminUsername', pwd: '$dbPassword', roles: [{ role: 'root', db: 'admin' }] })

db.auth('$dbAdminUsername', '$dbPassword')

db.getSiblingDB('\$external').runCommand(
    {
        createUser: '$dbUsername',
        roles: [
            { role: 'dbAdmin', db: '$dbName' },
            { role: 'readWrite', db: '$dbName' },
            { role: 'userAdminAnyDatabase', db: 'admin' }
        ],
        writeConcern: { w: 'majority', wtimeout: 5000 }
    }
)
" >/mongo-tmp-init

if [[ -n $localhostUsername ]]; then
    echo "
use admin

db.auth('$dbAdminUsername', '$dbPassword')

db.getSiblingDB('\$external').runCommand(
    {
        createUser: '$localhostUsername',
        roles: [
            { role: 'dbAdmin', db: '$dbName' },
            { role: 'readWrite', db: '$dbName' },
            { role: 'userAdminAnyDatabase', db: 'admin' }
        ],
        writeConcern: { w: 'majority', wtimeout: 5000 }
    }
)
" >>/mongo-tmp-init
fi

echo "use \$external

db.getUsers()
" >>/mongo-tmp-init

cat /mongo-tmp-init && mongo --tls --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsAllowInvalidHostnames </mongo-tmp-init

echo "The replication initialized successfully......................................................................................."

rm /mongo-tmp-init

fg
