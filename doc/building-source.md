# Building `neoteric` from source

[Updated 19 Jan 2025]

If you need to build `neoteric` from source code, the following instructions should be helpful.  If they are unclear, let us know (or better yet, change it and create apull request).

## Tools

The development setup for `neoteric` is to use Visual Studio 2022 (Community edition, which is free, is fine) on Windows.
The source code tree can be retrieved using `gitfo` (details below)

VSCode or the command line would work, it's just not part of the dev team's process so it's not documented.  Same goes for building on Linux or Mac.  It should work just fine, but you'll likely have to do some different procedures than what is outlined below.

For targeting the Meadow F7 hardware platform with Visual Studio 2022, you will want to install the Wilderness Labs CLI and extensions.  [Details on doing that are available on their web site](https://developer.wildernesslabs.co/Meadow/Getting_Started/MCUs/F7_Feather/).  **Go install those tools before continuing below**.

## Required Source

As of now, `neoteric` use direct project references to several libraries in the Meadow stack.  Yes, this can be a bit of a pain for new developers, but it allows us to debug and make necessary modification to the Meadow libraries while we work, and therefore saves the dev team work and headache.  We'll look at providing tools to allow direct Nuget references if there's a desire for it.

Pulling all of the required source is simple using a tool called [`gitfo`](https://github.com/adrianstevens/Gitfo).  Basically `gitfo` allows you to pull source from multiple repositories with one command.

First, create an empty directory on your development machine for the source code.  We'll assume you put it at `C:\source` for these samples.  Adjust that path to match whatever you actually use.

Next, install `gitfo` from the Command Line:

```
> dotnet tool install --global RadishTools.Gitfo --version 0.3.0
```

Now download or create a file named `.gitfo` (starts with a period character) in that folder [with the contents found here].

In the command window, navigate to the folder and run `gitfo sync`

```
> cd C:\source
> gitfo sync
| Gitfo v0.3.0
|
| Sync Meadow.Contracts...succeeded
| Sync Meadow.Core...succeeded
| Sync Meadow.Foundation...succeeded
| Sync Meadow.Logging...succeeded
| Sync Meadow.Modbus...succeeded
| Sync Meadow.Units...succeeded
| Sync MQTTnet...succeeded
| Sync neoteric...succeeded
|
| Repo name                      | Current branch                 | Ahead  | Behind | Dirty  |
| ------------------------------ | ------------------------------ | ------ | ------ | ------ |
| Meadow.Contracts               | develop                        | 0      | 0      | False  |
| Meadow.Core                    | develop                        | 0      | 0      | False  |
| Meadow.Foundation              | develop                        | 0      | 0      | False  |
| Meadow.Logging                 | develop                        | 0      | 0      | False  |
| Meadow.Modbus                  | develop                        | 0      | 0      | False  |
| Meadow.Units                   | develop                        | 0      | 0      | False  |
| MQTTnet                        | develop                        | 0      | 0      | False  |
| neoteric                       | main                           | 0      | 0      | False  |
```

Now open the `neoteric` solution with Visual Studio.  It will be at `c:\source\yoshimoshi\neoteric\src\neoteric.sln`.

You can now build and test your specific application.  For example, if you are using the Transfer Case Controller, navigate to `Modules/TransferCase/src/Neoteric.TransferCase.F7`, set is as the startup project and build it.

Once it has built, you can modify and deploy the application as normal.  Again, using the Transfer Case Controller as an example, plug the device in via USB, make sure you have the right port selcted in the toolbar, then right-click the `Neoteric.TransferCase.F7` project and select `deploy`.  It will build and deploy the app to the hardware.