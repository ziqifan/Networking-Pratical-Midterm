#include "Server.h"

Server::Server()
{
	isServerRunning = true;
	lobbySeat[0] = 0;
	lobbySeat[1] = 0;
	lobbySeat[2] = 0;
	lobbySeat[3] = 0;
	status = ServerStatus::Lobby;
	ptr = NULL;
}

Server::~Server()
{
}

void Server::CreateServer()
{
	//Initialize winsock
	WSADATA wsa;

	int error;
	error = WSAStartup(MAKEWORD(2, 2), &wsa);

	if (error != 0) {
		printf("Failed to initialize %d\n", error);
		return;
	}

	//Create a Server socket
	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_DGRAM;
	hints.ai_protocol = IPPROTO_UDP;
	hints.ai_flags = AI_PASSIVE;

	if (getaddrinfo(NULL, PORT, &hints, &ptr) != 0) {
		printf("Getaddrinfo failed!! %d\n", WSAGetLastError());
		WSACleanup();
		return;
	}

	server_socket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

	if (server_socket == INVALID_SOCKET) {
		printf("Failed creating a socket %d\n", WSAGetLastError());
		WSACleanup();
		return;
	}

	// Bind socket
	if (bind(server_socket, ptr->ai_addr, (int)ptr->ai_addrlen) == SOCKET_ERROR) {
		printf("Bind failed: %d\n", WSAGetLastError());
		closesocket(server_socket);
		freeaddrinfo(ptr);
		WSACleanup();
		return;
	}

	printf("Server is now running!\n");
}

void Server::UpdateRecv()
{
	while (isServerRunning) {
		if (status == ServerStatus::Lobby)
		{
			struct sockaddr_in fromAddr;
			int fromlen;
			fromlen = sizeof(fromAddr);
			memset(recv_buf, 0, BUF_LEN);
			if (recvfrom(server_socket, recv_buf, sizeof(recv_buf), 0, (struct sockaddr*) & fromAddr, &fromlen) == SOCKET_ERROR) {
				printf("recvfrom() failed...%d\n", WSAGetLastError());
				return;
			}
			printf("Received: %s\n", recv_buf);

			//////For Testing
			temp = fromAddr;

			// TODO:
			// Join require
			// Seat require
			// Player leave seat
			// Player leave Lobby
		}
		else if (status == ServerStatus::Game)
		{
			struct sockaddr_in fromAddr;
			int fromlen;
			fromlen = sizeof(fromAddr);
			memset(recv_buf, 0, BUF_LEN);
			if (recvfrom(server_socket, recv_buf, sizeof(recv_buf), 0, (struct sockaddr*) & fromAddr, &fromlen) == SOCKET_ERROR) {
				printf("recvfrom() failed...%d\n", WSAGetLastError());
				return;
			}
			BroadcastMessageToAll(recv_buf);


			// TODO:
			// Player leave game
		}
		else 
		{
			printf("Player status is incorrect!!!!");
		}
	}
}

void Server::UpdateSend()
{
	while (isServerRunning) {
		if (status == ServerStatus::Lobby)
		{
			std::string line;
			std::getline(std::cin, line);

			if (line.size() > 0)
				//BroadcastMessageToAll(line);
				//////For Testing
				HandleSending(line, temp);

			// check for start game
			if (checkSeatFull()) {
				status = ServerStatus::Game;
				//TODO:
				BroadcastMessageToAll("");
			}
		}
		else if (status == ServerStatus::Game)
		{

		}
		else
		{
			printf("Player status is incorrect!!!!");
		}
	}
}

void Server::HandleSending(std::string _message, sockaddr_in _adress)
{
	int fromlen;
	fromlen = sizeof(_adress);
	char* temp = (char*)_message.c_str();
	if (sendto(server_socket, temp, BUF_LEN, 0, (struct sockaddr*) & _adress, fromlen) == SOCKET_ERROR)
	{
		printf("sendto() failed %d\n", WSAGetLastError());
	}
}

void Server::HandleSending(char* _message, sockaddr_in _adress)
{
	int fromlen;
	fromlen = sizeof(_adress);
	if (sendto(server_socket, _message, BUF_LEN, 0, (struct sockaddr*) & _adress, fromlen) == SOCKET_ERROR)
	{
		printf("sendto() failed %d\n", WSAGetLastError());
	}
}

void Server::BroadcastMessageToAll(std::string _message)
{
	std::cout << _message;
	if (users.listUsers.size() != 0)
	{
		for (std::list <User> ::iterator it = users.listUsers.begin(); it != users.listUsers.end(); it++)
		{
			HandleSending(_message, (*it)._adress);
		};
	}
}

void Server::BroadcastMessageToAll(char* _message)
{
	printf(_message);
	if (users.listUsers.size() != 0)
	{
		for (std::list <User> ::iterator it = users.listUsers.begin(); it != users.listUsers.end(); it++)
		{
			HandleSending(_message, (*it)._adress);
		};
	}
}

void Server::CloseServer()
{
	closesocket(server_socket);
	freeaddrinfo(ptr);
	WSACleanup();
}

void Server::join(std::string _name, sockaddr_in _adress)
{
	// Notice all player in lobby
	// TODO:
	BroadcastMessageToAll("");

	// Sendback conformation, id and seat info
	int userId = users.getNewID();
	std::string seatInfo;
	for (int i = 0; i < 4; i++)
	{
		seatInfo += ":";
		seatInfo += std::to_string(lobbySeat[i]);
	}
	// TODO:
	HandleSending("", _adress);

	// Add to player list
	users.addUser(_name, _adress, userId);
}

bool Server::checkSeatFull()
{
	bool check = true;
	for (int i = 0; i < 4; i++)
	{
		if (lobbySeat[i] == 0)
		{
			check = false;
			break;
		}
	}
	return check;
}

void Server::takeSeat(int _userId, int _seatId)
{
	if (lobbySeat[_seatId] == 0)
	{
		lobbySeat[_seatId] = _userId;
		// Send back conformation to user
		// TODO:
		HandleSending("", users.getAdress(_userId));
		// Update seat changing to all user
		std::string seatInfo;
		for (int i = 0; i < 4; i++)
		{
			seatInfo += ":";
			seatInfo += std::to_string(lobbySeat[i]);
		}
		// TODO:
		BroadcastMessageToAll("");

	}
	else
	{
		// Seat is occupied
		// TODO:
		HandleSending("", users.getAdress(_userId));
	}
}

void Server::leftSeat(int _id)
{
	// Remove user from seat
	for (int i = 0; i < 4; i++)
	{
		if (lobbySeat[i] == _id)
		{
			lobbySeat[i] = 0;
			break;
		}
	}
	// Update seat changing to all user
	std::string seatInfo;
	for (int i = 0; i < 4; i++)
	{
		seatInfo += ":";
		seatInfo += std::to_string(lobbySeat[i]);
	}
	// TODO:
	BroadcastMessageToAll("");
}

void Server::leftLobby(int _id)
{
	// Remove user from list of user
	users.removeUser(_id);

	// Let all user know
	//TODO:
	BroadcastMessageToAll("");
}

void Server::leftGame(int _id)
{
	// Remove user from list of user
	users.removeUser(_id);

	// Let all user know
	//TODO:
	BroadcastMessageToAll("");
}
