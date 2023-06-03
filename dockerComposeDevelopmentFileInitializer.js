const YAML = require('js-yaml');
const FS = require('fs');

let args = {};
let tmp = process.argv;
tmp.shift();
tmp.shift();
tmp.forEach((v, i) => {
    let keyValue = v.split('=');
    if (keyValue.length !== 2) throw new Error('invalid arguments for dockerComposeDevelopmentFileInitializer.js file')
    args[keyValue[0]] = keyValue[1];
});

let raw = FS.readFileSync(args.ymlFile);
let yml = YAML.load(raw);

yml.services['user_management_mongo_express'].environment.ME_CONFIG_BASICAUTH_USERNAME = args.username;
yml.services['user_management_mongo_express'].environment.ME_CONFIG_BASICAUTH_PASSWORD = args.password;
yml.services['user_management_mongo_express'].environment.ME_CONFIG_MONGODB_ADMINUSERNAME = args.username;
yml.services['user_management_mongo_express'].environment.ME_CONFIG_MONGODB_ADMINPASSWORD = args.rootPassword;
yml.services['user_management_mongo_express'].environment.ME_CONFIG_MONGODB_SERVER = args.service;
yml.services['user_management_mongo_express'].environment.ME_CONFIG_MONGODB_AUTH_DATABASE = args.db;

yml.services.user_management_mongodb.ports = ["8082:" + args.port];
yml.services.user_management_mongodb.command = " mongos --configdb user_management_configReplicaSet1/user_management_configServer1:" + args.port + ",user_management_configServer2:" + args.port + ",user_management_configServer3:" + args.port + " --bind_ip 0.0.0.0 --port " + args.port;

yml.services.user_management_configServer1.command = "mongod --configsvr --replSet user_management_configReplicaSet1 --port " + args.port + " --dbpath /data/db";
yml.services.user_management_configServer2.command = "mongod --configsvr --replSet user_management_configReplicaSet1 --port " + args.port + " --dbpath /data/db";
yml.services.user_management_configServer3.command = "mongod --configsvr --replSet user_management_configReplicaSet1 --port " + args.port + " --dbpath /data/db";

yml.services.user_management_shardServer1.command = "mongod --shardsvr --replSet user_management_shardReplicaSet1 --port " + args.port + " --dbpath /data/db";
yml.services.user_management_shardServer2.command = "mongod --shardsvr --replSet user_management_shardReplicaSet1 --port " + args.port + " --dbpath /data/db";
yml.services.user_management_shardServer3.command = "mongod --shardsvr --replSet user_management_shardReplicaSet1 --port " + args.port + " --dbpath /data/db";

FS.writeFile(args.ouput, YAML.dump(yml), function (err) {
    if (err) throw err
    console.log('successful')
});