**FALM Housekeeping**
=================
The new version of our cleaning tool is now in a custom section and has been completely rewritten to take advantage of AngularJS

ATTENTION! To view the new FALM section you must enable it for all users that can use this tool.

In the new FALM section you can find the following tools:
- **Umbraco Logs Manager**: to view and delete Umbraco DB Logs events and to view the TraceLog (this new function is a simplified version of Diplo Trace Log Viewer project - https://our.umbraco.org/projects/developer-tools/diplo-trace-log-viewer/)
- **Media Folder Manager**: to delete (in filesystem) under '/media' all folders which have no entry in the DB (orphans)
- **Delete Users Manager**: to remove Umbraco users (except the admin user)
- **Version Manager**: to view and cleanup the version history that Umbraco mantains for each content node

**ATTENTION! To view the new FALM section you must enable it for all users that can use this tool**

**Latest Changes**
==============
- v7.6.0.8  - Fix: highlighting selected node + Update transaltions
- v7.6.0.7  - Fix: removed control and installation of FALM key in core language files. Thanks to Bjarne Fyrstenborg for the support
- v7.6.0.6  - Some UI Fixes. Many thanks to Bjarne Fyrstenborg for the support
- v7.6.0.3  - Updated naming to prevent conflicts
- v7.6.0.2  - Updated NuGet Package
- v7.6.0.1  - Minor fixes
- v7.6.0.0  - New version: totally rewrite using AngularJS and now in a Custom Section. Fixed SQLCE compatibility (Not yet for Versions manager)
- v7.0.0.1  - Bug Fix: Resolved Error 404 when click all main headers (Logs, Media, Users, Versions) and resolved conflict in Users Manager

**Version History**
===============
- v7.6.0.8  - For Umbraco v7.5+
- v7.0.0.1  - For Umbraco v7+
- v6.1.3.0  - For Umbraco v6 to v7.2
- v4.11.0.2 - For Umbraco v4.8 to v4.11
- v4.5.0.1  - For Umbraco v4.5 to v4.7.2
- v1.1.0.0  - For Umbraco v3 to v4.0