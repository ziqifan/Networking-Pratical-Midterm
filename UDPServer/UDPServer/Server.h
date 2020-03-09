#include "AllUsers.h"
#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdio.h>
#include <iostream>
#include <string>

#pragma comment(lib, "Ws2_32.lib")

#define PORT "8888" 
#define BUF_LEN 512

enum class ServerStatus
{
	Lobby = 1,
	Game = 2,
};

class Server {
public:
	Server();
	~Server();

	void CreateServer();// Initialize the server
	void UpdateRecv();// Handle the receiving loop
	void UpdateSend();// Handle the sending loop
	void HandleSending(std::string _message, struct sockaddr_in _adress);// Send a string to certain adress
	void HandleSending(char* _message, struct sockaddr_in _adress);// Send a char* to certain adress
	void BroadcastMessageToAll(std::string _message);// Send a string to all users (Also print in command)
	void BroadcastMessageToAll(char* _message);// Send a char* to all users (Also print in command)
	void CloseServer();// Shutdown server

	void join(std::string _name, struct sockaddr_in _adress);// Process when a new player join server
	bool checkSeatFull();// Check is all seat occupied or not
	void takeSeat(int _userId, int _seatId);// Process when user try to take a seat
	void leftSeat(int _id);// Process when user left the seat
	void leftLobby(int _id);// Process when user left the lobby
	void leftGame(int _id);// Process when user left during the gameplay

	bool isServerRunning;

private:
	SOCKET server_socket;
	struct addrinfo hints, *ptr;
	char recv_buf[BUF_LEN];

	AllUsers users;
	int lobbySeat[4];
	ServerStatus status;

	///////For Testing
	struct sockaddr_in temp;
};

