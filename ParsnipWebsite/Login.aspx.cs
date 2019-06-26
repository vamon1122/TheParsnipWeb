﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ParsnipData.Cookies;
using ParsnipData.Logs;
using ParsnipData.UacApi;
using BenLog;

namespace ParsnipWebsite
{
    public partial class Login : System.Web.UI.Page
    {
        private User myUser;
        private string Redirect;
        Log DebugLog = new Log("Debug");

        protected void Page_Load(object sender, EventArgs e)
        {
            /*
            new LogEntry(Log.Default) { text = "Detecting device and setting deviceType cookie..." };
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "GetDeviceType", "createCookie(\"deviceType\", deviceDetect())", true);
            if (Cookie.Exists("deviceType"))
            {
                new LogEntry(Log.Default) { text = string.Format("----------Cookie exists! deviceType = {0}", Cookie.Read("deviceType")) };
            }
            else
            {
                new LogEntry(Log.Default) { text = "----------Device type cookie did not exist" };
            }
            */

            if (Request.QueryString["url"] != null)
            {
                Redirect = Request.QueryString["url"];
                Warning.Attributes.CssStyle.Add("display", "block");

            }
            else
            {
                Redirect = "home";
            }



            myUser = new User("login");

            if (String.IsNullOrEmpty(inputUsername.Text) && String.IsNullOrWhiteSpace(inputUsername.Text))
            {
                if (myUser.LogIn(false))
                {
                    WriteCookie();
                    Response.Redirect(Redirect);
                }
                else
                {
                    inputUsername.Text = myUser.Username;
                }
            }
        }

        private void WriteCookie()
        {
            Cookie.WritePerm("accountType", myUser.AccountType);
            System.Diagnostics.Debug.WriteLine("----------accountType = " + myUser.AccountType);
            System.Diagnostics.Debug.WriteLine("----------accountType = " + Cookie.Read("accountType"));
        }

        protected void ButLogIn_Click(object sender, EventArgs e)
        {
            new LogEntry(DebugLog) { text = "Login Clicked! Remember password = " + RememberPwd.Checked };

            if (myUser.LogIn(inputUsername.Text, true, inputPwd.Text, RememberPwd.Checked, false))
            {
                new LogEntry(new Log("login/out")) { text = String.Format("{0} logged in from {1} {2}.", myUser.FullName, myUser.PosessivePronoun, Data.DeviceType) };
                WriteCookie();
                Response.Redirect(Redirect);
            }
        }
    }
}