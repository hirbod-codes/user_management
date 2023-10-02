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

if [[ -z $Kestrel__Endpoints__Https__Certificate || -z $JWT__SECRET_KEY || -z $DB_NAME || -z $DB_OPTIONS__IsSharded || -z $DB_OPTIONS__DatabaseName || -z $DB_OPTIONS__Host || -z $DB_OPTIONS__Port || -z $DB_OPTIONS__Username || -z $DB_OPTIONS__Password || -z $DB_DATABASE_NAME || -z $DB_USERNAME || -z $DB_PASSWORD || -z $DB_SERVER_PORT ]]; then
    echo invalid arguments
    exit 1
fi

sudo docker secret rm $(sudo docker secret ls -q)

echo $Kestrel__Endpoints__Https__Certificate | sudo docker secret create Kestrel__Endpoints__Https__Certificate -

echo $JWT__SECRET_KEY | sudo docker secret create JWT__SECRET_KEY -

echo $DB_NAME | sudo docker secret create DB_NAME -

echo $DB_OPTIONS__IsSharded | sudo docker secret create DB_OPTIONS__IsSharded -
echo $DB_OPTIONS__DatabaseName | sudo docker secret create DB_OPTIONS__DatabaseName -
echo $DB_OPTIONS__Host | sudo docker secret create DB_OPTIONS__Host -
echo $DB_OPTIONS__Port | sudo docker secret create DB_OPTIONS__Port -
echo $DB_OPTIONS__Username | sudo docker secret create DB_OPTIONS__Username -
echo $DB_OPTIONS__Password | sudo docker secret create DB_OPTIONS__Password -

echo $DB_DATABASE_NAME | sudo docker secret create DB_DATABASE_NAME -
echo $DB_USERNAME | sudo docker secret create DB_USERNAME -
echo $DB_PASSWORD | sudo docker secret create DB_PASSWORD -
echo $DB_SERVER_PORT | sudo docker secret create DB_SERVER_PORT -
