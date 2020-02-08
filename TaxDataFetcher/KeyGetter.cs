using System;
using System.Text;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace TaxDataFetcher
{
    class KeyGetter
    {
        private static readonly string dbUrlPath = "DbKey/DbKey.txt";

        public static string GetDBUrl()
        {
            if (File.Exists(dbUrlPath))
            {
                using (StreamReader sr = new StreamReader(dbUrlPath, Encoding.UTF8))
                {
                    string key = sr.ReadToEnd();
                    return key;
                }
            }
            else return "";
        }
        public static void SetDBUrl(string url)
        {
            if (File.Exists(dbUrlPath))
            {
                using (StreamWriter sw = new StreamWriter(dbUrlPath))
                {
                    sw.Write(url);
                }
            }
        }

        public static string GetABI(string name)
        {
            using (StreamReader sr = new StreamReader("AxieData/" + name + ".txt", Encoding.UTF8))
            {
                string abi = sr.ReadToEnd();
                return abi;
            }
        }
    }
}
