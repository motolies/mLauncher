using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace mEX
{
    public static class DependencyObjectEx
    {
        /// <summary>
        /// hitTest로 콘트롤을 찾을 수가 없어서 임시방편으로 쓰는 확장메소드
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T GetVisualParent<T>(this DependencyObject element) where T : DependencyObject
        {
            while (element != null && !(element is T))
                element = VisualTreeHelper.GetParent(element);
            return (T)element;
        }
    }
}
