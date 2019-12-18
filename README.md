# Quick Chat M1 | EFREI P2021

This application is a multithreaded C# Messaging app on Windows (Gui version), and Linux, MacOs (console version). It was designed to be complete yet pleasant to use.

## Features

* Send pictures and display them in the chat
* Console & WPF client version. A WPF client can communicate with the console version
* Send private messages
* Edit/delete a message
* Create, edit a conversation name & description
* Only be notified by a conversion once you join it
* Send a temporary file
* Send a temporary audio file (snapchat like)
* Subscription (console version)

### Prerequisites
You need .Net Core 3.1 or higher to be able to run this project

```
If you don't have it you can download it at https://dotnet.microsoft.com/download
```

### Installing & Running


To use this software, you can open the .sln file in visual studio and run the server project with the following name

```
Web_App_Core_Server
```

You can now connect it with two kinds of clients by running the projects

```
Web_App_Core_Client (console client) or WebChatGuiClient (graphic client)
```

You can set your custom IP Address & port in the Server.cs file in the Web_App_Core_Server project.
For the client side, you can set them in the file Program.cs (for Web_App_Core_Client) or LoginWindow.xaml.cs (for WebChatGuiClient)

You can then connect as a Quick Chat user by using the following credentials : 

Username : François Password : 123 or Username : gaga Password : gaga

For the console version, use the following command to directly connect : 

```
connect | u:{your username} p:{your password}
```

Or the following to subscribe :
```
subscribe | u:{your username} p:{your password}
```

## Built With

* [.NET Core](https://github.com/dotnet/core) - A software cross platform framework
* [WPF](https://github.com/dotnet/wpf) - The graphic framework used for the client
* [MaterialDesignXAML](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) - For having new styles in WPF

## Author

* **Gillioen François** 
*Github* - (https://github.com/frankfg94) 
*GitLab* - (https://gitlab.com/frankfg94)

## License

This project has no license

## Additionnal informations
Created for a school project in two weeks<br/>
Lines of code : approx 2700
