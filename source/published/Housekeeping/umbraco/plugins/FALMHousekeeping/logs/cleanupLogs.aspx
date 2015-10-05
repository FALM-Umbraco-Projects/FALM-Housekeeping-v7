<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cleanupLogs.aspx.cs" Inherits="FALMHousekeepingLogsManager.cleanupLogs" MasterPageFile="../../../masterpages/umbracoPage.Master" Title="F.A.L.M. Housekeeping - Cleanup Logs" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols.DatePicker" Assembly="controls" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="cphBody" ContentPlaceHolderID="body" runat="server">

	<cc1:UmbracoPanel ID="umbPanelCleanupLogs" Text="F.A.L.M. Housekeeping" runat="server" Width="612px" Height="600px" hasMenu="false">
		
		<cc1:Pane ID="PanelCleanupLogs" runat="server" Text="Cleanup logs">
			<asp:Label ID="lblSubTitle" runat="server"></asp:Label>
			<table>
				<tr>
					<td><small><asp:Label ID="lblFilterByNodeId" runat="server"></asp:Label></small></td>
					<td><small><asp:Label ID="lblFilterByUser" runat="server"></asp:Label></small></td>
					<td><small><asp:Label ID="lblFilterByLogType" runat="server"></asp:Label></small></td>
					<td><small><asp:Label ID="lblFilterByDateRangeFrom" runat="server"></asp:Label></small></td>
					<td><small><asp:Label ID="lblFilterByDateRangeTo" runat="server"></asp:Label></small></td>
				</tr>
				<tr>
					<td>
						<asp:TextBox ID="txtbNodeID" runat="server" />
					<//td>
					<td>
						<asp:DropDownList ID="ddlUsers" AppendDataBoundItems="true" runat="server" DataSourceID="SqlDSUsers" DataTextField="userName" DataValueField="Id">
						</asp:DropDownList>
						<asp:SqlDataSource ID="SqlDSUsers" runat="server" ConnectionString="<%$ ConnectionStrings:umbracoDbDSN %>" SelectCommand="SELECT [Id], [userName] FROM [umbracoUser] ORDER BY [userName]" />
					</td>
					<td>
						<asp:DropDownList ID="ddlLogTypes" AppendDataBoundItems="true" runat="server">
						</asp:DropDownList>
					</td>
					<td>
						<cc2:DateTimePicker ID="dtpckrDateFrom" runat="server" ShowTime="false" />
					</td>
					<td>
						<cc2:DateTimePicker ID="dtpckrDateTo" runat="server" ShowTime="false" />
					</td>
				</tr>
				<tr>
					<td colspan="4">
						<asp:Button ID="btnShowLogs" runat="server" Text="Show logs to delete"   OnClick="btnShowLogs_Click" />
					</td>
				</tr>
			</table>

			<p><asp:Literal ID="ltrlLogTotal" runat="server" /></p>
				
			<p><asp:Literal ID="ltrlLogInfo" runat="server" /></p>
				
			<asp:GridView ID="gvLogTypesList" runat="server" AutoGenerateColumns="True" AllowPaging="False" AllowSorting="False" BackColor="#FFFFFF" BorderColor="#DEDFDE" BorderWidth="1" GridLines="Both" CellPadding="4" ForeColor="#000000" OnRowDataBound="gvLogTypesList_RowDataBound">
				<RowStyle BackColor="#F7F7DE" />
				<HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" HorizontalAlign="Center" Wrap="false" />
				<AlternatingRowStyle BackColor="White" />
				<SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="#FFFFFF" />
				<PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" />
				<PagerStyle BackColor="#6B696B" ForeColor="#FFFFFF" HorizontalAlign="Right" />
				<FooterStyle BackColor="#CCCC99" />
				<EmptyDataTemplate>
					<strong><asp:Label ID="lblNoLogsToShow" runat="server"></asp:Label></strong>
				</EmptyDataTemplate>
			</asp:GridView>

			<p><asp:Button ID="btnDelete" runat="server" Text="Confirm deletion" OnClick="btnDelete_Click" Visible="false" /></p>
		
		</cc1:Pane>
	
	</cc1:UmbracoPanel>

</asp:Content>
