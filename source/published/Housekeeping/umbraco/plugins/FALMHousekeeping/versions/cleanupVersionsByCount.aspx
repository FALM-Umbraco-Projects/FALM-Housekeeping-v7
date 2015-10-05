<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cleanupVersionsByCount.aspx.cs" Inherits="FALMHousekeepingVersionsManager.cleanupVersionsByCount" MasterPageFile="../../../masterpages/umbracoPage.Master" Title="F.A.L.M. Housekeeping - Cleanup Versions by Count" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="cphBody" ContentPlaceHolderID="body" runat="server">

	<cc1:UmbracoPanel ID="umbPanelCleanupVersionsByCount" Text="F.A.L.M. Housekeeping" runat="server" Width="612px" Height="600px" hasMenu="false">
		
		<cc1:Pane ID="PanelCleanupVersionsByCount" runat="server" Text="Cleanup versions by Count">
			<asp:Label ID="lblSubTitle" runat="server"></asp:Label>
			<asp:Label ID="lblWarning" runat="server"></asp:Label>
			<asp:Label ID="ltrlInfoMessage" runat="server" Text=""></asp:Label>
			<p>
				<asp:Label ID="lblNumberOfVersionsToKeep" runat="server"></asp:Label>&nbsp;&nbsp;&nbsp;
				<asp:TextBox ID="txtNVer" Width="20" MaxLength="2" runat="server" />&nbsp;&nbsp;&nbsp;
				<asp:Button ID="btnClearVersions" runat="server" Text="Delete" OnClick="btnClearVersions_Click" />
			</p>
				
			<p>&nbsp;</p>
				
			<p><asp:Literal ID="ltrlVersions" runat="server" Text="" /></p>
		
		</cc1:Pane>
	
	</cc1:UmbracoPanel>

</asp:Content>