using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AADB2C.UserMigration.Models
{
    public class UsersModel
    {
        public List<UserModel> Users;
        public bool GenerateRandomPassword;

        public UsersModel()
        {
            Users = new List<UserModel>();
        }

        /// <summary>
        /// Parse JSON string into UsersModel
        /// </summary>
        public static UsersModel Parse(string JSON)
        {
            return  JsonConvert.DeserializeObject(JSON, typeof(UsersModel)) as UsersModel;
        }
        /// <summary>
        /// Serialize the object into Json string
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class UserModel
    {
        public string email { set; get; }
        public string displayName { set; get; }
        public string firstName { set; get; }
        public string lastName { set; get; }
        public string password { set; get; }
    }
}
