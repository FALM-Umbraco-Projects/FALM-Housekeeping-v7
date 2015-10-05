<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="showVersions.aspx.cs" Inherits="FALMHousekeepingVersionsManager.showVersions" MasterPageFile="../../../masterpages/umbracoPage.Master" Title="F.A.L.M. Housekeeping - Show versions" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols.DatePicker" Assembly="controls" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="cphBody" ContentPlaceHolderID="body" runat="server">

	<cc1:UmbracoPanel ID="umbPanelShowVersions" Text="F.A.L.M. Housekeeping" runat="server" Width="612px" Height="600px" hasMenu="false">
		
		<cc1:Pane ID="PanelShowVersions" runat="server" Text="Show versions">
			<asp:Label ID="lblSubTitle" runat="server"></asp:Label>
			<table>
				<tr>
					<td><small><asp:Label ID="lblFilterByNodeId" runat="server"></asp:Label></small></td>
					<td><small><asp:Label ID="lblFilterByDateRangeFrom" runat="server"></asp:Label></small></td>
					<td><small><asp:Label ID="lblFilterByDateRangeTo" runat="server"></asp:Label></small></td>
				</tr>
				<tr>
					<td>
						<asp:TextBox ID="txtbNodeID" runat="server" />
					<//td>
					<td>
						<cc2:DateTimePicker ID="dtpckrDateFrom" runat="server" ShowTime="false" />
					</td>
					<td>
						<cc2:DateTimePicker ID="dtpckrDateTo" runat="server" ShowTime="false" />
					</td>
				</tr>
				<tr>
					<td colspan="4">
						<asp:Button ID="btnShowVersions" runat="server" Text="Show versions" OnClick="btnShowVersions_Click" />
					</td>
				</tr>
			</table>

			<p>&nbsp;</p>
				
			<p><asp:Literal ID="ltrlVersions" runat="server" Text="" /></p>
				
			<p><asp:Literal ID="ltrlTblVersions" runat="server" Text="" /></p>

			<asp:GridView ID="gvCurVer" runat="server" AutoGenerateColumns="False" AllowPaging="True" BackColor="#FFFFFF" BorderColor="#DEDFDE" BorderWidth="1px" CellPadding="4" ForeColor="Black" Width="100%" DataSourceID="SqlDSCurrentVersion" OnRowDataBound="gvCurVer_RowDataBound" OnPageIndexChanging="gvCurVer_PageIndexChanging" Visible="False" >
				<RowStyle BackColor="#F7F7DE" VerticalAlign="Top" />
				<HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" HorizontalAlign="Center" Wrap="false" />
				<AlternatingRowStyle BackColor="#FFFFFF" VerticalAlign="Top" />
				<SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="#FFFFFF" />
				<PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" position="Bottom" pagebuttoncount="10"/>

				<PagerStyle BackColor="#6B696B" ForeColor="#FFFFFF" HorizontalAlign="Right" />
				<EmptyDataRowStyle BorderStyle="None" />
				<Columns>
					<asp:TemplateField HeaderText="Current published version">
						<ItemTemplate>
							<%# Eval("text", "{0}") %> <small>(Id: <asp:Label ID="lblNodeId" Text='<%# Eval("nodeId", "{0}") %>' runat="server" /> - Created: <%# convertToShortDateTime(Eval("updateDate","{0}"))%>)</small></ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Version history">
						<ItemTemplate>
							<asp:Literal ID="ltrlHistVer" Text="" runat="server" />
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
				<EmptyDataTemplate>
					<strong><asp:Label ID="lblNoVersionsToShow" runat="server"></asp:Label></strong>
				</EmptyDataTemplate>
				<FooterStyle BackColor="#CCCC99" />
			</asp:GridView>
			<asp:SqlDataSource ID="SqlDSCurrentVersion" runat="server" ConnectionString="<%$ ConnectionStrings:umbracoDbDSN %>" SelectCommand="" />
		
		</cc1:Pane>
	
	</cc1:UmbracoPanel>

</asp:Content>