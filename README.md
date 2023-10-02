# Instructions

## In Development Environment (Linux/WSL)

### for single mongodb container as the database run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory ./ && \
    sudo docker compose -f ./docker-compose.mongodb.development.yml --env-file ./.env.mongodb.development up -d --build --remove-orphans -V
```

### for sharded mongodb cluster as the database run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory ./ && \
    ./generate_certificates.sh --projectRootDirectory ./ && \
    sudo docker compose -f ./docker-compose.sharded_mongodb.development.yml --env-file ./.env.sharded_mongodb.development up -d --build --remove-orphans -V
```

## In IntegrationTest Environment (Linux/WSL)

### for single mongodb container as the database run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory ./ && \
    sudo docker compose -f ./docker-compose.mongodb.integration_test.yml --env-file ./.env.mongodb.integration_test up --build --remove-orphans -V --exit-code-from user_management
```

### for sharded mongodb cluster as the database run

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory ./ && \
    ./generate_certificates.sh --projectRootDirectory ./ && \
    sudo docker compose -f ./docker-compose.sharded_mongodb.integration_test.yml --env-file ./.env.sharded_mongodb.integration_test up --build --remove-orphans -V --exit-code-from user_management
```

## In UnitTest Environment (Linux/WSL)

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory ./ && \
    sudo docker compose -f ./docker-compose.unit_test.yml --env-file ./.env.unit_test up --build --remove-orphans -V --exit-code-from user_management
```

## for docker compose only with sharded mongodb cluster (so you can run the dotnet application outside docker container)

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory ./ && \
    ./generate_certificates.sh --projectRootDirectory ./ && \
    sudo docker compose -f ./docker-compose.sharded_mongodb.yml --env-file ./.env.sharded_mongodb up -d --build --remove-orphans -V
```

## for docker compose only with single mongodb container (so you can run the dotnet application outside docker container)

```bash
cd path-to-project-root-directory/ && \
    sudo chmod ug+x ./*.sh && \
    ./prepare_environment_variables.sh --projectRootDirectory ./ && \
    sudo docker compose -f ./docker-compose.mongodb.yml --env-file ./.env.mongodb up -d --build --remove-orphans -V
```

## Caveat

if you don't specify a .env file relative path (aka `ENV_FILE_PATH=.env.file dotnet run`) for running a project outside the container,
it will use the default ".env.mongodb.development" value as the env file path.

also remember to change environment values in .env file properly when running outside a docker container.

DB_OPTIONS__Host=localhost
DB_OPTIONS__Port=the_port_of_db_container
(and if it is sharded mongodb cluster)
DB_OPTIONS__CaPem=security/ca/ca.pem
DB_OPTIONS__CertificateP12=security/user_management/app.p12
