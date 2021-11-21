"use strict";

var clients = [];

var connection = new signalR.HubConnectionBuilder().withUrl("/main").build();

const ClientStates = {
    online: "online",
    offline: "offline"
};

function renderClientList() {
    var clientList = document.getElementById("clientList");
    clientList.innerHTML = '';

    clients.forEach(client => {
        var li = document.createElement("li");

        document.getElementById("clientList").appendChild(li);

        li.textContent = `${client.id} was seen at ${client.lastSeen}`;

        li.setAttribute("class", client.status);
    });
}

connection.on("RegisterDevicePresence", (deviceId, timestamp) => {
    var knownClient = clients.find((client) => client.id === deviceId);
    if (knownClient) {
        knownClient.lastSeen = new Date(timestamp);
        knownClient.status = ClientStates.online;
    } else {
        var newClient = {
            id: deviceId,
            lastSeen: new Date(timestamp)
        };

        clients.push(newClient);
    }

    renderClientList();
});

connection.on("RegisterDeviceOffline", (deviceId) => {
    var knownClient = clients.find((client) => client.id === deviceId);
    if (knownClient === null || knownClient === undefined) {
        return;
    }

    knownClient.status = ClientStates.offline;

    renderClientList();
});

connection.start().then(function () {
    connection.invoke("RegisterAsMaster");
}).catch(function (err) {
    return console.error(err.toString());
});