using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBJ.Tool.Json
{
    public class JsonPool
    {

        private static string _loadFloder = "";
        public static string LoadFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_loadFloder))
                {
                    try
                    {
                        _loadFloder = ConfigurationSettings.AppSettings["LoadFolder"].ToString();
                    }
                    catch
                    {

                    }
                }
                return _loadFloder;
            }            
        }

        private static string _saveFloder = "";
        public static string SaveFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_saveFloder))
                {
                    try
                    {
                        _saveFloder = ConfigurationSettings.AppSettings["SaveFolder"].ToString();
                    }
                    catch
                    {

                    }
                }
                return _saveFloder;
            }
        }
    }
}
