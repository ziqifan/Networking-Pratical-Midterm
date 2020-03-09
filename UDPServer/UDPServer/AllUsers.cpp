#include "AllUsers.h"

void AllUsers::addUser(std::string name, sockaddr_in adress, int index)
{
	listUsers.push_back(User(name, adress, index));
}

std::string AllUsers::getName(int id)
{
	for (std::list <User> ::iterator it = listUsers.begin(); it != listUsers.end(); it++)
	{
		if ((*it)._index == id) {
			return (*it)._name;
		}
	}
	printf("Id of user is not exit!!!!!Name cannot be found!!!\n");
	return std::string();
}

sockaddr_in AllUsers::getAdress(int id)
{
	for (std::list <User> ::iterator it = listUsers.begin(); it != listUsers.end(); it++)
	{
		if ((*it)._index == id) {
			return (*it)._adress;
		}
	}
	printf("Id of user is not exit!!!!!Adress cannot be found!!!\n");
	return sockaddr_in();
}

int AllUsers::getNewID()
{
	if (listUsers.size() == 0)
	{
		return 1;
	}
	else 
	{
		return (*listUsers.end())._index++;
	}
}

void AllUsers::removeUser(int id)
{
	for (std::list <User> ::iterator it = listUsers.begin(); it != listUsers.end(); it++)
	{
		if ((*it)._index == id) {
			listUsers.erase(it);
			return;
		}
	}
	printf("Id of user is not exit!!!!!Cannot remove user!!!\n");
}

