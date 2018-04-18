# Remote-Repository-Prototypes
Purpose:
A Code Repository is a Server responsible for managing source code resources, e.g., files and documents. A fully developed Repository will support file persistance, managment of versions, and the acquisition and publication of source and document files. This project focuses on providing the communication channels and Graphical User Interfaces used to implement a Remote Repository, to be developed in Project #4.

In this project we will develop:

A C++ message-passing communication system, based on sockets. It supports asynchronous communication between multiple concurrent Client processes and the Repository Server. You will find CommWithFileXfer or CppHttpClientServer as a good place to start your development of this prototype.
A Graphical User Interface (GUI) used to send and retrieve packages from the repository, and to request check-in and check-out operations, and view package descriptions, full file text, and package metadata.
Requirements:
Remote Repository Prototype Project:
- use Visual Studio 2017 and the standard C++ libraries, as provided in the ECS computer labs. You may also use C#, the .Net Windows Presentation Foundation framework, and C++\CLI for the graphical part of each Client.
- provide an asynchronous message-passing communication channel, used for all communication between Clients and the Remote Repository Server. The channel will use HTTP style messages for sending requests, receiving replies, and transferring files between Clients and Server. Opening a new window displaying text of a Repository file will satisfy this requirement.
- provide a Client graphical user interface that supports tabs for the views:
    Connecting to Servers
    check-in
    check-out
    browsing specified package descriptions
    viewing full file text
    viewing file metadata
