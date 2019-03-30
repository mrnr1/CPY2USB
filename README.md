# CPY2USB
CPY2USB is a C# .NET application for Windows that allows for copying of data from a directory to multiple USB drives upon being inserted. We originally decided to write this application because we needed to load data from a preset source to over 250 USB drives. We recognized that other scripts and programs *could* help us achieve this task, however they didn't provide the flexibility that we needed.

###### Requirements
* Windows 7 or later (x86 / x64)
* .NET Framework v4.6 or higher

###### Features
*Please see the File -> Settings window to enable or disable these features*
* Multithreaded transfers (you may concurrently copy to multiple USBs)
* SHA256 file verification (to ensure that the copied file matches the source file)
* Ability to overwrite files if they already exist on the USB
* Ability to delete all the files on the USB before copy operations
* Ability to only copy if the newly inserted USB drive matches a given drive letter
* Safely ejects USB from Windows when finished
* Persistent settings (once you save your configuration, they will persist for your login user)

###### Usage
1. Start the program and adjust the configuration (File -> Settings). Browse for a source folder from which data will be copied to the USBs.
2. Press the Start Monitoring button
3. Insert USB drives and wait for operations to finish (repeat for all USBs)
4. Once you're finished copying data to all of your USBs, press the Stop Monitoring button and exit the application.

###### Screenshots
![Image of Main Window](https://raw.githubusercontent.com/mrnr1/CPY2USB/master/MainWindow.png)

![Image of Settings Window](https://raw.githubusercontent.com/mrnr1/CPY2USB/master/Settings.png)
