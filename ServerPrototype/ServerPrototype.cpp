/////////////////////////////////////////////////////////////////////////
// ServerPrototype.cpp - Console App that processes incoming messages  //
// ver 1.1                                                             //
// Author - Parag Taneja, OOD , Spring 2018
// Source - Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018           //
/////////////////////////////////////////////////////////////////////////

/*

Required Files:
* ---------------
* ServerPrototype.h , FileSystem.h
*

* Functions
*Files Server::getFiles(const Repository::SearchPath& path) - Returns all the file in specifed Director
*Dirs Server::getDirs(const Repository::SearchPath& path) - Returns all the directories in specified Directory
*void show(const T& t, const std::string& msg) - Will print the message on servers console

*Functors
std::function<Msg(Msg)> echo - A functor  which invokes when server receives command echo in message
std::function<Msg(Msg)> req_connect -  A functor  which invokes when server receives command req_connect in message
std::function<Msg(Msg)> clientsendingfile -  A functor  which invokes when server receives command clientsendingfile in message
std::function<Msg(Msg)> req_threadquit - A functor  which invokes when server receives command req_threadquit in message
std::function<Msg(Msg)> clientaskingfile - A functor  which invokes when server receives command clientaskingfile in message
std::function<Msg(Msg)> req_browseaskingfile - A functor  which invokes when server receives command req_browseaskingfile in message
std::function<Msg(Msg)> req_categoryByBrowse - functor  which invokes when server receives command req_categoryByBrowse in message
std::function<Msg(Msg)> req_metadata - functor which invokes when server receives command req_metadta in message
std::function<Msg(Msg)> req_foldername - A functor  which invokes when server receives command req_foldername in message
std::function<Msg(Msg)> getFiles - A functor  which invokes when server receives command getFiles in message
std::function<Msg(Msg)> getDirs - A functor  which invokes when server receives command getDirs in message

* Maintenance History:
* --------------------
* ver 1.1 : April 8th, 2018
	- Added moe client handlers to Servers dispacther unordered map.
* ver 1.0 : March 27th , 2018
*   - first release
*/

#include "ServerPrototype.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include <chrono>

namespace MsgPassComm = MsgPassingCommunication;

using namespace Repository;
using namespace FileSystem;
using Msg = MsgPassingCommunication::Message;

//Returns all the file in specifed Director
Files Server::getFiles(const Repository::SearchPath& path)
{
  return Directory::getFiles(path);
}

//Returns all the directories in specified Directory
Dirs Server::getDirs(const Repository::SearchPath& path)
{
  return Directory::getDirectories(path);
}

//Will print the message on servers console
template<typename T>
void show(const T& t, const std::string& msg)
{
  std::cout << "\n  " << msg.c_str();
  for (auto item : t)
  {
    std::cout << "\n    " << item.c_str();
  }
}

//A functor  which invokes when server receives command echo in message
std::function<Msg(Msg)> echo = [](Msg msg) {
  Msg reply = msg;
  reply.to(msg.from());
  reply.from(msg.to());
  return reply;
};

//A functor  which invokes when server receives command req_connect in message
std::function<Msg(Msg)> req_connect = [](Msg msg) {

	Msg reply = msg;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("reply_connect");
	reply.attribute("content", "Server-> Connection Established ..");
	return reply; //Reply will posted in Servers Sender Queue from where it is dequeued and sent via Socket
	
};

//A functor  which invokes when server receives command clientsendingfile in message
std::function<Msg(Msg)> clientsendingfile = [](Msg msg) {

	Msg reply = msg;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("clientfilesendingreply"); //Making a reply and adding a commnd Clientfilesendingreply for which client already has dispacther
	reply.attribute("content", "Server -> File chunk saved in server ");
	reply.attribute("tab", msg.value("tab"));
	return reply;
	
};

//A functor  which invokes when server receives command req_threadquit in message
std::function<Msg(Msg)> req_threadquit = [](Msg msg) {

	Msg reply = msg; //Creating a message
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("reply_threadquit");
	reply.attribute("content", "Server -> Disconnected");
	reply.attribute("tab", msg.value("tab"));
	return reply;

};

//A functor  which invokes when server receives command clientaskingfile in message
std::function<Msg(Msg)> clientaskingfile = [](Msg msg) {

	Msg reply = msg;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("clientfileaskingreply");
	reply.attribute("serversending", "True"); //setting one more attribute to help comm package that file is being transferred from server to client
	std::string filenameWithNamespace = msg.value("nmspc") + "/" + msg.value("filename");
	reply.attribute("file", filenameWithNamespace); //adding file attribute which helps comm package to identify it as a file during transfer
	reply.attribute("content", "Server -> File chunk is sent succesfully to client...");
	return reply;

};

//A functor  which invokes when server receives command req_browseaskingfile in message
std::function<Msg(Msg)> req_browseaskingfile = [](Msg msg) {

	Msg reply = msg;
	reply.to(msg.from()); //setting to section message which will be client port number.
	reply.from(msg.to());
	reply.command("reply_browseaskingfile");
	reply.attribute("serversending", "True");
	std::string filenameWithNamespace = msg.value("nmspc") + "/" + msg.value("filename");
	reply.attribute("file", filenameWithNamespace);
	reply.attribute("content", "Server -> Browsed File is sent succesfully to client...");
	return reply;

};

//A functor  which invokes when server receives command req_categoryByBrowse in message
std::function<Msg(Msg)> req_categoryByBrowse = [](Msg msg) {
	Msg reply = msg;
	reply.to(msg.from());
	reply.from(msg.to()); //seting from section whcih is servers port number becasue message is being sent from server to client
	reply.command("reply_categoryByBrowse");
	std::string filenames;
	std::string nmspc;
	if (msg.value("categoryvalue") == "Category1")
	{
		filenames = "Message.h";
		nmspc = "CR4c3";
	}
	if (msg.value("categoryvalue") == "Category2")
	{
		filenames = "Logger.h";
		nmspc = "Log";
	}
	reply.attribute("filenames", filenames);
	reply.attribute("nmspcs",nmspc);
	reply.attribute("content", "Server -> Files are ");
	//reply.attribute("serversending", "True");
	return reply;

};


//A functor which invokes when server receives command req_metadta in message
std::function<Msg(Msg)> req_metadata = [](Msg msg) {
	Msg reply = msg;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("reply_metadata");
	reply.attribute("tab", msg.value("tab"));
	reply.attribute("filename", msg.value("filename"));
	reply.attribute("metadata_name", "Parag Taneja"); //Creating metadata 
	reply.attribute("metadta_description", "File contains the definition of Parag.h"); //creating metadata
	reply.attribute("metadata_date", "April 4th,2018"); //creating metadata
	reply.attribute("metadata_children", "Parag.cpp"); //creating metadata
	reply.attribute("content", "Server -> Metadata Sent"); 
	return reply;
};
//A functor  which invokes when server receives command req_foldername in message
std::function<Msg(Msg)> req_foldername = [](Msg msg) {
	Msg reply = msg;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("reply_foldername");
	std::cout << "He HE --------------" + FileSystem::Directory::getCurrentDirectory();
	FileSystem::Directory::create(storageRoot + "/" + msg.value("nmspc"));
	reply.attribute("content","Server -> Name Space " + msg.value("nmspc") + " is added in server repo");
	return reply;
};

//A functor  which invokes when server receives command getFiles in message
std::function<Msg(Msg)> getFiles = [](Msg msg) {
  Msg reply;
  reply.to(msg.from());
  reply.from(msg.to());
  reply.command("getFiles");
  reply.attribute("tab", msg.value("tab"));
  std::string path = msg.value("path");
  if (path != "")
  {
    std::string searchPath = storageRoot;
    if (path != ".")
      searchPath = searchPath + "\\" + path;
    Files files = Server::getFiles(searchPath);
    size_t count = 0;
    for (auto item : files)
    {
      std::string countStr = Utilities::Converter<size_t>::toString(++count);
      reply.attribute("file" + countStr, item); //adding file attribute to add the all the filename
    }
  }
  else
  {
    std::cout << "\n  getFiles message did not define a path attribute";
  }
  return reply;
};

//A functor  which invokes when server receives command getDirs in message
std::function<Msg(Msg)> getDirs = [](Msg msg) {
  Msg reply;
  reply.to(msg.from());
  reply.from(msg.to());
  reply.command("getDirs");
  reply.attribute("tab", msg.value("tab"));
  std::string path = msg.value("path");
  if (path != "")
  {
    std::string searchPath = storageRoot;
    if (path != ".")
      searchPath = searchPath + "\\" + path;
    Files dirs = Server::getDirs(searchPath);
    size_t count = 0;
    for (auto item : dirs)
    {
      if (item != ".." && item != ".")
      {
        std::string countStr = Utilities::Converter<size_t>::toString(++count);
        reply.attribute("dir" + countStr, item); //adding attribute dir to list all folders with in directory 
      }
    }
  }
  else
  {
    std::cout << "\n  getDirs message did not define a path attribute";
  }
  return reply;
};

//main function to stat the server
int main()
{
  std::cout << "\n  Testing Server Prototype \n ==========================\n";
  Server server(serverEndPoint, "ServerPrototype"); // Creating server instance
  server.start(); //startign the server which will start its listener queue and receivers queue
  std::cout << "\n  testing getFiles and getDirs methods\n --------------------------------------";
  Files files = server.getFiles();
  show(files, "Files:");
  Dirs dirs = server.getDirs();
  show(dirs, "Dirs:");
  std::cout << "\n\n  testing message processing\n ----------------------------";
  server.addMsgProc("echo", echo);
  server.addMsgProc("getFiles", getFiles);
  server.addMsgProc("getDirs", getDirs);
  server.addMsgProc("serverQuit", echo);
  server.addMsgProc("req_connect", req_connect); //Adding functor req_connect in server dispatcher
  server.addMsgProc("clientaskingfile", clientaskingfile); //Adding functor clientaskingfile in server dispatcher
  server.addMsgProc("clientsendingfile", clientsendingfile); //Adding functor clientsendingfile in server dispatcher
  server.addMsgProc("req_categoryByBrowse", req_categoryByBrowse); //Adding functor req_categoryByBrowse in server dispatcher
  server.addMsgProc("req_browseaskingfile", req_browseaskingfile); //Adding functor req_browseaskingfile in server dispatcher
  server.addMsgProc("req_foldername", req_foldername); //Adding functor req_foldername in server dispatcher
  server.addMsgProc("req_metadata", req_metadata); //Adding functor req_metadata in server dispatcher
  server.addMsgProc("req_threadquit", req_threadquit); //Adding functor req_threadquit in server dispatcher
  server.processMessages();
  Msg msg(serverEndPoint, serverEndPoint);  // send to self
  msg.name("msgToSelf");
  msg.command("echo");
  msg.attribute("verbose", "show me");
  server.postMessage(msg);
  std::this_thread::sleep_for(std::chrono::milliseconds(1000));
  msg.command("getFiles");
  msg.remove("verbose");
  msg.attributes()["path"] = storageRoot;
  server.postMessage(msg);
  std::this_thread::sleep_for(std::chrono::milliseconds(1000));
  msg.command("getDirs");
  msg.attributes()["path"] = storageRoot;
  server.postMessage(msg);
  std::this_thread::sleep_for(std::chrono::milliseconds(1000));
  std::cout << "\n  press enter to exit";
  std::cin.get();
  std::cout << "\n";
  msg.command("serverQuit");
  server.postMessage(msg);
  server.stop();
  return 0;
}

