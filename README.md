# ServiceManager
A tool for managing mostly console applications like Cronjobs.

Developed with C# and make use of Telerik WinForms controls.

The network communication is done via WCF. It allows you to use Net Pipes or TCP connections with a custom Port.

Authentication is not integrated yet. => Todo List

- [Main Window](#mainwindow)
- [Network Settings](#network-settings)
- [Notification Settings](#notification-settings)
- [Service Settings - Start](#service-settings---start)
- [Service Settings - Shutdown](#service-settings---shutdown)
- [Service Settings - Logging](#service-settings---logging)
- [Live Console In and Output](#live-console-in-and-output)



## MainWindow

<img src="Screens/Mainwindow.png" />


## Network Settings

Choose between Net Pipes and TCP connection for your Service Manager.

<img src="Screens/Settings.png" />

## Notification Settings

Choose a notification provider for getting messages over server or service status changes.

<img src="Screens/Settings_3.png" />

## Service Settings - Start

Enable automatic start with the Service (system) and allow automatic restart when service ends or crashes.

<img src="Screens/Settings_1.png" />

## Service Settings - Shutdown

Timeout before service gets killed when not responding to exit signal.

Good for console applications allow pressing ENTER before running Timeout.

After the timeout it will get killed.

<img src="Screens/Settings_2.png" />

## Service Settings - Logging

Log your services console and error outputs right into memory /and on disk. So you can use the Live Console outputs full potential.

<img src="Screens/Settings_4.png" />


## Live Console In and Output

Get directly output from your console applications and send back informations over the network.

<img src="Screens/Live Console Out and Input.png" />

Example:

<img src="Screens/livelogs_example.gif" />


Icons where used from:
https://www.iconfinder.com/iconsets/ui-basic-glyph
