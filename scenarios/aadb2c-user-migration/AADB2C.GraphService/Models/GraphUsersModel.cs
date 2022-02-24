using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AADB2C.GraphService
{
    public class GraphUsersModel
    {
        public string odatametadata { get; set; }
        public List<GraphUserModel> value { get; set; }

        public static GraphUsersModel Parse(string JSON)
        {
            return JsonConvert.DeserializeObject(JSON.Replace("odata.metadata", "odatametadata"), typeof(GraphUsersModel)) as GraphUsersModel;
        }
    }
}
