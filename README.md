# CPY2USB
CPY2USB is a C# application for Windows that allows you to copy data from a directory to multiple USB drives upon being inserted.

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

###### Usage
1. Start the program and adjust the configuration (File -> Settings). Browse for a source folder from which data will be copied to the USBs.
2. Press the Start Monitoring button
3. Insert USB drives and wait for operations to finish (repeat for all USBs)
4. Once you're finished copying data to all of your USBs, press the Stop Monitoring button and exit the application.

###### Screenshots
