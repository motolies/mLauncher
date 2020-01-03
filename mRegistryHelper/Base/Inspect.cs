using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace mRegistryHelper.Base
{
    internal class Inspect
    {
        static readonly string Dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static readonly string AppExe = System.AppDomain.CurrentDomain.FriendlyName;
        internal static readonly string RunAdminMsg = @"레지스트리 설정은 관리자 권한으로만 가능합니다.
관리자 권한으로 실행시키시겠습니까? [Y/n]";

        internal static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (identity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }

        internal static void RunRunas(string param)
        {
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.UseShellExecute = true;
            procInfo.FileName = Path.Combine(Dir, AppExe);
            procInfo.Arguments = param;
            procInfo.WorkingDirectory = Environment.CurrentDirectory;
            procInfo.Verb = "runas";

            Process.Start(procInfo);
        }


    }
}
