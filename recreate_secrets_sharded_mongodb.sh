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
    echo "projectRootDirectory is a required parameter."
    exit 1
elif [[ $projectRootDirectory == '/' ]]; then
    echo "Project root directory must not be system root '/'"
    exit 1
fi

if [[ $useTestValues == "true" ]]; then
    Kestrel__Endpoints__Https__Certificate=$(car $projectRootDirectory/security/user_management_https/app.p12)
    USER_MANAGEMENT_Jwt__SecretKey="123abc123abc"
    USER_MANAGEMENT_DB_NAME=mongodb
    USER_MANAGEMENT_DB_OPTIONS__IsSharded="true"
    CertificateP12=$(cat $projectRootDirectory/security/user_management/app.p12)
    USER_MANAGEMENT_DB_OPTIONS__DatabaseName=user_management_db
    USER_MANAGEMENT_DB_OPTIONS__Host=user_management_mongodb
    USER_MANAGEMENT_DB_OPTIONS__Port=27017
    USER_MANAGEMENT_DB_OPTIONS__Username=CN=user_management,OU=mongodb_client,O=user_management
    CRT_USERNAME=CN=user_management,OU=mongodb_client,O=user_management
    DB_USERNAME=hirbod
    DB_PASSWORD=password
    DB_DATABASE_NAME=user_management_db
    DB_SERVER_PORT=27017
    TLS_CAFILE=$(cat $projectRootDirectory/security/ca/ca.crt)
    TLS_CLUSTER_CAFILE=$(cat $projectRootDirectory/security/ca/ca.crt)
    MONGODB_TLS_CLUSTER_FILE=$(cat $projectRootDirectory/security/user_management_mongodb_member/app.pem)
    MONGODB_TLS_CERTIFICATE_KEY_FILE=$(cat $projectRootDirectory/security/user_management_mongodb/app.pem)
    CONFIG_1_TLS_CLUSTER_FIL=$(cat $projectRootDirectory/security/user_management_configServer1_member/app.pem)
    CONFIG_1_TLS_CERTIFICATE_KEY_FIL=$(cat $projectRootDirectory/security/user_management_configServer1/app.pem)
    CONFIG_2_TLS_CLUSTER_FIL=$(cat $projectRootDirectory/security/user_management_configServer2_member/app.pem)
    CONFIG_2_TLS_CERTIFICATE_KEY_FIL=$(cat $projectRootDirectory/security/user_management_configServer2/app.pem)
    CONFIG_3_TLS_CLUSTER_FIL=$(cat $projectRootDirectory/security/user_management_configServer3_member/app.pem)
    CONFIG_3_TLS_CERTIFICATE_KEY_FIL=$(cat $projectRootDirectory/security/user_management_configServer3/app.pem)
    SHARD_1_TLS_CLUSTER_FILE=$(cat $projectRootDirectory/security/user_management_shardServer1_member/app.pem)
    SHARD_1_TLS_CERTIFICATE_KEY_FILE=$(cat $projectRootDirectory/security/user_management_shardServer1/app.pem)
    SHARD_2_TLS_CLUSTER_FILE=$(cat $projectRootDirectory/security/user_management_shardServer2_member/app.pem)
    SHARD_2_TLS_CERTIFICATE_KEY_FILE=$(cat $projectRootDirectory/security/user_management_shardServer2/app.pem)
    SHARD_3_TLS_CLUSTER_FILE=$(cat $projectRootDirectory/security/user_management_shardServer3_member/app.pem)
    SHARD_3_TLS_CERTIFICATE_KEY_FILE=$(cat $projectRootDirectory/security/user_management_shardServer3/app.pem)
fi

sudo docker secret rm $(sudo docker secret ls -q)

if [[ -z $Kestrel__Endpoints__Https__Certificate ]]; then   echo "Kestrel__Endpoints__Https__Certificate   parameter is required.";exit 1;fi
if [[ -z $USER_MANAGEMENT_Jwt__SecretKey ]]; then           echo "USER_MANAGEMENT_Jwt__SecretKey           parameter is required.";exit 1;fi
if [[ -z $USER_MANAGEMENT_DB_NAME ]]; then                  echo "USER_MANAGEMENT_DB_NAME                  parameter is required.";exit 1;fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__IsSharded ]]; then    echo "USER_MANAGEMENT_DB_OPTIONS__IsSharded    parameter is required.";exit 1;fi
if [[ -z $CertificateP12 ]]; then                           echo "CertificateP12                           parameter is required.";exit 1;fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__DatabaseName ]]; then echo "USER_MANAGEMENT_DB_OPTIONS__DatabaseName parameter is required.";exit 1;fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Host ]]; then         echo "USER_MANAGEMENT_DB_OPTIONS__Host         parameter is required.";exit 1;fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Port ]]; then         echo "USER_MANAGEMENT_DB_OPTIONS__Port         parameter is required.";exit 1;fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Username ]]; then     echo "USER_MANAGEMENT_DB_OPTIONS__Username     parameter is required.";exit 1;fi

if [[ -z $CRT_USERNAME ]]; then                             echo "CRT_USERNAME                             parameter is required."; exit 1;fi
if [[ -z $DB_USERNAME ]]; then                              echo "DB_USERNAME                              parameter is required."; exit 1;fi
if [[ -z $DB_PASSWORD ]]; then                              echo "DB_PASSWORD                              parameter is required."; exit 1;fi
if [[ -z $DB_DATABASE_NAME ]]; then                         echo "DB_DATABASE_NAME                         parameter is required."; exit 1;fi
if [[ -z $DB_SERVER_PORT ]]; then                           echo "DB_SERVER_PORT                           parameter is required."; exit 1;fi
if [[ -z $TLS_CAFILE ]]; then                               echo "TLS_CAFILE                               parameter is required."; exit 1;fi
if [[ -z $TLS_CLUSTER_CAFILE ]]; then                       echo "TLS_CLUSTER_CAFILE                       parameter is required."; exit 1;fi

if [[ -z $MONGODB_TLS_CLUSTER_FILE ]]; then                 echo "MONGODB_TLS_CLUSTER_FILE                 parameter is required."; exit 1;fi
if [[ -z $MONGODB_TLS_CERTIFICATE_KEY_FILE ]]; then         echo "MONGODB_TLS_CERTIFICATE_KEY_FILE         parameter is required."; exit 1;fi
if [[ -z $CONFIG_1_TLS_CLUSTER_FIL ]]; then                 echo "CONFIG_1_TLS_CLUSTER_FIL                 parameter is required."; exit 1;fi
if [[ -z $CONFIG_1_TLS_CERTIFICATE_KEY_FIL ]]; then         echo "CONFIG_1_TLS_CERTIFICATE_KEY_FIL         parameter is required."; exit 1;fi
if [[ -z $CONFIG_2_TLS_CLUSTER_FIL ]]; then                 echo "CONFIG_2_TLS_CLUSTER_FIL                 parameter is required."; exit 1;fi
if [[ -z $CONFIG_2_TLS_CERTIFICATE_KEY_FIL ]]; then         echo "CONFIG_2_TLS_CERTIFICATE_KEY_FIL         parameter is required."; exit 1;fi
if [[ -z $CONFIG_3_TLS_CLUSTER_FIL ]]; then                 echo "CONFIG_3_TLS_CLUSTER_FIL                 parameter is required."; exit 1;fi
if [[ -z $CONFIG_3_TLS_CERTIFICATE_KEY_FIL ]]; then         echo "CONFIG_3_TLS_CERTIFICATE_KEY_FIL         parameter is required."; exit 1;fi
if [[ -z $SHARD_1_TLS_CLUSTER_FILE ]]; then                 echo "SHARD_1_TLS_CLUSTER_FILE                 parameter is required."; exit 1;fi
if [[ -z $SHARD_1_TLS_CERTIFICATE_KEY_FILE ]]; then         echo "SHARD_1_TLS_CERTIFICATE_KEY_FILE         parameter is required."; exit 1;fi
if [[ -z $SHARD_2_TLS_CLUSTER_FILE ]]; then                 echo "SHARD_2_TLS_CLUSTER_FILE                 parameter is required."; exit 1;fi
if [[ -z $SHARD_2_TLS_CERTIFICATE_KEY_FILE ]]; then         echo "SHARD_2_TLS_CERTIFICATE_KEY_FILE         parameter is required."; exit 1;fi
if [[ -z $SHARD_3_TLS_CLUSTER_FILE ]]; then                 echo "SHARD_3_TLS_CLUSTER_FILE                 parameter is required."; exit 1;fi
if [[ -z $SHARD_3_TLS_CERTIFICATE_KEY_FILE ]]; then         echo "SHARD_3_TLS_CERTIFICATE_KEY_FILE         parameter is required."; exit 1;fi

echo $Kestrel__Endpoints__Https__Certificate    | sudo docker secret create Kestrel__Endpoints__Https__Certificate -
echo $USER_MANAGEMENT_Jwt__SecretKey            | sudo docker secret create USER_MANAGEMENT_Jwt__SecretKey -
echo $USER_MANAGEMENT_DB_NAME                   | sudo docker secret create USER_MANAGEMENT_DB_NAME -
echo $USER_MANAGEMENT_DB_OPTIONS__IsSharded     | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__IsSharded -
echo $CertificateP12                            | sudo docker secret create CertificateP12 -
echo $USER_MANAGEMENT_DB_OPTIONS__DatabaseName  | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__DatabaseName -
echo $USER_MANAGEMENT_DB_OPTIONS__Host          | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Host -
echo $USER_MANAGEMENT_DB_OPTIONS__Port          | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Port -
echo $USER_MANAGEMENT_DB_OPTIONS__Username      | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Username -

echo $CRT_USERNAME                              | sudo docker secret create CRT_USERNAME -
echo $DB_USERNAME                               | sudo docker secret create DB_USERNAME -
echo $DB_PASSWORD                               | sudo docker secret create DB_PASSWORD -
echo $DB_DATABASE_NAME                          | sudo docker secret create DB_DATABASE_NAME -
echo $DB_SERVER_PORT                            | sudo docker secret create DB_SERVER_PORT -
echo $TLS_CAFILE                                | sudo docker secret create TLS_CAFILE -
echo $TLS_CLUSTER_CAFILE                        | sudo docker secret create TLS_CLUSTER_CAFILE -

echo $MONGODB_TLS_CLUSTER_FILE                  | sudo docker secret create MONGODB_TLS_CLUSTER_FILE -
echo $MONGODB_TLS_CERTIFICATE_KEY_FILE          | sudo docker secret create MONGODB_TLS_CERTIFICATE_KEY_FILE -
echo $CONFIG_1_TLS_CLUSTER_FIL                  | sudo docker secret create CONFIG_1_TLS_CLUSTER_FIL -
echo $CONFIG_1_TLS_CERTIFICATE_KEY_FIL          | sudo docker secret create CONFIG_1_TLS_CERTIFICATE_KEY_FIL -
echo $CONFIG_2_TLS_CLUSTER_FIL                  | sudo docker secret create CONFIG_2_TLS_CLUSTER_FIL -
echo $CONFIG_2_TLS_CERTIFICATE_KEY_FIL          | sudo docker secret create CONFIG_2_TLS_CERTIFICATE_KEY_FIL -
echo $CONFIG_3_TLS_CLUSTER_FIL                  | sudo docker secret create CONFIG_3_TLS_CLUSTER_FIL -
echo $CONFIG_3_TLS_CERTIFICATE_KEY_FIL          | sudo docker secret create CONFIG_3_TLS_CERTIFICATE_KEY_FIL -
echo $SHARD_1_TLS_CLUSTER_FILE                  | sudo docker secret create SHARD_1_TLS_CLUSTER_FILE -
echo $SHARD_1_TLS_CERTIFICATE_KEY_FILE          | sudo docker secret create SHARD_1_TLS_CERTIFICATE_KEY_FILE -
echo $SHARD_2_TLS_CLUSTER_FILE                  | sudo docker secret create SHARD_2_TLS_CLUSTER_FILE -
echo $SHARD_2_TLS_CERTIFICATE_KEY_FILE          | sudo docker secret create SHARD_2_TLS_CERTIFICATE_KEY_FILE -
echo $SHARD_3_TLS_CLUSTER_FILE                  | sudo docker secret create SHARD_3_TLS_CLUSTER_FILE -
echo $SHARD_3_TLS_CERTIFICATE_KEY_FILE          | sudo docker secret create SHARD_3_TLS_CERTIFICATE_KEY_FILE -
