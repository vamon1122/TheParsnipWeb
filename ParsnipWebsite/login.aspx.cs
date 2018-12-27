﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UacApi;
using BenLog;
using CookieApi;

namespace TheParsnipWeb
{
    public partial class LogInBarrier : System.Web.UI.Page
    {
        private User MyAccount;
        private string Redirect;
        LogWriter AccountLog = new LogWriter("Account Object.txt", AppDomain.CurrentDomain.BaseDirectory);

        protected void Page_Load(object sender, EventArgs e)
        {

            
            

            AccountLog.Warning("PAGE IS BEING LOADED");
            if (Request.QueryString["url"] != null)
            {
                Redirect = Request.QueryString["url"];
                Warning.Attributes.CssStyle.Add("display", "block");
                WebpageLabel.Text = Redirect;
            }
            else
            {
                Redirect = "Home.aspx";
            }
                


            MyAccount = new User();

            if (String.IsNullOrEmpty(inputUsername.Text) && String.IsNullOrWhiteSpace(inputUsername.Text)){
                if (MyAccount.LogIn(false))
                {
                    Response.Redirect(Redirect);
                }
                else
                {

                    AccountLog.Warning("Input username was reset");
                    inputUsername.Text = MyAccount.Username;

                }
            }
        }

        protected void ButLogIn_Click(object sender, EventArgs e)
        {
            AccountLog.Warning("Button Was clicked!");

            System.Diagnostics.Debug.WriteLine("CheckBox = " + RememberPwd.Checked);

            if (MyAccount.LogIn(inputUsername.Text, true, inputPwd.Text, RememberPwd.Checked, false))
            {
                Response.Redirect(Redirect);
            }
        }
    }
}