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

if (args.user_management_http_port == undefined || args.user_management_http_port == null || args.user_management_http_port == "" || args.user_management_https_port == undefined || args.user_management_https_port == null || args.user_management_https_port == "" || args.user_management_mongo_express_port == undefined || args.user_management_mongo_express_port == null || args.user_management_mongo_express_port == "" || args.dbPort == undefined || args.dbPort == null || args.dbPort == "" || args.ouput == undefined || args.ouput == null || args.ouput == "")
    throw new Error("Invalid arguments");

let yml = {};

yml.version = "3.8";

yml.networks = {
    user_management_mongodb: {
        driver: "overlay"
    },
    frontend: {
        driver: "overlay"
    }
};

yml.volumes = {
    data: {},
    config_data: {},
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

let secrets = [
    "user-management-db-name",
    "user-management-db-admin-username",
    "user-management-db-password",
    "user-management-db-root-password"
];

yml.secrets = {};

secrets.map(s => {
    yml.secrets[s] = { external: true };
    return s;
});

// Mounted volumes don't support windows!!
yml.services = {
    user_management: {
        image: "ghcr.io/hirbod-codes/user_management:latest",
        ports: [`${args.user_management_http_port}:5000`, `${args.user_management_https_port}:5001`],
        secrets: ["user-management-db-name", "jwt-secret-key"],
        environment: {
            ENVIRONMENT: "Production",
            ASPNETCORE_ENVIRONMENT: "Production",
            "MongoDB::host": "user_management_mongodb",
            "MongoDB::port": `${args.dbPort}`,
            "MongoDB::DatabaseName": "/run/secrets/user-management-db-name",
            "MongoDB::username": `${args.dbUsername}`,
            "Jwt::SecretKey": "/run/secrets/jwt-secret-key",
        },
        networks: ["user_management_mongodb", "frontend"]
    },
    user_management_mongodb: {
        image: "ghcr.io/hirbod-codes/user_management_mongodb:latest",
        networks: ["user_management_mongodb", "frontend"],
        secrets: ["user-management-db-admin-username", "user-management-db-password", "user-management-db-name"],
        ports: ["8081:27017"],
        environment: {
            configReplSet: "user_management_configReplicaSet",
            configMember0: "user_management_configServer1",
            configMember1: "user_management_configServer2",
            configMember2: "user_management_configServer3",
            shardReplSet: "user_management_shardReplicaSet",
            shardMember0: "user_management_shardServer1",
            shardMember1: "user_management_shardServer2",
            shardMember2: "user_management_shardServer3",
            dbPort: `${args.dbPort}`,
            dbAdminUsername: "/run/secrets/user-management-db-admin-username",
            dbPassword: "/run/secrets/user-management-db-password",
            dbName: "/run/secrets/user-management-db-name"
        },
        volumes: [
            "user_management_mongos_data:/data/db",
            "user_management_mongos_config_data:/data/configdb"
        ]
    },
    user_management_configServer1: {
        image: "ghcr.io/hirbod-codes/user_management_config_server1:latest",
        networks: ["user_management_mongodb"],
        environment: {
            replSet: "user_management_configReplicaSet",
            dbPort: `${args.dbPort}`,
            member0: "user_management_configServer1",
            member1: "user_management_configServer2",
            member2: "user_management_configServer3"
        },
        volumes: [
            "user_management_configServer1:/data/db",
            "user_management_configServer1_config:/data/configdb"
        ]
    },
    user_management_configServer2: {
        image: "ghcr.io/hirbod-codes/user_management_config_server2:latest",
        command: `mongod --configsvr --replSet user_management_configReplicaSet --bind_ip \"0.0.0.0\" --port ${args.dbPort} --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile /security/app.pem --tlsClusterFile /security/member.pem --tlsCAFile /security/ca.pem --tlsClusterCAFile /security/ca.pem`,
        networks: ["user_management_mongodb"],
        volumes: [
            "user_management_configServer2:/data/db",
            "user_management_configServer2_config:/data/configdb"
        ]
    },
    user_management_configServer3: {
        image: "ghcr.io/hirbod-codes/user_management_config_server3:latest",
        command: `mongod --configsvr --replSet user_management_configReplicaSet --bind_ip \"0.0.0.0\" --port ${args.dbPort} --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile /security/app.pem --tlsClusterFile /security/member.pem --tlsCAFile /security/ca.pem --tlsClusterCAFile /security/ca.pem`,
        networks: ["user_management_mongodb"],
        volumes: [
            "user_management_configServer3:/data/db",
            "user_management_configServer3_config:/data/configdb"
        ]
    },
    user_management_shardServer1: {
        image: "ghcr.io/hirbod-codes/user_management_shard_server1:latest",
        networks: ["user_management_mongodb"],
        environment: {
            replSet: "user_management_shardReplicaSet",
            dbPort: `${args.dbPort}`,
            member0: "user_management_shardServer1",
            member1: "user_management_shardServer2",
            member2: "user_management_shardServer3"
        },
        volumes: [
            "user_management_shardServer1:/data/db",
            "user_management_shardServer1_config:/data/configdb"
        ]
    },
    user_management_shardServer2: {
        image: "ghcr.io/hirbod-codes/user_management_shard_server2:latest",
        command: `mongod --shardsvr --replSet user_management_shardReplicaSet --port ${args.dbPort} --bind_ip "0.0.0.0" --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile /security/app.pem --tlsClusterFile /security/member.pem --tlsCAFile /security/ca.pem --tlsClusterCAFile /security/ca.pem`,
        environment: {
            replSet: "user_management_shardReplicaSet",
            dbPort: `${args.dbPort}`
        },
        networks: ["user_management_mongodb"],
        volumes: [
            "user_management_shardServer2:/data/db",
            "user_management_shardServer2:/data/configdb"
        ]
    },
    user_management_shardServer3: {
        image: "ghcr.io/hirbod-codes/user_management_shard_server3:latest",
        command: `mongod --shardsvr --replSet user_management_shardReplicaSet --port ${args.dbPort} --bind_ip "0.0.0.0" --dbpath /data/db --tlsMode requireTLS --clusterAuthMode x509 --tlsCertificateKeyFile /security/app.pem --tlsClusterFile /security/member.pem --tlsCAFile /security/ca.pem --tlsClusterCAFile /security/ca.pem`,
        environment: {
            replSet: "user_management_shardReplicaSet",
            dbPort: `${args.dbPort}`
        },
        networks: ["user_management_mongodb"],
        volumes: [
            "user_management_shardServer3:/data/db",
            "user_management_shardServer3:/data/configdb"
        ]
    }
}

FS.writeFile(args.ouput, YAML.dump(yml, { indent: 4, flowLevel: -1, lineWidth: 50000 }), function (err) {
    if (err) throw err
    console.log('successful')
});