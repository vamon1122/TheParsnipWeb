﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="ParsnipWebsite.Home" %>
<%@ Register Src="~/Custom_Controls/Menu/Menu.ascx" TagPrefix="menuControls" TagName="Menu" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!-- BOOTSTRAP START -->
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.1.2/css/bootstrap.min.css" integrity="sha384-Smlep5jCw/wG7hdkwQ/Z5nLIefveQRIY9nfy6xoR1uRYBtpZgI6339F5dgvm/e9B" crossorigin="anonymous" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.1.2/js/bootstrap.min.js" integrity="sha384-o+RDsa0aLu++PJvFqy8fFScvbHFLtbvScb8AjopnFD+iEQ7wo/CG0xlczd+2O/em" crossorigin="anonymous"></script>
    <!-- BOOTSTRAP END -->

    <script src="../Javascript/Useful_Functions.js"></script>
    <link id="link_style" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="Css/Shared_Style.css" />
    <script src="../Javascript/Apply_Style.js"></script>
    <title>Home</title>
</head>
<body class="fade0p5" id="body" style="text-align:center">
    <label class="censored" id="pageId">home.html</label>
    
    <!--FOR JS DYNAMIC PAGE CREATION DO NOT MOVE START-->

    

    <!--FOR JS DYNAMIC PAGE CREATION DO NOT MOVE END-->
    
    
        <menuControls:Menu runat="server" ID="Menu" />

    <h2>Home</h2>
    <div class="padded-text center_div">
        <asp:Label runat="server" ID="WelcomeLabel"></asp:Label>
            <br />
        <br />
        <h3>*NEW*</h3>
        - [19/06/19] Share photos! (Recipient doesn't even need an account to view shared photos!!!)
    </div>
    <hr class="break" />
    <h3>LATEST VIDEO: Aaron Gets Wavy</h3>
    <video class="video-container" controls="controls">
            <source src="Resources/Media/Videos/Local/Aaron%20Gets%20Wavy.mp4" type="video/mp4" />
            Your browser does not support HTML5 video.
        </video>

    <hr class="break" />

    <!--DEVICE DETECT END-->
    <!--SCRIPTS-->
</body>
</html>
