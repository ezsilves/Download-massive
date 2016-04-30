<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Setup.aspx.cs" Inherits="SATDownloadApp._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="Button2" runat="server" Text="Test Connection" TabIndex="11" OnClick="Button2_Click" />
        <asp:Button ID="Button3" runat="server" Text="Build Schema" TabIndex="11" OnClick="Button3_Click" />
        <asp:Button ID="Button1" runat="server" Text="Awesomium" OnClick="Button1_Click" />
        <br />
        <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
    </div>
    </form>
</body>
</html>
