## MonoDevelop.Xml

This is the XML language service from [MonoDevelop](https://github.com/mono/monodevelop). It has been
extracted and the MonoDevelop dependencies have been removed. The parser has no external dependencies, and
the code completion has been ported to the [ Visual Studio Editor core](https://github.com/microsoft/vs-editor-api).

## Port Status

### Complete

* Parser

### In Progress

* Completion

### TODO

* Formatting
* Settings
* Indenter

## Build Status

Status | Platform | Runtimes
--- | --- | ---
[![Build Status](https://travis-ci.org/mhutch/MonoDevelop.Xml.svg?branch=master)](https://travis-ci.org/mhutch/MonoDevelop.Xml) | Linux | Mono
[![Build status](https://ci.appveyor.com/api/projects/status/wcr0yvau0y5vs81j?svg=true)](https://ci.appveyor.com/project/mhutch/monodevelop-xml) | Windows | .NET Framework
