# EO59 API data downloading demo application
This is a simple .NET Core console application that will connect to EO59 data api at https://api.eo59.com and download data sets
belonging to issued subscription key. 

Please note that each susbcription key is project based and will contain only data sets from that project, to download multiple projects
this sample application needs to be modified to support array of project definitions. For simplicity, downloading only one project is 
demonstrated, however code is structured in a way that supporting multiple projects can be addedd with minimal effort.

## Installing demo
First, the enviroment needs to be prepared.
.NET Core supports Windows, Linux and Apple platforms, to build application from source code a SDK version
of .NET Core 5 is needed on the computer, however after code is compiled it is possible to distribute just executable with or without .NET runtime.
More information on application publishing can be found at https://docs.microsoft.com/en-us/dotnet/core/deploying/

.NET 5 SDK can be downloaded at https://dotnet.microsoft.com/download 

Once .NET SDK is installed on computer this project can be cloned via git, if git is not installed on computer it can be downloaded at https://git-scm.com/downloads
For simple example following command can be run in terminal / command line to clone the repository:
```
git clone https://github.com/EO59-LLc/api-demo.git
```
As alternative you can also just download the repository as zip file from the menu on top of the page.

## Preparing to run demo
Once repository is on computer you need to use console / terminal or IDE to restore dependencies via command
```
dotnet restore
```
Please note:
Before running the project you need to configure settings in applicationsettings.json file, namely there are 3 parameters that need to be configured
* Subscription.Name - enter human readable name, this will be used as print out in logs as well as file name for caching, can be something like **project**
* Subscription.ApiKey - enter the subscription key sent by EO59 or from management page.
* StorageBasePath - provide file system path where data files will be cached, in case of window you need to double backslash to avoid breaking json format, 
for example ```c:\somedir``` needs to be written as ```c:\\somedir ```

For IDE we recommend using https://code.visualstudio.com/ or https://www.jetbrains.com/rider/

## Running demo
Once project settings are configured you can run sample application with:
```
dotnet run
```

# Notes about limits
Deformation data is gathered from Sattelites, thus it is not working in real time, depending on orbital location the update cycle can be from week to two weeks.
Data sets are also sizable, as such it is recommended that you will check for updates maximum once in 24 hours, or once a week in case of full automation.
For manual download EO59 always notifies clients via email when the new data is available.
