using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mFileSearch.Base
{
    public class FileGroupItem : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            FileFound found = item as FileFound;

            if (string.IsNullOrWhiteSpace(found.File))
                return "Unknown File Path";
            else
                return found.File;
        }
    }
}
