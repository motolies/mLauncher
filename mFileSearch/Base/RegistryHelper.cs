using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mFileSearch.Base
{
    internal class RegistryHelper
    {
        internal static readonly string Dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        internal static readonly string AppExe = System.AppDomain.CurrentDomain.FriendlyName;
        static readonly string RegApp = "mRegistryHelper.exe";

        public static bool RunRunasRegistry(string param, out string result)
        {
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.UseShellExecute = true;
            procInfo.FileName = Path.Combine(Dir, RegApp);
            procInfo.Arguments = param;
            procInfo.WorkingDirectory = Environment.CurrentDirectory;
            procInfo.Verb = "runas";

            Process p = new Process();
            p.StartInfo = procInfo;

            try
            {
                p.Start();
                result = p.StandardOutput.ReadToEnd();
                return true;
            }
            catch (Exception x)
            {
                result = x.Message;
                return false;
            }
            finally
            {
                //Wait for the process to end.
                p.WaitForExit();
                p.Close();
            }

        }
    }
}
