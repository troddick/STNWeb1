//------------------------------------------------------------------------------
//----- STNServicesMembershipProvider.cs ----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Membership provider for REST Services
// 
//   discussion:   
//  ex: http://bojanskr.blogspot.com/2011/12/custom-membership-provider.html
//  another better example: http://theintegrity.co.uk/2010/11/asp-net-mvc-2-custom-membership-provider-tutorial-part-2/  -- using STNServiceCaller like
//  validateUser does.
//     

#region Comments
// 09.27.12 - JB - Created
#endregion

using System;
using System.Web;
using System.Web.Security;
using System.Configuration;
using System.Data.Linq;

using RestSharp;

using STNServices;
using STNServices.Authentication;
using STNWeb.Utilities;

namespace STNWeb.Providers
{
    public class STNServicesMembershipProvider : MembershipProvider
    {
        #region class variables
        
        private int newPasswordLength = 16;

        #endregion class variables

        
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword)) return false;
            if (oldPassword == newPassword) return false;
            MembershipUser user = GetUser(username);

            if (user == null) return false;
            //CustomDataDataContext db = new CustomDataDataContext();
            //var RawUser = (from u in db.Users
            //               where u.UserName == user.UserName && u.DeletedOn == null
            //               select u).FirstOrDefault();

            //if (string.IsNullOrWhiteSpace(RawUser.Password)) return false;
            //RawUser.Password = EncodePassword(newPassword);
            //db.SubmitChanges();
            return true;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }
            //if ((RequiresUniqueEmail && (GetUserNameByEmail(email) != String.Empty)))
            //{
            //    status = MembershipCreateStatus.DuplicateEmail;
            //    return null;
            //}

            MembershipUser membershipUser = GetUser(username, false);

            if (membershipUser == null)
            {
                try
                {
                    //using (CustomDataDataContext _db = new CustomDataDataContext())
                    //{
                    MEMBER user = new MEMBER();

                    user.USERNAME = username;
                    //user.password = EncodePassword(password);
                    user.EMAIL = email.ToLower();
                    //_db.Users.InsertOnSubmit(user);
                    //_db.SubmitChanges();
                    status = MembershipCreateStatus.Success;
                    return GetUser(username, false);
                    //}
                }
                catch
                {
                    status = MembershipCreateStatus.ProviderError;
                }
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }
            return null;
        }
       
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public MembershipUser GetUser(string username)
        {
            MembershipUser CustomMembershipUser = null;
            //using (CustomDataDataContext _db = new CustomDataDataContext())
            //{
                try
                {
                    //MEMBER user = (from u in _db.Users
                    //             where u.UserName == username && u.DeletedOn == null
                    //             select u)
                    //             .FirstOrDefault();
                    //if (user != null)
                    //{
                    //    CustomMembershipUser = new CustomMembershipUser(
                    //        this.Name,
                    //        user.UserName,
                    //        null,
                    //        user.Email,
                    //        "",
                    //        "",
                    //        true,
                    //        false,
                    //        user.CreatedOn,
                    //        DateTime.Now,
                    //        DateTime.Now,
                    //        default(DateTime),
                    //        default(DateTime),
                    //        user.CompanyFK,
                    //        user.Name);
                    //}
                }
                catch
                {
                }
            //}
                return CustomMembershipUser;
        }
        
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser memUser = null;
            try
            {
                
                if (username.Length == 0) { return null; }

                STNServiceCaller serviceCaller = STNServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/Members?username={userName}";
                request.RootElement = "Member";
                request.AddParameter("userName", username, ParameterType.UrlSegment);
                MEMBER thisMember = serviceCaller.Execute<MEMBER>(request);

                if (thisMember != null)
                {
                    memUser = new MembershipUser("STNServicesMembershipProvider",
                                                  thisMember.USERNAME, null, thisMember.EMAIL, string.Empty,
                                                  string.Empty, true, false, DateTime.Now,
                                                  DateTime.Now, DateTime.Now,
                                                  DateTime.Now, DateTime.Now);
                }
            }
            catch
            {
            }
            return memUser;

        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return 3; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {

            get { return 8; }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            STNServiceCaller serviceCaller = STNServiceCaller.Instance;
            return serviceCaller.setAuthentication(username, password);
            
        }

    }
}