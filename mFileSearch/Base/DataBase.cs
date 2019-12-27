using mDB.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mFileSearch.Base
{
    internal class DataBase
    {
        private static mDB.SQLite.Connection conn;
        internal static string DbFile;

        static DataBase()
        {
            DbFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Settings.db");
            conn = new mDB.SQLite.Connection(DbFile);
            conn.Execute("select 1;");
        }

        internal static T GetValue<T>(string sql)
        {
            return conn.ExecuteValue<T>(sql);
        }

        internal static DataTable ExecuteReader(string sql)
        {
            return conn.ExecuteReader(sql);
        }

        internal static void Execute(string sql)
        {
            conn.Execute(sql);
        }

        internal static DataTable GetFolders()
        {
            return conn.ExecuteReader("SELECT * FROM Folder ORDER BY CreateDate;");
        }

        internal static DataTable GetConditions()
        {
            return conn.ExecuteReader("SELECT * FROM Condition ORDER BY CreateDate;");
        }

        internal static DataTable GetFilters()
        {
            return conn.ExecuteReader("SELECT * FROM Filters ORDER BY CreateDate;");
        }

        internal static DataTable GetExtentionWithout()
        {
            return conn.ExecuteReader("SELECT Id FROM ExtentionWithout ORDER BY CreateDate;");
        }

        internal static void DeleteFolder(TargetFolder folder)
        {
            conn.Execute(string.Format("DELETE FROM Folder WHERE Path = '{0}';", folder.Path));
        }

        internal static void SetFolder(TargetFolder folder)
        {
            conn.Execute(string.Format("INSERT OR IGNORE INTO Folder(Path, IsEnable, CreateDate) VALUES('{0}', {1}, DATETIME('NOW', 'LOCALTIME'));"
                , folder.Path, folder.Enable));
        }

        internal static void SetFolderEnable(TargetFolder folder)
        {
            conn.Execute(string.Format("UPDATE Folder SET IsEnable = {1} WHERE Path = '{0}';", folder.Path, folder.Enable));
        }

        internal static void SetCondition(string condition)
        {
            conn.Execute(string.Format("INSERT OR IGNORE INTO Condition(Id, CreateDate) VALUES('{0}', DATETIME('NOW', 'LOCALTIME'));"
                , condition));
        }

        internal static void SetFilter(string filter)
        {
            conn.Execute(string.Format("INSERT OR IGNORE INTO Filters(Id, IsBuiltIn, CreateDate) VALUES('{0}', 0, DATETIME('NOW', 'LOCALTIME'));"
                , filter));
        }

        internal static void SetExtentionWithout(string extentionWithout)
        {
            conn.Execute(string.Format("INSERT OR IGNORE INTO ExtentionWithout(Id, IsBuiltIn, CreateDate) VALUES('{0}', 0, DATETIME('NOW', 'LOCALTIME'));"
                , extentionWithout));
        }

        internal static void ResetCondition()
        {
            conn.Execute("DELETE FROM Condition;");
        }
        internal static void ResetFilter()
        {
            conn.Execute("DELETE FROM Filters WHERE IsBuiltIn = 0;");
        }

        internal static void SetExtentionWithout(DataTable extentionWithout)
        {
            foreach (DataRow row in extentionWithout.Rows)
            {
                if (row.RowState == DataRowState.Added)
                    conn.Execute(string.Format("INSERT OR IGNORE INTO ExtentionWithout(Id, IsBuiltIn, CreateDate) VALUES('{0}', 0, DATETIME('NOW', 'LOCALTIME'));", row["Id"].ToString()));
                else if (row.RowState == DataRowState.Modified)
                    conn.Execute(string.Format("UPDATE ExtentionWithout SET Id = '{0}' WHERE Id = '{1}';", row["Id"].ToString(), row["Id", DataRowVersion.Original].ToString()));
                else if (row.RowState == DataRowState.Deleted)
                    conn.Execute(string.Format("DELETE FROM ExtentionWithout WHERE Id = '{0}';", row["Id", DataRowVersion.Original].ToString()));
            }
        }

        internal static void InitDB()
        {


            #region 스키마 조회?
            // 테이블 조회 후 테이블이 없으면 테이블을 만들고 기본값을 넣는다
            DataTable dt = conn.ExecuteReader("SELECT name FROM sqlite_master WHERE type = 'table';");
            if (dt.Rows.Count < 1)
            {

            }
            #endregion

            // 테이블 생성
            string initTables =
@"
CREATE TABLE IF NOT EXISTS Condition 
(
    Id    TEXT NOT NULL PRIMARY KEY,
	CreateDate   TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Filters 
(
    Id    TEXT NOT NULL PRIMARY KEY,
    IsBuiltIn    INTEGER NOT NULL,
	CreateDate   TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Folder 
(
    Path    TEXT NOT NULL PRIMARY KEY,
	IsEnable    INTEGER NOT NULL,
    CreateDate   TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ExtentionWithout
(
    Id    TEXT NOT NULL PRIMARY KEY,
    IsBuiltIn    INTEGER NOT NULL,
	CreateDate   TEXT NOT NULL
);

INSERT OR IGNORE INTO Filters(Id, IsBuiltIn, CreateDate)
VALUES('*.*', 1, DATETIME('NOW', 'LOCALTIME'))
,('*.*', 1, DATETIME('NOW', 'LOCALTIME'))
,('*.txt', 1, DATETIME('NOW', 'LOCALTIME'))
,('*.java;*.jsp;*.css;*.js;*.htm;*.html;*.xml;*.properties', 1, DATETIME('NOW', 'LOCALTIME'))
,('*.c;*.cpp;*.h;*.cs;*.sql;*.html;*.aspx;*.css;*.js;*.py;*.xml', 1, DATETIME('NOW', 'LOCALTIME'));

INSERT OR IGNORE INTO ExtentionWithout(Id, IsBuiltIn, CreateDate)
VALUES('.dll', 1, DATETIME('NOW', 'LOCALTIME'))
,('.exe', 1, DATETIME('NOW', 'LOCALTIME'))
,('.zip', 1, DATETIME('NOW', 'LOCALTIME'))
,('.7z', 1, DATETIME('NOW', 'LOCALTIME'))
,('.db', 1, DATETIME('NOW', 'LOCALTIME'))
,('.svn-base', 1, DATETIME('NOW', 'LOCALTIME'))
,('.pdb', 1, DATETIME('NOW', 'LOCALTIME'))
,('.cache', 1, DATETIME('NOW', 'LOCALTIME'));

";
            conn.Execute(initTables);





        }

    }
}
