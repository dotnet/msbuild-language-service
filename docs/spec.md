# MSBuild Language Service

![image](https://user-images.githubusercontent.com/9797472/59120644-cb992380-890a-11e9-8e57-7ad892a5d182.png)

Initial Project Specification

- [MSBuild Language Service](#msbuild-language-service)
  - [Overview](#overview)
    - [Elevator Pitch](#elevator-pitch)
    - [Customers](#customers)
    - [Problem Statement](#problem-statement)
    - [Existing Solutions or Expectations](#existing-solutions-or-expectations)
      - [Migrators](#migrators)
      - [MSBuild language services](#msbuild-language-services)
    - [Goals](#goals)
  - [Requirements](#requirements)
    - [Terminology](#terminology)
    - [Functional Requirements](#functional-requirements)
      - [IN01 - Installation on Visual Studio](#in01---installation-on-visual-studio)
      - [IN02 - Installation on Visual Studio Code](#in02---installation-on-visual-studio-code)
      - [PR01 - Completion for package name when on a package reference node](#pr01---completion-for-package-name-when-on-a-package-reference-node)
      - [PR02 - Completion for a package version when on a package reference node](#pr02---completion-for-a-package-version-when-on-a-package-reference-node)
        - [PR02.1 - General](#pr021---general)
        - [PR02.2 - Pre-release packages](#pr022---pre-release-packages)
      - [PR03 - Go to definition on a package reference](#pr03---go-to-definition-on-a-package-reference)
      - [PR04 - Present package information when the mouse hovers over the package reference](#pr04---present-package-information-when-the-mouse-hovers-over-the-package-reference)
      - [PP01 - Completion for common properties](#pp01---completion-for-common-properties)
      - [PP01.1 - reserved and well-known properties](#pp011---reserved-and-well-known-properties)
      - [PP01.2 - user defined properties](#pp012---user-defined-properties)
      - [PP02 - Present property information when the mouse hovers over a common property](#pp02---present-property-information-when-the-mouse-hovers-over-a-common-property)
      - [PP03 - Go to definition on properties](#pp03---go-to-definition-on-properties)
      - [EX01 - Completion for `Condition` attributes](#ex01---completion-for-condition-attributes)
      - [EX02 - Completion for `$()` properties](#ex02---completion-for--properties)
      - [EX03 - Completion for `@()` items](#ex03---completion-for--items)
      - [EX04 Completion for `%()` item metadata](#ex04-completion-for--item-metadata)
        - [EX04.1 - well-known item metadata](#ex041---well-known-item-metadata)
        - [EX04.2 - user defined item metadata](#ex042---user-defined-item-metadata)
      - [EX05 - Completion for task metadata](#ex05---completion-for-task-metadata)
      - [WRN1 - Fade out unnecessary properties](#wrn1---fade-out-unnecessary-properties)
      - [WRN2 - Warn if property doesn't exist](#wrn2---warn-if-property-doesnt-exist)
  - [Design Decisions](#design-decisions)
    - [Technology Decisions](#technology-decisions)
    - [Architecture Decisions](#architecture-decisions)
    - [Packaging Decisions](#packaging-decisions)
  - [Testing](#testing)
    - [Test Approach](#test-approach)
      - [unit tests](#unit-tests)
      - [functional tests](#functional-tests)
      - [integration tests](#integration-tests)
    - [Test Design](#test-design)
      - [Technology Decisions](#technology-decisions-1)
      - [Architectural Overview](#architectural-overview)

## Overview

### Elevator Pitch

We will build a set of tools to give MSBuild project files modern language tooling wherever people use MSBuild ensuring that modernization of .NET can continue.

### Customers

Most .NET users today use the Visual Studio IDE with smaller numbers using Visual Studio Code (VS Code) and Visual Studio for Mac (VS4MAc). We need to ensure that .NET developers that use these IDEs can be effective with MSBuild project files.

### Problem Statement

MSBuild is a turing-complete build specification language used for all of .NET. Traditionally, the lack of tooling has made working with it difficult for most users to make changes to how their projects build. .NET core introduced a more terse syntax for specifying how projects build while removing most boilerplate statements. This new sdk-style format is highly sought after by developers of both open and closed source software, but there is no definitive way to migrate to it other than modifying large sets of MSBuild files. With the advent of .NET Core, the success of people migrating to this new platform depends on their ability to successfully modify existing projects.

### Existing Solutions or Expectations

There exist several tools that attempt to change the format of MSBuild projects or provide tooling for them.

#### Migrators

- [CsprojToVs2017](https://github.com/hvanbakel/CsprojToVs2017) attempts to migrate your project to the new format using text transformations
- [ProjectSimplifier](https://github.com/srivatsn/ProjectSimplifier) attempts to use the MSBuild evaluation model to move projects to the new format

#### MSBuild language services

- [ProjFileTools](https://github.com/dotnet/ProjFileTools) A Visual Studio extension that provides:
  - completion for package references
  - quickinfo for common elements
  - find all references on properties and targets
  - go to definition on properties and items

- [MonoDevelop.MSBuildEditor](https://github.com/mhutch/MonoDevelop.MSBuildEditor/tree/library) A VS4Mac add in that provides:
  - completion for all MSBuild constructs
  - quickinfo for all common MSBuild constructs
  - completion for package references

- [MSBuild project file tools](https://github.com/tintoy/msbuild-project-tools-vscode) A VS Code extension that provides:
  - completion for package references
  - quickinfo for common elements
  - snippets for common tasks

### Goals

- .NET programmers in their editor of choice can migrate to .NET Core
- MSBuild files can be reasoned about by developers

## Requirements

### Terminology

| Term                     | Definition                                                                                                                                                                                                       |
| ------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| .NET Core                | A Cross platform runtime that can be used on linux, macOS, or windows                                                                                                                                            |
| .NET Core SDK            | A set of tools needed to build .NET Core projects                                                                                                                                                                |
| .NET Framework           | A windows only language runtime                                                                                                                                                                                  |
| arcade                   | A set of common tasks and targets used to build projects in github.com/dotnet, lives [here](http://github.com/dotnet/arcade)                                                                                     |
| Completion               | The common name for suggesting things that could be typed in the current context in an IDE. More info [here](https://docs.microsoft.com/en-us/visualstudio/ide/using-intellisense?view=vs-2015#complete-word)    |
| csproj                   | The file and extension and therefore common name used to refer to project files for the C# programming language                                                                                                   |
| Design-Time Build        | A special build that a code editor starts in MSBuild to calculate the command line options for a compilation. More info [here](https://github.com/dotnet/project-system/blob/master/docs/design-time-builds.md) |
| Expressions              | A series of statements that results in a new value. See [here](https://en.wikipedia.org/wiki/Expression_(computer_science))                                                                                      |
| fsproj                   | The file and extension and therefore common name used to refer to project files for the F# programming language                                                                                                   |
| Go to definition         | See [here](https://docs.microsoft.com/en-us/visualstudio/ide/go-to-and-peek-definition?view=vs-2019)                                                                                                             |
| Items                    | Inputs into a build system, typically files. See [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-items?view=vs-2019)                                                                        |
| Language Server Protocol | This protocol defines the format of messages sent using JSON-RPC between the development tool and the language servers. More info [here](https://microsoft.github.io/language-server-protocol/)                  |
| MSBuild                  | A build engine used in .NET (info [here](https://en.wikipedia.org/wiki/MSBuild))                                                                                                                                 |
| MSBuild Language Service | A term referring to the project in this repo                                                                                                                                                                     |
| Nuget                    | A technology to resolve binary packages for .NET                                                                                                                                                                 |
| nuget.org                | A service to host binary packages                                                                                                                                                                                |
| Package Reference        | A means to declare a binary dependency in a project file                                                                                                                                                         |
| Project Evaluation       | The process MSBuild uses to "compile" projects, targets, and properties into single result.                                                                                                                      |
| Properties               | Name/value pairs used to configure a build (info [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-properties?view=vs-2019))                                                                  |
| sdk-style                | The new project format used for .NET Core projects. Requires a use-sdk attribute in the project xml node                                                                                                        |
| Targets                  | A set of tasks for MSBuild to execute (info [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-targets?view=vs-2019))                                                                          |
| Tasks                    | A generic action that MSBuild need to perform (info [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-tasks?view=vs-2019))                                                                    |
| vbproj                   | The file and extension and therefore common name used to refer to project files for the Visual Basic programming language                                                                                         |
| Visual Studio            | A IDE for windows built using C++ and C#                                                                                                                                                                         |
| Visual Studio Code       | A text editor for Linux, macOS, and Windows built on electron                                                                                                                                                    |
| Visual Studio for Mac    | An IDE for macOS based on the MonoDevelop code base                                                                                                                                                               |
| xUnit                    | The unit testing framework that most of .NET uses                                                                                                                                                                |

### Functional Requirements

| No.                                                                                          | Requirement                                                                  | Priority |
| -------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- | -------- |
| [IN01](#in01---installation-on-visual-studio)                                                | Installation on Visual Studio                                                | 01       |
| [IN02](#in02---installation-on-visual-studio-code)                                           | Installation on Visual Studio Code                                           | 02       |
| [PR01](#pr01---completion-for-package-name-when-on-a-package-reference-node)                 | Completion for package name when on a package reference node                 | 01       |
| [PR02](#pr02---completion-for-a-package-version-when-on-a-package-reference-node)            | Completion for a package version when on a package reference node            | 01       |
| [PR03](#pr03---go-to-definition-on-a-package-reference)                                      | Go to definition on a package reference                                      | 03       |
| [PR04](#pr04---present-package-information-when-the-mouse-hovers-over-the-package-reference) | Present package information when the mouse hovers over the package reference | 02       |
| [PP01](#pp01---completion-for-common-properties)                                              | Completion for common properties                                             | 01       |
| [PP02](#pp02---present-property-information-when-the-mouse-hovers-over-a-common-property)    | Present property information when the mouse hovers over a common property    | 01       |
| [PP03](#pp03---go-to-definition-on-properties)                                               | Go to definition on properties                                               | 03       |
| [EX01](#ex01---completion-for-condition-attributes)                                          | Completion for `Condition` attributes                                        | 01       |
| [EX02](#ex02---completion-for--properties)                                                   | Completion for `$()` properties                                              | 02       |
| [EX03](#ex03---completion-for--items)                                                        | Completion for `@()` items                                                   | 02       |
| [EX04](#ex04-completion-for--item-metadata)                                                  | Completion for `%()` item metadata                                           | 02       |
| [EX05](#ex05---completion-for-task-metadata)                                                 | Completion for task metadata                                                 | 03       |
| [WRN1](#wrn1---fade-out-unnecessary-properties)                                              | Fade out unnecessary properties                                              | 03       |
| [WRN2](#wrn2---warn-if-property-doesnt-exist)                                                | Warn if property doesn't exist                                               | 03       |

#### IN01 - Installation  on Visual Studio

- **Requirement**: *The MSBuild Language Service shall be install-able into Visual Studio 2019 version 16.2 or newer*
- **Validation**: Manually verify that installation succeeds.

#### IN02 - Installation  on Visual Studio Code

- **Requirement**: *The MSBuild Language Service shall be install-able into Visual Studio Code 1.35 or newer*
- **Validation**: Manually verify that installation succeeds.

#### PR01 - Completion for package name when on a package reference node

- **Requirement**: *When the user begins typing within a package-reference-node name attribute the MSBuild Language Service shall provide suggestions for the name based on existing packages defined on nuget.org.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files)

#### PR02 - Completion for a package version when on a package reference node

##### PR02.1 - General

- **Requirement**: *When the user begins typing within a package-reference-node version attribute the MSBuild Language Service shall provide suggestions for the version based on existing released packages defined on nuget.org.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/nuget/guides/api/query-for-all-published-packages)

##### PR02.2 - Pre-release packages

- **Requirement**: *When the user has specified a valid option and the MSBuild Language Service shall provide suggestions for the version based on pre-release packages in addition to release packages.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/nuget/create-packages/prerelease-packages)

#### PR03 - Go to definition on a package reference

- **Requirement**: *When the user invokes the go-to-definition command the MSBuild Language Service shall navigate them to the correct package page on nuget.org.*

#### PR04 - Present package information when the mouse hovers over the package reference

- **Requirement**: *When the user hovers their mouse over a package reference the MSBuild Language Service shall display information about the package from nuget.org.*

#### PP01 - Completion for common properties

#### PP01.1 - Reserved and well-known properties

- **Requirement**: *When the user types in a property context the MSBuild Language Service shall display completions for well-known properties.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-reserved-and-well-known-properties?view=vs-2019)

#### PP01.2 - User defined properties

- **Requirement**: *When the user types in a property context the MSBuild Language Service shall display completions for  properties imported or defined by the user that are in scope.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/property-element-msbuild?view=vs-2019)

#### PP02 - Present property information when the mouse hovers over a common property

- **Requirement**: *When the user hovers their mouse over a common property the MSBuild Language Service shall display information about the property from https://docs.microsoft.com.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-properties?view=vs-2019)

#### PP03 - Go to definition on properties

- **Requirement**: *When the user invoked the go-to-definition command the MSBuild Language Service shall take them to the location where that property value is assigned.*

#### EX01 - Completion for `Condition` attributes

- **Requirement**: *When the user types inside a conditional attribute the MSBuild Language Service shall display completions for defined items that are in scope.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-conditions?view=vs-2019)

#### EX02 - Completion for `$()` properties

- **Requirement**: *When the user types inside a `$()` expression the MSBuild Language Service shall display completions for defined properties that are in scope.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/special-characters-to-escape?view=vs-2019)

#### EX03 - Completion for `@()` items

- **Requirement**: *When the user types inside a `@()` expression the MSBuild Language Service shall display completions for defined items that are in scope.*

#### EX04  Completion for `%()` item metadata

##### EX04.1 - Well-known item metadata

- **Requirement**: *When the user types inside a `%()` expression the MSBuild Language Service shall display completions for well-known item metadata.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-well-known-item-metadata?view=vs-2019)

##### EX04.2 - User defined item metadata

- **Requirement**: *When the user types inside a `%()` expression the MSBuild Language Service shall display completions for defined metadata that are in scope.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/special-characters-to-escape?view=vs-2019)

#### EX05 - Completion for task metadata

- **Requirement**: *When the user types inside a `%()` expression the MSBuild Language Service shall display completions for task metadata that are in scope.*
- **Additional Details**: See documentation [here](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-tasks?view=vs-2019)

#### WRN1 - Fade out unnecessary properties

- **Requirement**: *When the user opens a file that contains properties that are re-defined and re-assigned to values they already have the MSBuild Language Service shall indicate that these properties are unnecessary.*

#### WRN2 - Warn if property doesn't exist

- **Requirement**: *When the user opens a file that contains expressions which refer to non-existent properties the MSBuild Language Service shall indicate that these properties are not defined.*

## Design Decisions

### Technology Decisions

The language-server-protocol is currently the best cross-platform technology we have for building a language service that works in multiple IDEs. It therefore seems like the correct initial approach is to start with an LSP implementation.

### Architecture Decisions

We want the majority of the libraries to be .NET Standard 2.0 if possible to ensure maximum portability. There is interest in other products and components hosting this service in many different ways. Dependencies on external projects should be discouraged.

### Packaging Decisions

arcade will be used to package and deploy this project. It should give us most of the infrastructure for integrating with Visual Studio and shipping.

## Testing

### Test Approach

#### Unit tests

Unit tests should be the main set of test artifacts for the project.

#### Functional tests

Functional testing should still use xunit but create a server and submit requests against that in order to verify that functionality is working.

#### Integration tests

Integration tests should be considered out of scope for this project.

### Test Design

#### Technology Decisions

xunit is the standard for the github.com/dotnet repos and should be the first choice when authoring tests.

#### Architectural Overview

Internal visibility should not be necessary for functional tests as the LSP design should mean that REST requests against the process will be enough. Access to product internals will likely need to happen for unit tests
