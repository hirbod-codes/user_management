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

# Validating Arguments
if [[ -z $projectRootDirectory ]]; then
    echo "Insufficient arguments"
    exit
elif [[ $projectRootDirectory == '/' ]]; then
    echo "Project root directory must not be system root '/'"
    exit
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.mongodb.development" ]]; then
    echo "ENVIRONMENT=Development

Logging_LogLevel_Default=information
Logging_LogLevel_Microsoft_AspNetCore=information

APP_HTTP_PORT=5000
APP_HTTPS_PORT=5001

Jwt_SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==

DB_NAME=mongodb
DATABASE_NAME=user_management_db
DB_SERVER_PORT=27017
DB_CONTAINER_PORT=8081

ME_CONTAINER_PORT=8082
ME_CONFIG_BASICAUTH_USERNAME=hirbod
ME_CONFIG_BASICAUTH_PASSWORD=password

DB_USERNAME=hirbod
DB_PASSWORD=password
" >$projectRootDirectory/.env.mongodb.development
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.mongodb.integration_test" ]]; then
    echo "ENVIRONMENT=IntegrationTest

Logging_LogLevel_Default=information
Logging_LogLevel_Microsoft_AspNetCore=information

Jwt_SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==

DB_NAME=mongodb
DATABASE_NAME=user_management_db
DB_SERVER_PORT=27017

DB_USERNAME=hirbod
DB_PASSWORD=password
" >$projectRootDirectory/.env.mongodb.integration_test
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.sharded_mongodb.development" ]]; then
    echo "ENVIRONMENT=Development

Logging_LogLevel_Default=information
Logging_LogLevel_Microsoft_AspNetCore=information

APP_HTTP_PORT=5000
APP_HTTPS_PORT=5001

Jwt_SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==

DB_NAME=mongodb
DATABASE_NAME=user_management_db
DB_SERVER_PORT=27017
DB_CONTAINER_PORT=8081

ME_CONTAINER_PORT=8082
ME_CONFIG_BASICAUTH_USERNAME=hirbod
ME_CONFIG_BASICAUTH_PASSWORD=password

DB_USERNAME=hirbod
DB_PASSWORD=password

CRT_USERNAME=CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US
" >$projectRootDirectory/.env.sharded_mongodb.development
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.sharded_mongodb.integration_test" ]]; then
    echo "ENVIRONMENT=IntegrationTest

Logging_LogLevel_Default=information
Logging_LogLevel_Microsoft_AspNetCore=information

Jwt_SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==

DB_NAME=mongodb
DATABASE_NAME=user_management_db
DB_SERVER_PORT=27017

DB_USERNAME=hirbod
DB_PASSWORD=password

CRT_USERNAME=CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US
" >$projectRootDirectory/.env.sharded_mongodb.integration_test
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.unit_test" ]]; then
    echo "ENVIRONMENT=UnitTest

Logging_LogLevel_Default=information
Logging_LogLevel_Microsoft_AspNetCore=information

Jwt_SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==
" >$projectRootDirectory/.env.unit_test
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.mongodb" ]]; then
    echo "DB_SERVER_PORT=27017
DB_CONTAINER_PORT=8081

DB_USERNAME=hirbod
DB_PASSWORD=password

ME_CONTAINER_PORT=8082
ME_CONFIG_BASICAUTH_USERNAME=hirbod
ME_CONFIG_BASICAUTH_PASSWORD=password
" >$projectRootDirectory/.env.mongodb
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.sharded_mongodb" ]]; then
    echo "DB_SERVER_PORT=27017
DB_CONTAINER_PORT=8081

ME_CONTAINER_PORT=8082
ME_CONFIG_BASICAUTH_USERNAME=hirbod
ME_CONFIG_BASICAUTH_PASSWORD=password

DB_USERNAME=hirbod
DB_PASSWORD=password

CRT_USERNAME=CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US
" >$projectRootDirectory/.env.sharded_mongodb
fi
