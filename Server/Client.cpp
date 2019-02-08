#include "Client.h"

Client::Client(int clientID, SOCKET clientSocket) {
	this->clientID = clientID;
	this->roomID = -1;
	this->clientSocket = clientSocket;
}
int Client::getClientID() {
	return clientID;
}
int Client::getRoomID() {
	return roomID;
}
void Client::setRoomID(int roomID) {
	this->roomID = roomID;
}
SOCKET Client::getClientSocket() {
	return clientSocket;
}