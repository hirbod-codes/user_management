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

if [[ $help == "true" || $h == "true" ]]; then
    echo "Following arguments are expected:
    --jwtSecretKey value    --> required
    --shouldClone           --> optional, will clone if repositories don't exist
    "
    exit
fi

if [[ -z $jwtSecretKey ]]; then
    echo "Invalid arguments"
    exit
fi

if [[ $shouldClone == "true" ]]; then
    if [[ ! -d "./src/user_management" ]]; then
        git clone https://hirbod-codes/user_management
    fi

    if [[ ! -d "./src/user_management" ]]; then
        echo "failed to clone repositories"
        exit
    fi
fi

sudo docker stack rm app
sudo docker rm -f $(sudo docker ps -aq)
sudo docker secret rm $(sudo docker secret ls -q)

echo $jwtSecretKey | sudo docker secret create jwt-secret-key -

    # --refreshDocker \
./src/user_management/docker-compose-up.sh \
    --projectRootDirectory ./src/user_management \
    --environment Production \
    --generateCerts \
    --dbName user_management_db \
    --dbPort 27017 \
    --user_management_http_port 5000 \
    --user_management_https_port 5001 \
    --user_management_mongo_express_port 44122 \
    --dbUsername CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US \
    --dbAdminUsername hirbod \
    --dbPassword password \
    --dbRootPassword password \
    --ymlFile ./src/user_management/tmp.yml 

sudo docker secret ls

read -p "Should we proceed? (y/yes): " answer

if [[ $answer == [Yy] || $answer == [Yy][Ee][Ss] ]]; then
    sudo docker stack deploy \
        -c ./swarm.yml \
        -c ./src/user_management/tmp.yml \
        app
else
    exit
fi

sudo docker service ls
