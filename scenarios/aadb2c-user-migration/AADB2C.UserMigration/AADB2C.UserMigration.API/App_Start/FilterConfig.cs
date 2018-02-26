using System.Web;
using System.Web.Mvc;

namespace AADB2C.UserMigration.API
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
