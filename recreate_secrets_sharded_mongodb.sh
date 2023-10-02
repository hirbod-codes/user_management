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

if [[ -z $Kestrel__Endpoints__Https__Certificate ]]; then echo Kestrel__Endpoints__Https__Certificate parameter is required. && exit 1; fi
if [[ -z $Jwt__SecretKey ]]; then echo Jwt__SecretKey parameter is required. && exit 1; fi
if [[ -z $DB_NAME ]]; then echo DB_NAME parameter is required. && exit 1; fi
if [[ -z $DB_OPTIONS__IsSharded ]]; then echo DB_OPTIONS__IsSharded parameter is required. && exit 1; fi
if [[ -z $DB_OPTIONS_CaPem ]]; then echo DB_OPTIONS_CaPem parameter is required. && exit 1; fi
if [[ -z $DB_OPTIONS_CertificateP12 ]]; then echo DB_OPTIONS_CertificateP12 parameter is required. && exit 1; fi
if [[ -z $DB_OPTIONS__DatabaseName ]]; then echo DB_OPTIONS__DatabaseName parameter is required. && exit 1; fi
if [[ -z $DB_OPTIONS__Host ]]; then echo DB_OPTIONS__Host parameter is required. && exit 1; fi
if [[ -z $DB_OPTIONS__Port ]]; then echo DB_OPTIONS__Port parameter is required. && exit 1; fi
if [[ -z $DB_OPTIONS__Username ]]; then echo DB_OPTIONS__Username parameter is required. && exit 1; fi
if [[ -z $ME_CRT ]]; then echo ME_CRT parameter is required. && exit 1; fi
if [[ -z $DB_USERNAME ]]; then echo DB_USERNAME parameter is required. && exit 1; fi
if [[ -z $DB_PASSWORD ]]; then echo DB_PASSWORD parameter is required. && exit 1; fi
if [[ -z $DB_SERVER_PORT ]]; then echo DB_SERVER_PORT parameter is required. && exit 1; fi
if [[ -z $CRT_USERNAME ]]; then echo CRT_USERNAME parameter is required. && exit 1; fi
if [[ -z $DB_DATABASE_NAME ]]; then echo DB_DATABASE_NAME parameter is required. && exit 1; fi
if [[ -z $TLS_CAFILE ]]; then echo TLS_CAFILE parameter is required. && exit 1; fi
if [[ -z $TLS_CLUSTER_CAFILE ]]; then echo TLS_CLUSTER_CAFILE parameter is required. && exit 1; fi
if [[ -z $MONGODB_TLS_CLUSTER_FILE ]]; then echo MONGODB_TLS_CLUSTER_FILE parameter is required. && exit 1; fi
if [[ -z $MONGODB_TLS_CERTIFICATE_KEY_FILE ]]; then echo MONGODB_TLS_CERTIFICATE_KEY_FILE parameter is required. && exit 1; fi
if [[ -z $CONFIG_1_TLS_CLUSTER_FILE ]]; then echo CONFIG_1_TLS_CLUSTER_FILE parameter is required. && exit 1; fi
if [[ -z $CONFIG_1_TLS_CERTIFICATE_KEY_FILE ]]; then echo CONFIG_1_TLS_CERTIFICATE_KEY_FILE parameter is required. && exit 1; fi
if [[ -z $CONFIG_2_TLS_CLUSTER_FILE ]]; then echo CONFIG_2_TLS_CLUSTER_FILE parameter is required. && exit 1; fi
if [[ -z $CONFIG_2_TLS_CERTIFICATE_KEY_FILE ]]; then echo CONFIG_2_TLS_CERTIFICATE_KEY_FILE parameter is required. && exit 1; fi
if [[ -z $CONFIG_3_TLS_CLUSTER_FILE ]]; then echo CONFIG_3_TLS_CLUSTER_FILE parameter is required. && exit 1; fi
if [[ -z $CONFIG_3_TLS_CERTIFICATE_KEY_FILE ]]; then echo CONFIG_3_TLS_CERTIFICATE_KEY_FILE parameter is required. && exit 1; fi
if [[ -z $SHARD_1_TLS_CLUSTER_FILE ]]; then echo SHARD_1_TLS_CLUSTER_FILE parameter is required. && exit 1; fi
if [[ -z $SHARD_1_TLS_CERTIFICATE_KEY_FILE ]]; then echo SHARD_1_TLS_CERTIFICATE_KEY_FILE parameter is required. && exit 1; fi
if [[ -z $SHARD_2_TLS_CLUSTER_FILE ]]; then echo SHARD_2_TLS_CLUSTER_FILE parameter is required. && exit 1; fi
if [[ -z $SHARD_2_TLS_CERTIFICATE_KEY_FILE ]]; then echo SHARD_2_TLS_CERTIFICATE_KEY_FILE parameter is required. && exit 1; fi
if [[ -z $SHARD_3_TLS_CLUSTER_FILE ]]; then echo SHARD_3_TLS_CLUSTER_FILE parameter is required. && exit 1; fi

echo $Kestrel__Endpoints__Https__Certificate | sudo docker secret create Kestrel__Endpoints__Https__Certificate -
echo $Jwt__SecretKey | sudo docker secret create Jwt__SecretKey -

echo $DB_NAME | sudo docker secret create DB_NAME -
echo $DB_OPTIONS__IsSharded | sudo docker secret create DB_OPTIONS__IsSharded -
echo $DB_OPTIONS_CaPem | sudo docker secret create DB_OPTIONS_CaPem -
echo $DB_OPTIONS_CertificateP12 | sudo docker secret create DB_OPTIONS_CertificateP12 -
echo $DB_OPTIONS__DatabaseName | sudo docker secret create DB_OPTIONS__DatabaseName -
echo $DB_OPTIONS__Host | sudo docker secret create DB_OPTIONS__Host -
echo $DB_OPTIONS__Port | sudo docker secret create DB_OPTIONS__Port -
echo $DB_OPTIONS__Username | sudo docker secret create DB_OPTIONS__Username -

echo $ME_CRT | sudo docker secret create ME_CRT -

echo $DB_USERNAME | sudo docker secret create DB_USERNAME -
echo $DB_PASSWORD | sudo docker secret create DB_PASSWORD -
echo $DB_SERVER_PORT | sudo docker secret create DB_SERVER_PORT -
echo $CRT_USERNAME | sudo docker secret create CRT_USERNAME -
echo $DB_DATABASE_NAME | sudo docker secret create DB_DATABASE_NAME -

echo $TLS_CAFILE | sudo docker secret create TLS_CAFILE -
echo $TLS_CLUSTER_CAFILE | sudo docker secret create TLS_CLUSTER_CAFILE -

echo $MONGODB_TLS_CLUSTER_FILE | sudo docker secret create MONGODB_TLS_CLUSTER_FILE -
echo $MONGODB_TLS_CERTIFICATE_KEY_FILE | sudo docker secret create MONGODB_TLS_CERTIFICATE_KEY_FILE -

echo $CONFIG_1_TLS_CLUSTER_FILE | sudo docker secret create CONFIG_1_TLS_CLUSTER_FILE -
echo $CONFIG_1_TLS_CERTIFICATE_KEY_FILE | sudo docker secret create CONFIG_1_TLS_CERTIFICATE_KEY_FILE -
echo $CONFIG_2_TLS_CLUSTER_FILE | sudo docker secret create CONFIG_2_TLS_CLUSTER_FILE -
echo $CONFIG_2_TLS_CERTIFICATE_KEY_FILE | sudo docker secret create CONFIG_2_TLS_CERTIFICATE_KEY_FILE -
echo $CONFIG_3_TLS_CLUSTER_FILE | sudo docker secret create CONFIG_3_TLS_CLUSTER_FILE -
echo $CONFIG_3_TLS_CERTIFICATE_KEY_FILE | sudo docker secret create CONFIG_3_TLS_CERTIFICATE_KEY_FILE -

echo $SHARD_1_TLS_CLUSTER_FILE | sudo docker secret create SHARD_1_TLS_CLUSTER_FILE -
echo $SHARD_1_TLS_CERTIFICATE_KEY_FILE | sudo docker secret create SHARD_1_TLS_CERTIFICATE_KEY_FILE -
echo $SHARD_2_TLS_CLUSTER_FILE | sudo docker secret create SHARD_2_TLS_CLUSTER_FILE -
echo $SHARD_2_TLS_CERTIFICATE_KEY_FILE | sudo docker secret create SHARD_2_TLS_CERTIFICATE_KEY_FILE -
echo $SHARD_3_TLS_CLUSTER_FILE | sudo docker secret create SHARD_3_TLS_CLUSTER_FILE -
echo $SHARD_3_TLS_CERTIFICATE_KEY_FILE | sudo docker secret create SHARD_3_TLS_CERTIFICATE_KEY_FILE -
