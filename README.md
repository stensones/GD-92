# GD-92
Want to create your own GD-92 solution in .NET Core? This GitHub project implments the core of the [GD-92 v2.2 specification](https://github.com/stensones/GD-92/blob/master/gd92.pdf)

## NuGet packages
The Domain model for you to use in your projects. This contains the `entities`, `value objects`, etc.. that GD-92 defines.

### Stensones.GD92.Fields
Defines every GD-92 field

### Stensones.GD92.Messages
Defines every GD-92 message

## GR-92 Implementation Projects
A pile of Docker images where each image is a router, MTA, or UA.
All images share a network (via docker Compose or K8?)

### General Solution layout
Clean architecture; every solution has the following projects (and thier corresponding unit tests in a `[project].tests.unit` project)

 - The API (all UAs, MTAs and the router expose a REST API)
   - `[project].api
   - a ASP.NET Core web API project, this hosts the REST API
 - The Application project
   - `[project].application
 - The Infrastucture project
   - `[project].infrastucture
 - The Domain project
   - `[project].domain
   - this uses the domain NuGet project.

``` mermaid
graph LR;

API["REST API (ASP.NET Core API)"];
App["Application"];
Dom["Domain"];
Inf["Infrastucture"];

API --> App --> Dom;
App --> Inf;
```

These projects run in a docker image (See the `Dockerfile`). Each docker image exposes a REST API, the various containers use this to implement IPC between the apps. If using docker-compose the need to share a network for this to work! **TBC**.

### Router
Implements the GD-92 router. This is port 25 in GD-92 definition. Exposed as TCP port 10025 ?

### LAN MTA
Implements a LAN MTA to allow the host machine (of the docker images) to communicate with another GD-92 node on annother machine in your network.

### NM UA
Implements a node mananger UA, requires some sort of persistns storage for node config, files? or Mongo DB? **TBC**

### IO UA
**TBC**
### Sim UA
A UI that can display received GD-92 messages, and can create and send GD-92 messages. **TBC**
