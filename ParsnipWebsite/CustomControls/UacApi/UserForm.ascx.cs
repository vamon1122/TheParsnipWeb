﻿using System;
using UacApi;
using LogApi;
using ParsnipApi;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace TheParsnipWeb
{
    public partial class UserForm1 : System.Web.UI.UserControl
    {
        public User _myUser;
        public User myUser { get { return _myUser; } set { Debug.WriteLine(string.Format("myUser (id = \"{0}\") was set in UserForm", value.id)); _myUser = value; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            //UpdateForm();
        }

        

        public UserForm1()
        {
            Debug.WriteLine("UserForm1 was initialised without a user. Checking cookies...");
            if (CookieApi.Cookie.Read("formUser") != null)
            {
                if(CookieApi.Cookie.Read("formUser").Length != 0)
                {
                    Debug.WriteLine("Cookies were found!");
                    myUser = new User(new Guid(CookieApi.Cookie.Read("formUser")));
                    myUser.Select();
                }
                else Debug.WriteLine("Cookies were NOT found! 1");
            }
            else
            {
                Debug.WriteLine("Cookies were NOT found! 2");
            }
            
        }

        public UserForm1(User pUser)
        {
            Debug.WriteLine(string.Format("UserForm1 was initialised with a user ({0} {1})", pUser.username, pUser.id));
            myUser = pUser;
            UpdateForm();
        }

        public void UpdateForm()
        {
            System.Diagnostics.Debug.WriteLine("----------UpdateForm()");
            System.Diagnostics.Debug.WriteLine("----------username = " + username.Text);
            System.Diagnostics.Debug.WriteLine("----------myUser.username = " + myUser.username);
            System.Diagnostics.Debug.WriteLine("----------myUser.id = " + myUser.id);
            username.Text = myUser.username;
            email.Text = myUser.email;
            //updatePwd()
            forename.Text = myUser.forename;
            surname.Text = myUser.surname;
            gender.Value = myUser.Gender;
            if (myUser.dob.ToString("dd/MM/yyyy") != "01/01/0001")
                dobInput.Value = myUser.dob.ToString("dd/MM/yyyy");
            else
                dobInput.Value = "";
            address1.Text = myUser.address1;
            address2.Text = myUser.address2;
            address3.Text = myUser.address3;
            postCode.Text = myUser.postCode;
            mobilePhone.Text = myUser.mobilePhone;
            homePhone.Text = myUser.homePhone;
            workPhone.Text = myUser.workPhone;
            dateTimeCreated.Attributes.Remove("placeholder");
            dateTimeCreated.Attributes.Add("placeholder", myUser.dateTimeCreated.Date.ToString("dd/MM/yyyy"));
            accountType.Value = myUser.accountType;
            accountStatus.Value = myUser.accountStatus;
        }

        void UpdateFormAccount()
        {
            if(CookieApi.Cookie.Read("formUser").Length > 0)
            {
                myUser = new User(new Guid(CookieApi.Cookie.Read("formUser")));
                myUser.Select();
            }
            if (myUser == null)
            {
                System.Diagnostics.Debug.WriteLine("My user is null. Adding new myUser");
                myUser = new User("UpdateFormAccount (UserForm1)");
            }
            System.Diagnostics.Debug.WriteLine(string.Format("username.Text = {0}", username.Text));
            System.Diagnostics.Debug.WriteLine(string.Format("forename.Text = {0}", forename.Text));
            myUser.username = username.Text;
            System.Diagnostics.Debug.WriteLine(string.Format("myUser.Username = username.Text ({0})", username.Text));

            myUser.email = email.Text;
            myUser.pwd = password1.Text;
            myUser.forename = forename.Text;
            myUser.surname = surname.Text;
            myUser.Gender = gender.Value.Substring(0, 1);
            System.Diagnostics.Debug.WriteLine("DOB = " + dobInput.Value);

            
            if (DateTime.TryParse(dobInput.Value, out DateTime result))
                myUser.dob = Convert.ToDateTime(dobInput.Value);
            myUser.address1 = address1.Text;
            myUser.address2 = address2.Text;
            myUser.address3 = address3.Text;
            myUser.postCode = postCode.Text;
            myUser.mobilePhone = mobilePhone.Text;
            myUser.homePhone = homePhone.Text;
            myUser.workPhone = workPhone.Text;
            myUser.dateTimeCreated = ParsnipApi.Data.adjustedTime;
            myUser.accountType = accountType.Value;
            myUser.accountStatus = accountStatus.Value;
            myUser.accountType = accountType.Value;

        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Insert / Update button was clicked. myUser.id = " + myUser.id);
            UpdateFormAccount();
            if (myUser.Validate())
            {
                if (myUser.Update())
                {
                    new LogEntry(myUser.id) { text = String.Format("{0} created / edited an account for {1} via the UserForm", myUser.fullName, myUser.fullName) };
                }
                else
                    new LogEntry(myUser.id) { text = String.Format("{0} tried to create / edit an account for {1} via the UserForm, but there was an error whilst updating the database", myUser.fullName, myUser.fullName) };

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("User failed to validate!");
                new LogEntry(myUser.id) { text = String.Format("{0} attempted to create / edit an account for {1} via the UserForm, but the user failed fo validate!", myUser.fullName, myUser.fullName) };
            }
        }
    }
}