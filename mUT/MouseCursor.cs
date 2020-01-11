using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace mUT
{
    public class MouseCursor
    {


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool GetCursorPos(out POINT pt);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        public static System.Windows.Point GetCursorPos(Visual visual)
        {
            POINT tmpPoint = new POINT();
            GetCursorPos(out tmpPoint);

            System.Windows.Point CursorPoint = new System.Windows.Point(tmpPoint.X, tmpPoint.Y);
            var transform = PresentationSource.FromVisual(visual).CompositionTarget.TransformFromDevice;
            return transform.Transform(CursorPoint);
        }









    }
}
