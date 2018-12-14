# Kongverge tutorial

Kongverge is a tool to configure a kong server by moving its state into sync with the "desired state" given in configuration files.
If the admin API is configured for Basic Authentication, you can pass a username and password on the command-line.
The password can also be passed via _redirected_ `stdin`, in which case you still need to add the `--password` option but just leave the value blank.
Run `kongverge --help` and `kongverge [command] --help` for documentation on how to pass command-line arguments.
Values for command-line options can be specified by either having a space after the name, or by using `=` or `:` to separate the name from
the value. The exception to this is `--password` which requires a `=` or `:` due to the value being optionally read from _redirected_ `stdin`.

Kongverge supports three commands:
* `run` (Synchronize configuration on the target host from the specified folder)
* `dry-run` (Perform a dry-run synchronize of configuration on the target host from the specified folder (doesn't make any changes to the target host)
* `export` (Export existing configuration from the target host to the specified folder)

Some examples are as follows:
```
echo "123" | kongverge --host kong.mycompany.com --port 9001 --user alice --password run c:\code\kongvergefiles
kongverge --host=kong.mycompany.com --user=alice --password=123 dry-run c:\code\kongvergefiles
kongverge --host:kong.mycompany.com --user:alice --password:123 export c:\code\kongvergefiles
```

## Example 1

If I have e.g. a new Kong server at http://kong.mycompany.com with no routes. It is a default setup with the admin port `8001` open to me.

In order to define a route, e.g. forward requests to `/order/location` to the `orderlocationapi` I then make the following configuration file in a folder e.g. `c:\code\kongvergefiles\orderlocation.json`

```json
{
  "name": "orderlocation",
  "host": "orderlocationapi.mycompany.com",
  "routes": [
    {
      "protocols": [ "http", "https" ],
      "paths": [ "/(?i)order/(?i)location$" ],
      "strip_path": false
    }
  ]
}
```

And I run Kongverge: `kongverge --host kong.mycompany.com --verbose run "c:\code\kongvergefiles"`
it will put the server into the state that the file specifies

The output is:
```
[19:22:16 INF] ************** Kongverge 2.0.0.0 **************
[19:22:16 INF] Performing live integration: Changes will be made to kong.mycompany.com
[19:22:16 INF] Reading files from c:\code\kongvergefiles
[19:22:16 VRB] Reading c:\code\kongvergefiles\orderlocation.json
[19:22:16 INF] Configuration from files contains 1 service, 0 plugins, 1 route
[19:22:16 INF] Reading configuration from Kong
[19:22:16 VRB] Getting plugins from Kong
[19:22:16 VRB] Getting services from Kong
[19:22:16 VRB] Getting routes from Kong
[19:22:16 INF] Configuration from Kong contains 0 services, 0 plugins, 0 routes
[19:22:16 VRB] Target configuration and existing both have zero plugins
[19:22:16 VRB] Converging 1 target service with 0 existing services
[19:22:16 VRB] Creating service {Name: orderlocation} which exists in target configuration but not in Kong
[19:22:16 VRB] Target service {Id: c4875f99-3b4a-460f-a512-8dae4fb7dbaf, Name: orderlocation} and existing both have zero plugins
[19:22:16 VRB] Converging 1 target route attached to service {Id: c4875f99-3b4a-460f-a512-8dae4fb7dbaf, Name: orderlocation} with 0 existing routes
[19:22:16 VRB] Creating route {Hosts: null, Paths: [/(?i)order/(?i)location$], Methods: null, Protocols: [http, https]} attached to service {Id: c4875f99-3b4a-460f-a512-8dae4fb7dbaf, Name: orderlocation} which exists in target configuration but not in Kong
[19:22:16 VRB] Target route {Id: 2127357e-6714-4608-a755-1969d5048918, Hosts: null, Paths: [/(?i)order/(?i)location$], Methods: null, Protocols: [http, https]} attached to service {Id: c4875f99-3b4a-460f-a512-8dae4fb7dbaf, Name: orderlocation} and existing both have zero plugins
[19:22:16 INF] Created 1 service, 0 plugins, 1 route
[19:22:16 INF] Updated 0 services, 0 plugins, 0 routes
[19:22:16 INF] Deleted 0 services, 0 plugins, 0 routes
[19:22:16 INF] ************** Finished **************
```

If I run Kongverge a second time, it finds nothing to do, as the server is already in the desired state and the output is:
```
[19:22:20 INF] ************** Kongverge 2.0.0.0 **************
[19:22:20 INF] Performing live integration: Changes will be made to kong.mycompany.com
[19:22:20 INF] Reading files from c:\code\kongvergefiles
[19:22:20 VRB] Reading c:\code\kongvergefiles\orderlocation.json
[19:22:20 INF] Configuration from files contains 1 service, 0 plugins, 1 route
[19:22:20 INF] Reading configuration from Kong
[19:22:20 VRB] Getting plugins from Kong
[19:22:20 VRB] Getting services from Kong
[19:22:20 VRB] Getting routes from Kong
[19:22:20 INF] Configuration from Kong contains 1 service, 0 plugins, 1 route
[19:22:20 INF] Target configuration and existing both have zero plugins
[19:22:20 VRB] Converging 1 target service with 1 existing service
[19:22:20 VRB] Identical service {Id: c4875f99-3b4a-460f-a512-8dae4fb7dbaf, Name: orderlocation} found in Kong matching target configuration
[19:22:20 VRB] Target service {Id: c4875f99-3b4a-460f-a512-8dae4fb7dbaf, Name: orderlocation} and existing both have zero plugins
[19:22:20 VRB] Converging 1 target route attached to service {Id: c4875f99-3b4a-460f-a512-8dae4fb7dbaf, Name: orderlocation} with 1 existing route
[19:22:20 VRB] Identical route {Id: 2127357e-6714-4608-a755-1969d5048918, Hosts: null, Paths: [/(?i)order/(?i)location$], Methods: null, Protocols: [http, https]} attached to service {Id: c4875f99-3b4a-460f-a512-8dae4fb7dbaf, Name: orderlocation} found in Kong matching target configuration
[19:22:20 VRB] Target route {Id: 2127357e-6714-4608-a755-1969d5048918, Hosts: null, Paths: [/(?i)order/(?i)location$], Methods: null, Protocols: [http, https]} attached to service {Id: c4875f99-3b4a-460f-a512-8dae4fb7dbaf, Name: orderlocation} and existing both have zero plugins
[19:22:20 INF] Created 0 services, 0 plugins, 0 routes
[19:22:20 INF] Updated 0 services, 0 plugins, 0 routes
[19:22:20 INF] Deleted 0 services, 0 plugins, 0 routes
[19:22:20 INF] ************** Finished **************
```


## Healthcheck

A health check that is terminated at kong, using a plugin.

```json
{
  "name": "healthcheck",
  "host": "www.example.com",
  "port": 80,
  "protocol": "http",
  "retries": 5,
  "connect_timeout": 60000,
  "write_timeout": 60000,
  "read_timeout": 60000,
  "plugins": [
    {
      "name": "request-termination",
      "config": {
        "message": "All good",
        "status_code": 200
      }
    }
  ],
  "routes": [
    {
      "protocols": [ "http", "https" ],
      "paths": [ "/health/check" ],
      "regex_priority": 20,
      "strip_path": true
    }
  ]
}
```
Plugins can also be nested within specific routes in order to apply them only on that route.
