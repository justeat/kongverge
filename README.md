<img src="Kongverge.png" align="right" />

[![NuGet](https://img.shields.io/nuget/v/kongverge.svg?maxAge=3600)](https://www.nuget.org/packages/kongverge/)
[![Build status](https://ci.appveyor.com/api/projects/status/github/justeat/kongverge?branch=master&svg=true)](https://ci.appveyor.com/project/justeattech/kongverge/branch/master)

# Kongverge

A Desired State Configuration tool for [Kong](https://github.com/kong/kong).

A command-line tool written in cross-platform [.NET Core 2.2](http://dot.net).

See the [tutorial](Tutorial.md) for examples of how to use Kongverge.

## Installation

Kongverge is built [as a .NET core global tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools).

You can install `kongverge` as a global tool as follows:

* First, have [.NET Core SDK 2.1 or later installed](https://www.microsoft.com/net/download). At a commandline, `dotnet --list-sdks` should succeed and show an item for `2.1.0` or later.
* Install with `dotnet tool install kongverge --global`.
* You should then be able to run `kongverge` from the commandline e.g. `kongverge --help`

Other operations:

* Update to latest: `dotnet tool update kongverge --global`
* Uninstall: `dotnet tool uninstall kongverge --global`

This should all work on development and build machines, on Windows, Mac and Linux, as long as the _.NET Core 2.1 SDK_ is installed. Alternatively, this can be built as a standard _.NET Core_ application
which only requires the _.NET Core runtime_ rather than the _SDK_ (technically you only need the SDK to _install/update/uninstall_ a .NET core global tool - _running_ it only requires the runtime).

## Kong DTOs

Kongverge uses several DTOs to read from files and write to Kong (and vice versa). For simplicity, the field names on these objects generally match what is present in Kong.
See `KongConfiguration`, `KongRoute`, `KongService`, `KongPlugin` which are used to serialize these kong concepts.

These objects also handle matching - i.e. reconciling the state described by files with the state in Kong, and performing actions in Kong as needed to make them the same. The possible cases for these objects are:

* Unchanged; The object in Kong is identical to the object in config, so no action is required.
* Changed; the object in Kong is matched with an object in config, but not all of the properties are the same. Action is required to update the object in place.
* New; the object needs to be added to Kong.
* Deleted; the object needs to be removed from Kong.

Kongverge is smart enough to perform some basic normalization when comparing object graphs to find differences: It does some basic sorting of arrays and sorting of object properties during comparison.

Kong's plugin model is more complex, as each plugin has its own set of properties used to configure it. We model this as a `JObject` and the equality comparison checks a normalized version of this object graph.

Kongverge is smart enough to validate these configuration objects when reading from files, taking into account that some properties have default values if omitted (in the case of plugins, it does this by
inspecting the associated schema object), and thus will **not** show an omitted default value as _changed_.

## Testing

Integration tests exist to test Kongverge against a real instance of Kong running in Docker. In order to build the integration tests project, you will need to have Docker installed locally.
For development and CI-build purposes, the following steps are required:

* Before running the integration tests, start the docker services by running `docker-compose start` from within the integration tests project folder.
* After running the integration tests (or when required), stop the docker services by running `docker-compose stop` from within the integration tests project folder.
