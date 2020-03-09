#pragma once
#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdio.h>
#include <iostream>
#include <string>
#include <list>
#include <iterator>
#include <vector>

#pragma comment(lib, "Ws2_32.lib")

struct User
{
	std::string _name;
	struct sockaddr_in _adress;
	int _index;
	User(std::string name, struct sockaddr_in adress, int index)
	{
		_name = name;
		_adress = adress;
		_index = index;
	}
};

class AllUsers
{

public:
	std::list <User> listUsers;

	void addUser(std::string name, struct sockaddr_in adress, int index);//Add one user to list
	void removeUser(int id);//Remove user in list of certain ID
	std::string getName(int id);//Get name of a user in certain ID
	struct sockaddr_in getAdress(int id);//Get adress of user in certain ID
	int getNewID();

};