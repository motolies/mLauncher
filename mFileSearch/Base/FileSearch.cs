using mEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mFileSearch.Base
{
    public class FileSearch
    {
        public static bool IsStop { get; set; }

        public static void SearchFile(string condition, string filter, bool isSubFolder, bool isRegex)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<string> files = new List<string>();
            string searchTerm = readControlText(cbCondition).ToLower();
            string[] filter = readControlText(cbFilter).Trim().ToLower().Replace("*.*", string.Empty).Replace("*", string.Empty).Split(';');

            foreach (string folder in chkLstFolder.CheckedItems)
            {
                SearchOption so = isSubFolder ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                files.AddRange(Directory.GetFiles(folder, "*.*", so));
            }

            var withoutFiles = files.Where(ext => !ext.EndWith(set.woExtensionList));
            var fileList = withoutFiles.Where(ext => ext.ToLower().EndWith(filter));

            int totalFile = fileList.Count();
            int curFile = 0;
            Parallel.ForEach(fileList, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (file, stateFile) =>
            {
                if (IsStop)
                    stateFile.Stop();

                int lineNumber = 1;

                Encoding encoding = GetFileEncoding(file);
                if (encoding == null)
                    return;

                foreach (string line in File.ReadLines(file, encoding))
                {
                    if (IsStop)
                        break;

                    if ((isRegex && Regex.IsMatch(line, searchTerm, RegexOptions.IgnoreCase)) || line.ToLower().Contains(searchTerm))
                    {
                        MatchedCount++;
                        lock (syncLineNumber)
                        {
                            //검색 중간중간에 리스트에 뿌린다
                            AddItemDele(file, lineNumber.ToString(), line);
                        }
                    }
                    lineNumber++;
                }
                curFile++;
                if (curFile % 10 == 0)
                    tw.ReportProgress((float)curFile / (float)totalFile);

            });
            tw.ReportProgress((float)1);
            sw.Stop();
            Console.WriteLine("검색종료 : " + sw.ElapsedMilliseconds);
        }


        //인코딩 확인
        private static Encoding GetFileEncoding(string srcFile)
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
        private static bool ValidateUTF8WithoutBOM(string filePath)
        {
            //ansi로 인코딩 된 파일을 utf-8을 사용하여 읽으면 각 기본 언어마다 특수문자가 포함되는 것같다.
            //euc-kr 에서 utf-8로 읽으면 "� = (char)0xfffd)"가 생기는 것 같다. 
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
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
