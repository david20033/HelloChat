"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
let currentUserId = document.getElementById("FromId").value;
var friendIds = Array.from(document.querySelectorAll(".friend-id")).map(input => input.value);
const messagesContainer = document.getElementById("messages");
let typingTimer;
const doneTypingInterval = 1000;
//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (messageId, message) {
    var messageEl = document.getElementById("TypingBalon");
    if (messageEl) {
        messageEl.remove();
    }
    var messageContainer = document.createElement("div");

    var div = document.createElement("div");
    div.classList.add("message-row")
    div.classList.add("to")
    var p = document.createElement("p");
    p.classList.add("message-balon");
    p.classList.add("message-to");

    p.textContent = message;
    p.id = messageId;
    const reactionContainer = createReactionContainer();
    const { deleteSvg, reactSvg } = createMessageOptionsIcons();
    div.appendChild(reactionContainer);
    div.appendChild(deleteSvg);
    div.appendChild(reactSvg);
    messageContainer.appendChild(p);
    messageContainer.style.order="0"
    div.appendChild(messageContainer);
    document.getElementById("messages").appendChild(div);
    var messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});
connection.on("SendMessage", function (messageId, message) {
    var messageContainer = document.createElement("div");

    var div = document.createElement("div");
    div.classList.add("message-row")
    div.classList.add("from")
    var p = document.createElement("p");
    p.classList.add("message-balon");
    p.classList.add("message-from");

    p.textContent = message;
    p.id = messageId;
    const reactionContainer = createReactionContainer();
    const { deleteSvg, reactSvg } = createMessageOptionsIcons();
    div.appendChild(reactionContainer);
    div.appendChild(deleteSvg);
    div.appendChild(reactSvg);
    messageContainer.appendChild(p);
    messageContainer.style.order = "1"
    div.appendChild(messageContainer);
    document.getElementById("messages").appendChild(div);

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

connection.on("ReceiveGlobalDeleteMessage", function (MessageId) {
    var message = document.getElementById(MessageId);
    message.classList.add("message-removed");
    message.innerText = "Message Removed";
    var parentDiv = message.parentElement;
    parentDiv.classList.add("no-click");
});
connection.on("ReceiveLocalDeleteMessage", function (MessageId) {
    var message = document.getElementById(MessageId);
    var parentDiv = message.parentElement;
    parentDiv.style.display = "none";
});
connection.on("ReceiveMessageReaction", function (MessageId, Reaction, UserId) {
    var messageBallon = document.getElementById(MessageId);
    var messageBallonParent = messageBallon.parentElement;
    //var ReactionContainer = document.createElement("div");
    //reactionContainer.classList.add("messageBallonParent");
    var messageReaction = messageBallonParent.querySelector(".message-reaction");
    if (!messageReaction) {
        var div = document.createElement("div");
        div.classList.add("message-reaction");
        if (messageBallon.classList.contains('message-to')) {
            div.classList.add("left");
        } else { 
            div.classList.add("right");
        }
        messageReaction = div;
    }


    var p = messageBallonParent.querySelector("[data-user-id='" + UserId + "']");


    if (!p) {
        p = document.createElement("p");
        p.setAttribute("data-user-id", UserId);
        messageReaction.appendChild(p);
    }

    switch (Reaction) {
        case "Love":
            p.innerText = "♥️";
            break;
        case "Like":
            p.innerText = "👍";
            break;
        case "Laugh":
            p.innerText = "😂";
            break;
        case "Smile":
            p.innerText = "😊";
            break;
        case "Angry":
            p.innerText = "😡";
            break;
        default:
            return;
    }

    messageBallon.appendChild(div);
});

connection.on("ReceiveActiveString", function (ActiveString) {
    var ActiveEl = document.getElementById("last-active");
    ActiveEl.innerText = ActiveString;
});
connection.on("ReceiveMessageNotification", function (CurrConversation) {
    const Conversation = document.querySelector(`[data-conversation-id='${CurrConversation}']`);
    Conversation.querySelector(".new-message").style.display = "block";
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
document.getElementById("messages").addEventListener("click", function (event) {
    const deleteBtn = event.target.closest(".message-options.deleteOwnMessage");

    if (deleteBtn) {
        const messageRow = deleteBtn.closest(".message-row");
        if (messageRow) {
            const messageBalon = messageRow.querySelector(".message-balon");
            if (messageBalon) {
                const MessageId = messageBalon.id;
                const ToId = document.getElementById("ToId").value;
                const FromId = document.getElementById("FromId").value;

                connection.invoke("SendGlobalDeleteMessage", FromId, ToId, MessageId).catch(function (err) {
                    console.error(err.toString());
                });
            }
        }
    }
});

document.querySelectorAll(".message-row").forEach(function (element) {
    const deleteBtn = element.querySelector(".message-options.deleteSenderMessage");
    if (deleteBtn) {
        deleteBtn.addEventListener("click", function () {
            const messageBalon = element.querySelector(".message-balon");
            if (messageBalon) {
                const MessageId = messageBalon.id;
                var FromId = document.getElementById("FromId").value;
                connection.invoke("SendLocalDeleteMessage", FromId, MessageId).catch(function (err) {
                    console.error(err.toString());
                });
            }
        });
    }
});

function UpdateActiveStatus() {
    const User = document.getElementById("FromId").value;
    connection.invoke("SendCurrentActiveStatus",User).catch(function (err) {
        console.error(err.toString());
    });
}
setInterval(UpdateActiveStatus, 1 * 60 * 1000);

//another js
window.addEventListener("load", function () {
    var messagesDiv = document.getElementById("messages");
    messagesDiv.scrollTop = messagesDiv.scrollHeight;
});


messagesContainer.addEventListener("mouseover", function (event) {
    const messageRow = event.target.closest(".message-row");
    if (messageRow && messagesContainer.contains(messageRow)) {
        const options = messageRow.querySelectorAll(".message-options");
        if (options.length > 0) {
            options.forEach(opt => opt.style.display = "block");
        }
    }
});

messagesContainer.addEventListener("mouseout", function (event) {
    const messageRow = event.target.closest(".message-row");
    if (messageRow && messagesContainer.contains(messageRow)) {
        const options = messageRow.querySelectorAll(".message-options");
        if (options.length > 0) {
            options.forEach(opt => opt.style.display = "none");
        }
    }
});

document.addEventListener('DOMContentLoaded', function () {
    document.getElementById("messages").addEventListener('click', function (e) {
        if (e.target.classList.contains('React')) {
            e.stopPropagation();

            document.querySelectorAll('.reaction-container').forEach(c => c.style.display = 'none');

            const messageRow = e.target.closest('.message-row');
            const reactionContainer = messageRow.querySelector('.reaction-container');
            if (reactionContainer) {
                reactionContainer.style.display = 'flex';
            }
        }
    });

    document.addEventListener('click', function (e) {
        if (!e.target.closest('.reaction-container') && !e.target.classList.contains('React')) {
            document.querySelectorAll('.reaction-container').forEach(c => c.style.display = 'none');
        }
    });

    document.getElementById("messages").addEventListener('click', function (e) {
        if (e.target.classList.contains('reaction-button')) {
            const reactionType = e.target.classList.contains('love-reaction') ? 'Love' :
                e.target.classList.contains('like-reaction') ? 'Like' :
                    e.target.classList.contains('laugh-reaction') ? 'Laugh' :
                        e.target.classList.contains('smile-reaction') ? 'Smile' :
                            e.target.classList.contains('angry-reaction') ? 'Angry' : null;

            if (reactionType) {
                const ToId = document.getElementById("ToId").value;
                const FromId = document.getElementById("FromId").value;
                const parent = e.target.closest(".message-row"); 
                const messageBalon = parent.querySelector(".message-balon");
                const messageId = messageBalon.id;

                connection.invoke("SendMessageReaction", messageId, reactionType, FromId, ToId)
                    .catch(function (err) {
                        console.error(err.toString());
                    });

                const reactionContainer = parent.querySelector('.reaction-container');
                if (reactionContainer) {
                    reactionContainer.style.display = 'none';
                }
            }
        }
    });
});

function createMessageOptionsIcons() {
    const deleteSvg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    deleteSvg.setAttribute("class", "message-options deleteOwnMessage size-6");
    deleteSvg.setAttribute("xmlns", "http://www.w3.org/2000/svg");
    deleteSvg.setAttribute("fill", "none");
    deleteSvg.setAttribute("viewBox", "0 0 24 24");
    deleteSvg.setAttribute("stroke-width", "1.5");
    deleteSvg.setAttribute("stroke", "currentColor");

    const deletePath = document.createElementNS("http://www.w3.org/2000/svg", "path");
    deletePath.setAttribute("stroke-linecap", "round");
    deletePath.setAttribute("stroke-linejoin", "round");
    deletePath.setAttribute("d", "m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0");
    deleteSvg.appendChild(deletePath);

    const reactSvg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    reactSvg.setAttribute("class", "message-options React size-6");
    reactSvg.setAttribute("xmlns", "http://www.w3.org/2000/svg");
    reactSvg.setAttribute("fill", "none");
    reactSvg.setAttribute("viewBox", "0 0 24 24");
    reactSvg.setAttribute("stroke-width", "1.5");
    reactSvg.setAttribute("stroke", "currentColor");

    const reactPath = document.createElementNS("http://www.w3.org/2000/svg", "path");
    reactPath.setAttribute("stroke-linecap", "round");
    reactPath.setAttribute("stroke-linejoin", "round");
    reactPath.setAttribute("d", "M12 6.75a.75.75 0 1 1 0-1.5.75.75 0 0 1 0 1.5ZM12 12.75a.75.75 0 1 1 0-1.5.75.75 0 0 1 0 1.5ZM12 18.75a.75.75 0 1 1 0-1.5.75.75 0 0 1 0 1.5Z");
    reactSvg.appendChild(reactPath);

    return { deleteSvg, reactSvg };
}

function createReactionContainer() {
    const container = document.createElement('div');
    container.classList.add('reaction-container');
    const reactions = [
        { class: 'love-reaction', emoji: '♥️' },
        { class: 'like-reaction', emoji: '👍' },
        { class: 'laugh-reaction', emoji: '😂' },
        { class: 'smile-reaction', emoji: '😊' },
        { class: 'angry-reaction', emoji: '😡' }
    ];

    reactions.forEach(reaction => {
        const reactionButton = document.createElement('p');
        reactionButton.classList.add('reaction-button', reaction.class);
        reactionButton.textContent = reaction.emoji;
        container.appendChild(reactionButton);
    });

    return container;
}

