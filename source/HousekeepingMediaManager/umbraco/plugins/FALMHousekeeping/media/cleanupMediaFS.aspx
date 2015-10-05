<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cleanupMediaFS.aspx.cs" Inherits="FALMHousekeepingMediaManager.cleanupMediaFS" MasterPageFile="../../../masterpages/umbracoPage.Master" Title="F.A.L.M. Housekeeping - Cleanup Media File System" %>

<%@ Register TagPrefix="cc1" Assembly="controls" Namespace="umbraco.uicontrols" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="head" runat="server" />

<asp:Content ID="cphBody" ContentPlaceHolderID="body" runat="server">
	<cc1:UmbracoPanel ID="umbPanelCleanupMediaFS" Text="F.A.L.M. Housekeeping" runat="server" Width="612px" Height="600px" hasMenu="false">
		<cc1:Pane ID="PanelCleanupMediaFS" Style="padding-right: 10px; padding-left: 10px; padding-bottom: 10px; padding-top: 10px;" runat="server" CssClass="innerContent">
			<asp:Label ID="lblSubTitle" runat="server"></asp:Label><br />
			<p>
                <%--Il sito contiene data type di tipo "Image Cropper"?&nbsp;&nbsp;&nbsp;
                Si <asp:RadioButton ID="radioImageCropperSi" GroupName="ImageCropper" runat="server" />&nbsp;&nbsp;
                No <asp:RadioButton ID="radioImageCropperNo" GroupName="ImageCropper" Checked="true" runat="server" />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;--%>
			    <asp:Button ID="btnCheckOrphan" runat="server" OnClick="btnCheckOrphan_Click" />
			</p>
			<p>
				<asp:Literal ID="ltrlDeletable" Text="" runat="server" /></p>
			<p>
				<asp:Literal ID="ltrlWarning" Text="" runat="server" /></p>
			<p>
				<asp:Button ID="btnDeleteOrphan" runat="server" Visible="false" OnClick="btnDeleteOrphan_Click" /></p>
		</cc1:Pane>
	</cc1:UmbracoPanel>
</asp:Content>
