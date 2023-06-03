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

echo initiating database containers...

sleep 10s

echo config replica set...
sudo docker exec -i user_management_configServer1 mongo <$scriptFileDirectory/mongodb/configServerInitializer

sleep 30s

echo shard replica set...
sudo docker exec -i user_management_shardServer1 mongo <$scriptFileDirectory/mongodb/shardServerInitializer

sleep 30s

echo mongos container...
sudo docker exec -i user_management_mongodb mongo <$scriptFileDirectory/mongodb/mongosInitializer
