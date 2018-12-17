<%@ Page Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true"
    CodeFile="PurchaseHistory.aspx.cs" Inherits="PurchaseHistory" Title="PurchaseHistory" %>
    <asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
        <div style="max-width:800px">
        
                    <br />
                    Billing history for:
                    <asp:Label ID="lblBillee" runat="server" Text="name" Font-Bold="True"></asp:Label>
                    <br />
                    <asp:GridView ID="gvBills" runat="server" 
    AutoGenerateColumns="False" Width="800px" onrowcommand="gvBills_RowCommand" 
                    DataKeyNames="BILL_ID" EnableModelValidation="True">
                    <Columns>
                        <asp:BoundField DataField="BILL_ID" 
                        HeaderText="Bill ID">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField HeaderText="Purchaser ID" DataField="PURCHASER_ID" 
                            Visible="False">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField HeaderText="Date purchased" 
                        DataField="DATE_PURCHASED">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="NOMINAL_PRICE" 
                        HeaderText="Nominal price (£)">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField HeaderText="Discount (£) (N/A)" 
                        DataField="DISCOUNT" Visible="False">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="PRICE_DUE" 
                        HeaderText="Total Price (inc. VAT £)">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:BoundField DataField="BILL_STATUS" HeaderText="Status">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:BoundField>
                        <asp:ButtonField CommandName="DETAILS" HeaderText="Details" Text="Details">
                        <HeaderStyle BackColor="#B6C6D6" />
                        </asp:ButtonField>
                    </Columns>
                </asp:GridView>
                    <br />
                    <asp:Panel ID="pnlBillDetails" runat="server" Visible="False">
                        Details of bill number:
                        <asp:Label ID="lblBillID" runat="server" Font-Bold="True" Text="0"></asp:Label>
                        
                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:LinkButton ID="lbInvoice" runat="server" onclick="lbInvoice_Click">Download invoice</asp:LinkButton>
                        
                        <asp:GridView ID="gvBillDetails" runat="server" AutoGenerateColumns="False" 
                            DataKeyNames="LINE_ID" Width="800px">
                            <Columns>
                                <asp:BoundField DataField="LINE_ID" HeaderText="Line ID">
                                <HeaderStyle BackColor="#B6C6D6" />
                                </asp:BoundField>
                                <asp:BoundField DataField="TYPE" HeaderText="Type">
                                <HeaderStyle BackColor="#B6C6D6" />
                                </asp:BoundField>
                                <asp:BoundField DataField="AFFECTED_ID" HeaderText="Affected ID">
                                <HeaderStyle BackColor="#B6C6D6" />
                                </asp:BoundField>
                                <asp:BoundField DataField="NAME" HeaderText="Name">
                                <HeaderStyle BackColor="#B6C6D6" />
                                </asp:BoundField>
                                <asp:BoundField DataField="MONTHS" HeaderText="Months">
                                <HeaderStyle BackColor="#B6C6D6" />
                                </asp:BoundField>
                                <asp:BoundField DataField="COST_PER_MONTH" HeaderText="Cost / month (£)">
                                <HeaderStyle BackColor="#B6C6D6" />
                                </asp:BoundField>
                                <asp:BoundField DataField="COST" HeaderText="Cost (£)">
                                <HeaderStyle BackColor="#B6C6D6" />
                                </asp:BoundField>
                            </Columns>
                        </asp:GridView>
                        <br />
                    </asp:Panel>
                    <br />
                    <br />
                    <br />
                    <br />
                    <br />
                    <br />
                    <br />
        
                    </div>
</asp:Content>
