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

if [[ (-z $dbUsername || -z $dbAdminUsername || -z $dbPassword || -z $dbName || -z $dbPort) && (-z $dbUsernameFile || -z $dbAdminUsernameFile || -z $dbPasswordFile || -z $dbNameFile || -z $dbPortFile) ]]; then
    echo "Insufficient parameters provided."
    exit
fi

if [[ -z $configReplSet || -z $configMember0 || -z $configMember1 || -z $configMember2 ]]; then
    echo "Insufficient parameters provided."
    exit
fi

if [[ -z $dbUsername && -z $dbAdminUsername && -z $dbPassword && -z $dbName && -z $dbPort ]]; then
    dbUsername="$(cat $dbUsernameFile)"
    dbAdminUsername="$(cat $dbAdminUsernameFile)"
    dbPassword="$(cat $dbPasswordFile)"
    dbName="$(cat $dbNameFile)"
    dbPort="$(cat $dbPortFile)"
fi

if [[ -z $tlsClusterFile || -z $tlsCertificateKeyFile || -z $tlsCAFile ]]; then
    tlsClusterFile=/security/member.pem
    tlsCertificateKeyFile=/security/app.pem
    tlsCAFile=/security/ca.pem
fi

echo "\n\nWaiting...................................................................................\n\n"
sleep 180s
echo "\n\nWaited...................................................................................\n\n"

mongos --bind_ip "0.0.0.0" --port $dbPort --configdb "$configReplSet/$configMember0:$dbPort,$configMember1:$dbPort,$configMember2:$dbPort" --tlsMode requireTLS --clusterAuthMode x509 --tlsClusterFile $tlsClusterFile --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile &

echo "\n\nWaiting...................................................................................\n\n"
sleep 60s
echo "\n\nWaited...................................................................................\n\n"

echo "
use admin

db.createUser({ user: \"$dbAdminUsername\", pwd: \"$dbPassword\", roles: [{ role: \"root\", db: \"admin\" }] })

db.auth(\"$dbAdminUsername\", \"$dbPassword\")

sh.addShard(\"$shardReplSet/$shardMember0:$dbPort,$shardMember1:$dbPort,$shardMember2:$dbPort\")

db.getSiblingDB(\"\$external\").runCommand(
    {
        createUser: \"$dbUsername\",
        roles: [
            { role: \"dbAdmin\", db: \"$dbName\" },
            { role: \"readWrite\", db: \"$dbName\" },
            { role: \"userAdminAnyDatabase\", db: \"admin\" }
        ],
        writeConcern: { w: \"majority\", wtimeout: 5000 }
    }
)

" >/mongo-tmp-init

if [[ -n $localhostUsername ]]; then
    echo "
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

mongo --tls --tlsCertificateKeyFile $tlsCertificateKeyFile --tlsCAFile $tlsCAFile --tlsAllowInvalidHostnames </mongo-tmp-init

echo "The mongos instance initialized successfully......................................................................................."

rm /mongo-tmp-init

fg
