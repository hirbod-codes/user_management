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
if [[ -z $dir || -z $configFile || -z $extensions || -z $cn || -z $ou || -z $caCrt || -z $caKey ]]; then
    echo "Invalid arguments provided."
    exit 1
fi

if [[ $help == "true" || $h == "true" ]]; then
    echo "
caCrt                   ==> is required
caKey                   ==> is required
configFile              ==> is required
extensions              ==> is required
dir                     ==> is required
ou                      ==> is required
cn                      ==> is required
san                     ==> is optional
shouldExportPkcs12      ==> is optional
password                ==> is optional
    "
    exit 0
fi

key=$dir/app.key
csr=$dir/app.csr
crt=$dir/app.crt
pem=$dir/app.pem
p12=$dir/app.p12

echo "#########################"
echo "#########################"
echo "CN ==> $cn"

mkdir -p $dir

openssl genrsa -out $key

if [[ -n $san ]]; then
    openssl req -new -config $configFile -noenc -key $key -out $csr -subj /O=user_management/OU=$ou/CN=$cn -addext "subjectAltName = DNS:$san"
else
    openssl req -new -config $configFile -noenc -key $key -out $csr -subj /O=user_management/OU=$ou/CN=$cn
fi
openssl x509 -req -days 730 -in $csr -CA $caCrt -CAkey $caKey -CAcreateserial -out $crt -extensions req_ext -extfile $configFile
cat $crt $key >$dir/app.pem

if [[ $shouldExportPkcs12 == "true" ]]; then
    openssl pkcs12 -export -password pass:$password -in $crt -inkey $key -out $p12
fi

echo ""
