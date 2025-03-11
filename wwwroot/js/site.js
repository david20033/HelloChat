"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
let currentUserId = document.getElementById("FromId").value;
//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (FromId, ToId, message) {
    var currentUserId = document.getElementById("FromId").value;

    var p = document.createElement("p");
    p.classList.add("message-balon");

    if (FromId === currentUserId) {
        p.classList.add("message-from"); 
    } else {
        p.classList.add("message-to"); 
    }

    p.textContent = message;
    document.getElementById("messages").appendChild(p);

    var messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var message = document.getElementById("Content").value;
    var ToId = document.getElementById("ToId").value;
    var FromId = document.getElementById("FromId").value;
    connection.invoke("SendMessage", FromId, ToId, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
//another js
window.addEventListener("load", function () {
    var messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});