using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAPBusiness
{
    public static class EAPBusiness
    {
        public static object IsNull(object value, object defaultValue)
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
            {
                return defaultValue;
            }
            return value;
        }
    }
}
