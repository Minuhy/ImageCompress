using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImgZipBat
{
    public class Tools
    {
        public static void Log(string str, params object[] values)
        {
            Console.WriteLine(str, values);
        }
    }
}
