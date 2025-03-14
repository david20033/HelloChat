"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
let currentUserId = document.getElementById("FromId").value;
var friendIds = Array.from(document.querySelectorAll(".friend-id")).map(input => input.value);
let typingTimer;
const doneTypingInterval = 1000;
//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (messageId, message) {
    var messageEl = document.getElementById("TypingBalon");
    messageEl.remove();
    var p = document.createElement("p");
    p.classList.add("message-balon");
    p.classList.add("message-to");

    p.textContent = message;
    p.id = messageId;
    document.getElementById("messages").appendChild(p);

    var messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});
connection.on("SendMessage", function (messageId, message) {

    var p = document.createElement("p");
    p.classList.add("message-balon");
    p.classList.add("message-from");

    p.textContent = message;
    p.id = messageId;
    document.getElementById("messages").appendChild(p);

    var messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});
connection.on("ReceiveTyping", function () {
    if (document.getElementById("TypingBalon") != null) {
        return;
    }
    const chatContainer = document.getElementsByClassName("messages")[0];

    var p = document.createElement("p");
    p.classList.add("message-balon");
    p.classList.add("message-to");
    p.classList.add("typing");
    p.id = "TypingBalon";
    for (var i = 0; i < 3; i++) {
        var span = document.createElement("span");
        span.classList.add("dot");
        p.appendChild(span);
    }
    chatContainer.appendChild(p);
    messageEl.scrollTop = messagesDiv.scrollHeight;
});
connection.on("ReceiveStopTyping", function () {
    var messageEl = document.getElementById("TypingBalon");
    messageEl.classList.add("hidden");
    setTimeout(() => {
        messageEl.style.display = "none";
        messageEl.remove();
    }, 400);
    setTimeout(() => {
        const chatContainer = document.getElementsByClassName("messages");
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }, 100);
});
connection.on("OnlineUsers", function (OnlineFriendIds)
{
    OnlineFriendIds.forEach(function (id) {
        var userDiv = document.getElementById(id);
        userDiv.style.display = "inline";
    });
});

connection.on("FriendDisconnected", function (userId) {
    var userDiv = document.getElementById(userId);
        userDiv.style.display = "none";
});
connection.on("FriendConnected", function (userId) {
    var userDiv = document.getElementById(userId);
    userDiv.style.display = "inline";
});
connection.on("ReceiveSeen", function (messageId) {
    const elements = document.querySelectorAll(".seen-time");
    const lastElement = elements[elements.length - 1];
    if (lastElement) {
        lastElement.remove();
    }
    var message = document.getElementById(messageId);
    var small = document.createElement("small");
    small.classList.add("seen-time");
    var strong = document.createElement("strong");
    strong.innerText = "Read";
    small.appendChild(strong);
    const now = new Date();
    const hours = now.getHours().toString().padStart(2, '0');
    const minutes = now.getMinutes().toString().padStart(2, '0');
    const currentTime = `${hours}:${minutes}`;
    small.appendChild(document.createTextNode(" "+currentTime));
    message.appendChild(small);
    var messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
    connection.invoke("GetOnlineUsers", friendIds).catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});
document.querySelectorAll(".ConversationChanger").forEach(function (element) {
    element.addEventListener("click", function (event) {

        var userId = this.getAttribute("data-user-id");
        var conversationId = this.getAttribute("data-conversation-id");

        connection.invoke("SetCurrentUserConversation", currentUserId, conversationId)
            .then(function () {
                window.location.href = "/Home/Index?id=" + userId;
            })
            .catch(function (err) {
                console.error(err.toString());
            });
    });
});


document.getElementById("Content").addEventListener("input", function (event) {
    clearTimeout(typingTimer);
    var ToId = document.getElementById("ToId").value;
    var FromId = document.getElementById("FromId").value;
    connection.invoke("SendTyping", FromId, ToId).catch(function (err) {
        return console.error(err.toString());
    });
    typingTimer = setTimeout(function () {
        connection.invoke("SendStopTyping", FromId, ToId).catch(function (err) {
            return console.error(err.toString());
        });
    }, doneTypingInterval);
})
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

document.querySelectorAll(".message-row").forEach(function (element) {
    element.addEventListener("mouseenter", function () {
        const options = element.querySelectorAll(".message-options");
        if (options) {
            options[0].style.display = "block";
            options[1].style.display = "block";
        }
    });

    element.addEventListener("mouseleave", function () {
        const options = element.querySelectorAll(".message-options");
        if (options) {
            options[0].style.display = "none";
            options[1].style.display = "none";
        }
    });
});