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

if [[ $useTestValues == "true" ]]; then
    if [[ -z $projectRootDirectory ]]; then
        echo "projectRootDirectory is a required parameter."
        exit 1
    elif [[ $projectRootDirectory == '/' ]]; then
        echo "Project root directory must not be system root '/'"
        exit 1
    fi

    AppHttpsKey="$(cat $projectRootDirectory/security/user_management_https/app.key)"
    AppHttpsCrt="$(cat $projectRootDirectory/security/user_management_https/app.crt)"
    AppKey="$(cat $projectRootDirectory/security/user_management/app.key)"
    AppCrt="$(cat $projectRootDirectory/security/user_management/app.crt)"
    USER_MANAGEMENT_Jwt__SecretKey="123abc123abc"
    USER_MANAGEMENT_ADMIN_USERNAME=hirbod
    USER_MANAGEMENT_ADMIN_PASSWORD="Pass%w0rd!99"
    USER_MANAGEMENT_ADMIN_EMAIL="your_functional_email@email.com"
    USER_MANAGEMENT_ADMIN_PHONE_NUMBER=
    USER_MANAGEMENT_DB_NAME=mongodb
    USER_MANAGEMENT_DB_OPTIONS__IsSharded="false"
    USER_MANAGEMENT_DB_OPTIONS__DatabaseName=user_management_db
    USER_MANAGEMENT_DB_OPTIONS__Servers__0__Host=user_management_replicaSet_p
    USER_MANAGEMENT_DB_OPTIONS__Servers__0__Port=27017
    USER_MANAGEMENT_DB_OPTIONS__Servers__1__Host=user_management_replicaSet_s_1
    USER_MANAGEMENT_DB_OPTIONS__Servers__1__Port=27017
    USER_MANAGEMENT_DB_OPTIONS__Servers__2__Host=user_management_replicaSet_s_2
    USER_MANAGEMENT_DB_OPTIONS__Servers__2__Port=27017
    USER_MANAGEMENT_DB_OPTIONS__Username=CN=user_management,OU=mongodb_client,O=user_management
    DB_USERNAME=CN=user_management,OU=mongodb_client,O=user_management
    DB_ADMIN_USERNAME=hirbod
    DB_PASSWORD=password
    DB_DATABASE_NAME=user_management_db
    DB_SERVER_PORT=27017
    CA="$(cat $projectRootDirectory/security/ca/ca.crt)"
    CLUSTER_CA="$(cat $projectRootDirectory/security/ca/ca.crt)"
    USER_MANAGEMENT_REPLICA_SET_P_CRT="$(cat $projectRootDirectory/security/user_management_replicaSet_p/app.pem)"
    USER_MANAGEMENT_REPLICA_SET_P_MEMBER_CRT="$(cat $projectRootDirectory/security/user_management_replicaSet_p_member/app.pem)"
    USER_MANAGEMENT_REPLICA_SET_S_1_CRT="$(cat $projectRootDirectory/security/user_management_replicaSet_s_1/app.pem)"
    USER_MANAGEMENT_REPLICA_SET_S_1_MEMBER_CRT="$(cat $projectRootDirectory/security/user_management_replicaSet_s_1_member/app.pem)"
    USER_MANAGEMENT_REPLICA_SET_S_2_CRT="$(cat $projectRootDirectory/security/user_management_replicaSet_s_2/app.pem)"
    USER_MANAGEMENT_REPLICA_SET_S_2_MEMBER_CRT="$(cat $projectRootDirectory/security/user_management_replicaSet_s_2_member/app.pem)"
fi

sudo docker secret rm $(sudo docker secret ls -q)

if [[ -z $AppHttpsKey ]];                                   then echo "AppHttpsKey                                  parameter is required."; exit 1; fi
if [[ -z $AppHttpsCrt ]];                                   then echo "AppHttpsCrt                                  parameter is required."; exit 1; fi
if [[ -z $AppKey ]];                                        then echo "AppKey                                       parameter is required."; exit 1; fi
if [[ -z $AppCrt ]];                                        then echo "AppCrt                                       parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_Jwt__SecretKey ]];                then echo "USER_MANAGEMENT_Jwt__SecretKey               parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_ADMIN_USERNAME ]];                then echo "USER_MANAGEMENT_ADMIN_USERNAME               parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_ADMIN_PASSWORD ]];                then echo "USER_MANAGEMENT_ADMIN_PASSWORD               parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_ADMIN_EMAIL ]];                   then echo "USER_MANAGEMENT_ADMIN_EMAIL                  parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_ADMIN_PHONE_NUMBER ]];            then echo "USER_MANAGEMENT_ADMIN_PHONE_NUMBER           parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_NAME ]];                       then echo "USER_MANAGEMENT_DB_NAME                      parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__IsSharded ]];         then echo "USER_MANAGEMENT_DB_OPTIONS__IsSharded        parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__DatabaseName ]];      then echo "USER_MANAGEMENT_DB_OPTIONS__DatabaseName     parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Servers__0__Host ]];  then echo "USER_MANAGEMENT_DB_OPTIONS__Servers__0__Host parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Servers__0__Port ]];  then echo "USER_MANAGEMENT_DB_OPTIONS__Servers__0__Port parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Servers__1__Host ]];  then echo "USER_MANAGEMENT_DB_OPTIONS__Servers__1__Host parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Servers__1__Port ]];  then echo "USER_MANAGEMENT_DB_OPTIONS__Servers__1__Port parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Servers__2__Host ]];  then echo "USER_MANAGEMENT_DB_OPTIONS__Servers__2__Host parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Servers__2__Port ]];  then echo "USER_MANAGEMENT_DB_OPTIONS__Servers__2__Port parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_DB_OPTIONS__Username ]];          then echo "USER_MANAGEMENT_DB_OPTIONS__Username         parameter is required."; exit 1; fi
if [[ -z $DB_SERVER_PORT ]];                                then echo "DB_SERVER_PORT                               parameter is required."; exit 1; fi
if [[ -z $CA ]];                                            then echo "CA                                           parameter is required."; exit 1; fi
if [[ -z $CLUSTER_CA ]];                                    then echo "CLUSTER_CA                                   parameter is required."; exit 1; fi
if [[ -z $DB_DATABASE_NAME ]];                              then echo "DB_DATABASE_NAME                             parameter is required."; exit 1; fi
if [[ -z $DB_ADMIN_USERNAME ]];                             then echo "DB_ADMIN_USERNAME                            parameter is required."; exit 1; fi
if [[ -z $DB_USERNAME ]];                                   then echo "DB_USERNAME                                  parameter is required."; exit 1; fi
if [[ -z $DB_PASSWORD ]];                                   then echo "DB_PASSWORD                                  parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_REPLICA_SET_P_CRT ]];             then echo "USER_MANAGEMENT_REPLICA_SET_P_CRT            parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_REPLICA_SET_P_MEMBER_CRT ]];      then echo "USER_MANAGEMENT_REPLICA_SET_P_MEMBER_CRT     parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_REPLICA_SET_S_1_CRT ]];           then echo "USER_MANAGEMENT_REPLICA_SET_S_1_CRT          parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_REPLICA_SET_S_1_MEMBER_CRT ]];    then echo "USER_MANAGEMENT_REPLICA_SET_S_1_MEMBER_CRT   parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_REPLICA_SET_S_2_CRT ]];           then echo "USER_MANAGEMENT_REPLICA_SET_S_2_CRT          parameter is required."; exit 1; fi
if [[ -z $USER_MANAGEMENT_REPLICA_SET_S_2_MEMBER_CRT ]];    then echo "USER_MANAGEMENT_REPLICA_SET_S_2_MEMBER_CRT   parameter is required."; exit 1; fi

echo "$AppHttpsKey"                                     | sudo docker secret create AppHttpsKey -
echo "$AppHttpsCrt"                                     | sudo docker secret create AppHttpsCrt -
echo "$AppKey"                                          | sudo docker secret create AppKey -
echo "$AppCrt"                                          | sudo docker secret create AppCrt -
echo "$HttpsCertificatePassword"                        | sudo docker secret create HttpsCertificatePassword -
echo "$USER_MANAGEMENT_Jwt__SecretKey"                  | sudo docker secret create USER_MANAGEMENT_Jwt__SecretKey -
echo "$USER_MANAGEMENT_ADMIN_USERNAME"                  | sudo docker secret create USER_MANAGEMENT_ADMIN_USERNAME -
echo "$USER_MANAGEMENT_ADMIN_PASSWORD"                  | sudo docker secret create USER_MANAGEMENT_ADMIN_PASSWORD -
echo "$USER_MANAGEMENT_ADMIN_EMAIL"                     | sudo docker secret create USER_MANAGEMENT_ADMIN_EMAIL -
echo "$USER_MANAGEMENT_ADMIN_PHONE_NUMBER"              | sudo docker secret create USER_MANAGEMENT_ADMIN_PHONE_NUMBER -
echo "$USER_MANAGEMENT_DB_NAME"                         | sudo docker secret create USER_MANAGEMENT_DB_NAME -
echo "$USER_MANAGEMENT_DB_OPTIONS__IsSharded"           | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__IsSharded -
echo "$USER_MANAGEMENT_DB_OPTIONS__DatabaseName"        | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__DatabaseName -
echo "$USER_MANAGEMENT_DB_OPTIONS__Servers__0__Host"    | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Servers__0__Host -
echo "$USER_MANAGEMENT_DB_OPTIONS__Servers__0__Port"    | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Servers__0__Port -
echo "$USER_MANAGEMENT_DB_OPTIONS__Servers__1__Host"    | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Servers__1__Host -
echo "$USER_MANAGEMENT_DB_OPTIONS__Servers__1__Port"    | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Servers__1__Port -
echo "$USER_MANAGEMENT_DB_OPTIONS__Servers__2__Host"    | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Servers__2__Host -
echo "$USER_MANAGEMENT_DB_OPTIONS__Servers__2__Port"    | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Servers__2__Port -
echo "$USER_MANAGEMENT_DB_OPTIONS__Username"            | sudo docker secret create USER_MANAGEMENT_DB_OPTIONS__Username -

echo "$DB_SERVER_PORT"                                  | sudo docker secret create DB_SERVER_PORT -
echo "$CA"                                              | sudo docker secret create CA -
echo "$CLUSTER_CA"                                      | sudo docker secret create CLUSTER_CA -
echo "$DB_DATABASE_NAME"                                | sudo docker secret create DB_DATABASE_NAME -
echo "$DB_ADMIN_USERNAME"                               | sudo docker secret create DB_ADMIN_USERNAME -
echo "$DB_USERNAME"                                     | sudo docker secret create DB_USERNAME -
echo "$DB_PASSWORD"                                     | sudo docker secret create DB_PASSWORD -
echo "$USER_MANAGEMENT_REPLICA_SET_P_CRT"               | sudo docker secret create USER_MANAGEMENT_REPLICA_SET_P_CRT -
echo "$USER_MANAGEMENT_REPLICA_SET_P_MEMBER_CRT"        | sudo docker secret create USER_MANAGEMENT_REPLICA_SET_P_MEMBER_CRT -
echo "$USER_MANAGEMENT_REPLICA_SET_S_1_CRT"             | sudo docker secret create USER_MANAGEMENT_REPLICA_SET_S_1_CRT -
echo "$USER_MANAGEMENT_REPLICA_SET_S_1_MEMBER_CRT"      | sudo docker secret create USER_MANAGEMENT_REPLICA_SET_S_1_MEMBER_CRT -
echo "$USER_MANAGEMENT_REPLICA_SET_S_2_CRT"             | sudo docker secret create USER_MANAGEMENT_REPLICA_SET_S_2_CRT -
echo "$USER_MANAGEMENT_REPLICA_SET_S_2_MEMBER_CRT"      | sudo docker secret create USER_MANAGEMENT_REPLICA_SET_S_2_MEMBER_CRT -
