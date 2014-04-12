using System;
using System.Collections.Generic;
using System.Text;

namespace LEAFDB
{
    class MysqlHelper
    {
        public static string generateConnectionString(string hostnameOrIP, string username, string password, string database, ushort port = 3306)
        {
            if (hostnameOrIP.Length > 0 && port > 0 && port <= 65535 && username.Length > 0 && password.Length > 0 && database.Length > 0)
                return "Server=" + hostnameOrIP + ";Database=" + database + ";Port=" + port + ";Uid=" + username + ";Pwd=" + password + ";";

            return "";
        }

        public static string generateConnectionString(string hostnameOrIP, string username, string password, ushort port = 3306)
        {
            if (hostnameOrIP.Length > 0 && port > 0 && port <= 65535 && username.Length > 0 && password.Length > 0)
                return "Server=" + hostnameOrIP + ";Port=" + port + ";Uid=" + username + ";Pwd=" + password + ";";

            return "";
        }

        public static string generateConnectionStringFromSavedSettings(bool includeDatabaseInConnectionString)
        {
            if (includeDatabaseInConnectionString)
                return generateConnectionString(Properties.Settings.Default["connectionStringHostnameIP"].ToString(), Properties.Settings.Default["connectionStringUsername"].ToString(), Properties.Settings.Default["connectionStringPassword"].ToString(), Properties.Settings.Default["connectionStringDatabase"].ToString(), Convert.ToUInt16(Properties.Settings.Default["connectionStringPort"]));

            return generateConnectionString(Properties.Settings.Default["connectionStringHostnameIP"].ToString(), Properties.Settings.Default["connectionStringUsername"].ToString(), Properties.Settings.Default["connectionStringPassword"].ToString(), Convert.ToUInt16(Properties.Settings.Default["connectionStringPort"]));
        }
    }
}
