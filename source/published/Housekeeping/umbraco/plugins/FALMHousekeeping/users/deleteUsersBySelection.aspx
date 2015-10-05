<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="deleteUsersBySelection.aspx.cs" Inherits="FALMHousekeepingUsersManager.deleteUsersBySelection" MasterPageFile="../../../masterpages/umbracoPage.Master" Title="F.A.L.M. Housekeeping - Delete Users by selection" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<asp:Content ID="cphHead" ContentPlaceHolderID="head" runat="server">
	<script type="text/javascript" language="javascript">
		// Function to check or uncheck a user
		function check_uncheck(Val)
		{
			var ValChecked = Val.checked;
			var ValId = Val.id;
			var frm = document.forms[0];
			// Loop through all elements
			for (i = 0; i < frm.length; i++) {
				// Look for Header Template's Checkbox
				//As we have not other control other than checkbox we just check following statement
				if (this != null) {
					if (ValId.indexOf('chkbCheckAllUsers') != -1) {
						// Check if main checkbox is checked,
						// then select or deselect datagrid checkboxes
						if (ValChecked)
							frm.elements[i].checked = true;
						else
							frm.elements[i].checked = false;
					}
					else if (ValId.indexOf('chkbUser') != -1) {
						// Check if any of the checkboxes are not checked, and then uncheck top select all checkbox
						if (frm.elements[i].checked == false)
							frm.elements[1].checked = false;
					}
				} // if
			} // for
		} // function

		// Delete confirmation method
		function confirmMsg(frm)
		{
			var noSelected = 0;
			// loop through all elements
			for (i = 0; i < frm.length; i++) {
				// Look for our checkboxes only
				if (frm.elements[i].name.indexOf("chkbUser") != -1) {
					// If any are checked then confirm alert, otherwise nothing happens
					if (frm.elements[i].checked)
						return confirm("<%= umbraco.library.GetDictionaryItem("users_NodesAssociation") %>")
					else
						noSelected = 1;
				} // if
			} // for

			if (noSelected == 1)
				return alert("<%= umbraco.library.GetDictionaryItem("users_NoUsersSelected") %>")
		} // function
	</script>
</asp:Content>
<asp:Content ID="cphBody" ContentPlaceHolderID="body" runat="server">
	<cc1:UmbracoPanel ID="umbPanelCleanupUser" Text="F.A.L.M. Housekeeping" runat="server" Width="612px" Height="600px" hasMenu="false">
		<cc1:Pane ID="PanelCleanupUser" runat="server">
			<asp:Label ID="lblSubTitle" runat="server"></asp:Label>
			<asp:Literal ID="ltrlUsers" runat="server"></asp:Literal>
			<asp:GridView ID="gvUsers" runat="server" DataSourceID="SqlDSUsers" AllowPaging="False" PageSize="20" AutoGenerateColumns="False" Width="1px" BackColor="#FFFFFF" BorderColor="#DEDFDE" BorderWidth="1" GridLines="Both" CellPadding="4" ForeColor="#000000" OnRowDataBound="gvUsers_RowDataBound">
				<RowStyle BackColor="#F7F7DE" />
				<HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" HorizontalAlign="Center" Wrap="false" />
				<AlternatingRowStyle BackColor="White" />
				<SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="#FFFFFF" />
				<Columns>
					<asp:TemplateField HeaderText="">
						<HeaderTemplate>
							<asp:CheckBox ID="chkbCheckAllUsers" onclick="return check_uncheck(this);" runat="server" />
						</HeaderTemplate>
						<HeaderStyle HorizontalAlign="Center" Width="50" />
						<ItemTemplate>
							<asp:CheckBox ID="chkbUser" Text="" onclick="return check_uncheck (this);" runat="server" />
						</ItemTemplate>
						<ItemStyle HorizontalAlign="Center" Width="50" />
					</asp:TemplateField>
						
					<asp:TemplateField HeaderText="" ItemStyle-Wrap="False">
						<HeaderTemplate>
							<asp:Label ID="lblHeaderID" runat="server" Text="ID utente"></asp:Label>
						</HeaderTemplate>
						<HeaderStyle HorizontalAlign="Center" Width="50" />
						<ItemTemplate>
							<asp:Label ID="lblUserId" runat="server" Text='<%# Bind("id") %>' />
						</ItemTemplate>
						<ItemStyle HorizontalAlign="Center" Width="50" />
					</asp:TemplateField>
						
					<asp:TemplateField HeaderText="" ItemStyle-Wrap="False">
						<HeaderTemplate>
							<asp:Label ID="lblHeaderUserName" runat="server"><% = umbraco.library.GetDictionaryItem("users_UserName") %></asp:Label>
						</HeaderTemplate>
						<HeaderStyle HorizontalAlign="Center" Width="150" />
						<ItemTemplate>
							<asp:Label ID="lblUserName" runat="server" Text='<%# Bind("userName") %>' />
						</ItemTemplate>
						<ItemStyle HorizontalAlign="Left" Width="150" />
					</asp:TemplateField>
						
					<asp:TemplateField HeaderText="" ItemStyle-Wrap="False">
						<HeaderTemplate>
							<asp:Label ID="lblHeaderLogin" runat="server"><% = umbraco.library.GetDictionaryItem("users_UserLogin") %></asp:Label>
						</HeaderTemplate>
						<HeaderStyle HorizontalAlign="Center" Width="150" />
						<ItemTemplate>
							<asp:Label ID="lblUserUserLogin" runat="server" Text='<%# Bind("userLogin") %>' />
						</ItemTemplate>
						<ItemStyle HorizontalAlign="Left" Width="150" />
					</asp:TemplateField>
						
					<asp:TemplateField HeaderText="" ItemStyle-Wrap="False">
						<HeaderTemplate>
							<asp:Label ID="lblHeaderUserEmail" runat="server"><% = umbraco.library.GetDictionaryItem("users_UserEmail") %></asp:Label>
						</HeaderTemplate>
						<HeaderStyle HorizontalAlign="Center" Width="150" />
						<ItemTemplate>
							<asp:Label ID="lblUserEmail" runat="server" Text='<%# Bind("userEmail") %>' />
						</ItemTemplate>
						<ItemStyle HorizontalAlign="Left" Width="150" />
					</asp:TemplateField>
						
					<asp:TemplateField HeaderText="" ItemStyle-Wrap="False">
						<HeaderTemplate>
								<asp:Label ID="lblHeaderUserType" runat="server"><% = umbraco.library.GetDictionaryItem("users_UserType") %></asp:Label>
						</HeaderTemplate>
						<HeaderStyle HorizontalAlign="Center" Width="100" />
						<ItemTemplate>
							<asp:Label ID="lblUserType" runat="server" Text='<%# Bind("userTypeAlias") %>' />
						</ItemTemplate>
						<ItemStyle HorizontalAlign="Center" Width="100" />
					</asp:TemplateField>

					<asp:TemplateField HeaderText="" Visible="False">
						<HeaderTemplate>
							<asp:Label ID="lblHeaderUserDisabled" runat="server"><% = umbraco.library.GetDictionaryItem("users_UserDisabled") %></asp:Label>
						</HeaderTemplate>
						<HeaderStyle HorizontalAlign="Center" Width="50" />
						<ItemTemplate>
							<asp:Label ID="lblUserDisabled" runat="server" Text='<%# Bind("userDisabled") %>' />
						</ItemTemplate>
						<ItemStyle HorizontalAlign="Center" Width="50" />
					</asp:TemplateField>
				</Columns>
				<PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" />
				<PagerStyle BackColor="#6B696B" ForeColor="#FFFFFF" HorizontalAlign="Right" />
				<FooterStyle BackColor="#CCCC99" />
			</asp:GridView>
			<asp:SqlDataSource ID="SqlDSUsers" runat="server" ConnectionString="<%$ ConnectionStrings:umbracoDbDSN %>" SelectCommand="SELECT [umbracoUser].[id] AS id, [umbracoUser].[userName] AS userName, [umbracoUser].[userLogin] AS userLogin, [umbracoUser].[userEmail] AS userEmail, [umbracoUserType].[userTypeAlias] AS userTypeAlias, [umbracoUser].[userDisabled] FROM [umbracoUser] INNER JOIN [umbracoUserType] ON [umbracoUser].[userType] = [umbracoUserType].[id] WHERE ([umbracoUser].[id] &lt;&gt; @id) ORDER BY [umbracoUser].[id] ASC" OnSelected="SqlDSUsers_Selected">
				<SelectParameters>
					<asp:Parameter DefaultValue="0" Name="id" Type="Int32" />
				</SelectParameters>
			</asp:SqlDataSource>
			<p>
				<asp:Button ID="btnDeleteUsers" runat="server" Text="Delete selected users" OnClientClick="return confirmMsg(this.form)" OnClick="btnDeleteUsers_Click" />
			</p>
		</cc1:Pane>
	</cc1:UmbracoPanel>
</asp:Content>
