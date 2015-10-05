<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cleanupVersionsByDate.aspx.cs" Inherits="FALMHousekeepingVersionsManager.cleanupVersionsByDate" MasterPageFile="../../../masterpages/umbracoPage.Master" Title="F.A.L.M. Housekeeping - Cleanup Versions by Date" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols.DatePicker" Assembly="controls" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="cphBody" ContentPlaceHolderID="body" runat="server">

	<cc1:UmbracoPanel ID="umbPanelCleanupVersionsByDate" Text="F.A.L.M. Housekeeping" runat="server" Width="612px" Height="600px" hasMenu="false">
		
		<cc1:Pane ID="PanelCleanupVersionsByDate" runat="server" Text="Cleanup versions by Date">
			<asp:Label ID="lblSubTitle" runat="server"></asp:Label>
			<asp:Label ID="lblWarning" runat="server"></asp:Label>
			<asp:Label ID="ltrlInfoMessage" runat="server" Text="" />
			<p>
				<span style="float: left;"><asp:Label ID="lblDeleteAllVersionsUpTo" runat="server"></asp:Label>&nbsp;&nbsp;&nbsp;</span>
				<cc2:DateTimePicker ID="dtpckrDate" runat="server" ShowTime="false" />&nbsp;&nbsp;&nbsp;
				<asp:Button ID="btnClearVersions" runat="server" Text="Delete" OnClick="btnClearVersions_Click" />
			</p>
				
			<p>&nbsp;</p>
				
			<p><asp:Literal ID="ltrlVersions" runat="server" Text="" /></p>
		
		</cc1:Pane>
	
	</cc1:UmbracoPanel>

</asp:Content>