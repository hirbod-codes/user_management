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

# For https connections
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

# For https connections
mkdir -p $projectRootDirectory/security/user_management_https/
openssl req -x509 -nodes -days 1000 -newkey rsa:4096 -sha256 -keyout ./security/user_management_https/private.key -out ./security/user_management_https/server.crt -config ./src/user_management/https_x509.ext -extensions https
openssl pkcs12 -password pass: -export -out ./security/user_management_https/https.pfx -inkey ./security/user_management_https/private.key -in ./security/user_management_https/server.crt
