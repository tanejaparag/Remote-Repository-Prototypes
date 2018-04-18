cd Debug
start ServerPrototype.exe
timeout /t 4
cd "../Client/bin/x86/Debug"
start Client.exe
timeout /t 50
start Client.exe -port 8082