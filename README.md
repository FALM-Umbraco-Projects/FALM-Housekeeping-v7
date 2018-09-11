**FALM Housekeeping**
=================
The new version of our cleaning tool is now in a custom section and has been completely rewritten to take advantage of AngularJS

_To view the FALM section Umbraco Administrators must enable it for all users that can use the tool_

**_WARNING! Before install current version I recommend to uninstall all previous versions of FALM Housekeeping to remove old and unused references and files_**

In the new FALM section you can find the following tools:
- **Umbraco Logs Manager**: to view and delete Umbraco DB Logs events and to view the TraceLog (this new function is a simplified version of Diplo Trace Log Viewer project - https://our.umbraco.org/projects/developer-tools/diplo-trace-log-viewer/)
- **Media Folder Manager**: to delete (in filesystem) under '/media' all folders which have no entry in the DB (orphans)
- **Delete Users Manager**: to remove Umbraco users (except the admin user)
- **Version Manager**: to view and cleanup the version history that Umbraco maintains for each content node
- **Recycle Bin Manager**: to cleanup Content and Media Recycle Bin
- **Cache and TEMP directories Manager**: to cleanup "App_Data/cache" and "App_Data/TEMP" directories on File System<br /><br />

**_TESTED WITH: Umbraco v7.7.13 - v7.8.3 - v7.9.6 - v7.10.4 - v7.11.1 - v7.12.2_**


**Latest Changes**
==============
- v7.7.2.4 - Enhancement: Versions manager, now use the core pagination component and umb-table layout
- v7.7.2.3 - Enhancement: Logs manager, now use the core pagination component and umb-table layout
- v7.7.2.2 - Various Fixes
	- Fix: Resolved issue #50 (GitHub)
	- Fix: Resolved issue #51 (GitHub)
	- Fix: Resolved issue #52 (GitHub)
	- Fix: Resolved issue #54 (GitHub)
	- Fix: Resolved issue #55 (GitHub)
- v7.7.2.1 - Various Fixes
    - Fix: Resolved issue #48 (GitHub) in Versions manager UI
    - Fix: DBLogs manager UI
- v7.7.2.0 - New feature and Various Fixes
    - New feature: Cache and TEMP directories Manager
        - Clean "App_Data/cache" and "App_Data/TEMP" directories on File System
        - Possibility to create a Service Page to auto clean "App_Data/cache" directory on File System. This page can be used with a scheduler (for example I use "Url Task Scheduler For V7" https://our.umbraco.org/projects/backoffice-extensions/url-task-scheduler-for-v7/) to be able to schedule automatic cleaning of both recycle bins.<br />
          This action will create 2 FALM DocumentTypes, 1 FALM Template and 2 FALM nodes, simply by clicking a button.<br />
          **_After creation of the Service Page, I recommend to add all FALM nodes and template urls in the robots.txt file to avoid being indexed by search engines_**
    - Fix: Recycle Bins Cleanup Service Page
- v7.7.1.0 - New feature and Various Fixes
    - New: Recycle Bin Manager
        - Clean Content and Media Recycle Bins
        - Possibility to create a Service Page to auto clean both recycle bins. This page can be used with a scheduler (for example I use "Url Task Scheduler For V7" https://our.umbraco.org/projects/backoffice-extensions/url-task-scheduler-for-v7/) to be able to schedule automatic cleaning of both recycle bins.<br />
          This action will create 2 FALM DocumentTypes, 1 FALM Template and 2 FALM nodes, simply by clicking a button.<br />
          **_After creation of the Service Page, I recommend to add all FALM nodes and template urls in the robots.txt file to avoid being indexed by search engines_**
    - Fix: Resolved issue #45 (GitHub) with bootstrap-datapicker. Removed reference and files from package because the stile affected Umbraco core datepicker
    - Fix: Updated UI
    - Fix: Updated User Manager
- v7.7.0.2 - Fix: Versions Manager now works with SQLCE
- v7.7.0.1 - Bug: Resolved issue #43 (GitHub) in Versions Manager
- v7.7.0.0 - Various Fixes
    - Bug: Resolved issue #40 (GitHub) in Users manager
    - Fix: Updated Versions manager UI
    - Fix: Updated TraceLogs manager UI
    - Bug: Resolved issue #41 (GitHub) in Media manager (Thank you to TFAstudio for the support)
    - Fix: Updated Media manager UI

**Version History**
===============
- v7.7.2.4 - For Umbraco v7.7+
- v7.7.2.1_for_Umbraco_v7_6_13 - For Umbraco v7.6.0 to v7.6.13
- v7.7.2.0  - For Umbraco v7.6+
- v7.6.0.3  - For Umbraco until v7.5.14
- v7.0.0.1  - For Umbraco v7+
- v6.1.3.0  - For Umbraco v6 to v7.2
- v4.11.0.2 - For Umbraco v4.8 to v4.11
- v4.5.0.1  - For Umbraco v4.5 to v4.7.2
- v1.1.0.0  - For Umbraco v3 to v4.0