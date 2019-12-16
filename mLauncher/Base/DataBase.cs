using mDB.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mLauncher.Base
{
    internal class DataBase
    {
        internal static void InitDB(Connection conn)
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
    Id    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Name  TEXT NOT NULL,
	Seq   INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Button (
	TabId	INTEGER NOT NULL,
	Col	INTEGER NOT NULL,
	Row	INTEGER NOT NULL,
	Name	TEXT NOT NULL,
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
";
            conn.Execute(initTables);





        }

    }
}
