using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AADB2C.GraphService
{
    public class GraphUserModel
    {
        public string objectId { get; set; }
        public bool accountEnabled { get; set; }
        public IList<SignInNames> signInNames { get; set; }
        public string creationType { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string surname { get; set; }
        public PasswordProfile passwordProfile { get; set; }
        public string passwordPolicies { get; set; }

        public GraphUserModel() { }
        public GraphUserModel(string signInName, string password, string displayName, string givenName, string surname)
        {
            this.accountEnabled = true;

            this.signInNames = new List<SignInNames>();
            this.signInNames.Add(new SignInNames(signInName));

            // always set to 'LocalAccount
            this.creationType = "LocalAccount";

            this.displayName = displayName;
            this.givenName = givenName;
            this.surname = surname;

            this.passwordProfile = new PasswordProfile(password);

            this.passwordPolicies = "DisablePasswordExpiration,DisableStrongPassword";
        }

        /// <summary>
        /// Serialize the object into Json string
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static GraphUserModel Parse(string JSON)
        {
            return JsonConvert.DeserializeObject(JSON, typeof(GraphUserModel)) as GraphUserModel;
        }
    }

    public class PasswordProfile
    {
        public string password { get; set; }
        public bool forceChangePasswordNextLogin { get; set; }

        public PasswordProfile(string password)
        {
            this.password = password;

            // always set to false
            this.forceChangePasswordNextLogin = false;
        }
    }
    public class SignInNames
    {
        public string type { get; set; }
        public string value { get; set; }

        public SignInNames(string email)
        {
            // Type must be 'emailAddress' (or 'userName')
            this.type = "emailAddress";

            // The user email address
            this.value = email;
        }
    }

    public class GraphUserSetPasswordModel
    {
        public PasswordProfile passwordProfile { get; }
        public string passwordPolicies { get; }

        public GraphUserSetPasswordModel(string password)
        {
            this.passwordProfile = new PasswordProfile(password);
            this.passwordPolicies = "DisablePasswordExpiration,DisableStrongPassword";
        }
    }
}
