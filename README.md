# GOA - An Online Board Game Made By Unity
This project contains both the Client and Server sides. When you start the game, you need to choose either to start as a Host or to connect to the Host as a Client. You can specify your IP address in the input field.
![IP](https://user-images.githubusercontent.com/18097644/226150126-d36853f2-1225-4d4c-bc2d-673ae7e61ff1.PNG)

# How To Play
## Lobby
After that, you will connect to a room, where you can wait for 3 ~ 5 players to join, or simply add more AI bots. Note that the AI bot used is very simple, it may not provide a high degree of user experience. Only the host player can start the game.
![Lobby](https://user-images.githubusercontent.com/18097644/226150129-fec11559-64f9-4276-aa1d-6b6ed5c7315e.PNG)

## GamePlay
You have to choose your character at the start of the game. Then the players should take their actions by order. For more detail, you can see the rule books below. The game will continue till a player win the game, or a player loses a game then the rest of the players play the last round to decide their ranks.
![GamePlay](https://user-images.githubusercontent.com/18097644/226150579-179d53ff-2b6c-418a-ad95-043c91bf321f.PNG)

# Technique Detail
## Networking
This project uses TCP/IP Socket to implement Client and Backend communication. The backend API subscript to the listener so that they will get noticed when received a message from the client, and will process the order one by one from the request pool. The Client side uses a similar logic, too. The models used by communication and processing are shared by both client and the backend in the project folder.

## Client MVP
use MVP schema to control the logic and data flow

## Other Plugins
[Mast](https://github.com/rayark/mast) provides functional choices to adopt the concept of Coroutine independently of Unity.

# Rules And Information About The Original BoardGame
## Rule Book
https://drive.google.com/file/d/0B3JoldWcYXRkSVlEQ1hpZEVTTE0/view?usp=share_link&resourcekey=0-a9KVvJ5HWiEXAXemTAcoxw
## Cards and Characters
https://docs.google.com/spreadsheets/d/12qjrSQr4qJYlz4vaLITyTZiYcfgyKRJh2Sy9HIlccA4/edit?usp=share_link
## Gameplay Video
https://drive.google.com/file/d/0B2jz22BZN24MUGxYYUFtQTIzcTQ/view?usp=share_link&resourcekey=0-puvxO-GY4sV9799S02P3bA
