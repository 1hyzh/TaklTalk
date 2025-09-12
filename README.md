# TaklTalk
### Talk in a local connection

The program should look like this when you open it:



<img width="1113" height="623" alt="image" src="https://github.com/user-attachments/assets/07dda3de-c242-4c44-baf4-cffb46a8ac4e" />


### You can choose either to host or connect to a server

### To connect between two computers one should join the servers IP adress (can be found in ipconfig in windows or hostname -I in linux or directly in the application)


# How to open in linux

## Using the install script

just use the script if you want.

### x64

```bash
wget https://raw.githubusercontent.com/1hyzh/TaklTalk/refs/heads/main/linux-x64-download.sh && chmod +x linux-x64-download.sh && ./linux-x64-download.sh
```

### arm64

```bash
wget https://raw.githubusercontent.com/1hyzh/TaklTalk/refs/heads/main/linux-arm64-download.sh && chmod +x linux-arm64-download.sh && ./linux-arm64-download.sh
```


## Manual way

To open TaklTalk in linux first you need to download the binary or build it yourself.
To download it you can either download it from releases or pasting this in your terminal:

### x64

```bash
wget https://github.com/1hyzh/TaklTalk/releases/download/1.0/TaklTalk-1.0-linux-x64
```

### arm64

```bash
wget https://github.com/1hyzh/TaklTalk/releases/download/1.0/TaklTalk-1.0-linux-arm64
```

Then you should make the binary executable by doing this:

### x64

```bash
chmod +x TaklTalk-1.0-linux-x64
```
### arm64

```bash
chmod +x TaklTalk-1.0-linux-arm64
```

after that you just run the program:

### x64

```bash
./TaklTalk-1.0-linux-x64
```
### arm64

```bash
./TaklTalk-1.0-linux-arm64
```


# To build it yourself:

## Linux

### You should have installed the dotnet-sdk packet for doing this, to install it you can do

```bash
sudo apt install dotnet-sdk-8.0
```

or

```bash
sudo snap install dotnet-sdk --classic
```

First you should create a console project like this:

```bash 
dotnet new console -o TaklTalk && cd TaklTalk
```

Then delete the default Program.cs and replace it with the one in this repo:

```bash
rm Program.cs && wget https://raw.githubusercontent.com/1hyzh/TaklTalk/refs/heads/main/Program.cs
```

(you can double check that the file has been replaced by doing 'cat Program.cs')

after that publish the project with this line, remember to replace the linux-x64 with the architectury you need

```bash
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:IncludeNativeLibrariesForSelfExtract=true && cd ./bin/Release/net8.0/linux-x64/publish 
```

and then open it by doing

```bash
./TaklTalk
```


## Windows

Download Visual Studio and install the .NET Development package
After that create C# Console app project, replace the Program.cs with the one in this repo and you can either just click run (which will output various files) or publish into a folder (if you do it right will output only one file)
