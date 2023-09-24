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

if [[ (-z $dbAdminUsernameFile || -z $dbPasswordFile || -z $dbNameFile) && (-z $dbAdminUsername || -z $dbPassword || -z $dbName) ]]; then
    echo "Insufficient parameters provided."
    exit
fi

if [[ -z $dbAdminUsername && -z $dbPassword && -z $dbName ]]; then
    dbAdminUsername=$dbAdminUsernameFile
    dbPassword=$dbPasswordFile
    dbName=$dbNameFile
fi

echo "\n\nWaiting...................................................................................\n\n"
sleep 160s
echo "\n\nWaited...................................................................................\n\n"

mongos --bind_ip "0.0.0.0" --port $dbPort --configdb "$configReplSet/$configMember0:$dbPort,$configMember1:$dbPort,$configMember2:$dbPort" --tlsMode requireTLS --clusterAuthMode x509 --tlsClusterFile /security/member.pem --tlsCertificateKeyFile /security/app.pem --tlsCAFile /security/ca.pem --tlsClusterCAFile /security/ca.pem &

echo "\n\nWaiting...................................................................................\n\n"
sleep 60s
echo "\n\nWaited...................................................................................\n\n"

status=$(mongo --tls --tlsCertificateKeyFile /security/member.pem --tlsCAFile /security/ca.pem --tlsAllowInvalidHostnames --quiet --eval 'rs.status().ok')
echo "status------------------------------------->$status"

if [[ $status != "1" ]]; then
    echo "
use admin
db.createUser({ user: \"$dbAdminUsername\", pwd: \"$dbPassword\", roles: [{ role: \"root\", db: \"admin\" }] })
db.auth(\"$dbAdminUsername\", \"$dbPassword\")

sh.addShard(\"$shardReplSet/$shardMember0:$dbPort,$shardMember1:$dbPort,$shardMember2:$dbPort\");
sh.status();

db.getSiblingDB(\"\$external\").runCommand(
    {
        createUser: \"CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US\",
        roles: [
            { role: \"dbAdmin\", db: \"$dbName\" },
            { role: \"readWrite\", db: \"$dbName\" },
            { role: \"userAdminAnyDatabase\", db: \"admin\" }
        ],
        writeConcern: { w: \"majority\", wtimeout: 5000 }
    }
)

use \$external
db.getUsers()

" >/mongo-tmp-init

    mongo --tls --tlsCertificateKeyFile /security/member.pem --tlsCAFile /security/ca.pem --tlsAllowInvalidHostnames </mongo-tmp-init

    echo "The mogos instance initialized successfully......................................................................................."

    rm /mongo-tmp-init
else
    echo "The mogos instance already initialized......................................................................................."
fi

fg
