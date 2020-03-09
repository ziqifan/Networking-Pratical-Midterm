///// UDP Server
#include "Server.h"
#include <thread>

Server myServer;
std::thread receiveThread, sendThread;

void receiveFunction() {
	myServer.UpdateRecv();
}

void sendFunction() {
	myServer.UpdateSend();
	exit(0);
}

int main() {
	myServer.CreateServer();

	std::thread receiveThread = std::thread(receiveFunction);
	std::thread sendThread = std::thread(sendFunction);

	receiveThread.join();
	sendThread.join();

	myServer.CloseServer();

	return 0;
}