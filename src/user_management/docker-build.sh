sudo docker build --tag ghcr.io/hirbod-codes/user_management:latest -f ./Dockerfile.production .
sleep 1s
sudo docker build --tag ghcr.io/hirbod-codes/user_management:latest -f ./Dockerfile.developement .
sleep 1s

sudo docker build --tag ghcr.io/hirbod-codes/user_management_mongodb:latest -f ./Dockerfile.mongos.production .
sleep 1s


sudo docker build --tag ghcr.io/hirbod-codes/user_management_config_server1:latest -f ./Dockerfile.config.production .
sleep 1s
sudo docker build --tag ghcr.io/hirbod-codes/user_management_config_server2:latest -f ./Dockerfile.config2.production .
sleep 1s
sudo docker build --tag ghcr.io/hirbod-codes/user_management_config_server3:latest -f ./Dockerfile.config3.production .
sleep 1s

sudo docker build --tag ghcr.io/hirbod-codes/user_management_shard_server1:latest -f ./Dockerfile.shard.production .
sleep 1s
sudo docker build --tag ghcr.io/hirbod-codes/user_management_shard_server2:latest -f ./Dockerfile.shard2.production .
sleep 1s
sudo docker build --tag ghcr.io/hirbod-codes/user_management_shard_server3:latest -f ./Dockerfile.shard3.production .
sleep 1s
