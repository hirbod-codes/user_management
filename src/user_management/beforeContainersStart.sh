#!/bin/bash

# This script shoudn't be called directly

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

if [[ -z $environment || -z $projectRootDirectory || -z $dbPort || -z $dbName || -z $generateCerts || -z $dbUsername || -z $dbAdminUsername || -z $dbPassword || -z $dbRootPassword || -z $ymlFile || -z $user_management_http_port || -z $user_management_https_port || -z $user_management_mongo_express_port ]]; then
    echo "Invalid arguments"
    exit
fi

# This script shoudn't be called directly

echo --------------------------------------------------------------------------------------

echo 'Preparing user_management service environment variables...'

if [[ $environment == "Development" ]]; then
    appsettings=$projectRootDirectory/appsettings.Development.json

    echo "Preparing $appsettings ..."

    node >$projectRootDirectory/out.json <<EOF
    var data = require("$appsettings");

    if (!data.MongoDB) {
        data.MongoDB = {};
    }

    data.MongoDB.username = 'CN=user_management,OU=mongodb_client,O=user_management,ST=NY,C=US';
    data.MongoDB.host = 'user_management_mongodb';
    data.MongoDB.port = '$dbPort';
    data.MongoDB.caPem = '/security/ca.pem';
    data.MongoDB.certificateP12 = '/security/app.p12';
    data.MongoDB.DatabaseName = '$dbName';
    data.Jwt.SecretKey = 'TW9zaGVFcmV6UHJpdmF0ZUtleQ==';

    console.log(JSON.stringify(data, null, 4));
EOF

    mv $projectRootDirectory/out.json $appsettings
fi

if [[ $generateCerts == "true" ]]; then
    echo --------------------------------------------------------------------------------------

    createClientCert() {
        local client=$1

        if [[ -z $2 ]]; then
            local ou=mongodb
        else
            local ou=$2
        fi

        mkdir -p $projectRootDirectory/security/$client/

        openssl req -new -sha256 -nodes -newkey rsa:4096 -keyout $projectRootDirectory/security/$client/app.key -out $projectRootDirectory/security/$client/app.csr -subj /C=US/ST=NY/O=user_management/OU=$ou/CN=$client
        openssl req -in $projectRootDirectory/security/$client/app.csr -noout -subject
        openssl x509 -req -sha256 -CA $projectRootDirectory/security/ca/ca.pem -CAkey $projectRootDirectory/security/ca/ca.key -days 730 -CAserial $projectRootDirectory/security/ca/ca.srl -extfile $projectRootDirectory/x509.ext -extensions client -in $projectRootDirectory/security/$client/app.csr -out $projectRootDirectory/security/$client/app.crt
        cat $projectRootDirectory/security/$client/app.crt $projectRootDirectory/security/$client/app.key >$projectRootDirectory/security/$client/app.pem
    }

    createServiceCert() {
        local service=$1

        mkdir -p $projectRootDirectory/security/$service/

        openssl req -new -sha256 -nodes -newkey rsa:4096 -keyout $projectRootDirectory/security/$service/app.key -out $projectRootDirectory/security/$service/app.csr -subj /C=US/ST=NY/O=user_management/OU=mongodb/CN=$service
        openssl req -in $projectRootDirectory/security/$service/app.csr -noout -subject
        openssl x509 -req -sha256 -CA $projectRootDirectory/security/ca/ca.pem -CAkey $projectRootDirectory/security/ca/ca.key -days 730 -CAserial $projectRootDirectory/security/ca/ca.srl -extfile $projectRootDirectory/x509.ext -extensions server -in $projectRootDirectory/security/$service/app.csr -out $projectRootDirectory/security/$service/app.crt
        cat $projectRootDirectory/security/$service/app.crt $projectRootDirectory/security/$service/app.key >$projectRootDirectory/security/$service/app.pem

        openssl req -new -sha256 -nodes -newkey rsa:4096 -keyout $projectRootDirectory/security/$service/app.key -out $projectRootDirectory/security/$service/member.csr -subj /C=US/ST=NY/O=user_management/OU=mongodb/CN=member
        openssl req -in $projectRootDirectory/security/$service/member.csr -noout -subject
        openssl x509 -req -sha256 -CA $projectRootDirectory/security/ca/ca.pem -CAkey $projectRootDirectory/security/ca/ca.key -days 730 -CAserial $projectRootDirectory/security/ca/ca.srl -extfile $projectRootDirectory/x509.ext -extensions server -in $projectRootDirectory/security/$service/member.csr -out $projectRootDirectory/security/$service/member.crt
        cat $projectRootDirectory/security/$service/member.crt $projectRootDirectory/security/$service/app.key >$projectRootDirectory/security/$service/member.pem
    }

    echo "Preparing services x509 certificates..."

    rm -r $projectRootDirectory/security

    mkdir -p $projectRootDirectory/security/ca/
    echo "" >$projectRootDirectory/security/ca/ca.srl

    openssl req -new -sha256 -nodes -newkey rsa:4096 -keyout $projectRootDirectory/security/ca/ca.key -out $projectRootDirectory/security/ca/ca.csr -subj /C=US/ST=NY/O=user_management/OU=mongodb/CN=user_management_certificate_authority
    openssl req -in $projectRootDirectory/security/ca/ca.csr -noout -subject
    openssl x509 -req -sha256 -extfile $projectRootDirectory/x509.ext -extensions ca -in $projectRootDirectory/security/ca/ca.csr -signkey $projectRootDirectory/security/ca/ca.key -days 1095 -out $projectRootDirectory/security/ca/ca.pem

    createClientCert localhost mongodb_client
    createClientCert local_client mongodb_client

    createClientCert user_management mongodb_client

    createClientCert user_management_mongo_express mongodb_client

    createServiceCert user_management_mongodb
    openssl pkcs12 -export -password pass: -in $projectRootDirectory/security/user_management/app.crt -inkey $projectRootDirectory/security/user_management/app.key -out $projectRootDirectory/security/user_management/app.p12

    createServiceCert user_management_configServer1
    createServiceCert user_management_configServer2
    createServiceCert user_management_configServer3

    createServiceCert user_management_shardServer1
    createServiceCert user_management_shardServer2
    createServiceCert user_management_shardServer3
fi

echo --------------------------------------------------------------------------------------

echo "Preparing docker compose yml file..."

if [[ $environment == "Development" ]]; then
    node $projectRootDirectory/dockerComposeDevelopmentFileInitializer.js \
        dbUsername=$dbUsername \
        dbAdminUsername=$dbAdminUsername \
        dbPassword=$dbPassword \
        dbPort=$dbPort \
        dbName=$dbName \
        ouput=$ymlFile
elif [[ $environment == "Production" ]]; then
    node $projectRootDirectory/dockerComposeProductionFileInitializer.js \
        dbPort=$dbPort \
        dbUsername=$dbUsername \
        user_management_http_port=$user_management_http_port \
        user_management_https_port=$user_management_https_port \
        user_management_mongo_express_port=$user_management_mongo_express_port \
        ouput=$ymlFile
fi
