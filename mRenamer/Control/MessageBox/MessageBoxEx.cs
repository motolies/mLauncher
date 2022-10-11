using System.Windows;

namespace mRenamer.Control.MessageBox
{
    public class MessageBoxEx
    {
        public static void Show(Window owner, string message)
        {
            MessageBoxWindow box = new MessageBoxWindow();
            box.Owner = owner;
            box.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            box.Message = "완료";
            box.ShowDialog();
        }
    }
}
