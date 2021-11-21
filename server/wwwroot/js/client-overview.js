"use strict";

var clients = [];

var connection = new signalR.HubConnectionBuilder().withUrl("/main").build();

function renderClientList() {
    var clientList = document.getElementById("clientList");
    clientList.innerHTML = '';

    clients.forEach(client => {
        var li = document.createElement("li");

        document.getElementById("clientList").appendChild(li);

        li.textContent = `${client.id} was seen at ${client.lastSeen}`;
    });
}

connection.on("RegisterDevicePresence", (deviceId, timestamp) => {
    var knownClient = clients.find((client) => client.id === deviceId);
    if (knownClient) {
        knownClient.lastSeen = new Date(timestamp);
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
    clients = clients.filter((client) => client.id !== deviceId);

    renderClientList();
});

connection.start().then(function () {
    connection.invoke("RegisterAsMaster");
}).catch(function (err) {
    return console.error(err.toString());
});