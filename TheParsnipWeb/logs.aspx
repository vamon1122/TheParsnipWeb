﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="logs.aspx.cs" Inherits="TheParsnipWeb.logs" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta name="viewport" content="width=device-width, initial-scale=1">
        <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.1.2/css/bootstrap.min.css" integrity="sha384-Smlep5jCw/wG7hdkwQ/Z5nLIefveQRIY9nfy6xoR1uRYBtpZgI6339F5dgvm/e9B" crossorigin="anonymous" />
       <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.1.2/js/bootstrap.min.js" integrity="sha384-o+RDsa0aLu++PJvFqy8fFScvbHFLtbvScb8AjopnFD+iEQ7wo/CG0xlczd+2O/em" crossorigin="anonymous"></script>
    <link rel="stylesheet" type="text/css" href="ParsnipStyle.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="table-wrapper-scroll-y">

  <asp:Table class="table table-bordered table-striped" runat="server" id="LogTable">
    
  </asp:Table>

</div>
    </form>
</body>
</html>