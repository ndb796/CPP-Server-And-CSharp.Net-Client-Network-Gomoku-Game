#ifndef GOMOKU_CLIENT_H
#define GOMOKU_CLIENT_H
#include <Winsock.h>
class Client {
private:
	int clientID;
	int roomID;
	SOCKET clientSocket;
public:
	Client(int clientID, SOCKET clientSocket);
	int getClientID();
	int getRoomID();
	void setRoomID(int roomID);
	SOCKET getClientSocket();
};
#endif