# ServiceManager
[![CI](https://github.com/MajMcCloud/ServiceManager/actions/workflows/ci.yml/badge.svg)](https://github.com/MajMcCloud/ServiceManager/actions/workflows/ci.yml)

A tool for managing mostly console applications like Cronjobs.

Developed with C# using the standard WinForms controls.

The network communication is done via WCF. It allows you to use Net Pipes or TCP connections with a custom Port.

Authentication is not integrated yet. => Todo List

## Build

Targets the .NET Framework 4.8.1 (the last version of the classic .NET Framework). The reference assemblies are restored from NuGet, so no targeting pack has to be installed. No third party UI controls, so a plain clone builds out of the box:

```
msbuild ServiceManager.sln /t:Restore /p:Configuration=Release
msbuild ServiceManager.sln /t:Rebuild /p:Configuration=Release
```

which uploads the `ServiceManager.Service\bin\Release` and `ServiceManager.UI\bin\Release` folders as build artifacts.

- [Main Window](#mainwindow)
- [Network Settings](#network-settings)
- [Notifications](#notifications)
  - [Notification Settings](#notification-settings)
  - [Telegram](#notifications---telegram)
  - [Windows Event Log](#notifications---windows-event-log)
  - [Webhook](#notifications---webhook)
  - [SMTP Mail](#notifications---smtp-mail)
  - [Syslog](#notifications---syslog)
  - [Script Hook](#notifications---script-hook)
- [Service Settings - Start](#service-settings---start)
- [Service Settings - Shutdown](#service-settings---shutdown)
- [Service Settings - Logging](#service-settings---logging)
- [Live Console In and Output](#live-console-in-and-output)

## MainWindow

<img src="Screens/Mainwindow.png" />


## Network Settings

Choose between Net Pipes and TCP connection for your Service Manager.

<img src="Screens/Settings.png" />

## Notifications

Notifications stay off until at least one provider is listed. They are configured in one place only:
`Manager` -> `Open config` -> the `Notifications` tab.

Six providers ship in the box, and each one is switched on per event, so the same event can go into the
event log and to a webhook while Telegram only receives the errors:

| Provider | Needs | Reachable |
| --- | --- | --- |
| [Telegram](#notifications---telegram) | internet access, a bot token | anywhere the ServiceManager has internet |
| [Windows Event Log](#notifications---windows-event-log) | no network, administrator rights for the first entry | the machine the service runs on |
| [Webhook](#notifications---webhook) | internet access or a local endpoint | any tool that accepts a request |
| [SMTP Mail](#notifications---smtp-mail) | an SMTP server, or the pickup folder of IIS | any inbox |
| [Syslog](#notifications---syslog) | a collector over UDP or TCP | a log server on the network |
| [Script Hook](#notifications---script-hook) | a program on the machine | whatever that program can do |

### Notification Settings

Pick every provider that should react to an event and decide per provider which events it receives.
Add registers a provider, Remove drops it again, Configure opens the settings of the selected one.
An empty list switches notifications off, a provider that is not listed never receives a message.

A settings file of an older version keeps working: its single provider and the global ticks become
one entry of this list the first time the file is read.

The seven events are the ones the ServiceManager reports: *On Start*, *On Stop* and *On Restart* belong to a
managed service, *On Console Output* and *On Console Error* to its console, *On Server Start* and
*On Server Shutdown* to the ServiceManager itself.

<img src="Screens/Settings_3.png" />

### Notifications - Telegram

Example of telegram messages you will receive when used.

<img src="Screens/Notifications_Telegram.png" />

### Notifications - Windows Event Log

The provider without any network: every event becomes an entry in the Windows Event Log and, when
enabled, a balloon tip on the desktop. It keeps working without internet access and while the service
runs with no user logged on - only the balloon tip needs an interactive session.

Registering the event source writes into the registry, so the first entry needs administrator rights;
the settings form says so while they are missing. Every kind of event gets its own event id and its
own category, which lets the Event Viewer filter on a single kind, e.g. all console errors.

### Notifications - Webhook

One request to an endpoint of your choice. The body is written in the settings, so the same provider
serves a Slack incoming webhook, a Discord webhook, an n8n or Node-RED flow and an endpoint of your
own.

`{text}` `{details}` `{kind}` `{level}` `{service}` `{instance}` and `{timestamp}` are replaced.
Inside a JSON body the values are escaped, so a console line containing a quote cannot break the
message, while a plain text body receives them as they are. A placeholder the provider does not know
is sent untouched, which keeps templates written for another tool working.

Additional headers go one per line as `Name: value`, enough for an `Authorization` header of a hosted
service or the `X-...` header of a flow. A failed delivery is repeated as often as configured, the reason lands in
the console of the ServiceManager. The settings form has a test button that answers what the endpoint
replied, and it can accept a self signed certificate - for this one endpoint, not for the process.

### Notifications - SMTP Mail

One mail per event through the SMTP server of the installation. Port 25 unencrypted and 587 with STARTTLS
are what a mail server offers, 465 is not supported. Username and password stay empty for a server that
takes the machine as it is.

The subject names the prefix, the level, the instance and the service, so a mail rule can pick the errors out
before anyone reads them. The body stays plain text: the fields as a small table and the console line below
it, because a mail client hides every console line it mistakes for a tag. The same event inside *Seconds
between identical mails* is mailed once, which keeps a service that crashes every few seconds from filling
the inbox.

*Pickup directory* needs no server at all: the mail is written into the drop folder of the local SMTP service,
which is also the way to look at a message without sending it anywhere. The form has a test button that
writes a real mail through the configured settings.

### Notifications - Syslog

One line per event to a collector such as rsyslog, Graylog or the journal of a NAS. UDP is the usual
transport and needs no handshake, TCP reaches a collector that answers and reports what it did not take.

The line starts with the priority in angle brackets, which is the facility times eight plus the severity of
the event: an error on local0 reads 131, a warning 132. A facility outside 0 to 23 falls back to local0.
RFC 3164 is what an older collector expects, RFC 5424 adds the version and the zone of the sender. The stamp
is the time of the event rather than the time of sending, so the collector sees the order the events happened
in even when a delivery had to be repeated.

A collector cuts a line around 1024 bytes, so the provider cuts the text itself and keeps the header whole -
a cut line still names the service it came from. The test button of the form sends one line over the
configured transport and answers what the collector did not accept.

### Notifications - Script Hook

A program on the machine receives the event: a batch file, a PowerShell script, a Node tool, anything that
can be started. It is started directly rather than through a shell, so a path with a space or an ampersand in
it stays one path.

The event is one json object, by default written to the standard input of the program. *Hand the event as one
extra argument* hands the same object as the last argument instead, for a program that reads its arguments.
The quoting follows the rules of the operating system, so a service name containing a quote cannot end the
argument early. *Arguments* is written behind the program exactly as it stands in the settings, and a program
that does not finish within the timeout is killed.

A non zero exit code counts as a refusal and the first line of the answer goes into the console of the
ServiceManager. The program is started once per event and never repeated.


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