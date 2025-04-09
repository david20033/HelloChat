# 📱 Hello Chat – ASP.NET MVC Real-Time Messaging Application

## 🧩 Overview

**Hello Chat** is a real-time messaging web application built with **ASP.NET MVC** and **SignalR**, allowing users to chat with friends through text or image messages. It includes a fully functional friendship system, user notifications, and interactive features like message reactions, online status, and typing indicators.

---

## 🚀 Features

- 🔐 **Authentication & Authorization**
- 👥 **Friendship System** (Add, Accept, Remove, Cancel Requests)
- 💬 **Real-Time Chat** using SignalR
- 🖼️ **Text & Image Messaging**
- 😊 **Message Reactions** (Love, Like, Laugh, Smile, Angry)
- 🔄 **Global Delete (own message)** and **Local Delete (for other user)**
- 👀 **Read Receipts & Typing Indicators**
- 🔔 **Notification System** for Friend Requests
- 🧭 **Navigation & User Search**
- ✏️ **User Profile Management**
- 🟢 **Online Status Indication**

---

## 📦 Prerequisites

Make sure you have the following installed:

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

---

## ⚙️ Setup Instructions

1. **Clone the repository**  
   ```bash
   git clone https://github.com/david20033/HelloChat
   cd HelloChat
   ```

2. **Restore dependencies**  
   ```bash
   dotnet restore
   ```

3. **Set up the database**  
   - Create a SQL Server database named `MyCinemaDB`.
   - Update the connection string in `appsettings.Development.json` if needed.

4. **Apply migrations**  
   ```bash
   dotnet ef database update
   ```

5. *(Optional)* **Seed the database**  
   - In `Program.cs`, uncomment the seed method.
   - Seeding will create 5 users:
     ```
     Emails:
     - daviddimitrov123@mail.com
     - vanko10@mail.com
     - grigor123@mail.com
     - sisito@mail.com
     - koki@mail.com
     Password: 123456a
     ```
   - It will also populate sample conversations.

---

## 🧭 Navigation

- **Home** → Main friends/chat page  
- **Search Users** → User profiles and actions based on friendship status  
- **Notifications** → Friend request sent/received info  
- **Profile** → View and edit your profile  
- **Logout** → Sign out of the application  

---

## 🏠 Home Page

Displays:

- All friends with:
  - Name, profile picture
  - Last message preview
  - Unseen message indicator
  - Online status (green dot)
- Clicking a friend opens the chat conversation

---

## 💬 Chat UI

- **Message Display:**
  - Loads last 10 messages
  - Scroll to top loads 10 more messages
- **Message Types:**
  - Text
  - Image
- **Reactions:**
  - ❤️ Love
  - 👍 Like
  - 😂 Laugh
  - 🙂 Smile
  - 😠 Angry
- **Message Options:**
  - Global delete (your own message)
  - Local delete (message for yourself)
- **Indicators:**
  - Seen (last read message)
  - Typing status
- **Input Section:**
  - Textbox to write messages
  - Button to attach images
  - Button to send

---

## 👤 Conversation Header

- Profile image
- First & last name
- Online status (text)
- Burger menu → Info panel with:
  - User info
  - All image messages in conversation

---

## 🔍 User Search

- Typing in nav bar triggers search container with matching results
- Clicking a result redirects to user profile page

---

## 👥 User Profile Page

- Displays profile image, full name
- Action buttons vary by friendship status:
  - Add Friend
  - Accept Request
  - Cancel Request
  - Remove Friend
  - Edit Profile *(if it's the current user)*

---

## 📝 Edit Profile

- Editable fields:
  - First Name
  - Last Name
  - Email
  - Phone Number
  - Profile Image

---

## 🔔 Notifications

- Shows friend requests:
  - Sent
  - Received & accepted

---

## 📁 Repository

GitHub: [https://github.com/david20033/HelloChat](https://github.com/david20033/HelloChat)
