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

sudo rm -fr $projectRootDirectory/security

echo "#########################"
echo "#########################"
mkdir -p $projectRootDirectory/security/ca
openssl genrsa -out $projectRootDirectory/security/ca/ca.key
openssl req -x509 -sha256 -config ca.cnf -key $projectRootDirectory/security/ca/ca.key -out $projectRootDirectory/security/ca/ca.crt -subj /O=user_management/OU=mongodb/CN=user_management_certificate_authority
echo ""

bash -c "./generate_certificate.sh --dir $projectRootDirectory/security/localhost --caCrt $projectRootDirectory/security/ca/ca.crt --caKey $projectRootDirectory/security/ca/ca.key --configFile client.cnf --extensions req_ext --ou mongodb_client --cn localhost"
bash -c "./generate_certificate.sh --dir $projectRootDirectory/security/user_management --caCrt $projectRootDirectory/security/ca/ca.crt --caKey $projectRootDirectory/security/ca/ca.key --configFile client.cnf --extensions req_ext --ou mongodb_client --cn user_management --san user_management -shouldExportPkcs12"

bash -c "./generate_certificate.sh --dir $projectRootDirectory/security/user_management_replicaSet_p --caCrt $projectRootDirectory/security/ca/ca.crt --caKey $projectRootDirectory/security/ca/ca.key --configFile client.cnf --extensions req_ext --ou mongodb --cn user_management_replicaSet_p --san user_management_replicaSet_p"
bash -c "./generate_certificate.sh --dir $projectRootDirectory/security/user_management_replicaSet_s_1 --caCrt $projectRootDirectory/security/ca/ca.crt --caKey $projectRootDirectory/security/ca/ca.key --configFile client.cnf --extensions req_ext --ou mongodb --cn user_management_replicaSet_s_1 --san user_management_replicaSet_s_1"
bash -c "./generate_certificate.sh --dir $projectRootDirectory/security/user_management_replicaSet_s_2 --caCrt $projectRootDirectory/security/ca/ca.crt --caKey $projectRootDirectory/security/ca/ca.key --configFile client.cnf --extensions req_ext --ou mongodb --cn user_management_replicaSet_s_2 --san user_management_replicaSet_s_2"
