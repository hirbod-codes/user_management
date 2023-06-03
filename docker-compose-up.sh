# $1 is the environment variable ['dev', 'development', 'prod', 'production']
# --fresh if present as second parameter, cleans the docker's data like images etc.
# set directory of this script
if [[ $0 =~ ^\.\/{1}.* ]]; then
    path=$0
else
    path=./$0
fi
path=${path/.\//}
fullScriptPath=$(pwd)/$path
scriptFileDirectory=${fullScriptPath%/*}

if [[ $scriptFileDirectory == '/' ]]; then
    exit
fi

echo ----------------------------------------------------------------------------------------------------------------------------

sudo chown -R :root $scriptFileDirectory && sudo chmod -R u+rwxs $scriptFileDirectory

echo ----------------------------------------------------------------------------------------------------------------------------

sudo chmod ug+x $scriptFileDirectory/*.sh

echo ----------------------------------------------------------------------------------------------------------------------------

npm install --prefix $scriptFileDirectory/

echo ----------------------------------------------------------------------------------------------------------------------------

if [[ $1 == "dev" || $1 == "development" ]]; then
    echo 'DB_NAME=user_management_db
DB_SERVICE=user_management_mongodb
DB_PORT=27017
DB_USERNAME=hirbod
DB_PASSWORD=password
DB_ROOT_PASSWORD=password
' >$scriptFileDirectory/.env
fi

echo ----------------------------------------------------------------------------------------------------------------------------

if [[ $2 == "--fresh" ]]; then
    bash -c $scriptFileDirectory/docker-fresh.sh
fi

echo ----------------------------------------------------------------------------------------------------------------------------

bash -c $scriptFileDirectory/beforeContainersStart.sh

echo ----------------------------------------------------------------------------------------------------------------------------

cat $scriptFileDirectory/docker-compose.development.yml

echo ----------------------------------------------------------------------------------------------------------------------------

sudo docker compose -f $scriptFileDirectory/docker-compose.development.yml up -d --build --remove-orphans

echo ----------------------------------------------------------------------------------------------------------------------------

bash -c $scriptFileDirectory/afterContainersStart.sh

echo ----------------------------------------------------------------------------------------------------------------------------
