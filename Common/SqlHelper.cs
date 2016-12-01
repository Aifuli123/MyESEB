using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PhoneCallOrder
{
    public class SQLHelper
    {
        #region 得到SqlConnection对象connection
        /// <summary>
        /// 得到SqlConnection对象connection
        /// </summary>
        [ThreadStatic]
        public static SqlConnection Connection;
        //public static SqlConnection Connection
        //{
        //    get
        //    {
        //        return connection;
        //    }
        //    set
        //    {
        //        connection = value;
        //    }
        //}
        #endregion

        //Sql语句访问----------------------------------------------------------------------------------------

        #region 增删改...【Sql语句访问】
        /// <summary>
        /// 增删改...【Sql语句访问】
        /// </summary>
        /// <param name="cmdText">cmdText语句</param>
        /// <param name="pars">参数列表。选填！</param>
        /// <returns></returns>
        public static bool ExecuteCommand(string cmdText, params SqlParameter[] pars)
        {
            using (SqlCommand cmd = new SqlCommand(cmdText, Connection))
            {
                cmd.Parameters.AddRange(pars);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        #endregion


        #region 增删改...【Sql语句访问】
        /// <summary>
        /// 增删改...【Sql语句访问】
        /// </summary>
        /// <param name="cmdText">cmdText语句</param>
        /// <param name="pars">参数列表。选填！</param>
        /// <returns></returns>
        public static bool ExecuteSQLBulkInsert(DataTable dt, List<string> colsName = null)
        {

            using (var sbc = new SqlBulkCopy(Connection))
            {
                //服务器上目标表的名称     
                sbc.DestinationTableName = dt.TableName;
                sbc.BatchSize = 1;
                sbc.BulkCopyTimeout = 10;
                if (colsName != null)
                {
                    for (int i = 0; i < colsName.Count; i++)
                    {
                        //列映射定义数据源中的列和目标表中的列之间的关系     
                        sbc.ColumnMappings.Add(colsName[i], colsName[i]);

                    }

                }
                else
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        //列映射定义数据源中的列和目标表中的列之间的关系     
                        sbc.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);

                    }
                }
                sbc.WriteToServer(dt);

            }
            return true;


        }
        #endregion

        #region 增删改，支持事务[已合并成一条sql语句的事务]...【Sql语句访问】
        /// <summary>
        /// 增删改，支持事务[已合并成一条sql语句的事务]...【Sql语句访问】
        /// </summary>
        /// <param name="ifTran">是否是支持事务</param>
        /// <param name="cmdText">cmdText语句</param>
        /// <param name="pars">参数列表。选填！</param>
        /// <returns></returns>
        public static bool ExecuteCommand(bool ifTran, string cmdText, params SqlParameter[] pars)
        {
            SqlTransaction tran = Connection.BeginTransaction();
            SqlCommand cmd = new SqlCommand(cmdText, Connection, tran);
            cmd.Parameters.AddRange(pars);
            try
            {
                cmd.ExecuteNonQuery();//因为SqlServer使用的是T-SQL，能同时执行多条sql
                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }
        #endregion

        #region 查询第一行一列值...【Sql语句访问】
        /// <summary>
        /// 查询第一行一列值...【Sql语句访问】
        /// </summary>
        /// <param name="cmdText">cmdText语句</param>
        /// <param name="pars">参数列表。选填！</param>
        /// <returns></returns>
        public static object GetScalar(string cmdText, params SqlParameter[] pars)
        {
            using (SqlCommand cmd = new SqlCommand(cmdText, Connection))
            {
                cmd.Parameters.AddRange(pars);
                return cmd.ExecuteScalar();
            }
        }
        #endregion

        #region 返回DataTable...【Sql语句访问】
        /// <summary>
        /// 返回DataTable...【Sql语句访问】
        /// </summary>
        /// <param name="cmdText">cmdText语句</param>
        /// <param name="pars">参数列表。选填！</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string cmdText, params SqlParameter[] pars)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand(cmdText, Connection))
            {
                cmd.Parameters.AddRange(pars);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                return ds.Tables[0];
            }
        }
        public static DataTable GetDataTable(string cmdText)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand(cmdText, Connection))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                return ds.Tables[0];
            }
        }
        #endregion

        //存储过程访问---------------------------------------------------------------------------------------

        #region 增删改...【存储过程访问】
        /// <summary>
        /// 增删改...【存储过程访问】
        /// </summary>
        /// <param name="proName">存储过程名称</param>
        /// <param name="pars">参数列表。选填！</param>
        /// <returns></returns>
        public static bool ExecuteCommand_StPro(string proName, params SqlParameter[] pars)
        {
            using (SqlCommand cmd = new SqlCommand(proName, Connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(pars);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        #endregion

        #region 查询第一行一列值...【存储过程访问】
        /// <summary>
        /// 查询第一行一列值...【存储过程访问】
        /// </summary>
        /// <param name="proName">存储过程名称</param>
        /// <param name="pars">参数列表。选填！</param>
        /// <returns></returns>
        public static object GetScalar_StPro(string proName, params SqlParameter[] pars)
        {
            using (SqlCommand cmd = new SqlCommand(proName, Connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(pars);
                return cmd.ExecuteScalar();
            }
        }
        #endregion

        #region 查询存储过程输出值...【存储过程访问】
        /// <summary>
        /// 查询存储过程输出值...【存储过程访问】
        /// </summary>
        /// <param name="proName">存储过程名称</param>
        /// <param name="pars">参数列表。选填！</param>
        /// <returns></returns>
        public static object GetOutScalar_StPro(string proName, params SqlParameter[] pars)
        {
            using (SqlCommand cmd = new SqlCommand(proName, Connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(pars);
                cmd.ExecuteNonQuery();
                return cmd.Parameters["@flag"].Value;
            }
        }
        #endregion

        #region 返回DataTable...【存储过程访问】
        /// <summary>
        /// 返回DataTable...【存储过程访问】
        /// </summary>
        /// <param name="proName">存储过程名称</param>
        /// <param name="pars">参数列表。选填！</param>
        /// <returns></returns>
        public static DataTable GetDataTable_StPro(string proName, params SqlParameter[] pars)
        {
            DataSet ds = new DataSet();
            using (SqlCommand cmd = new SqlCommand(proName, Connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(pars);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                return ds.Tables[0];
            }
        }
        #endregion


        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        private static string strConnect = ConfigurationManager.AppSettings["DBCenterConnectStr"];
        /// <summary>
        /// 插入DataTable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="columnMapping">字段映射</param>
        /// <param name="tableName">表名</param>
        /// <param name="Count">每次插入条数</param>
        public static bool BulkToSqlServer(DataTable dt, string tableName, IList<SqlBulkCopyColumnMapping> columnMapping, int count = 500)
        {
            if (dt.Rows.Count == 0)
                return true;

            #region 前半段sql
            StringBuilder sqlsb = new StringBuilder();
            sqlsb.Append("insert into " + tableName + " (");
            int flag = 0;
            foreach (var item in columnMapping)
            {
                flag++;
                if (flag < columnMapping.Count)
                {
                    sqlsb.Append(item.DestinationColumn + ",");
                }
                else
                {
                    sqlsb.Append(item.DestinationColumn + ") values\r\n");
                }
            }
            string sqlFront = sqlsb.ToString();
            StringBuilder valueSb = new StringBuilder();
            #endregion

            List<string> sqlList = new List<string>();
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                i++;
                flag = 0;
                valueSb.Append("(");
                foreach (var item in columnMapping)
                {
                    flag++;
                    string val = dr[item.SourceColumn].ToString();
                    if (flag < columnMapping.Count)
                    {
                        if (dr[item.SourceColumn] == DBNull.Value)//为NULL直接插NULL
                        {
                            valueSb.Append("NULL,");
                        }
                        else if (dt.Columns[item.SourceColumn].DataType == typeof(int)
                            || dt.Columns[item.SourceColumn].DataType == typeof(double)
                            || dt.Columns[item.SourceColumn].DataType == typeof(float)
                            || dt.Columns[item.SourceColumn].DataType == typeof(decimal)
                            || dt.Columns[item.SourceColumn].DataType == typeof(long)
                            )//数字
                        {
                            valueSb.Append(val + ",");
                        }
                        else//字符串,时间
                        {
                            valueSb.Append("'" + val + "',");
                        }
                    }
                    else
                    {
                        if (dr[item.SourceColumn] == DBNull.Value)
                        {
                            valueSb.Append("NULL),\r\n");
                        }
                        else if (dt.Columns[item.SourceColumn].DataType == typeof(int)
                            || dt.Columns[item.SourceColumn].DataType == typeof(double)
                            || dt.Columns[item.SourceColumn].DataType == typeof(float)
                            || dt.Columns[item.SourceColumn].DataType == typeof(decimal)
                            || dt.Columns[item.SourceColumn].DataType == typeof(long)
                            )//数字
                        {
                            valueSb.Append(val + "),\r\n");
                        }
                        else//字符串,时间
                        {
                            valueSb.Append("'" + val + "'),\r\n");
                        }
                    }
                }
                if (i % count == 0 || i == dt.Rows.Count)
                {
                    sqlList.Add(sqlFront + valueSb.ToString().Trim(',', '\r', '\n'));
                    valueSb.Clear();
                }
            }
            return ExecTran(sqlList.ToArray());
        }


        public static bool ExecTran(string[] Sqlstr)
        {
            using (SqlConnection conn = new SqlConnection(strConnect))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.Transaction = tran;
                        try
                        {
                            int count = Sqlstr.Length;
                            for (int i = 0; i < count; i++)
                            {
                                cmd.CommandText = Sqlstr[i];
                                var c = cmd.ExecuteNonQuery();
                            }
                            tran.Commit();
                            return true;
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }

            }
        }
    }
}
