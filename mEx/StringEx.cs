using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mEx
{
    public static class StringEx
    {
        public static List<string> LineReader(this string input)
        {
            List<string> rtn = new List<string>();
            using (StringReader reader = new StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    rtn.Add(line);
                }
            }
            return rtn;
        }

        public static bool EndWith(this string value, IEnumerable<string> values)
        {
            //https://stackoverflow.com/questions/1641499/how-can-i-use-linq-to-to-determine-if-this-string-endswith-a-value-from-a-colle
            return values.Any(value.EndsWith);
        }

    }
}
