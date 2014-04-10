This is Azure Publishing Console App
====================================

Copyright (c) 2001, 2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009, 2010, 2011,
2012, 2013 Python Software Foundation.  All rights reserved.

Build Instructions
------------------

Prior to running this program, a publish.config file must be created.
An example publish.config file with the required format could be found in the project directory.
It contains settings that serves as argument or parameters to the program.

Settings includes:
 serviceDefinition.csdef file path
 web role project .csproj file path
 azure dbase account parameters 
 e.t.c.

 Modifying the format or not specifying the correct values may cause gotchas in program execution.
