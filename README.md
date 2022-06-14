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
   - `[project].
 - The Application project
   - `[project].
 - The Infrastucture project
   - `[project].
 - The Domain project
   - `[project].domain
   - this uses the domain NuGet project.
### Router
### LAN MTA
### NM UA
### IO UA
### Sim UA

Stensones.GD92.Domain
Stensones.GD92.Domain.Tests.Unit

Stensones.GD92.Domain.Application

Stensones.GD92.Domain.Api

Stensones.GD92.Infrastucture