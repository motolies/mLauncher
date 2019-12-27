using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mUT
{
   public class FileIO
    {

        //인코딩 확인
        public static Encoding GetFileEncoding(string srcFile)
        {
            try
            {
                // *** Use Default of Encoding.Default (Ansi CodePage)
                Encoding enc = Encoding.Default;
                // *** Detect byte order mark if any - otherwise assume default
                byte[] buffer = new byte[10];
                FileStream file = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                file.Read(buffer, 0, 10);
                file.Close();
                if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                    enc = Encoding.UTF8; //with BOM
                else if (buffer[0] == 0xfe && buffer[1] == 0xff)
                    enc = Encoding.Unicode;
                else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff)
                    enc = Encoding.UTF32;
                else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76)
                    enc = Encoding.UTF7;
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                    // 1201 unicodeFFFE Unicode (Big-Endian)
                    enc = Encoding.GetEncoding(1201);
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                    // 1200 utf-16 Unicode
                    enc = Encoding.GetEncoding(1200);
                else if (ValidateUTF8WithoutBOM(srcFile))
                    enc = new UTF8Encoding(false);
                return enc;
            }
            catch
            {
                return null;
            }
        }
        public static bool ValidateUTF8WithoutBOM(string filePath)
        {
            //ansi로 인코딩 된 파일을 utf-8을 사용하여 읽으면 각 기본 언어마다 특수문자가 포함되는 것같다.
            //euc-kr 에서 utf-8로 읽으면 "� = (char)0xfffd)"가 생기는 것 같다. 
            using (FileStream fs = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                if (fs.Length < 1024 * 1000)
                {
                    if (sr.ReadToEnd().Contains((char)0xfffd))
                        return false;
                    else
                        return true;
                }

                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.Contains((char)0xfffd))
                        return false;
                }
            }
            return true;
        }

    }
}
