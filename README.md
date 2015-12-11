FALM-Housekeeping
=================

This package adds a tree node to the developer section with the following tools:
- Umbraco logs manager: with this tool you can view and delete Umbraco log events.
N.B.: in order to improve the performance of the log viewer, you can add SQL indexes to the coloumns DateStamp, UserId and LogHeader of umbracoLog table.
- Media folder cleanup: with this tool you can delete those file system folders under '/media' which have no entry in the DB (orphans)
N.B.: in the current release this feature works only with "UploadAllowDirectories" set to "true".
- Delete users: with this tool you can completely remove Umbraco users.
- Version manager: with this tool you can view and cleanup the version history that Umbraco mantains for each content node.

Umbraco Package
=================
You can find the latest Umbraco Package in "package" directory

Version History
=================
- v7.0.0.1   - For Umbraco v7
- v6.1.3.0   - For Umbraco v6 to v7
- v4.11.0.2  - For Umbraco v4.8 to v4.11
- v4.5.0.1   - For Umbraco v4.5 to v4.7.2
- v1.1.0.0   - For Umbraco v3 to v4.0
