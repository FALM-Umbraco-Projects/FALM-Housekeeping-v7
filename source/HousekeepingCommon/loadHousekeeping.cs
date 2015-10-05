using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;

using umbraco;
using umbraco.businesslogic;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;

namespace FALMHousekeeping
{
	/// <summary>
	/// class to create FALM Housekeeping treenode
	/// </summary>
	[Tree("developer", "FALMHousekeeping", "FALM Housekeeping", ".sprTreeFolder", ".sprTreeFolder", "", false, true, 100)]
	public class loadHousekeeping : BaseTree
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="application"></param>
		public loadHousekeeping(string application) : base(application) { }

		/// <summary>
		/// clear actions
		/// </summary>
		/// <param name="actions"></param>
		protected override void CreateRootNodeActions(ref List<IAction> actions)
		{
			actions.Clear();
		}

		/// <summary>
		/// clear actions
		/// </summary>
		/// <param name="actions"></param>
		protected override void CreateAllowedActions(ref List<IAction> actions)
		{
			actions.Clear();
		}

		/// <summary>
		/// create FALM Housekeeping root node
		/// </summary>
		/// <param name="rootNode"></param>
		protected override void CreateRootNode(ref XmlTreeNode rootNode)
		{
			rootNode.Text = "FALM Housekeeping";
			rootNode.NodeID = "init";
			rootNode.NodeType = "init" + TreeAlias;
			rootNode.Source = this.GetTreeServiceUrl("FALM Housekeeping");
			rootNode.Icon = FolderIcon;
			rootNode.OpenIcon = FolderIconOpen;
		}

		/// <summary>
		/// create FALM Housekeeping children nodes
		/// </summary>
		/// <param name="tree"></param>
		public override void Render(ref XmlTree tree)
		{
			switch (this.NodeKey)
			{
			    case "":
			    default:
					// Create tree node LOGS to view a content node xml
					var treeLogs = XmlTreeNode.Create(this);
					treeLogs.NodeID = "Logs";
					treeLogs.Text = "Logs";
					treeLogs.Icon = FolderIcon;
					treeLogs.OpenIcon = FolderIconOpen;
					treeLogs.Source = this.GetTreeServiceUrl("Logs");
					tree.Add(treeLogs);

					// Create tree node MEDIA to view a content node xml
					var treeMedia = XmlTreeNode.Create(this);
					treeMedia.NodeID = "Media";
					treeMedia.Text = "Media";
					treeMedia.Icon = FolderIcon;
					treeMedia.OpenIcon = FolderIconOpen;
					treeMedia.Source = this.GetTreeServiceUrl("Media");
					tree.Add(treeMedia);

					// Create tree node USERS to view a content node xml
					var treeUsers = XmlTreeNode.Create(this);
					treeUsers.NodeID = "Users";
					treeUsers.Text = "Users";
					treeUsers.Icon = FolderIcon;
					treeUsers.OpenIcon = FolderIconOpen;
					treeUsers.Source = this.GetTreeServiceUrl("Users");
					tree.Add(treeUsers);

					// Create tree node VERSIONS to view a content node xml
					var treeVersions = XmlTreeNode.Create(this);
					treeVersions.NodeID = "Versions";
					treeVersions.Text = "Versions";
					treeVersions.Icon = FolderIcon;
					treeVersions.OpenIcon = FolderIconOpen;
					treeVersions.Source = this.GetTreeServiceUrl("Versions");
					tree.Add(treeVersions);
					break;

				case "Logs":
					var xNodeShowLogs = XmlTreeNode.Create(this);
					xNodeShowLogs.NodeID = "Show Logs";
					xNodeShowLogs.Text = "Show Logs";
					xNodeShowLogs.Icon = "FALMHousekeeping/logs_viewer.gif";
					xNodeShowLogs.OpenIcon = "FALMHousekeeping/logs_viewer.gif";
					xNodeShowLogs.Action = "javascript:openHouseKeeping('logs/showLogs.aspx?action=showlogs');";
					tree.Add(xNodeShowLogs);

					var xNodeCleanupLogs = XmlTreeNode.Create(this);
					xNodeCleanupLogs.NodeID = "Cleanup Logs";
					xNodeCleanupLogs.Text = "Cleanup Logs";
					xNodeCleanupLogs.Icon = "FALMHousekeeping/logs_cleanup.gif";
					xNodeCleanupLogs.OpenIcon = "FALMHousekeeping/logs_cleanup.gif";
					xNodeCleanupLogs.Action = "javascript:openHouseKeeping('logs/cleanupLogs.aspx?action=cleanuplogs');";
					tree.Add(xNodeCleanupLogs);
					break;

				case "Media":
					var xNodeMediaCleanupFileSystem = XmlTreeNode.Create(this);
					xNodeMediaCleanupFileSystem.NodeID = "Cleanup File System";
					xNodeMediaCleanupFileSystem.Text = "Cleanup File System";
					xNodeMediaCleanupFileSystem.Icon = "FALMHousekeeping/media_folder_cleanup.gif";
					xNodeMediaCleanupFileSystem.OpenIcon = "FALMHousekeeping/media_folder_cleanup.gif";
					xNodeMediaCleanupFileSystem.Action = "javascript:openHouseKeeping('media/cleanupMediaFS.aspx?action=media');";
					tree.Add(xNodeMediaCleanupFileSystem);
					break;

				case "Users":
					var xNodeDeleteUsers = XmlTreeNode.Create(this);
					xNodeDeleteUsers.NodeID = "Delete Users";
					xNodeDeleteUsers.Text = "Delete Users";
					xNodeDeleteUsers.Icon = "FALMHousekeeping/users_delete.gif";
					xNodeDeleteUsers.OpenIcon = "FALMHousekeeping/users_delete.gif";
					xNodeDeleteUsers.Action = "javascript:openHouseKeeping('users/deleteUsersBySelection.aspx?action=users');";
					tree.Add(xNodeDeleteUsers);
					break;

				case "Versions":
					XmlTreeNode xNodeShowVersions = XmlTreeNode.Create(this);
					xNodeShowVersions.NodeID = "Show Versions";
					xNodeShowVersions.Text = "Show Versions";
					xNodeShowVersions.Icon = "FALMHousekeeping/versions_view.gif";
					xNodeShowVersions.OpenIcon = "FALMHousekeeping/versions_view.gif";
					xNodeShowVersions.Action = "javascript:openHouseKeeping('versions/showVersions.aspx?action=showversions');";
					tree.Add(xNodeShowVersions);

					XmlTreeNode xNodeVersionsCleanupByCount = XmlTreeNode.Create(this);
					xNodeVersionsCleanupByCount.NodeID = "Cleanup by Count";
					xNodeVersionsCleanupByCount.Text = "Cleanup by Count";
					xNodeVersionsCleanupByCount.Icon = "FALMHousekeeping/versions_cleanup.gif";
					xNodeVersionsCleanupByCount.OpenIcon = "FALMHousekeeping/versions_cleanup.gif";
					xNodeVersionsCleanupByCount.Action = "javascript:openHouseKeeping('versions/cleanupVersionsByCount.aspx?action=cleanupversionsbycount');";
					tree.Add(xNodeVersionsCleanupByCount);

					XmlTreeNode xNodeVersionsCleanupByDate = XmlTreeNode.Create(this);
					xNodeVersionsCleanupByDate.NodeID = "Cleanup by Date";
					xNodeVersionsCleanupByDate.Text = "Cleanup by Date";
					xNodeVersionsCleanupByDate.Icon = "FALMHousekeeping/versions_cleanup.gif";
					xNodeVersionsCleanupByDate.OpenIcon = "FALMHousekeeping/versions_cleanup.gif";
					xNodeVersionsCleanupByDate.Action = "javascript:openHouseKeeping('versions/cleanupVersionsByDate.aspx?action=cleanupversionsbydate');";
					tree.Add(xNodeVersionsCleanupByDate);
					break;
			}
		}

		/// <summary>
		/// functions javascript to open pages
		/// </summary>
		/// <param name="Javascript"></param>
		public override void RenderJS(ref StringBuilder Javascript)
		{
			Javascript.Append(@"
				function openHouseKeeping(id) 
				{
					parent.right.document.location.href = 'plugins/FALMHousekeeping/' + id;
				}");
		}
	}
}
