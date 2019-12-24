using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mEx
{
    public static class FromControlEx
    {
        public static void DoubleBuffered(this Control control, bool enable)
        {
            // https://stackoverflow.com/questions/87795/how-to-prevent-flickering-in-listview-when-updating-a-single-listviewitems-text
            var doubleBufferPropertyInfo = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            doubleBufferPropertyInfo.SetValue(control, enable, null);
        }
    }
}
