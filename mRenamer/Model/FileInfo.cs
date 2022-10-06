using mRenamer.Model;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mRenamer
{
    public class FileInfo
    {
        public string originalFileName { get; set; }
        public string oldFileName { get; set; }
        public string tempFileName { get; set; }
        public string targetFileName { get; set; }
        public string extentionName { get; set; }
        public string path { get; set; }

        public StatusEnum status { get; set; }

        public string fullPath
        {
            get
            {
                return Path.Combine(path, originalFileName);
            }
        }

        public void RemoveFileName()
        {
            this.targetFileName = extentionName;
        }

        public void RemoveFileNameWithPosition(int start, int length)
        {
            int maxCount = Path.GetFileNameWithoutExtension(targetFileName).Length;

            if (length > maxCount)
                length = maxCount;

            this.targetFileName = (Path.GetFileNameWithoutExtension(targetFileName)).Remove(start - 1, length) + extentionName;
        }

        public void AddName(PositionEnum position, string addName)
        {
            if (position == PositionEnum.PreFix)
                this.targetFileName = addName + targetFileName;
            else
                this.targetFileName = Path.GetFileNameWithoutExtension(targetFileName) + addName + extentionName;
        }

        public void ZeroPadding(PositionEnum position, int length)
        {
            this.targetFileName = ZeroPaddingFileName(position, targetFileName, length);
        }

        private string ZeroPaddingFileName(PositionEnum position, string fileName, int length)
        {
            var regex = new Regex(@"\d+");
            var matches = regex.Matches(fileName);
            string numberFormat = new string('0', length);

            int replacePosition = 0;
            if (position == PositionEnum.PostFix)
                replacePosition = matches.Count - 1;

            StringBuilder result = new StringBuilder();
            if (matches.Count > 0)
            {
                var match = matches[replacePosition];
                int number;
                var alphanum = int.TryParse(match.Value, out number)
                                    ? number.ToString(numberFormat)
                                    : match.Value;
                return result.Append(fileName.Substring(0, match.Index))
                        .Append(alphanum)
                        .Append(fileName.Substring(match.Index + match.Length)).ToString();
            }
            else
                return fileName;
        }

        public void ReplaceName(string from, string to)
        {
            this.targetFileName = Path.GetFileNameWithoutExtension(targetFileName).Replace(from, to) + extentionName;
        }

        public bool ExistsOriginFile()
        {
            return File.Exists(Path.Combine(path, originalFileName));
        }

        public void ChangeTempFileName()
        {
            Guid guid = Guid.NewGuid();
            string origin = Path.Combine(path, originalFileName);
            string temp = Path.Combine(path, guid.ToString().Replace("-", String.Empty).ToUpper() + extentionName);
            this.tempFileName = Path.GetFileName(temp);
            this.status = StatusEnum.Running;
            try
            {
                File.Move(origin, temp);
            }
            catch
            {
                this.status = StatusEnum.Error;
            }
        }

        public void ChangeTargetFileName()
        {
            string origin = Path.Combine(path, tempFileName);
            string target = Path.Combine(path, targetFileName);

            try
            {
                File.Move(origin, target);
                this.status = StatusEnum.Success;
                this.oldFileName = originalFileName;
                this.originalFileName = targetFileName;
            }
            catch
            {
                this.status = StatusEnum.Error;
            }
        }

        public void Rollback()
        {
            if (status == StatusEnum.Success)
            {
                // old로 변경
                string origin = Path.Combine(path, originalFileName);
                string target = Path.Combine(path, oldFileName);
                File.Move(origin, target);
                this.originalFileName = this.targetFileName = this.oldFileName;
                this.oldFileName = String.Empty;
            }
            else if (status == StatusEnum.Error)
            {
                // temp에서 변경, temp가 못 된 파일들은 관계 없을 듯
                string origin = Path.Combine(path, tempFileName);
                string target = Path.Combine(path, originalFileName);
                File.Move(origin, target);
            }

            this.status = StatusEnum.Ready;
        }


    }
}
