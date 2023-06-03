# set directory of this script
if [[ $0 =~ ^\/{1}.* ]]; then
    scriptFileDirectory=${0%/*}
else
    if [[ $0 =~ ^\.\/{1}.* ]]; then
        path=$0
    else
        path=./$0
    fi
    path=${path/.\//}
    fullScriptPath=$(pwd)/$path
    scriptFileDirectory=${fullScriptPath%/*}
fi

if [[ $scriptFileDirectory == '/' ]]; then
    exit
fi

echo 'Preparing user_management service environment variables...'

username=$(cat $scriptFileDirectory/.env | grep -i DB_USERNAME)
username=${username/DB_USERNAME=/}
password=$(cat $scriptFileDirectory/.env | grep -i DB_PASSWORD)
password=${password/DB_PASSWORD=/}
rootPassword=$(cat $scriptFileDirectory/.env | grep -i DB_ROOT_PASSWORD)
rootPassword=${rootPassword/DB_ROOT_PASSWORD=/}
port=$(cat $scriptFileDirectory/.env | grep -i DB_PORT)
port=${port/DB_PORT=/}
service=$(cat $scriptFileDirectory/.env | grep -i DB_SERVICE)
service=${service/DB_SERVICE=/}
db=$(cat $scriptFileDirectory/.env | grep -i DB_NAME)
db=${db/DB_NAME=/}

echo "username: $username"
echo "password: $password"
echo "rootPassword: $rootPassword"
echo "port: $port"
echo "service: $service"
echo "db: $db"

# -------------------------------------------------------------------------------------

echo "Preparing $scriptFileDirectory/appsettings.json..."

appsettings=$scriptFileDirectory/appsettings.json

node >$scriptFileDirectory/out.json <<EOF
var data = require("$appsettings");

if (!data.MongoDB) {
    data.MongoDB = {};
}

data.MongoDB.ConnectionString = 'mongodb://$username:$password@$service:$port/?retryWrites=true&w=majority';
data.MongoDB.DatabaseName = '$db';
data.Jwt.SecretKey = 'TW9zaGVFcmV6UHJpdmF0ZUtleQ==';

console.log(JSON.stringify(data));
EOF

mv $scriptFileDirectory/out.json $appsettings

# -------------------------------------------------------------------------------------

echo "Preparing $scriptFileDirectory/mongosInitializer..."

echo "sh.addShard(\"user_management_shardReplicaSet1/user_management_shardServer1:27017,user_management_shardServer2:27017,user_management_shardServer3:27017\") 
sh.status() 
use admin 
db.createUser({ user: \"root\", pwd: \"$rootPassword\", roles: [{ role: \"userAdminAnyDatabase\", db: \"admin\" }] }) 
db.createUser({ user: \"$username\", pwd: \"$password\", roles: [{ role: \"userAdminAnyDatabase\", db: \"admin\" }] })
" >$scriptFileDirectory/mongodb/mongosInitializer

# -------------------------------------------------------------------------------------

echo "Preparing ./configServerInitializer..."

echo "rs.initiate({ _id: \"user_management_configReplicaSet1\", configsvr: true, members: [{ _id: 0, host: \"user_management_configServer1:$port\" }, { _id: 1, host: \"user_management_configServer2:$port\" }, { _id: 2, host: \"user_management_configServer3:$port\" }] }) 
rs.status()" >$scriptFileDirectory/mongodb/configServerInitializer

# -------------------------------------------------------------------------------------

echo "Preparing ./shardServerInitializer..."

echo "rs.initiate({ _id: \"user_management_shardReplicaSet1\", members: [{ _id: 0, host: \"user_management_shardServer1:$port\" }, { _id: 1, host: \"user_management_shardServer2:$port\" }, { _id: 2, host: \"user_management_shardServer3:$port\" }] })
rs.status()" >$scriptFileDirectory/mongodb/shardServerInitializer

# -------------------------------------------------------------------------------------

echo "Preparing ./docker-compose.development.yml..."

node $scriptFileDirectory/dockerComposeDevelopmentFileInitializer.js ymlFile=$scriptFileDirectory/docker-compose.development.yml context=$scriptFileDirectory username=$username password=$password rootPassword=$rootPassword port=$port service=$service db=$db ouput=$scriptFileDirectory/tmp.yml

mv $scriptFileDirectory/tmp.yml $scriptFileDirectory/docker-compose.development.yml

# -------------------------------------------------------------------------------------
