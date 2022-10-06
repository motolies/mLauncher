using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace mRenamer
{
    public class FileMove
    {

        public static void Run(string sourceFile, string destinationFile)
        {
            // https://stackoverflow.com/questions/14162983/system-io-file-move-how-to-wait-for-move-completion
            try
            {
                using (FileStream sourceStream = File.Open(sourceFile, FileMode.Open))
                {
                    using (FileStream destinationStream = File.Create(destinationFile))
                    {
                        sourceStream.CopyTo(destinationStream);
                        sourceStream.Close();
                        File.Delete(sourceFile);
                    }
                }
            }
            catch (IOException ioex)
            {
                MessageBox.Show("An IOException occured during move, " + ioex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An Exception occured during move, " + ex.Message);
            }
        }

    }
}
