﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using LogApi;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Data.SqlClient;
using ParsnipApi;
using CookieApi;

namespace ParsnipService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Uac" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Uac.svc or Uac.svc.cs at the Solution Explorer and start debugging.
    public class Uac : IUac
    {
        private static readonly Log PageAccessLog = new Log("access");
        private static readonly Log PageAccessJustificationLog = new Log("access justification");
        private static readonly Log DebugLog = new Log("debug");
        private static readonly Log SessionLog = new Log("session");
        
        public User SecurePage(string pUrl, Page pPage, string pDeviceType, string pAccountType)
        {
            Debug.WriteLine("Securing page...");

            if (string.IsNullOrEmpty(pDeviceType) || string.IsNullOrWhiteSpace(pDeviceType))
            {
                Debug.WriteLine("Devicetype is empty, getting device info...");
                //new LogEntry(Log.Default) { text = "Attempted to secure the page but deviceInfo was incomplete. Getting device info..." };
                pPage.Response.Redirect("get_device_info?url=" + pUrl);
            }
            else
            {
                Debug.WriteLine("Devicetype is NOT empty...");
                //new LogEntry(Log.Default) { text = "Secure page - Device info is already complete!" };
            }


            var myUser = new User("Uac.SecurePage(4)");
            //Debug.WriteLine("----------Securing page");
            bool canAccess;
            string justification = "";


            Debug.WriteLine("Attempting to log user in...");
            if (myUser.LogIn())
            {
                Debug.WriteLine("User logged in");
                //Debug.WriteLine("----------Securing page, accountType = " + myUser.AccountType);

                if (pPage.Session["userName"] == null)
                {
                    pPage.Session["userName"] = myUser.Username;
                    new LogEntry(SessionLog) { text = string.Format("{0} started a new session from {1} {2}. Session ID = {3}.", myUser.FullName, myUser.PosessivePronoun, pDeviceType, pPage.Session.SessionID.ToString()) };
                }
                else
                    new LogEntry(SessionLog) { text = string.Format("{0} continued {1} session on {1} {2}. Session ID = {3}.", myUser.FullName, myUser.PosessivePronoun, pDeviceType, pPage.Session.SessionID.ToString()) };




                if (myUser.AccountStatus == "active")
                {
                    string accTypeDescriptor = "";
                    string x = myUser.AccountType.Substring(0, 1).ToLower();
                    if (x == "a" || x == "e" || x == "i" || x == "o" || x == "u" && myUser.AccountType != "user")
                        accTypeDescriptor += "an";
                    else
                        accTypeDescriptor += "a";

                    justification += string.Format("a) {0} account is active and b) ", myUser.PosessivePronoun);

                    string accessGrantedJustification(string pRequiredAccess)
                    {
                        return string.Format("this page requires {0} to have {1} access and {2} is {3} {4} which means that {2} has the required permission level.", myUser.ObjectiveGenderPronoun, pRequiredAccess, myUser.SubjectiveGenderPronoun, accTypeDescriptor, myUser.AccountType);
                    }

                    switch (pAccountType)
                    {
                        case "admin":
                            if (myUser.AccountType == "admin")
                            {
                                justification += accessGrantedJustification("admin");
                                canAccess = true;
                            }
                            else
                                canAccess = false;
                            break;
                        case "member":
                            if (myUser.AccountType == "admin" || myUser.AccountType == "member")
                            {
                                justification += accessGrantedJustification("member");
                                canAccess = true;
                            }
                            else
                                canAccess = false;
                            break;
                        case "user":
                            if (myUser.AccountType == "admin" || myUser.AccountType == "member" || myUser.AccountType == "user")
                            {
                                justification += accessGrantedJustification("user");
                                canAccess = true;
                            }
                            else
                                canAccess = false;
                            break;
                        default:
                            canAccess = false;
                            break;
                    }

                    if (canAccess)
                    {
                        //Debug.WriteLine("----------{0} is allowed to access {1}", myUser.FullName, pUrl);
                        if (!pPage.IsPostBack)
                        {
                            new LogEntry(PageAccessLog) { text = String.Format("{0} accessed the {1} page from {2} {3}.", myUser.FullName, pUrl, myUser.PosessivePronoun, pDeviceType, myUser.Forename, justification) };
                            DateTime start = DateTime.Now;
                            while (DateTime.Now < start.AddMilliseconds(1)) { }
                            new LogEntry(PageAccessJustificationLog) { text = String.Format("{0} was allowed to access the {1} page because {2}", myUser.FullName, pUrl, justification) };
                        }
                        else
                        {
                            new LogEntry(PageAccessLog) { text = String.Format("{0} reloaded the {1} page from {2} {3}.", myUser.FullName, pUrl, myUser.PosessivePronoun, pDeviceType, myUser.Forename, justification) };
                        }
                    }
                    else
                    {
                        if (!pPage.IsPostBack)
                        {
                            Debug.WriteLine("----------{0} is NOT allowed to access {1}", myUser.FullName, pUrl);
                            new LogEntry(PageAccessLog) { text = String.Format("{0} tried to access the {1} page but access was denied.", myUser.FullName, pUrl) };
                            DateTime start = DateTime.Now;
                            while (DateTime.Now < start.AddMilliseconds(1)) { }
                            new LogEntry(PageAccessJustificationLog) { text = String.Format("{0} was denied access to the {1} page because {2} did not have sufficient permissions.", myUser.FullName, pUrl, myUser.PosessivePronoun) };
                            pPage.Response.Redirect(String.Format("access-denied?url={0}", pUrl));
                        }
                        else
                        {
                            new LogEntry(PageAccessLog) { text = String.Format("{0} tried to reload the page to access the {1} page but access was denied.", myUser.FullName, pUrl) };
                        }
                    }

                }
                else
                {
                    canAccess = false;
                    if (!pPage.IsPostBack)
                    {
                        new LogEntry(PageAccessLog) { text = string.Format("{0} tried to access the {1} page from {2} {3} but access was denied.", myUser.FullName, pUrl, myUser.PosessivePronoun, pDeviceType) };
                        DateTime start = DateTime.Now;
                        while (DateTime.Now < start.AddMilliseconds(1)) { }
                        new LogEntry(PageAccessJustificationLog) { text = string.Format("{0} was denied access to the {1} page because {2} account is not active!", myUser.FullName, pUrl, myUser.PosessivePronoun) };
                    }
                    else
                    {
                        new LogEntry(PageAccessLog) { text = string.Format("{0} tried to reload the page to access the {1} page from {2} {3} but access was denied.", myUser.FullName, pUrl, myUser.PosessivePronoun, pDeviceType) };
                    }
                }




            }
            else
            {
                new LogEntry(PageAccessLog) { text = String.Format("Someone tried to access the {0} page from {1} {2}, without logging in!", pUrl, myUser.PosessivePronoun, pDeviceType) };
                pPage.Response.Redirect(String.Format("login?url={0}", pUrl));
            }

            return myUser;
        }

        public User SecurePage(string pUrl, Page pPage, string pDeviceType)
        {
            Debug.WriteLine("Attempting to secure page in...");
            return SecurePage(pUrl, pPage, pDeviceType, "user");
        }
        
        public static List<User> GetAllUsers()
        {
            bool logMe = false;

            if (logMe)
                Debug.WriteLine("----------Getting all users...");

            var users = new List<User>();
            using (SqlConnection conn = Parsnip.GetOpenDbConnection())
            {
                SqlCommand GetUsers = new SqlCommand("SELECT * FROM t_Users", conn);
                using (SqlDataReader reader = GetUsers.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User(reader));
                    }
                }
            }

            foreach (User temp in users)
            {
                if (logMe)
                    Debug.WriteLine(string.Format("Found user {0} with id {1}", temp.FullName, temp.Id));
            }

            return users;
        }

        public static User LogIn(string pUsername, string pPassword)
        {
            using (var openConn = Parsnip.GetOpenDbConnection())
            {
                try
                {
                    SqlCommand getId = new SqlCommand("SELECT * FROM t_Users WHERE username = @username AND password = @password", openConn);
                    getId.Parameters.Add(new SqlParameter("username", pUsername));

                    using (SqlDataReader reader = getId.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new User(reader);
                        }
                    }
                    throw new InvalidOperationException("There is no user with this username / password combination");
                }
                catch (Exception e)
                {
                    Debug.WriteLine("[LogIn] There was an exception whilst getting the id from the database: " + e);
                    throw new InvalidOperationException("There was an error whilst finding a user with username / password combination");
                }
            }
        }

        private bool IdExistsOnDb(Guid Id)
        {
            Debug.WriteLine(string.Format("Checking weather user exists on database by using Id = {0}", Id));
            using (SqlConnection openConnection = Parsnip.GetOpenDbConnection())
            {
                try
                {
                    SqlCommand findMeById = new SqlCommand("SELECT COUNT(*) FROM t_Users WHERE id = @id", openConnection);
                    findMeById.Parameters.Add(new SqlParameter("id", Id.ToString()));

                    int userExists;

                    using (SqlDataReader reader = findMeById.ExecuteReader())
                    {
                        reader.Read();
                        userExists = Convert.ToInt16(reader[0]);
                        //Debug.WriteLine("Found user by Id. userExists = " + userExists);
                    }

                    //Debug.WriteLine(userExists + " user(s) were found with the id " + Id);

                    if (userExists > 0)
                        return true;
                    else
                        return false;

                }
                catch (Exception e)
                {
                    Debug.WriteLine("There was an error whilst checking if user exists on the database by using thier Id: " + e);
                    return false;
                }
            }
        }

        private bool UsernameExistsOnDb(string pUsername)
        {
            Debug.WriteLine(string.Format("Checking weather user exists on database by using the username {0}", pUsername));
            using(SqlConnection openConnection = Parsnip.GetOpenDbConnection())
            try
            {
                SqlCommand findMeById = new SqlCommand("SELECT COUNT(*) FROM t_Users WHERE username = @username", openConnection);
                findMeById.Parameters.Add(new SqlParameter("username", pUsername));

                int userExists;

                using (SqlDataReader reader = findMeById.ExecuteReader())
                {
                    reader.Read();
                    userExists = Convert.ToInt16(reader[0]);
                    //Debug.WriteLine("Found user by Id. userExists = " + userExists);
                }

                Debug.WriteLine(userExists + " user(s) were found with the username " + pUsername);



                if (userExists > 0)
                    return true;
                else
                    return false;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("There was an error whilst checking if user exists on the database by using their username: " + e);
                return false;
            }
        }
        
        private bool DbInsert(User pUser)
        {
            if (pUser.Id.ToString() == Guid.Empty.ToString())
            {
                //pUser.Id = Guid.NewGuid();
                Debug.WriteLine("Id was empty when trying to insert user {0} into the database. A new guid was generated: {1}", pUser.FullName, pUser.Id);
            }

            if (pUser.Username != null && pUser.Forename != null && pUser.Surname != null)
            {
                using (SqlConnection openConnection = Parsnip.GetOpenDbConnection())
                {
                    try
                    {
                        if (!ExistsOnDb(openConnection))
                        {
                            SqlCommand InsertIntoDb = new SqlCommand("INSERT INTO t_Users (id, username, forename, surname, created, type, status) VALUES(@id, @username, @forename, @surname, @dateTimeCreated, @accountType, @accountStatus)", openConnection);

                            InsertIntoDb.Parameters.Add(new SqlParameter("id", pUser.Id));
                            InsertIntoDb.Parameters.Add(new SqlParameter("username", pUser.Username.Trim()));
                            InsertIntoDb.Parameters.Add(new SqlParameter("forename", pUser.Forename.Trim()));
                            InsertIntoDb.Parameters.Add(new SqlParameter("surname", pUser.Surname.Trim()));
                            InsertIntoDb.Parameters.Add(new SqlParameter("dateTimeCreated", Parsnip.adjustedTime));
                            InsertIntoDb.Parameters.Add(new SqlParameter("accountType", pUser.AccountType));
                            InsertIntoDb.Parameters.Add(new SqlParameter("accountStatus", pUser.AccountStatus));

                            InsertIntoDb.ExecuteNonQuery();

                            Debug.WriteLine(String.Format("Successfully inserted account \"{0}\" into database: ", pUser.Username));
                        }
                        else
                        {
                            Debug.WriteLine(string.Format("----------Tried to insert user {0} but they already existed on the database! Id = {1}", pUser.FullName, pUser.Id));
                        }
                    }
                    catch (Exception e)
                    {
                        string error = string.Format("[UacApi.User.DbInsert)] Failed to insert {0}'s account into the database: {1}", pUser.FullName, e);
                        Debug.WriteLine(error);
                        new LogEntry(Log.Default) { text = error };
                        return false;
                    }
                    new LogEntry(Log.Default) { text = string.Format("{0} was successfully inserted into the database!", pUser.FullName) };
                    return DbUpdate(openConnection);
                }
            }
            else
            {
                throw new InvalidOperationException("Account cannot be inserted. The account's properties: username, fname & sname, must be initialised before it can be inserted!");
            }
        }
        
        internal bool DbSelect(SqlConnection pOpenConn)
        {
            //AccountLog.Debug("Attempting to get user details...");
            //Debug.WriteLine(string.Format("----------DbSelect() - Attempting to get user details with id {0}...", Id));

            try
            {
                SqlCommand SelectAccount = new SqlCommand("SELECT * FROM t_Users WHERE id = @id", pOpenConn);
                SelectAccount.Parameters.Add(new SqlParameter("id", Id.ToString()));

                int recordsFound = 0;
                using (SqlDataReader reader = SelectAccount.ExecuteReader())
                {

                    while (reader.Read())
                    {

                        AddValues(reader);
                        recordsFound++;
                    }
                }
                //Debug.WriteLine(string.Format("----------DbSelect() - Found {0} record(s) ", recordsFound));

                if (recordsFound > 0)
                {
                    //Debug.WriteLine("----------DbSelect() - Got user's details successfully!");
                    //AccountLog.Debug("Got user's details successfully!");
                    return true;
                }
                else
                {
                    Debug.WriteLine("----------DbSelect() - No user data was returned");
                    //AccountLog.Debug("Got user's details successfully!");
                    return false;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("There was an exception whilst getting user data: " + e);
                return false;
            }
        }

        private bool DbUpdate(SqlConnection pOpenConn)
        {
            Debug.WriteLine("Attempting to update user with Id = " + Id);
            bool HasBeenInserted = true;

            if (HasBeenInserted)
            {
                try
                {
                    User temp = new User(Id);
                    temp.Select();

                    if (Username != temp.Username)
                    {
                        Debug.WriteLine(string.Format("----------Attempting to update {0}'s username...", temp.FullName));
                        if (string.IsNullOrEmpty(Username))
                        {
                            string e = string.Format("The username which was supplied for {0} was null or empty", temp.FullName);
                            Debug.WriteLine("----------{0}. An exception will be thrown since username is a mandatory field", e);
                            throw new InvalidCastException(e);
                        }


                        SqlCommand UpdateUsername = new SqlCommand("UPDATE t_Users SET username = @username WHERE id = @id", pOpenConn);
                        UpdateUsername.Parameters.Add(new SqlParameter("id", Id));
                        UpdateUsername.Parameters.Add(new SqlParameter("username", Username));

                        UpdateUsername.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s username was updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s username was not changed. Not updating {0}'s username.", temp.FullName));
                    }


                    if (Email != temp.Email || Email == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s email...", temp.FullName));

                        SqlCommand UpdateEmail = new SqlCommand("UPDATE t_Users SET email = @email WHERE id = @id", pOpenConn);

                        UpdateEmail.Parameters.Add(new SqlParameter("id", Id));
                        if (Email == "")
                        {
                            UpdateEmail.Parameters.Add(new SqlParameter("email", DBNull.Value));
                            Debug.WriteLine(string.Format("----------{0}'s email will be set to NULL in the database", temp.FullName));
                        }
                        else
                            UpdateEmail.Parameters.Add(new SqlParameter("email", Email));

                        UpdateEmail.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s email was updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s email was not changed. Not updating {0}'s email", temp.FullName));
                    }

                    if (Password.Length > 0 && Password != temp.Password || Password == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s password...", temp.FullName));

                        SqlCommand UpdatePwd = new SqlCommand("UPDATE t_Users SET password = @password WHERE id = @id", pOpenConn);

                        UpdatePwd.Parameters.Add(new SqlParameter("id", Id));
                        if (Password == "")
                        {
                            UpdatePwd.Parameters.Add(new SqlParameter("password", DBNull.Value));
                            Debug.WriteLine(string.Format("----------{0}'s password will be set to NULL in the database", temp.FullName));
                        }
                        else
                            UpdatePwd.Parameters.Add(new SqlParameter("password", Password));

                        UpdatePwd.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s password updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s password was not changed. Not updating {0}'s password.", temp.FullName));
                    }

                    if (Forename != temp.Forename)
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s forename. {1}'s forename will be changed to \"{2}\"...", temp.FullName, temp.Forename, Forename));

                        if (string.IsNullOrEmpty(Forename))
                        {
                            string e = "The forename which was supplied was null or empty";
                            Debug.WriteLine(string.Format("----------{0}. An exception will be thrown since forename is a mandatory field", e));
                            throw new InvalidCastException(e);
                        }

                        SqlCommand UpdateForename = new SqlCommand("UPDATE t_Users SET forename = @forename WHERE id = @id", pOpenConn);

                        UpdateForename.Parameters.Add(new SqlParameter("id", Id));
                        UpdateForename.Parameters.Add(new SqlParameter("forename", Forename));



                        UpdateForename.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s forename updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s forename was not changed. Not updating {0}'s forename.", temp.FullName));
                    }

                    if (Surname != temp.Surname)
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s surname. {1}'s surname will be changed to \"{2}\"...", temp.FullName, temp.Surname, Surname));

                        if (string.IsNullOrEmpty(Surname))
                        {
                            string e = "The surname which was supplied was null or empty";
                            Debug.WriteLine(string.Format("----------{0}. An exception will be thrown since surname is a mandatory field", e));
                            throw new InvalidCastException(e);
                        }

                        SqlCommand updateSurname = new SqlCommand("UPDATE t_Users SET surname = @surname WHERE id = @id", pOpenConn);

                        updateSurname.Parameters.Add(new SqlParameter("id", Id));
                        updateSurname.Parameters.Add(new SqlParameter("surname", Surname));

                        updateSurname.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s surname was updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s surname was not changed. Not updating {0}'s surname.", temp.FullName));
                    }

                    if (_gender != temp.GenderUpper || _gender == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s gender...", temp.FullName));

                        SqlCommand updateGender = new SqlCommand("UPDATE t_Users SET gender = @gender WHERE id = @id", pOpenConn);

                        updateGender.Parameters.Add(new SqlParameter("id", Id));
                        if (_gender == "")
                        {
                            updateGender.Parameters.Add(new SqlParameter("gender", DBNull.Value));
                            Debug.WriteLine(string.Format("----------gender will be set to NULL in the database"));
                        }
                        else
                            updateGender.Parameters.Add(new SqlParameter("gender", _gender));

                        updateGender.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s gender updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s gender was not changed. Not updating {0}'s gender.", temp.FullName));
                    }

                    if (Dob != temp.Dob || Dob.ToString() == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s dob...", temp.FullName));

                        SqlCommand UpdateDob = new SqlCommand("UPDATE t_Users SET dob = @dob WHERE id = @id", pOpenConn);

                        UpdateDob.Parameters.Add(new SqlParameter("id", Id));

                        if (Dob == DateTime.MinValue)
                        {
                            Debug.WriteLine(string.Format("----------{0}'s dob will be set to NULL in the database", temp.FullName));
                            UpdateDob.Parameters.Add(new SqlParameter("dob", DBNull.Value));
                        }
                        else
                            UpdateDob.Parameters.Add(new SqlParameter("dob", Dob));

                        UpdateDob.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s dob was updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s dob was not changed. Not updatg dob.", temp.FullName));
                    }

                    if (Address1 != temp.Address1 || Address1 == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s address1...", temp.FullName));

                        SqlCommand UpdateAddress1 = new SqlCommand("UPDATE t_Users SET address1 = @address1 WHERE id = @id", pOpenConn);

                        UpdateAddress1.Parameters.Add(new SqlParameter("id", Id));
                        if (Address1 == "")
                        {
                            UpdateAddress1.Parameters.Add(new SqlParameter("address1", DBNull.Value));
                            Debug.WriteLine(string.Format("----------{0}'s address1 will be set to NULL in the database", temp.FullName));
                        }
                        else
                            UpdateAddress1.Parameters.Add(new SqlParameter("address1", Address1));

                        UpdateAddress1.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s address1 updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("{0}'s address1 was not changed. Not updating address1.", temp.FullName));
                    }

                    if (Address2 != temp.Address2 || Address2 == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s address2...", temp.FullName));

                        SqlCommand UpdateAddress2 = new SqlCommand("UPDATE t_Users SET address2 = @address2 WHERE id = @id", pOpenConn);

                        UpdateAddress2.Parameters.Add(new SqlParameter("id", Id));
                        if (Address2 == "")
                        {
                            UpdateAddress2.Parameters.Add(new SqlParameter("address2", DBNull.Value));
                            Debug.WriteLine(string.Format("----------{0}'s address2 will be set to NULL in the database", temp.FullName));
                        }

                        else
                            UpdateAddress2.Parameters.Add(new SqlParameter("address2", Address2));

                        UpdateAddress2.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s address2 was updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s address2 was not changed. Not updating address2.", temp.FullName));
                    }

                    if (Address3 != temp.Address3 || Address3 == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s address3...", temp.FullName));

                        SqlCommand UpdateAddress3 = new SqlCommand("UPDATE t_Users SET address3 = @address3 WHERE id = @id", pOpenConn);

                        UpdateAddress3.Parameters.Add(new SqlParameter("id", Id));
                        if (Address3 == "")
                        {
                            UpdateAddress3.Parameters.Add(new SqlParameter("address3", DBNull.Value));
                            Debug.WriteLine(string.Format("----------{0}'s address3 will be set to NULL in the database", temp.FullName));
                        }
                        else
                            UpdateAddress3.Parameters.Add(new SqlParameter("address3", Address3));

                        UpdateAddress3.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s address3 was updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s address3 was not changed. Not updating {0}'s address3.", temp.FullName));
                    }

                    if (PostCode != temp.PostCode || PostCode == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s postcode...", temp.FullName));

                        SqlCommand UpdatePostCode = new SqlCommand("UPDATE t_Users SET postCode = @postCode WHERE id = @id", pOpenConn);

                        UpdatePostCode.Parameters.Add(new SqlParameter("id", Id));
                        if (PostCode == "")
                        {
                            UpdatePostCode.Parameters.Add(new SqlParameter("postCode", DBNull.Value));
                            Debug.WriteLine(string.Format("----------{0}'s postCode will be set to NULL in the database", temp.FullName));
                        }
                        else
                            UpdatePostCode.Parameters.Add(new SqlParameter("postCode", PostCode));

                        UpdatePostCode.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s postCode was updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s postCode was not changed. Not updating {0}'s postcode.", temp.FullName));
                    }

                    if (MobilePhone != temp.MobilePhone || MobilePhone == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s mobilePhone...", temp.FullName));

                        SqlCommand UpdateMobilePhone = new SqlCommand("UPDATE t_Users SET mobilePhone = @mobilePhone WHERE id = @id", pOpenConn);

                        UpdateMobilePhone.Parameters.Add(new SqlParameter("id", Id));
                        if (MobilePhone == "")
                        {
                            UpdateMobilePhone.Parameters.Add(new SqlParameter("mobilePhone", DBNull.Value));
                            Debug.WriteLine(string.Format("----------{0}'s mobilePhone will be set to NULL in the database", temp.FullName));
                        }
                        else
                            UpdateMobilePhone.Parameters.Add(new SqlParameter("mobilePhone", MobilePhone));

                        UpdateMobilePhone.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s mobilePhone updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("{0}'s mobilePhone was not changed. Not updating {0}'s mobilePhone.", temp.FullName));
                    }

                    if (HomePhone != temp.HomePhone || HomePhone == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s homePhone...", temp.FullName));

                        SqlCommand UpdateHomePhone = new SqlCommand("UPDATE t_Users SET homePhone = @homePhone WHERE id = @id", pOpenConn);

                        UpdateHomePhone.Parameters.Add(new SqlParameter("id", Id));
                        if (HomePhone == "")
                        {
                            UpdateHomePhone.Parameters.Add(new SqlParameter("homePhone", DBNull.Value));
                            Debug.WriteLine(string.Format("----------{0}'s homePhone will be set to NULL in the database", temp.FullName));
                        }
                        else
                            UpdateHomePhone.Parameters.Add(new SqlParameter("homePhone", HomePhone));

                        UpdateHomePhone.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s homePhone was updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s homePhone was not changed. Not updating {0}'s home phone.", temp.FullName));
                    }

                    if (WorkPhone != temp.WorkPhone || WorkPhone == "")
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s workPhone...", temp.FullName));

                        SqlCommand updateWorkPhone = new SqlCommand("UPDATE t_Users SET workPhone = @workPhone WHERE id = @id", pOpenConn);

                        updateWorkPhone.Parameters.Add(new SqlParameter("id", Id));
                        if (WorkPhone == "")
                        {
                            updateWorkPhone.Parameters.Add(new SqlParameter("workPhone", DBNull.Value));
                            Debug.WriteLine(string.Format("----------{0}'s workPhone will be set to NULL in the database", temp.FullName));
                        }
                        else
                            updateWorkPhone.Parameters.Add(new SqlParameter("workPhone", WorkPhone));

                        updateWorkPhone.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s workPhone was updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s workPhone was not changed. Not updating {0}'s workPhone", temp.FullName));
                    }

                    if (AccountType != temp.AccountType)
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s accountType...", temp.FullName));

                        SqlCommand updateAccountType = new SqlCommand("UPDATE t_Users SET type = @accountType WHERE id = @id", pOpenConn);

                        updateAccountType.Parameters.Add(new SqlParameter("id", Id));
                        updateAccountType.Parameters.Add(new SqlParameter("accountType", AccountType));

                        updateAccountType.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s accountType updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s accountType was not changed. Not updating {0}'s accountType.", temp.FullName));
                    }

                    if (AccountStatus != temp.AccountStatus)
                    {
                        Debug.WriteLine(string.Format("----------Updating {0}'s accountStatus...", temp.FullName));

                        SqlCommand updateAccountStatus = new SqlCommand("UPDATE t_Users SET status = @accountStatus WHERE id = @id", pOpenConn);

                        updateAccountStatus.Parameters.Add(new SqlParameter("id", Id));
                        updateAccountStatus.Parameters.Add(new SqlParameter("accountStatus", AccountStatus));

                        updateAccountStatus.ExecuteNonQuery();

                        Debug.WriteLine(string.Format("----------{0}'s accountStatus updated successfully!", temp.FullName));
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("----------{0}'s accountStatus was not changed. Not updating {0}'s accountStatus.", temp.FullName));
                    }

                }
                catch (Exception e)
                {
                    string error = string.Format("[UacApi.User.DbUpdate] There was an error whilst updating {0}'s account ({1}): {2}", FullName, Username, e);
                    Debug.WriteLine(error);
                    new LogEntry(Log.Default) { text = error };
                    return false;
                }
                new LogEntry(Log.Default) { text = string.Format("{0}'s details were successfully updated on the database!", FullName) };
                return true;
            }
            else
            {
                throw new System.InvalidOperationException("Account cannot be updated. Account must be inserted into the database before it can be updated!");
            }
        }

        internal bool DbDelete(SqlConnection pOpenConn)
        {
            //AccountLog.Debug("Attempting to get user details...");
            //Debug.WriteLine(string.Format("----------DbSelect() - Attempting to get user details with id {0}...", Id));

            try
            {
                SqlCommand deleteAccount = new SqlCommand("DELETE FROM t_Users WHERE id = @id", pOpenConn);
                deleteAccount.Parameters.Add(new SqlParameter("id", Id.ToString()));

                int recordsFound = deleteAccount.ExecuteNonQuery();
                //Debug.WriteLine(string.Format("----------DbDelete() - Found {0} record(s) ", recordsFound));

                if (recordsFound > 0)
                {
                    //Debug.WriteLine("----------DbDelete() - Got user's details successfully!");
                    //AccountLog.Debug("Got user's details successfully!");
                    return true;
                }
                else
                {
                    Debug.WriteLine("----------DbDelete() - No user data was deleted");
                    //AccountLog.Debug("Got user's details successfully!");
                    return false;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("There was an exception whilst deleting user data: " + e);
                return false;
            }
        }
        

    }

    public class User
    {
        Log DebugLog = new Log("Debug");

        #region Properties
        private Guid _id;
        public Guid Id { get { return _id; } private set { /*Debug.WriteLine(string.Format("----------{0}'s id is being set to = {1}",_id, value));*/ _id = value; } }
        private string _username;
        public string Username { get { return _username; } set { /*Debug.WriteLine(string.Format("----------username is being set to = {0}", value));*/ _username = value; } }
        private string _email;
        public string Email { get { return _email; } set { /*Debug.WriteLine(string.Format("----------email is being set to = {0}", value));*/ _email = value; } }
        private string _pwd;
        public string Password { get { return _pwd; } set { /*Debug.WriteLine(string.Format("----------pwd is being set to = {0}", value));*/ _pwd = value; } }
        private string _forename;
        public string Forename { get { return _forename; } set { /*Debug.WriteLine(string.Format("----------forename is being set to = {0}", value));*/ _forename = value; } }
        private string _surname;
        public string Surname { get { return _surname; } set { /*Debug.WriteLine(string.Format("----------surname is being set to = {0}", value));*/ _surname = value; } }
        private DateTime _dob;
        public DateTime Dob { get { return _dob; } set { /*Debug.WriteLine(string.Format("----------dob is being set to = {0}", value));*/ _dob = value; } }
        private string _gender;
        public string GenderUpper
        {
            get
            {
                if (_gender == null)
                    return null;

                switch (_gender)
                {
                    case "M":
                        return "Male";
                    case "F":
                        return "Female";
                    case "O":
                        return "Other";
                    default:
                        return "Error";
                }
            }
            set
            {
                //Debug.WriteLine(string.Format("----------Gender is being set to = {0}", value));
                GenderLower = value;
            }
        }
        public string GenderLower
        {
            get
            {
                switch (_gender)
                {
                    case "M":
                        return "male";
                    case "F":
                        return "female";
                    case "O":
                        return "other";
                    default:
                        return "error";
                }
            }
            set
            {
                //Debug.WriteLine(string.Format("----------gender is being set to = {0}", value));
                if (value.Length > 0)
                {
                    string tempGender = value.Substring(0, 1).ToUpper();
                    if (tempGender == "M" || tempGender == "F" || tempGender == "O")
                        _gender = tempGender;
                    else
                        throw new InvalidCastException("Could not convert gender!");
                }
            }
        }
        public string PosessivePronoun
        {
            get
            {
                if (_gender == "M")
                    return "his";

                else if (_gender == "F")
                    return "her";
                else
                    return "their";
            }
        }
        public string SubjectiveGenderPronoun
        {
            get
            {
                if (_gender == "M")
                    return "he";

                else if (_gender == "F")
                    return "she";
                else
                    return "they";
            }
        }
        public string ObjectiveGenderPronoun
        {
            get
            {
                if (_gender == "M")
                    return "him";

                else if (_gender == "F")
                    return "her";
                else
                    return "them";
            }
        }
        private string _address1;
        public string Address1 { get { return _address1; } set { /*Debug.WriteLine(string.Format("----------address1 is being set to = {0}", value));*/ _address1 = value; } }
        private string _address2;
        public string Address2 { get { return _address2; } set { /*Debug.WriteLine(string.Format("----------address2 is being set to = {0}", value));*/ _address2 = value; } }
        private string _address3;
        public string Address3 { get { return _address3; } set { /*Debug.WriteLine(string.Format("----------address3 is being set to = {0}", value));*/ _address3 = value; } }
        private string _postCode;
        public string PostCode { get { return _postCode; } set { /*Debug.WriteLine(string.Format("----------postCode is being set to = {0}", value));*/ _postCode = value; } }
        private string _mobilePhone;
        public string MobilePhone { get { return _mobilePhone; } set { /*Debug.WriteLine(string.Format("----------mobilePhone is being set to = {0}", value));*/ _mobilePhone = value; } }
        private string _homePhone;
        public string HomePhone { get { return _homePhone; } set { /*Debug.WriteLine(string.Format("----------homePhone is being set to = {0}", value));*/ _homePhone = value; } }
        private string _workPhone;
        public string WorkPhone { get { return _workPhone; } set { /*Debug.WriteLine(string.Format("----------workPhone is being set to = {0}", value));*/ _workPhone = value; } }
        private DateTime _dateTimeCreated;
        public DateTime DateTimeCreated { get { return _dateTimeCreated; } set { /*Debug.WriteLine(string.Format("----------dateTimeCreated is being set to = {0}", value));*/ _dateTimeCreated = value; } }
        private DateTime _lastLogIn;
        public DateTime LastLogIn { get { return _lastLogIn; } set { /*Debug.WriteLine(string.Format("----------lastLogIn is being set to = {0}", value));*/ _lastLogIn = value; } }
        private string _accountType;
        public string AccountType { get { return _accountType; } set { /*Debug.WriteLine(string.Format("----------accountType is being set to = {0}", value));*/ _accountType = value; } }
        private string _accountStatus;
        public string AccountStatus { get { return _accountStatus; } set { /*Debug.WriteLine(string.Format("----------accountStatus is being set to = {0}", value));*/ _accountStatus = value; } }
        private Guid _createdByUserId;
        public Guid createdByUserId { get { return _createdByUserId; } set { /*Debug.WriteLine(string.Format("----------createdByUserId is being set to = {0}", value));*/ _createdByUserId = value; } }
        public string FullName { get { return string.Format("{0} {1}", Forename, Surname); } }
        public List<string> ValidationErrors { get; set; }
        #endregion

        #region Constructors
        public User(string pWhereAmI)
        {
            //Debug.WriteLine(string.Format("User was initialised without a guid. WhereAmI = {0} Their guid will be: {1}", pWhereAmI, Guid.Empty));
            Id = Guid.Empty;

            DateTimeCreated = Parsnip.adjustedTime;
        }

        public User(Guid pGuid)
        {
            //Debug.WriteLine("User was initialised with the guid: " + pGuid);
            Id = pGuid;
        }

        public User(SqlDataReader pReader)
        {
            //Debug.WriteLine("User was initialised with an SqlDataReader. Guid: " + pReader[0]);
            AddValues(pReader);
        }

        private User()
        {

        }
        #endregion

        #region Static Methods
        public static User GetLoggedInUser(string pUsername, string pPwd)
        {
            User tempUser = new User();
            tempUser.LogIn(pUsername, false, pPwd, false, true);
            return tempUser;
        }

        public static bool LogOut()
        {
            try
            {
                Cookie.WriteSession("userName", "");
                Cookie.WriteSession("userPwd", "");
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region LogIn / LogOut


        internal bool LogIn(string pUsername)
        {
            Username = pUsername;
            return DbSelect(Parsnip.GetOpenDbConnection());
        }
        #endregion

        #region Public Methods
        public bool LogIn()
        {
            return LogIn(true);
        }

        public bool LogIn(bool silent)
        {
            string[] Cookies = GetCookies();
            string CookieUsername = Cookies[0];
            Username = Cookies[0];
            string CookiePwd = Cookies[1];


            //Debug.WriteLine("CookieUsername = " + CookieUsername);
            //Debug.WriteLine("CookiePwd = " + CookiePwd);

            if (String.IsNullOrEmpty(CookieUsername) || String.IsNullOrWhiteSpace(CookieUsername) || String.IsNullOrEmpty(CookiePwd) || String.IsNullOrWhiteSpace(CookiePwd))
            {
                return false;
            }
            else
            {
                if (LogIn(CookieUsername, false, CookiePwd, false, silent))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public bool LogIn(string pUsername, bool pRememberUsername, string pPwd, bool pRememberPwd)
        {
            return LogIn(pUsername, pRememberUsername, pPwd, pRememberPwd, true);
        }

        public bool LogIn(string pUsername, bool pRememberUsername, string pPwd, bool pRememberPwd, bool silent)
        {
            //AccountLog.Info(String.Format("[LogIn] Logging in with Username = {0} & Pwd = {1}...",pUsername, pPwd));
            //Debug.WriteLine(string.Format("----------User.Login() for {0}", Username));

            new LogEntry(DebugLog) { text = "Logging in. pRememberPwd = " + pRememberPwd };

            string dbPwd = null;
            Username = pUsername;

            using (SqlConnection conn = Parsnip.GetOpenDbConnection())
            {
                //AccountLog.Debug("[LogIn] Sql connection opened succesfully!");



                if (GetPwdFromDb())
                {
                    if (pPwd == dbPwd)
                    {
                        //Debug.WriteLine(string.Format("----------User.Login() - Got password from db for user {0}. Id = {1}. Pwd = {2}", Username, Id, Password));
                        if (GetIdFromDb())
                        {
                            //AccountLog.Debug(String.Format("[LogIn] DbPwd == Pwd ({0} == {1})", dbPwd, pPwd));
                            if (DbSelect(conn))
                            {
                                //Debug.WriteLine(string.Format("----------User.Login() - Selected user {0} whilst logging in", Username));
                                if (pRememberUsername)
                                {
                                    //AccountLog.Debug(String.Format("[LogIn] RememberUsername = true. Writing permanent username cookie (userName = {0})", pUsername));
                                    //Debug.WriteLine("----------User.Login() - Username permanently remembered!");
                                    Cookie.WritePerm("userName", pUsername);
                                }

                                if (pRememberPwd)
                                {
                                    //AccountLog.Debug(String.Format("[LogIn] RememberPassword = true. Writing permanent password cookie (userPwd = {0})", pPwd));
                                    //Debug.WriteLine("----------User.Login() - Password permanently remembered!");
                                    Cookie.WritePerm("userPwd", pPwd);
                                    Cookie.WritePerm("userPwdPerm", pPwd);
                                    //Debug.WriteLine("----------User.Login() - PERMANENT Password cookie = " + GetCookies()[1]);
                                }
                                else
                                {
                                    //This check ensures that permanent cookies 
                                    //are not replaced with temporary ones
                                    if (!Cookie.Exists("userPwd"))
                                    {
                                        Cookie.WriteSession("userPwd", pPwd);
                                    }
                                }

                                if (SetLastLogIn())
                                {
                                    //AccountLog.Info("[LogIn] Logged in successfully!");
                                    if (!silent)
                                    {
                                        Debug.WriteLine(string.Format("----------User.Login() - {0} logged in LOUDLY", FullName));
                                    }
                                    else
                                    {
                                        //Debug.WriteLine(String.Format("----------User.Login() - {0} logged in SILENTLY", FullName));
                                    }

                                    return true;
                                }

                            }
                            else
                            {
                                Debug.WriteLine(string.Format("DbSelect failed when logging user {0} in", Username));
                            }
                        }
                        else
                            Debug.WriteLine("----------User.LogIn() - Failed to get user id");
                    }
                    else
                    {
                        Debug.WriteLine(string.Format("Error whilst logging in {0}. {1} != {2}", Username, pPwd, dbPwd));
                    }
                }

                else
                {
                    Debug.WriteLine(string.Format("GetPwdFromDb() failed when logging user {0} in", Username));
                    //AccountLog.Debug(String.Format("[LogIn] DbPwd != Pwd ({0} != {1}", dbPwd, pPwd));
                }
                //AccountLog.Error("[LogIn] Failed to log in.");
                return false;

                bool GetIdFromDb()
                {
                    try
                    {
                        SqlCommand getId = new SqlCommand("SELECT id FROM t_Users WHERE username = @username", conn);
                        getId.Parameters.Add(new SqlParameter("username", pUsername));

                        using (SqlDataReader reader = getId.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Id = new Guid(reader[0].ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("[LogIn] There was an exception whilst getting the id from the database: " + e);
                        return false;
                    }
                    //AccountLog.Debug("[LogIn] Got password from database successfully!");
                    return true;
                }

                bool SetLastLogIn()
                {
                    int RecordsAffected;

                    //AccountLog.Debug("[LogIn] Attempting to set LastLogIn...");
                    try
                    {
                        //AccountLog.Debug("username = " + username);
                        SqlCommand Command = new SqlCommand("UPDATE t_Users SET lastLogIn = @date WHERE username = @username;", conn);
                        Command.Parameters.Add(new SqlParameter("username", Username));
                        Command.Parameters.Add(new SqlParameter("date", Parsnip.adjustedTime));
                        RecordsAffected = Command.ExecuteNonQuery();

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("[LogIn] There was an exception whilst setting the LastLogIn: " + e);
                        return false;
                    }

                    //AccountLog.Debug(String.Format("[LogIn] Set LastLogIn successfully! {0} records were affected.", RecordsAffected));
                    return true;
                }

                bool GetPwdFromDb()
                {
                    //AccountLog.Debug("[LogIn] Attempting to get password from database...");
                    try
                    {
                        SqlCommand getPassword = new SqlCommand("SELECT password FROM t_Users WHERE username = @username", conn);
                        getPassword.Parameters.Add(new SqlParameter("Username", pUsername));

                        using (SqlDataReader reader = getPassword.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dbPwd = reader[0].ToString().Trim();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("[LogIn] There was an exception whilst getting the password from the database: " + e);
                        return false;
                    }
                    //AccountLog.Debug("[LogIn] Got password from database successfully!");
                    return true;
                }
            }
        }

        public bool Select()
        {
            return DbSelect(Parsnip.GetOpenDbConnection());
        }

        public bool Update()
        {
            bool success;
            SqlConnection UpdateConnection = Parsnip.GetOpenDbConnection();
            if (ExistsOnDb(UpdateConnection)) success = DbUpdate(UpdateConnection); else success = DbInsert(Password, UpdateConnection);
            UpdateConnection.Close();
            return success;
        }

        public bool Delete()
        {
            return DbDelete(Parsnip.GetOpenDbConnection());
        }

        public bool Validate()
        {
            ValidationErrors = new List<string>();

            bool validateSuccess = true;

            validateSuccess = validateUsername() ? validateSuccess : false;
            validateSuccess = validateEmail() ? validateSuccess : false;
            validateSuccess = validatePwd() ? validateSuccess : false;
            validateSuccess = validateForename() ? validateSuccess : false;
            validateSuccess = validateSurname() ? validateSuccess : false;
            validateSuccess = validateDob() ? validateSuccess : false;
            validateSuccess = validateGender() ? validateSuccess : false;
            validateSuccess = validateAddress1() ? validateSuccess : false;
            validateSuccess = validateAddress2() ? validateSuccess : false;
            validateSuccess = validateAddress3() ? validateSuccess : false;
            validateSuccess = validatePostCode() ? validateSuccess : false;
            validateSuccess = validateMobilePhone() ? validateSuccess : false;
            validateSuccess = validateHomePhone() ? validateSuccess : false;
            validateSuccess = validateWorkPhone() ? validateSuccess : false;
            validateSuccess = validateDateTimeCreated() ? validateSuccess : false;
            validateSuccess = validateAccountType() ? validateSuccess : false;
            validateSuccess = validateAccountStatus() ? validateSuccess : false;

            string validateSuccessString = validateSuccess ? "was validated successfully!" : "failed to be validated. See details below:";

            new LogEntry(Log.Default) { text = string.Format("{0} {1}", FullName, validateSuccessString) };

            return validateSuccess;

            bool validateUsername()
            {
                if (Username.Length == 0)
                {
                    new LogEntry(Log.Default) { text = "Cannot create a user without a username! Username: " + Username };
                    ValidationErrors.Add("username cannot be blank");
                    return false;
                }
                else if (Username.Length > 50)
                {
                    new LogEntry(Log.Default) { text = String.Format("Username is {0} characters long. Username must be no longer than 50 characters!", Username.Length) };
                    return false;
                }
                else
                {
                    return true;
                }
            }

            bool validateEmail()
            {
                string EmailAddress = Email;

                if (EmailAddress.Length != 0)
                {
                    int AsperandIndex; //Index of "@" sign
                    int PointIndex; //Index of "."
                    string Username;
                    string MailServer;
                    string DomainExtension;

                    if (EmailAddress.Contains("@"))
                    {
                        int NoOfAsperand = EmailAddress.Split('@').Length - 1;
                        if (NoOfAsperand == 1)
                        {
                            AsperandIndex = EmailAddress.IndexOf("@");
                            Username = EmailAddress.Substring(0, AsperandIndex);
                            //MyLog.Debug("Email: Username = " + Username);
                            if (EmailAddress.Substring(AsperandIndex + 1, EmailAddress.Length - AsperandIndex - 1).Contains("."))
                            {
                                PointIndex = EmailAddress.LastIndexOf('.');
                                MailServer = EmailAddress.Substring(AsperandIndex + 1, PointIndex - AsperandIndex - 1);
                                //MyLog.Debug("Email: Mail server = " + MailServer);
                                DomainExtension = EmailAddress.Substring(PointIndex + 1, EmailAddress.Length - PointIndex - 1);
                                return true;
                            }
                            else
                            {
                                //MyLog.Warning("Email address domain does not contain a \".\". Email address will be blank!");
                                new LogEntry(Log.Default) { text = String.Format("Email address \"{0}\" does not contain a dot. Email addresses must contain a dot.", EmailAddress) };
                                ValidationErrors.Add("email address must contain a dot");
                                return false;
                            }
                        }
                        else
                        {
                            //MyLog.Warning("Email address contains too many @'s. Email address will be blank!");
                            new LogEntry(Log.Default) { text = String.Format("Email address \"{0}\" contains too many '@' signs. Email addresses must contain only one '@' sign.", EmailAddress) };
                            ValidationErrors.Add("email address cannot contain more than one @");
                            return false;
                        }
                    }
                    else
                    {
                        new LogEntry(Log.Default) { text = String.Format("Email address \"{0}\" does not contain an '@' sign. Email addresses must contain an '@' sign.", EmailAddress) };
                        ValidationErrors.Add("email address must contain at least one @");
                        //MyLog.Warning("Email address does not contain an \"@\" sign. Email address will be blank!");
                        return false;
                    }
                }
                else
                {
                    //Don't really need to be warned about blank fields.
                    //MyLog.Warning(String.Format("Email \"{0}\" is made up from blank characters! Email address will be blank!", EmailVal));
                    return true;
                }
            }

            bool validatePwd()
            {
                if (Password.Trim().Length > 0)
                {
                    if (Password.Trim().Length > 5)
                        return true;
                    else
                    {
                        new LogEntry(Log.Default) { text = String.Format("----------Password \"{0}\" is too short. Passwords must be at least 5 characters long.", Password.Trim()) };
                        ValidationErrors.Add("if you have a password, it must be at least 5 characters long");
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }

            bool validateForename()
            {
                if (Forename.Length > 0)
                    return true;
                else
                {
                    new LogEntry(Log.Default) { text = String.Format("----------Forename \"{0}\" cannot be left blank!", Forename.Trim()) };
                    ValidationErrors.Add("forename cannot be blank");
                    return false;
                }
            }

            bool validateSurname()
            {
                if (Surname.Length > 0)
                    return true;
                else
                {
                    new LogEntry(Log.Default) { text = String.Format("----------Surname \"{0}\" cannot be left blank!", Forename.Trim()) };
                    ValidationErrors.Add("surname cannot be blank");
                    return false;
                }
            }


            bool validateDob()
            {
                return true;
            }

            bool validateGender()
            {
                if (_gender != null)
                {
                    string tempGender = _gender.ToString().ToUpper();
                    if (tempGender == "M" || tempGender == "F" || tempGender == "O")
                        return true;
                    else
                    {
                        new LogEntry(Log.Default) { text = String.Format("Gender \"{0}\" is not M, F or O. Gender must be M, F or O.", tempGender) };
                        ValidationErrors.Add("gender must be M, F or O");
                        return false;
                    };
                }
                else
                {
                    return true;
                }

            }


            bool validateAddress1()
            {
                if (Address1.Length > 50)
                {
                    ValidationErrors.Add("address1 must be no more than 50 characters long");
                    return false;
                }
                else
                    return true;
            }

            bool validateAddress2()
            {
                if (Address2.Length > 50)
                {
                    ValidationErrors.Add("address2 must be no more than 50 characters long");
                    return false;
                }
                else
                    return true;
            }

            bool validateAddress3()
            {
                if (Address3.Length > 50)
                {
                    ValidationErrors.Add("address1 must be no more than 50 characters long");
                    return false;
                }
                else
                    return true;
            }

            bool validatePostCode()
            {
                return true;
            }

            bool validateMobilePhone()
            {
                return true;
            }

            bool validateHomePhone()
            {
                return true;
            }

            bool validateWorkPhone()
            {
                return true;
            }


            bool validateDateTimeCreated()
            {
                return true;
            }

            bool validateAccountType()
            {
                return true;
            }

            bool validateAccountStatus()
            {
                return true;
            }
        }

        public bool ExistsOnDb()
        {
            return ExistsOnDb(Parsnip.GetOpenDbConnection());
        }


        #endregion

        #region Other Private / Internal Methods
        

        internal bool AddValues(SqlDataReader pReader)
        {
            bool logMe = false;

            if (logMe)
                Debug.WriteLine("----------Adding values...");

            try
            {
                if (logMe)
                    Debug.WriteLine(string.Format("----------Reading id: {0}", pReader[0]));

                Id = new Guid(pReader[0].ToString());

                if (logMe)
                    Debug.WriteLine(string.Format("----------Reading username: {0}", pReader[1]));

                Username = pReader[1].ToString().Trim();
                if (pReader[2] != DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading email");

                    Email = pReader[2].ToString().Trim();
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------email is blank. Skipping email");
                }


                if (logMe)
                    Debug.WriteLine("----------Reading pwd");
                Password = pReader[3].ToString().Trim();

                if (logMe)
                    Debug.WriteLine("----------Reading forename");
                Forename = pReader[4].ToString().Trim();

                if (logMe)
                    Debug.WriteLine("----------Reading surname");
                Surname = pReader[5].ToString().Trim();


                if (pReader[6] == DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------dob is blank. Skipping dob");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading dob");
                    Dob = Convert.ToDateTime(pReader[6]);
                }

                if (pReader[7] == DBNull.Value || pReader[7].ToString() == "")
                {
                    if (logMe)
                        Debug.WriteLine("----------gender is blank. Skipping gender");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading gender");

                    GenderLower = pReader[7].ToString();
                }
                if (pReader[8] == DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------address1 is blank. Skipping address1");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading address1");

                    Address1 = pReader[8].ToString().Trim();
                }
                if (pReader[9] == DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------address2 is blank. Skipping address2");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading address2");

                    Address2 = pReader[9].ToString().Trim();
                }
                if (pReader[10] == DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------address3 is blank. Skipping address3");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading address3");

                    Address3 = pReader[10].ToString().Trim();
                }
                if (pReader[11] == DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------postCode is blank. Skipping postCode");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading postCode");

                    PostCode = pReader[11].ToString().Trim();
                }
                if (pReader[12] == DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------mobilePhone is blank. Skipping mobilePhone");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading mobilePhone");

                    MobilePhone = pReader[12].ToString().Trim();
                }
                if (pReader[13] == DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------homePhone is blank. Skipping homePhone");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading homePhone");

                    HomePhone = pReader[13].ToString().Trim();
                }
                if (pReader[14] == DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------workPhone is blank. Skipping workPhone");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading workPhone");

                    WorkPhone = pReader[14].ToString().Trim();
                }

                if (logMe)
                    Debug.WriteLine("----------Reading dateTimeCreated");

                DateTimeCreated = Convert.ToDateTime(pReader[15]);
                if (logMe)
                    Debug.WriteLine("----------dateTimeCreated = " + DateTimeCreated);

                if (pReader[16] == DBNull.Value)
                {
                    if (logMe)
                        Debug.WriteLine("----------lastLogIn is blank. Skipping lastLogIn");
                }
                else
                {
                    if (logMe)
                        Debug.WriteLine("----------Reading lastLogIn");

                    LastLogIn = Convert.ToDateTime(pReader[16]);
                }

                if (logMe)
                    Debug.WriteLine(string.Format("----------Reading {0}'s accountType", FullName));

                AccountType = pReader[17].ToString().Trim();
                if (logMe)
                    Debug.WriteLine(string.Format("----------{0}'s accountType = {1}", FullName, AccountType));

                if (logMe)
                    Debug.WriteLine("----------Reading accountStatus");

                AccountStatus = pReader[18].ToString().Trim();

                if (logMe)
                    Debug.WriteLine("added values successfully!");

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("There was an error whilst reading the User's values: ", e);
                return false;
            }
        }

        private string[] GetCookies()
        {
            string[] UserDetails = new string[2];

            if (Cookie.Read("userName") != null)
            {
                Username = Cookie.Read("userName");
                UserDetails[0] = Username;
            }
            else
            {
                UserDetails[0] = "";
            }

            if (Cookie.Read("userPwd") != null)
            {
                UserDetails[1] = Cookie.Read("userPwd");

            }
            else
            {
                UserDetails[1] = "";
            }
            
            return UserDetails;
        }

        
        #endregion
    }
}