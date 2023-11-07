# Instructions

For development **environments** you can enter your preferred admin credentials in prepare_environment_variables.sh bash script.
Also for production **environments**, similarly you're free to modify recreate_secrets_mongodb.sh and recreate_secrets_sharded_mongodb.sh.

## In Development Environment (Linux/WSL) with mongodb replica set as the database, run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    sudo rm -fr src/user_management/bin src/user_management/obj
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.mongodb.base.yml -f ./docker-compose.mongodb.development.yml --env-file ./.env.mongodb.development up --build --remove-orphans -V -d
```

## In Development Environment (Linux/WSL) with sharded mongodb cluster as the database, run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    sudo rm -fr src/user_management/bin src/user_management/obj
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.sharded_mongodb.base.yml -f ./docker-compose.sharded_mongodb.development.yml --env-file ./.env.sharded_mongodb.development up --build --remove-orphans -V -d
```

## In IntegrationTest Environment (Linux/WSL) with mongodb replica set as the database, run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    sudo chmod ug+x ./mongodb/*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.mongodb.base.yml -f ./docker-compose.mongodb.integration_test.yml --env-file ./.env.mongodb.integration_test up --build --remove-orphans -V --exit-code-from user_management
```

## In IntegrationTest Environment (Linux/WSL) with sharded mongodb cluster as the database, run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.sharded_mongodb.base.yml -f ./docker-compose.sharded_mongodb.integration_test.yml --env-file ./.env.sharded_mongodb.integration_test up --build --remove-orphans -V --exit-code-from user_management
```

## For UnitTest (Linux/WSL)

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    sudo docker compose -f ./docker-compose.unit_test.yml --env-file ./.env.unit_test up --build --remove-orphans -V --exit-code-from user_management
```

## For docker compose only with sharded mongodb cluster (so you can run the dotnet application outside docker container)

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.sharded_mongodb.base.yml -f ./docker-compose.sharded_mongodb.yml --env-file ./.env.sharded_mongodb up --build --remove-orphans -V -d
```

## For docker compose only with mongodb replica set (so you can run the dotnet application outside docker container)

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.mongodb.base.yml -f ./docker-compose.mongodb.yml --env-file ./.env.mongodb up --build --remove-orphans -V -d
```

## For testing docker swarm with mongodb replica set as the database, run

```bash
sudo docker swarm init && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./generate_certificates.sh --projectRootDirectory . && \
    ./recreate_secrets_mongodb.sh --projectRootDirectory . --useTestValues && \
    sudo docker build --tag ghcr.io/hirbod-codes/user_management:latest -f ./src/user_management/Dockerfile.production ./ && \
    sudo docker stack deploy -c ./docker-compose.swarm.mongodb.base.yml -c ./docker-compose.mongodb.production.yml app
```

## For testing docker swarm with sharded mongodb cluster as the database, run

```bash
sudo docker swarm init && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./generate_certificates.sh --projectRootDirectory . && \
    ./recreate_secrets_sharded_mongodb.sh --projectRootDirectory . --useTestValues && \
    sudo docker build --tag ghcr.io/hirbod-codes/user_management:latest -f ./src/user_management/Dockerfile.production ./ && \
    sudo docker stack deploy -c ./docker-compose.swarm.sharded_mongodb.base.yml -c ./docker-compose.sharded_mongodb.production.yml app
```

## For dotnet application outside container

### For running the application

if you don't specify a .env file relative path (aka `ENV_FILE_PATH=.env.file dotnet run`) for running a project outside the container,
it will use the default ".env.mongodb.development" value as the env file path.

### For debugging the application

Temporarily set the environment in Program.cs like

```C#
Environment.SetEnvironmentVariable("ENV_FILE_PATH", ".env.file");
```

or run following:

```bash
USER_MANAGEMENT_ENV_FILE_PATH=.env.mongodb.integration_test dotnet test --filter "FullyQualifiedName~user_management_integration_tests"
# or
dotnet test --filter "FullyQualifiedName~user_management_unit_tests"
```

TO DO:

Change environment values in .env file properly when running outside a docker container.

DB_OPTIONS__Host=localhost\
DB_OPTIONS__Port=the_port_of_db_container\
DB_OPTIONS__CertificateP12=security/user_management/app.p12

In MongoContext:

Set AllowInsecureTls to true.\
Set DirectConnection to true.\
Set host name localhost and port of the primary server in Server property of MongoClient.

**Unset Servers property in MongoClient, because mongodb replica set uses dns names and they are only available in docker compose network.**

**Do not use MongoDB standalone container because integration tests need atomic transactions.**

a change to test github actions 123
