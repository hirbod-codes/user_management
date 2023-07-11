const YAML = require('js-yaml');
const FS = require('fs');

let args = {};
let tmp = process.argv;
tmp.shift();
tmp.shift();

tmp.forEach((v, i) => {
    let keyValue = v.split('=');

    let key = keyValue[0];
    keyValue.shift();
    let value = keyValue.join("=");

    args[key] = value;
});

if (args.dbUsername == undefined || args.dbUsername == null || args.dbUsername == "" || args.dbAdminUsername == undefined || args.dbAdminUsername == null || args.dbAdminUsername == "" || args.dbPassword == undefined || args.dbPassword == null || args.dbPassword == "" || args.dbPort == undefined || args.dbPort == null || args.dbPort == "" || args.dbName == undefined || args.dbName == null || args.dbName == "" || args.ouput == undefined || args.ouput == null || args.ouput == "")
    throw new Error("Invalid arguments");

let yml = {};

yml.version = "3.8";

yml.networks = {
    user_management_mongodb: {
        driver: "bridge"
    },
    frontend: {
        driver: "bridge"
    },
};

yml.volumes = {
    user_management_mongos_data: {},
    user_management_mongos_config_data: {},
    user_management_configServer1: {},
    user_management_configServer1_config: {},
    user_management_configServer2: {},
    user_management_configServer2_config: {},
    user_management_configServer3: {},
    user_management_configServer3_config: {},
    user_management_shardServer1: {},
    user_management_shardServer1_config: {},
    user_management_shardServer2: {},
    user_management_shardServer2_config: {},
    user_management_shardServer3: {},
    user_management_shardServer3_config: {},
};

yml.services = {
    user_management: {
        container_name: "user_management",
        build: {
            context: ".",
            dockerfile: "Dockerfile.development"
        },
        volumes: [
            "./:/app:rw",
            "./security/ca/ca.pem:/security/ca.pem",
            "./security/user_management/app.p12:/security/app.p12"
        ],
        ports: ["5000:5000", "5001:5001"],
        environment: {
            ASPNETCORE_ENVIRONMENT: "Development",
            ENVIRONMENT: "Development",
            DOTNET_WATCH_RESTART_ON_RUDE_EDIT: true,
        },
        depends_on: ["user_management_mongodb"],
        networks: ["frontend", "user_management_mongodb"]
    },
    user_management_mongo_express: {
        container_name: "user_management_mongo_express",
        image: "mongo-express:0.54.0",
        restart: "always",
        ports: ["8081:8081"],
        depends_on: ["user_management_mongodb"],
        networks: ["user_management_mongodb"],
        environment: {
            ME_CONFIG_BASICAUTH_USERNAME: args.dbAdminUsername,
            ME_CONFIG_BASICAUTH_PASSWORD: args.dbPassword,
            ME_CONFIG_MONGODB_SERVER: "user_management_mongodb",
            ME_CONFIG_MONGODB_PORT: args.dbPort,
            ME_CONFIG_MONGODB_ENABLE_ADMIN: true,
            ME_CONFIG_SITE_SSL_ENABLED: true,
            ME_CONFIG_SITE_SSL_CRT_PATH: "/security/ca.pem",
            ME_CONFIG_SITE_SSL_CRT_PATH: "/security/app.pem"
        },
        volumes: [
            "./security/ca/ca.pem:/security/ca.pem",
            "./security/user_management_mongo_express/app.pem:/security/app.pem"
        ]
    },
    local_client: {
        container_name: "local_client",
        image: "mongo:4.4.18",
        networks: ["user_management_mongodb", "frontend"],
        volumes: [
            "./security/:/security/"
        ]
    },
    user_management_mongodb: {
        container_name: "user_management_mongodb",
        image: "mongo:4.4.18",
        command: `bash -c "/mongodb/mongos_server.sh --dbUsername ${args.dbUsername} --dbAdminUsername ${args.dbAdminUsername} --dbPassword ${args.dbPassword} --dbName ${args.dbName} --dbPort ${args.dbPort} --configReplSet user_management_configReplicaSet --configMember0 user_management_configServer1 --configMember1 user_management_configServer2 --configMember2 user_management_configServer3 --shardReplSet user_management_shardReplicaSet --shardMember0 user_management_shardServer1 --shardMember1 user_management_shardServer2 --shardMember2 user_management_shardServer3"`,
        ports: [`8082:${args.dbPort}`],
        networks: ["user_management_mongodb", "frontend"],
        volumes: [
            "user_management_mongos_data:/data/db",
            "user_management_mongos_config_data:/data/configdb",
            "./mongodb/mongos_server.sh:/mongodb/mongos_server.sh",
            "./security/ca/ca.pem:/security/ca.pem",
            "./security/user_management_mongodb/member.pem:/security/member.pem",
            "./security/user_management_mongodb/app.pem:/security/app.pem",
        ]
    },
    user_management_configServer1: {
        container_name: "user_management_configServer1",
        image: "mongo:4.4.18",
        command: `bash -c "/mongodb/config_server.sh --dbPort ${args.dbPort} --replSet user_management_configReplicaSet --member0 user_management_configServer1 --member1 user_management_configServer2 --member2 user_management_configServer3"`,
        networks: ["user_management_mongodb"],
        volumes: ["user_management_configServer1:/data/db",
            "user_management_configServer1_config:/data/configdb",
            "./mongodb/config_server.sh:/mongodb/config_server.sh",
            "./security/ca/ca.pem:/security/ca.pem",
            "./security/user_management_configServer1/member.pem:/security/member.pem",
            "./security/user_management_configServer1/app.pem:/security/app.pem",
            "./security/local_client/app.pem:/security/local.pem"
        ]
    },
    user_management_configServer2: {
        container_name: "user_management_configServer2",
        image: "mongo:4.4.18",
        command: `mongod --configsvr --replSet user_management_configReplicaSet --bind_ip "0.0.0.0" --port ${args.dbPort} --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile /security/app.pem --tlsClusterFile /security/member.pem --tlsCAFile /security/ca.pem --tlsClusterCAFile /security/ca.pem`,
        networks: ["user_management_mongodb"],
        volumes: ["user_management_configServer2:/data/db",
            "user_management_configServer2_config:/data/configdb",
            "./security/ca/ca.pem:/security/ca.pem",
            "./security/user_management_configServer2/member.pem:/security/member.pem",
            "./security/user_management_configServer2/app.pem:/security/app.pem"
        ]
    },
    user_management_configServer3: {
        container_name: "user_management_configServer3",
        image: "mongo:4.4.18",
        command: `mongod --configsvr --replSet user_management_configReplicaSet --bind_ip "0.0.0.0" --port ${args.dbPort} --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile /security/app.pem --tlsClusterFile /security/member.pem --tlsCAFile /security/ca.pem --tlsClusterCAFile /security/ca.pem`,
        networks: ["user_management_mongodb"],
        volumes: ["user_management_configServer3:/data/db",
            "user_management_configServer3_config:/data/configdb",
            "./security/ca/ca.pem:/security/ca.pem",
            "./security/user_management_configServer3/member.pem:/security/member.pem",
            "./security/user_management_configServer3/app.pem:/security/app.pem"
        ]
    },
    user_management_shardServer1: {
        container_name: "user_management_shardServer1",
        image: "mongo:4.4.18",
        command: `bash -c "/mongodb/shard_server.sh shardServer1 --dbPort ${args.dbPort} --replSet user_management_shardReplicaSet --member0 user_management_shardServer1 --member1 user_management_shardServer2 --member2 user_management_shardServer3"`,
        networks: ["user_management_mongodb"],
        volumes: ["user_management_shardServer1:/data/db",
            "user_management_shardServer1_config:/data/configdb",
            "./mongodb/shard_server.sh:/mongodb/shard_server.sh",
            "./security/ca/ca.pem:/security/ca.pem",
            "./security/user_management_shardServer1/member.pem:/security/member.pem",
            "./security/user_management_shardServer1/app.pem:/security/app.pem",
            "./security/local_client/app.pem:/security/local.pem"
        ]
    },
    user_management_shardServer2: {
        container_name: "user_management_shardServer2",
        image: "mongo:4.4.18",
        command: `mongod --shardsvr --replSet user_management_shardReplicaSet --port ${args.dbPort} --bind_ip "0.0.0.0" --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile /security/app.pem --tlsClusterFile /security/member.pem --tlsCAFile /security/ca.pem --tlsClusterCAFile /security/ca.pem`,
        networks: ["user_management_mongodb"],
        volumes: ["user_management_shardServer2:/data/db",
            "user_management_shardServer2:/data/configdb",
            "./security/ca/ca.pem:/security/ca.pem",
            "./security/user_management_shardServer2/member.pem:/security/member.pem",
            "./security/user_management_shardServer2/app.pem:/security/app.pem"
        ]
    },
    user_management_shardServer3: {
        container_name: "user_management_shardServer3",
        image: "mongo:4.4.18",
        command: `mongod --shardsvr --replSet user_management_shardReplicaSet --port ${args.dbPort} --bind_ip "0.0.0.0" --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile /security/app.pem --tlsClusterFile /security/member.pem --tlsCAFile /security/ca.pem --tlsClusterCAFile /security/ca.pem`,
        networks: ["user_management_mongodb"],
        volumes: ["user_management_shardServer3:/data/db",
            "user_management_shardServer3_config:/data/configdb",
            "./security/ca/ca.pem:/security/ca.pem",
            "./security/user_management_shardServer3/member.pem:/security/member.pem",
            "./security/user_management_shardServer3/app.pem:/security/app.pem"
        ]
    }
}


FS.writeFile(args.ouput, YAML.dump(yml, { indent: 4, flowLevel: -1, lineWidth: 50000 }), function (err) {
    if (err) throw err
    console.log('successful')
});