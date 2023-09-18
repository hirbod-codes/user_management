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
    echo 'Following arguments are expected:
    --environment value                         --> required, valid values: Development, Dev, Production, Prod
    --useDevVariables                           --> usable only if $environment is Dev or Development
    --generateCerts
    --refreshDocker
    --doNotRunDocker
    --projectRootDirectory value                --> required
    --dbPort value                              --> required unless $useDevVariables is set
    --dbName value                              --> required unless $useDevVariables is set
    --dbUsername value                          --> required unless $useDevVariables is set
    --dbAdminUsername value                     --> required unless $useDevVariables is set
    --dbPassword value                          --> required unless $useDevVariables is set
    --dbRootPassword value                      --> required unless $useDevVariables is set
    --user_management_http_port value           --> required unless $useDevVariables is set
    --user_management_https_port value          --> required unless $useDevVariables is set
    --user_management_mongo_express_port value  --> required unless $useDevVariables is set
    --ymlFile value                             --> required unless $useDevVariables is set
    '
    exit
fi

# Validating Arguments
if [[ -z $projectRootDirectory ]]; then
    echo "Insufficient arguments"
    exit
elif [[ $projectRootDirectory == '/' ]]; then
    echo "Project root directory must not be system root '/'"
    exit
fi

if [[ ${projectRootDirectory:(-1)} == '/' ]]; then
    projectRootDirectory=${projectRootDirectory::-1}
fi

if [[ ! ($environment == "Development" || $environment == "Dev" || $environment == "Production" || $environment == "Prod") ]]; then
    echo "Invalid values for environment"
    exit
elif [[ $environment == "Dev" ]]; then
    environment="Development"
elif [[ $environment == "Prod" ]]; then
    environment="Production"
fi

if [[ $environment == "Development" && $useDevVariables == "true" ]]; then
    dbPort="27017"
    dbName="user_management_db"
    dbUsername="CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US"
    dbAdminUsername="hirbod"
    dbPassword="password"
    dbRootPassword="password"
    user_management_http_port=5000
    user_management_https_port=5001
    user_management_mongo_express_port=6001
    ymlFile=$projectRootDirectory/tmp.yml
fi

if [[ -z $projectRootDirectory || -z $dbPort || -z $dbName || -z $dbUsername || -z $dbAdminUsername || -z $dbPassword || -z $dbRootPassword || -z $user_management_http_port || -z $user_management_https_port || -z $user_management_mongo_express_port ]]; then
    echo "Invalid arguments"
    exit
fi

if [[ -z $generateCerts ]]; then
    generateCerts="false"
fi

echo ----------------------------------------------------------------------------------------------------------------------------

echo Set project files permissions...

sudo chown -R :root $projectRootDirectory && sudo chmod -R u+rwxs $projectRootDirectory

sudo chmod ug+x $projectRootDirectory/*.sh

echo ----------------------------------------------------------------------------------------------------------------------------

echo Install node dependencies...

npm install --prefix $projectRootDirectory/

if [[ $refreshDocker == "true" ]]; then
    echo ----------------------------------------------------------------------------------------------------------------------------

    echo Refreshing docker...

    bash -c $projectRootDirectory/docker-refresh.sh
fi

echo ----------------------------------------------------------------------------------------------------------------------------

echo Run prepration script: $projectRootDirectory/beforeContainersStart.sh

if [[ $environment == "Development" ]]; then
    bash -c "$projectRootDirectory/beforeContainersStart.sh \
        --generateCerts $generateCerts \
        --projectRootDirectory $projectRootDirectory \
        --ymlFile $ymlFile \
        --environment $environment \
        --dbName $dbName \
        --dbPort $dbPort \
        --dbUsername $dbUsername \
        --dbAdminUsername $dbAdminUsername \
        --dbPassword $dbPassword \
        --dbRootPassword $dbRootPassword \
        --user_management_http_port $user_management_http_port \
        --user_management_https_port $user_management_https_port \
        --user_management_mongo_express_port $user_management_mongo_express_port
        "
    bash -c "$projectRootDirectory/docker-build-development.sh --projectRootDirectory $projectRootDirectory"
elif [[ $environment == "Production" ]]; then
    bash -c "$projectRootDirectory/beforeContainersStart.sh \
        --generateCerts $generateCerts \
        --projectRootDirectory $projectRootDirectory \
        --ymlFile $ymlFile \
        --environment $environment \
        --dbName $dbName \
        --dbPort $dbPort \
        --dbUsername $dbUsername \
        --dbAdminUsername $dbAdminUsername \
        --dbPassword $dbPassword \
        --dbRootPassword $dbRootPassword \
        --user_management_http_port $user_management_http_port \
        --user_management_https_port $user_management_https_port \
        --user_management_mongo_express_port $user_management_mongo_express_port
        "
    bash -c "$projectRootDirectory/docker-build-production.sh --projectRootDirectory $projectRootDirectory"
else
    echo Invalid environment!
    exit
fi

echo ----------------------------------------------------------------------------------------------------------------------------

if [[ $doNotRunDocker == "true" ]]; then
    echo Skipping docker...
else
    if [[ $environment == "Development" ]]; then
        echo Run docker compose...
        sudo docker compose -f $ymlFile up -d --build --remove-orphans

        rm $ymlFile
    fi
fi

if [[ $environment == "Production" ]]; then
    echo Create or Replace docker swarm secrets...

    echo $dbName | sudo docker secret create user-management-db-name -

    echo $dbPort | sudo docker secret create user-management-db-port -

    echo $dbAdminUsername | sudo docker secret create user-management-db-admin-username -

    echo $dbPassword | sudo docker secret create user-management-db-password -

    echo $dbRootPassword | sudo docker secret create user-management-db-root-password -
fi
