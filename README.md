# Instructions

## In Development Environment (Linux/WSL) with mongodb replica set as the database, run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.mongodb.base.yml -f ./docker-compose.mongodb.development.yml --env-file ./.env.mongodb.development up -d --build --remove-orphans -V
```

## In Development Environment (Linux/WSL) with sharded mongodb cluster as the database, run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.sharded_mongodb.base.yml -f ./docker-compose.sharded_mongodb.development.yml --env-file ./.env.sharded_mongodb.development up -d --build --remove-orphans -V
```

## In IntegrationTest Environment (Linux/WSL) with mongodb replica set as the database, run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
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

## In UnitTest Environment (Linux/WSL)

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    sudo docker compose -f ./docker-compose.unit_test.yml --env-file ./.env.unit_test up --build --remove-orphans -V --exit-code-from user_management
```

## for docker compose only with sharded mongodb cluster (so you can run the dotnet application outside docker container)

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.sharded_mongodb.base.yml -f ./docker-compose.sharded_mongodb.yml --env-file ./.env.sharded_mongodb up -d --build --remove-orphans -V
```

## for docker compose only with mongodb replica set (so you can run the dotnet application outside docker container)

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory . --reset && \
    ./generate_certificates.sh --projectRootDirectory . && \
    sudo docker compose -f ./docker-compose.mongodb.base.yml -f ./docker-compose.mongodb.yml --env-file ./.env.mongodb up -d --build --remove-orphans -V
```

## for testing docker swarm with mongodb replica set as the database, run

```bash
sudo docker swarm init && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./generate_certificates.sh --projectRootDirectory . && \
    ./recreate_secrets_mongodb.sh --projectRootDirectory . --useTestValues && \
    sudo docker build --tag ghcr.io/hirbod-codes/user_management:latest -f ./src/user_management/Dockerfile.production ./ && \
    sudo docker stack deploy -c ./docker-compose.swarm.mongodb.base.yml -c ./docker-compose.mongodb.production.yml app
```

## for testing docker swarm with sharded mongodb cluster as the database, run

```bash
sudo docker swarm init && \
    sudo chmod ug+x ./*.sh ./mongodb/*.sh && \
    ./generate_certificates.sh --projectRootDirectory . && \
    ./recreate_secrets_sharded_mongodb.sh --projectRootDirectory . --useTestValues && \
    sudo docker build --tag ghcr.io/hirbod-codes/user_management:latest -f ./src/user_management/Dockerfile.production ./ && \
    sudo docker stack deploy -c ./docker-compose.swarm.sharded_mongodb.base.yml -c ./docker-compose.sharded_mongodb.production.yml app
```

## for dotnet application outside container

### for running the application

if you don't specify a .env file relative path (aka `ENV_FILE_PATH=.env.file dotnet run`) for running a project outside the container,
it will use the default ".env.mongodb.development" value as the env file path.

### for debugging the application

you can temporarily set the environment in Program.cs like

```C#
Environment.SetEnvironmentVariable("ENV_FILE_PATH", ".env.file");
```

TO DO:

Change environment values in .env file properly when running outside a docker container.

DB_OPTIONS__Host=localhost
DB_OPTIONS__Port=the_port_of_db_container
DB_OPTIONS__CertificateP12=security/user_management/app.p12

Copy security/ca/ca.crt to /etc/ssl/certs (so your mongodb c# driver can verify mongodb certificate) or set AllowInsecureTls to true.
