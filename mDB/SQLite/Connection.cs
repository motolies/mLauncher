using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mDB.SQLite
{
    public class Connection
    {
        static SQLiteConnection mConn;
        static string DBPath = "Settings.db";

        public Connection(string dbpath = null)
        {

            string mDBPath = string.Empty;
            if (!string.IsNullOrWhiteSpace(dbpath))
            {
                mDBPath = dbpath;
            }
            else
            {
                string dllPath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
                mDBPath = Path.Combine(dllPath, DBPath);
            }


            // db 파일이 있는지 검사
            if (!System.IO.File.Exists(mDBPath))
            {
                SQLiteConnection.CreateFile(mDBPath);  // SQLite DB 생성
            }

            string ConnectionString = string.Format("Data Source={0};Version=3;", mDBPath);

            mConn = new SQLiteConnection(ConnectionString);
            mConn.Open();

            this.Execute("PRAGMA foreign_keys=1;");

        }

        public DataTable ExecuteReader(string sql)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SQLiteCommand cmd = new SQLiteCommand(sql, mConn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        dt.Load(rdr);
                        return dt;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception("SQLite ERROR", e);
            }
        }

        public int Execute(string sql)
        {
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, mConn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception("SQLite ERROR", e);
            }
        }

        public int Execute(SQLiteCommand command)
        {
            try
            {
                command.Connection = mConn;
                return command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception("SQLite ERROR", e);
            }
        }

        public T ExecuteValue<T>(string sql)
        {
            try
            {
                DataTable dt = new DataTable();
                using (SQLiteCommand cmd = new SQLiteCommand(sql, mConn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        dt.Load(rdr);
                        if (dt.Rows.Count == 1)
                            return (T)dt.Rows[0][0];
                        else if (dt.Rows.Count < 1)
                            return default(T);
                        else
                            throw new Exception("리턴받은 로우의 갯수가 하나가 아닙니다. Where 조건을 확인해주세요.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception("SQLite ERROR", e);
            }
        }

    }
}
