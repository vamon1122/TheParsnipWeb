﻿<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="ConsumeProducts.aspx.cs" Inherits="ParsnipWebsite.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div><asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />
            <asp:Button ID="Button_GetUsers" runat="server" Text="Get Users" OnClick="Button_GetUsers_Click" />
        </div>
    </form>
    
</body>
</html>
