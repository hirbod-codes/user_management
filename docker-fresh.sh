sudo docker system prune --volumes -f
sudo docker system prune -af
sudo docker rm -f $(sudo docker ps -aq)
sudo docker volume rm $(sudo docker volume ls -q)
sudo docker network rm $(sudo docker network ls -q)
sudo docker rmi -f $(sudo docker image ls -a)
sudo docker system df
