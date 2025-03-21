"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
const currentUserId = document.getElementById("FromId")?.value;
const friendIds = Array.from(document.querySelectorAll(".friend-id")).map(input => input.value);
var messagesContainer;
let typingTimer;
const doneTypingInterval = 1000;

connection.on("ReceiveMessage", handleReceiveMessage);
connection.on("SendMessage", handleSendMessage);
connection.on("ReceiveTyping", showTypingIndicator);
connection.on("ReceiveStopTyping", hideTypingIndicator);
connection.on("OnlineUsers", showOnlineFriends);
connection.on("FriendDisconnected", id => toggleFriendOnlineStatus(id, false));
connection.on("FriendConnected", id => toggleFriendOnlineStatus(id, true));
connection.on("ReceiveSeen", markMessageAsSeen);
connection.on("ReceiveGlobalDeleteMessage", handleGlobalDeleteMessage);
connection.on("ReceiveLocalDeleteMessage", handleLocalDeleteMessage);
connection.on("ReceiveMessageReaction", handleMessageReaction);
connection.on("ReceiveActiveString", updateLastActive);
connection.on("ReceiveMessageNotification", showNewMessageNotification);

connection.start()
    .then(() => connection.invoke("GetOnlineUsers", friendIds).catch(console.error))
    .catch(console.error);

document.addEventListener("DOMContentLoaded", () => {
    setupConversationSwitching();
    setupMessageHoverEvents();
    setupReactionUIEvents();
    setupCommonEvents();
    messagesContainer?.scrollTo(0, messagesContainer.scrollHeight);
});

function setupCommonEvents() {
    messagesContainer = document.getElementById("messages");
    const sendButton = document.getElementById("sendButton");
    const contentInput = document.getElementById("Content");

    if (sendButton) sendButton.addEventListener("click", sendMessage);
    if (contentInput) contentInput.addEventListener("input", handleTyping);

    document.querySelectorAll(".message-row").forEach(setupSenderMessageDelete);

    if (messagesContainer) {
        messagesContainer.addEventListener("click", handleOwnMessageDelete);
    }
}

function setupConversationSwitching() {
    document.querySelectorAll(".ConversationChanger").forEach(el => {
        el.addEventListener("click", () => {
            const conversationId = el.getAttribute("data-conversation-id");
            connection.invoke("SetCurrentUserConversation", currentUserId, conversationId)
                .then(() => renderPartialConversation(conversationId))
                .catch(console.error);
        });
    });
}

function renderPartialConversation(conversationId) {
    fetch(`/Home/LoadConversation?conversationId=${conversationId}`)
        .then(res => res.text())
        .then(html => {
            const container = document.getElementById("chat-container");
            const el = document.querySelector(`[data-conversation-id='${conversationId}'] .new-message`);
            if (el) {
                el.style.display = "none";
            }
            container.innerHTML = html;
            setupCommonEvents();
            setupMessageHoverEvents();
            setupReactionUIEvents();
            scrollToBottom();
        })
        .catch(console.error);
}

function scrollToBottom() {
    messagesContainer?.scrollTo(0, messagesContainer.scrollHeight);
}

function updateActiveStatus() {
    const fromId = document.getElementById("FromId")?.value;
    if (fromId) {
        connection.invoke("SendCurrentActiveStatus", fromId).catch(console.error);
    }
}

setInterval(updateActiveStatus, 60000);

function showTypingIndicator() {
    if (document.getElementById("TypingBalon")) return;
    const div = document.createElement("div");
    div.id = "TypingBalon";
    div.className = "message received typing left";
    for (let i = 0; i < 3; i++) {
        const span = document.createElement("span");
        span.className = "dot";
        div.appendChild(span);
    }
    messagesContainer.appendChild(div);
    scrollToBottom();
}

function hideTypingIndicator() {
    const el = document.getElementById("TypingBalon");
    if (!el) return;
    el.classList.add("hidden");
    setTimeout(() => el.remove(), 400);
    setTimeout(scrollToBottom, 100);
}

function removeTypingIndicator() {
    const el = document.getElementById("TypingBalon");
    if (el) el.remove();
}

function sendMessage(event) {
    event.preventDefault();
    const content = document.getElementById("Content")?.value;
    const toId = document.getElementById("ToId")?.value;
    const fromId = document.getElementById("FromId")?.value;
    if (content && toId && fromId) {
        connection.invoke("SendMessage", fromId, toId, content).catch(console.error);
    }
}

function handleReceiveMessage(messageId, message) {
    removeTypingIndicator();
    const msgRow = createMessageRow("received", messageId, message, "deleteSenderMessage", "3");
    messagesContainer.appendChild(msgRow);
    scrollToBottom();
}

function handleSendMessage(messageId, message) {
    const msgRow = createMessageRow("sent", messageId, message, "deleteOwnMessage", "1");
    messagesContainer.appendChild(msgRow);
    scrollToBottom();
}

function createMessageRow(type, messageId, text, deleteClass, order) {
    const row = document.createElement("div");
    row.className = `message-row ${type === 'sent' ? 'from right' : 'to left'}`;

    const msgDiv = document.createElement("div");
    msgDiv.className = `message ${type}`;
    msgDiv.id = messageId;
    msgDiv.textContent = text;
    
    const reactionContainer = createReactionContainer();
    const { divDelete, divReact } = createMessageOptionsIcons(deleteClass, order);

    row.appendChild(reactionContainer);
    row.append(msgDiv, divDelete, divReact);
    return row;
}

function createReactionContainer() {
    const container = document.createElement("div");
    container.className = "reaction-container";
    const reactions = ["Love", "Like", "Laugh", "Smile", "Angry"];
    const emojis = ["♥️", "👍", "😂", "😊", "😡"];
    reactions.forEach((r, i) => {
        const p = document.createElement("p");
        p.className = `reaction-button ${r.toLowerCase()}-reaction`;
        p.textContent = emojis[i];
        container.appendChild(p);
    });
    return container;
}

function createMessageOptionsIcons(deleteClass, order) {
    const del = document.createElement("div");
    del.className = `message-options ${deleteClass}`;
    del.style.order = order;
    del.innerText = "🗑️";

    const react = document.createElement("div");
    react.className = "message-options React";
    react.style.order = order;
    react.innerText = "😃";

    return { divDelete: del, divReact: react };
}

function handleMessageReaction(messageId, reaction, userId) {
    const msg = document.getElementById(messageId);
    const parent = msg;
    if (!msg || !parent) return;

    let container = parent.querySelector(".message-reaction");
    if (!container) {
        container = document.createElement("div");
        container.className = `message-reaction ${msg.classList.contains("received") ? "left" : "right"}`;
        parent.appendChild(container);
    }

    let p = container.querySelector(`[data-user-id="${userId}"]`);
    if (!p) {
        p = document.createElement("p");
        p.setAttribute("data-user-id", userId);
        container.appendChild(p);
    }

    const emojiMap = {
        Love: "♥️", Like: "👍", Laugh: "😂", Smile: "😊", Angry: "😡"
    };
    if (emojiMap[reaction]) p.innerText = emojiMap[reaction];
}

function handleOwnMessageDelete(event) {
    const deleteBtn = event.target.closest(".deleteOwnMessage");
    if (!deleteBtn) return;
    const msg = deleteBtn.closest(".message-row")?.querySelector(".message");
    if (msg) {
        const msgId = msg.id;
        const toId = document.getElementById("ToId").value;
        const fromId = document.getElementById("FromId").value;
        connection.invoke("SendGlobalDeleteMessage", fromId, toId, msgId).catch(console.error);
    }
}

function setupSenderMessageDelete(row) {
    const btn = row.querySelector(".deleteSenderMessage");
    if (btn) {
        btn.addEventListener("click", () => {
            const msg = row.querySelector(".message");
            if (msg) {
                const msgId = msg.id;
                const fromId = document.getElementById("FromId").value;
                connection.invoke("SendLocalDeleteMessage", fromId, msgId).catch(console.error);
            }
        });
    }
}

function handleGlobalDeleteMessage(messageId) {
    const msg = document.getElementById(messageId);
    if (msg) {
        msg.classList.add("message-removed");
        msg.innerText = "Message Removed";
        msg.parentElement.classList.add("no-click");
    }
}

function handleLocalDeleteMessage(messageId) {
    const msg = document.getElementById(messageId);
    if (msg) msg.parentElement.style.display = "none";
}

function markMessageAsSeen(messageId) {
    document.querySelectorAll(".read-status-message").forEach(el => el.remove());
    const message = document.getElementById(messageId);
    const small = document.createElement("small");
    small.className = "read-status-message";
    const strong = document.createElement("strong");
    strong.innerText = "Read";
    const now = new Date();
    const time = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    small.append(strong, ` ${time}`);
    message.parentElement.appendChild(small);
    scrollToBottom();
}

function updateLastActive(status) {
    const el = document.getElementById("last-active");
    if (el) el.innerText = status;
}

function showOnlineFriends(ids) {
    ids.forEach(id => toggleFriendOnlineStatus(id, true));
}

function toggleFriendOnlineStatus(id, isOnline) {
    const el = document.getElementById(id);
    if (el) el.style.display = isOnline ? "inline" : "none";
}

function showNewMessageNotification(conversationId) {
    const el = document.querySelector(`[data-conversation-id='${conversationId}'] .new-message`);
    if (el) el.style.display = "block";
}

function handleTyping() {
    clearTimeout(typingTimer);
    const toId = document.getElementById("ToId").value;
    const fromId = document.getElementById("FromId").value;
    connection.invoke("SendTyping", fromId, toId).catch(console.error);

    typingTimer = setTimeout(() => {
        connection.invoke("SendStopTyping", fromId, toId).catch(console.error);
    }, doneTypingInterval);
}

function setupMessageHoverEvents() {
    messagesContainer?.addEventListener("mouseover", e => {
        const row = e.target.closest(".message-row");
        row?.querySelectorAll(".message-options").forEach(opt => opt.style.display = "block");
    });

    messagesContainer?.addEventListener("mouseout", e => {
        const row = e.target.closest(".message-row");
        row?.querySelectorAll(".message-options").forEach(opt => opt.style.display = "none");
    });
}

function setupReactionUIEvents() {
    const messages = document.getElementById("messages");

    messages?.addEventListener("click", e => {
        if (e.target.classList.contains("React")) {
            e.stopPropagation();
            document.querySelectorAll(".reaction-container").forEach(c => c.style.display = "none");
            const container = e.target.closest(".message-row")?.querySelector(".reaction-container");
            if (container) container.style.display = "flex";
        }

        if (e.target.classList.contains("reaction-button")) {
            const reaction = e.target.className.match(/(love|like|laugh|smile|angry)/)?.[0];
            const reactionType = reaction?.charAt(0).toUpperCase() + reaction?.slice(1);
            const toId = document.getElementById("ToId").value;
            const fromId = document.getElementById("FromId").value;
            const row = e.target.closest(".message-row");
            const msgId = row?.querySelector(".message")?.id;

            if (reactionType && msgId) {
                connection.invoke("SendMessageReaction", msgId, reactionType, fromId, toId).catch(console.error);
                row.querySelector(".reaction-container").style.display = "none";
            }
        }
    });

    document.addEventListener("click", e => {
        if (!e.target.closest(".reaction-container") && !e.target.classList.contains("React")) {
            document.querySelectorAll(".reaction-container").forEach(c => c.style.display = "none");
        }
    });
}
