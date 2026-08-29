# GD-92
Want to create your own GD-92 solution in .NET Core? This GitHub project implments the core of the [GD-92 v2.2 specification](https://github.com/stensones/GD-92/blob/master/gd92.pdf) or it will if I ever find the time to complete it!

**WARNING** Right now this is very incomplete!

## Physical solution/project layout

flatish structure. One folder per .NET project, with the same name as the project without the company name and application name prefixes (for example a .NET project called `Stensones.GD92.Domain.One` would be within a foldder called 'Domain.One' )

When adding a new project allways add a corresponding unit test project with a name the samme as the project it tests suffixed with `.Tests.Unit` The project being added should be marked as having its interals visible to that unit test project.

Unit test projects should use assertions from the NuGet library `AwesomeAssertions`

Integration test projects should have the naming converntion of '*.Tests.Integration'
Integration test projects should use ReqnRoll NuGet package to surface tests as `.feature' files with Gherkin syntax.

solution structure: **TBC**


This project is build using BDD. Before creating production code write Gherkin test in the appropriate integration test project, then use TDD to implement it.
Using TDD we write unit tests for desired new production code funcionality before writing code to satisfy those tests.
TDD is an inner loop to the BDD for a scenario.