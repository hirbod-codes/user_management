#!/bin/bash

while [ $# -gt 0 ]; do
    if [[ $1 == "--"* || $reset == "true" ]]; then
        if [[ -n $2 && $2 != "-"* || $reset == "true" ]]; then
            v="${1/--/}"
            declare $v="$2"
        else
            v="${1/--/}"
            declare $v="true"
        fi
    elif [[ $1 == "-"* ]]; then
        if [[ -n $2 && $2 != "-"* || $reset == "true" ]]; then
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

if [[ ! -e "$projectRootDirectory/.env.mongodb.development" || $reset == "true" ]]; then
    echo "ENVIRONMENT=Development

Logging__LogLevel__Default=information
Logging__LogLevel__Microsoft_AspNetCore=information

APP_HTTP_PORT=5000
APP_HTTPS_PORT=5001

Jwt__SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==

DB_NAME=mongodb

DB_OPTIONS__DatabaseName=user_management_db
DB_OPTIONS__Username=hirbod
DB_OPTIONS__Password=password
DB_OPTIONS__Host=user_management_mongodb
DB_OPTIONS__Port=27017
DB_OPTIONS__IsSharded=false

DB_DATABASE_NAME=user_management_db
DB_USERNAME=hirbod
DB_PASSWORD=password
DB_SERVER_PORT=27017
DB_CONTAINER_PORT=8081

ME_CONTAINER_PORT=8082
ME_CONFIG_BASICAUTH_USERNAME=hirbod
ME_CONFIG_BASICAUTH_PASSWORD=password
" >$projectRootDirectory/.env.mongodb.development
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.mongodb.integration_test" || $reset == "true" ]]; then
    echo "ENVIRONMENT=IntegrationTest

Logging__LogLevel__Default=information
Logging__LogLevel__Microsoft_AspNetCore=information

Jwt__SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==

DB_NAME=mongodb

DB_OPTIONS__DatabaseName=user_management_db
DB_OPTIONS__Username=hirbod
DB_OPTIONS__Password=password
DB_OPTIONS__Host=user_management_mongodb
DB_OPTIONS__Port=27017
DB_OPTIONS__IsSharded=false

DB_DATABASE_NAME=user_management_db
DB_USERNAME=hirbod
DB_PASSWORD=password
DB_SERVER_PORT=27017
" >$projectRootDirectory/.env.mongodb.integration_test
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.sharded_mongodb.development" || $reset == "true" ]]; then
    echo "ENVIRONMENT=Development

Logging__LogLevel__Default=information
Logging__LogLevel__Microsoft_AspNetCore=information

APP_HTTP_PORT=5000
APP_HTTPS_PORT=5001

Jwt__SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==

DB_NAME=mongodb

DB_OPTIONS__DatabaseName=user_management_db
DB_OPTIONS__Username=CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US
DB_OPTIONS__Password=password
DB_OPTIONS__Host=user_management_mongodb
DB_OPTIONS__Port=27017
DB_OPTIONS__IsSharded=true
DB_OPTIONS__CaPem=/security/ca/ca.pem
DB_OPTIONS__CertificateP12=/security/user_management/app.p12

DB_DATABASE_NAME=user_management_db
DB_USERNAME=hirbod
DB_PASSWORD=password
DB_SERVER_PORT=27017
DB_CONTAINER_PORT=8081
CRT_USERNAME=CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US

ME_CONTAINER_PORT=8082
ME_CONFIG_BASICAUTH_USERNAME=hirbod
ME_CONFIG_BASICAUTH_PASSWORD=password
" >$projectRootDirectory/.env.sharded_mongodb.development
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.sharded_mongodb.integration_test" || $reset == "true" ]]; then
    echo "ENVIRONMENT=IntegrationTest

Logging__LogLevel__Default=information
Logging__LogLevel__Microsoft_AspNetCore=information

Jwt__SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==

DB_OPTIONS__DatabaseName=user_management_db
DB_OPTIONS__Username=CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US
DB_OPTIONS__Password=password
DB_OPTIONS__Host=user_management_mongodb
DB_OPTIONS__Port=27017
DB_OPTIONS__IsSharded=true
DB_OPTIONS__CaPem=/security/ca/ca.pem
DB_OPTIONS__CertificateP12=/security/user_management/app.p12

DB_DATABASE_NAME=user_management_db
DB_USERNAME=hirbod
DB_PASSWORD=password
DB_SERVER_PORT=27017
CRT_USERNAME=CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US
" >$projectRootDirectory/.env.sharded_mongodb.integration_test
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.unit_test" || $reset == "true" ]]; then
    echo "ENVIRONMENT=UnitTest

Logging__LogLevel__Default=information
Logging__LogLevel__Microsoft_AspNetCore=information

Jwt__SecretKey=TW9zaGVFcmV6UHJpdmF0ZUtleQ==
" >$projectRootDirectory/.env.unit_test
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.mongodb" || $reset == "true" ]]; then
    echo "DB_DATABASE_NAME=user_management_db
DB_USERNAME=hirbod
DB_PASSWORD=password
DB_SERVER_PORT=27017
DB_CONTAINER_PORT=8081

ME_CONTAINER_PORT=8082
ME_CONFIG_BASICAUTH_USERNAME=hirbod
ME_CONFIG_BASICAUTH_PASSWORD=password
" >$projectRootDirectory/.env.mongodb
fi

# ----------------------------------------------------------------------------------------------------------------------------------------------------

if [[ ! -e "$projectRootDirectory/.env.sharded_mongodb" || $reset == "true" ]]; then
    echo "DB_DATABASE_NAME=user_management_db
DB_USERNAME=hirbod
DB_PASSWORD=password
DB_SERVER_PORT=27017
DB_CONTAINER_PORT=8081
CRT_USERNAME=CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US

ME_CONTAINER_PORT=8082
ME_CONFIG_BASICAUTH_USERNAME=hirbod
ME_CONFIG_BASICAUTH_PASSWORD=password
" >$projectRootDirectory/.env.sharded_mongodb
fi
