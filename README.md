# mskeiks
unity/photon 'mmo' game

Simple game with login screen, hub area, and dynamically instanced gameplay areas. 

Project abandoned because photon's cloud server system isn't well-suited to realtime gameplay: they don't allow for server-side code, so it's essentially p2p with extra latency from both clients communicating with the server instead of each other directly. Winter break was ending, so there wasn't time to learn an entirely new API.

Uses:
Unity game engine https://unity3d.com/
Playfab for database management (logins, items) https://playfab.com/
Photon for cloud server hosting https://www.photonengine.com/
