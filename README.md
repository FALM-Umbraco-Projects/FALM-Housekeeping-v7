***
**PLEASE CREATE NEW DISCUSSIONS ONLY ON MY GITHUB BITBUCKET CHANNEL**
-----------------------------------------------------------------
- _https://github.com/FALM-Umbraco-Projects/FALM-Housekeeping-v7/issues_
- _https://bitbucket.org/amxlab/falm-housekeeping-v7/issues_

**NUGET PACKAGE**
-------------
Now it is available the FALM Housekeeping NuGet Package:
You can find it at: _https://www.nuget.org/packages/FALMHousekeeping/_
Or you can install directly from command line:
_PM> Install-Package FALMHousekeeping -Version 7.6.0.2_
***

**FALM Housekeeping**
=================
Welcome to new generation of our cleaning tools totally rewrite using AngularJS and now in a Custom Section.

This package adds a new section with the following tools:
- Umbraco logs manager: with this tool you can view and delete Umbraco log events.
Now you can manage DB Log and the TraceLog (this is a simplified version of Diplo Trace Log Viewer project - https://our.umbraco.org/projects/developer-tools/diplo-trace-log-viewer/)
- Media folder cleanup: with this tool you can delete those file system folders under '/media' which have no entry in the DB (orphans)
- Delete users: with this tool you can completely remove Umbraco users.
- Version manager: with this tool you can view and cleanup the version history that Umbraco mantains for each content node.

**ATTENTION! To view the new FALM section you must enable it for all users that can use this tool**

**Latest Changes**
==============
- v7.6.0.2 - Updated NuGet Package
- v7.6.0.1 - Minor fixes
- v7.6.0.0 - New version: totally rewrite using AngularJS and now in a Custom Section. Fixed SQLCE compatibility (Not yet for Versions manager)
- v7.0.0.1 - Bug Fix: Resolved Error 404 when click all main headers (Logs, Media, Users, Versions) and resolved conflict in Users Manager

**Version History**
===============
- v7.6.0.2 - For Umbraco v7.5+
- v7.0.0.1 - For Umbraco v7+
- v6.1.3.0 - For Umbraco v6 to v7.2
- v4.11.0.2 - For Umbraco v4.8 to v4.11
- v4.5.0.1 - For Umbraco v4.5 to v4.7.2
- v1.1.0.0 - For Umbraco v3 to v4.0
