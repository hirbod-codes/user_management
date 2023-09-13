# Instructions

## In Development Environment (Linux/WSL)

1. you need to have npm installed.
2. run `cd path-to-project-root-directory/ && sudo chmod +x ./docker-compose-up.sh`.
3. run `./docker-compose-up.sh --projectRootDirectory ./ --refreshDocker --useDevVariables --generateCerts`

## In Production Environment (Linux/WSL)

1. you need to have npm installed.
2. run `sudo docker swarm init`
3. run `cd path-to-project-root-directory/ && sudo chmod +x ./docker-compose-up.sh`.
4. run `./docker-compose-up.sh --projectRootDirectory ./ --refreshDocker --useDevVariables --generateCerts`
