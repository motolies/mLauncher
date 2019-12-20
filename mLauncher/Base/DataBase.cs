using mDB.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mLauncher.Base
{
    internal class DataBase
    {
        private static mDB.SQLite.Connection conn;

        static DataBase()
        {
            conn = new mDB.SQLite.Connection();
            conn.Execute("select 1;");

        }

        internal static string GetSetting(string id)
        {
            return conn.ExecuteValue<string>(string.Format("SELECT Value FROM Setting WHERE Id = '{0}';", id));
        }

        internal static T GetValue<T>(string sql)
        {
            return conn.ExecuteValue<T>(sql);
        }

        internal static DataTable GetTabs()
        {
            return conn.ExecuteReader("SELECT * FROM Tab ORDER BY Seq;");
        }

        internal static DataTable GetButtons()
        {
            return conn.ExecuteReader("SELECT * FROM Button;");
        }

        internal static DataTable ExecuteReader(string sql)
        {
            return conn.ExecuteReader(sql);
        }

        internal static void Execute(string sql)
        {
            conn.Execute(sql);
        }

        internal static int InsertButton(string tabId, int col, int row, string name, string path, byte[] icon)
        {
            SQLiteCommand command = new SQLiteCommand();
            command.CommandText = "INSERT OR IGNORE INTO Button(TabId, Col, Row, Name, Path, Icon) VALUES (@tab, @col, @row, @name, @path, @icon);";
            command.Parameters.Add("@tab", DbType.String).Value = tabId;
            command.Parameters.Add("@col", DbType.Int32).Value = col;
            command.Parameters.Add("@row", DbType.Int32).Value = row;
            command.Parameters.Add("@name", DbType.String).Value = name;
            command.Parameters.Add("@path", DbType.String).Value = path;
            command.Parameters.Add("@icon", DbType.Binary).Value = icon;
            return conn.Execute(command);
        }

        internal static int DeleteButton(string tabId, int col, int row)
        {
            string sql = string.Format("DELETE FROM Button WHERE TabId = '{0}' AND Col = {1} AND Row = {2};", tabId, col, row);
            return conn.Execute(sql);
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
CREATE TABLE IF NOT EXISTS Tab 
(
    Id    TEXT NOT NULL PRIMARY KEY,
    Name  TEXT NOT NULL,
	Seq   INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Button (
	TabId	TEXT NOT NULL,
	Col	INTEGER NOT NULL,
	Row	INTEGER NOT NULL,
	Name	TEXT NOT NULL,
    Path	TEXT NOT NULL,
	Icon	BLOB,
	PRIMARY KEY(TabId,Col,Row),
	FOREIGN KEY(TabId) REFERENCES Tab(Id) ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Setting (
	Id	TEXT NOT NULL,
	Value	TEXT NOT NULL,
	DefaultValue	TEXT NOT NULL,
	Description	INTEGER NOT NULL,
	PRIMARY KEY(Id)
);

INSERT OR IGNORE INTO Setting(Id, Value, DefaultValue, Description)
VALUES('ROWS', 4, 4, 'ROW의 갯수')
, ('COLS', 8, 8, 'COLUMN의 갯수') ;

INSERT OR IGNORE INTO Tab(Id, Name, Seq)
VALUES( 'DEFAULT', '기본 프로그램', 1);




";
            conn.Execute(initTables);





        }

    }
}
