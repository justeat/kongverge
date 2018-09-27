# Kongverge tutorial

Kongverge is a tool to configure a kong server by moving its state into sync with the  "desired state" given in configuration files.


## Example 1


If I have e.g. a new Kong server at http://kong.mycompany.com with no routes. It is a default setup with the admin port `8001` open to me.

In order to define a route, e.g. forward requests to `/order/location` to the `orderlocationapi` I then make the following configuration file in a folder e.g. `c:\code\kongvergefiles\orderlocation.json`

````json
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
````

And I run Kongverge: `dotnet run --host kong.mycompany.com --port 8001 --input "c:\code\kongvergefiles"`
it will put the server into the state that the file specifies

The output is:
````
[14:43:49 INF] Reading files from c:\code\kongvergefiles
[14:43:50 INF] Reading c:\code\kongvergefiles\orderlocation.json
[14:43:51 INF] Adding service orderlocation
[14:43:51 INF] Adding route Paths: [/(?i)order/(?i)location$], Methods: [], Protocols: [http, https]
[14:43:52 INF] Finished
````

If I run Kongverge a second time, it finds nothing to do, as the server is already in the desired state and the output is:
````
[12:02:21 INF] Reading files from c:\code\kongvergefiles
[12:02:21 INF] Reading c:\code\kongvergefiles\orderlocation.json
[12:02:22 INF] Finished
````


## Healthcheck

A health check that is terminated at kong, using a plugin.

````json
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
  ]
  "routes": [
    {
      "protocols": [ "http", "https" ],
      "paths": [ "/health/check" ],
      "regex_priority": 20,
      "strip_path": true
    }
  ]
}
````

