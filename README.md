<img src="Kongverge.png" align="right" />

[![NuGet](https://img.shields.io/nuget/v/kongverge.svg?maxAge=3600)](https://www.nuget.org/packages/kongverge/)
[![Build status](https://ci.appveyor.com/api/projects/status/github/justeat/kongverge?branch=master&svg=true)](https://ci.appveyor.com/project/justeattech/kongverge/branch/master)

# Kongverge

A Desired State Configuration tool for Kong.

A command-line tool written in cross-platform [.NET Core 2.1](http://dot.net).

[Tutorial](Tutorial.md)

## Installation

Kongverge is built [as a .NET core global tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools).

You can install `kongverge` as a global tool as follows:

* First, have [.NET Core 2.1 or later installed](https://www.microsoft.com/net/download). At a commandline, `dotnet --list-runtimes` should succeed and show an item for `Microsoft.NETCore.App 2.1.0` or later.
* Install with `dotnet tool install kongverge --global`.
* You should then be able to run `kongverge` from the commandline e.g. `kongverge --help`

Other operations:

* Update to latest: `dotnet tool update kongverge --global`
* Uninstall: `dotnet tool uninstall kongverge --global`

This should all work on development and build machines, on windows, mac and linux, as long as .NET Core 2.1 is installed.

## Kong DTOs

Kongverge uses several DTOs to read from files and write to Kong (and vice versa). For simplicity, the field names on these objects generally match what is present in Kong. See `KongConfiguration`, `KongRoute`, `KongService`, `KongPlugin` which are used to serialise these kong concepts.

These objects also handle matching - i.e. reconciling the state described by files with the state in Kong, and performing actions in Kong as needed to make them the same. The possible cases for these objects are:

* Unchanged; The object in Kong is identical to the object in config, so no action is required.
* Changed; the object in Kong is matched with an object in config, but not all of the properties are the same. Action is required to update the object in place.
* New; the object needs to be added to Kong.
* Deleted; the object needs to be removed from Kong.

Kong's plugin model is more complex, as each plugin has its own set of properties used to configure it. We model this as a `Dictionary<string, object>` and the equality comparison checks the value of this object graph.
Beware that in order to prevent a plugin from being seen as different when it is actually the same, all optional values need to be supplied in the input files.
