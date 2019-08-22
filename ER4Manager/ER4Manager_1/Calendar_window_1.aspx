<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Calendar_window_1.aspx.cs" Inherits="Calendar_window_1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Calendar</title>
    <link href="MainStyleSheet.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <table style="width: 220px">
        <tr>
            <td colspan="2" rowspan="1" style="width: 50%; text-align: left">
                <asp:DropDownList ID="ddlChangeMonth" runat="server" AutoPostBack="True" 
                    OnSelectedIndexChanged="ddlChangeMonth_SelectedIndexChanged" Width="110px">
                    <asp:ListItem Value="01">January</asp:ListItem>
                    <asp:ListItem Value="02">February</asp:ListItem>
                    <asp:ListItem Value="03">March</asp:ListItem>
                    <asp:ListItem Value="04">April</asp:ListItem>
                    <asp:ListItem Value="05">May</asp:ListItem>
                    <asp:ListItem Value="06">June</asp:ListItem>
                    <asp:ListItem Value="07">July</asp:ListItem>
                    <asp:ListItem Value="08">August</asp:ListItem>
                    <asp:ListItem Value="09">September</asp:ListItem>
                    <asp:ListItem Value="10">October</asp:ListItem>
                    <asp:ListItem Value="11">November</asp:ListItem>
                    <asp:ListItem Value="12">December</asp:ListItem>
                </asp:DropDownList>
            </td>
            <td style="width: 50%; text-align: left">
                <asp:DropDownList ID="ddlChangeYear" runat="server" AutoPostBack="True" 
                    OnSelectedIndexChanged="ddlChangeYear_SelectedIndexChanged" Width="110px">           
                    <asp:ListItem>2019</asp:ListItem>
                    <asp:ListItem>2020</asp:ListItem>
                    <asp:ListItem>2021</asp:ListItem>
                    <asp:ListItem>2022</asp:ListItem>
                    <asp:ListItem>2023</asp:ListItem>
                    <asp:ListItem>2024</asp:ListItem>
                    <asp:ListItem>2025</asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td colspan="3">
                <asp:Calendar ID="Calendar1" runat="server" BackColor="#FFFFCC" 
                    BorderColor="#FFCC66" BorderWidth="1px" DayNameFormat="Shortest" 
                    Font-Names="Verdana" Font-Size="8pt" ForeColor="#663399" Height="200px" 
                    OnSelectionChanged="Calendar1_SelectionChanged" ShowGridLines="True" 
                    ShowTitle="False" Width="225px">
                    <SelectedDayStyle BackColor="#CCCCFF" Font-Bold="True" />
                    <TodayDayStyle BackColor="#FFCC66" ForeColor="White" />
                    <SelectorStyle BackColor="#FFCC66" />
                    <OtherMonthDayStyle ForeColor="#CC9966" />
                    <NextPrevStyle Font-Size="9pt" ForeColor="#FFFFCC" />
                    <DayHeaderStyle BackColor="#FFCC66" Font-Bold="True" Height="1px" />
                    <TitleStyle BackColor="#990000" Font-Bold="True" Font-Size="9pt" 
                        ForeColor="#FFFFCC" />
                </asp:Calendar>
                <input id="Button1" onclick="javascript:window.close()" type="button" 
                    value="Cancel" />&nbsp;
                <asp:Button ID="cmdClearDate" runat="server" OnClick="cmdClearDate_Click" 
                    Text="Clear date" Visible="False" />
                <asp:Button ID="cmdToday" runat="server" OnClick="cmdToday_Click" 
                    Text="Today" />
                <asp:Label ID="lblDidDDLChange" runat="server" Text="No" Visible="False"></asp:Label>
                <asp:Label ID="lblCalendarCounter" runat="server" Text="1" Visible="False"></asp:Label>
            </td>
        </tr>
    </table>
    <div>
    
    </div>
    </form>
</body>
</html>
