
using System;
using System.Collections;
using System.Configuration;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

public class MysqlHelper0911
{
    /// <summary>
    /// string server, string database, string login, string pass, int port
    /// </summary>
    public static string connectionString = "server=localhost;database=mysql;uid=root;pwd=";//数据库链接信息
    public MysqlHelper0911()
    {

    }

    #region ExecuteNonQuery
    //执行Sql语句，返回影响的记录数
    /// <summary>
    /// 执行Sql语句，返回影响的记录数
    /// </summary>
    /// <param name="SqlString">Sql语句</param>
    /// <returns>影响的记录数</returns>
    public static int ExecuteNonQuery(string SqlString)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            using (MySqlCommand cmd = new MySqlCommand(SqlString, connection))
            {
                try
                {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (MySqlException e)
                {
                    connection.Close();
                    throw e;
                }
            }
        }
    }
    /// <summary>
    /// 执行Sql语句，返回影响的记录数
    /// </summary>
    /// <param name="SqlString">Sql语句</param>
    /// <returns>影响的记录数</returns>
    public static int ExecuteNonQuery(string SqlString, params MySqlParameter[] cmdParms)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            using (MySqlCommand cmd = new MySqlCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, null, SqlString, cmdParms);
                    int rows = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    return rows;
                }
                catch (MySqlException e)
                {
                    throw e;
                }
            }
        }
    }
    #endregion

    #region ExecuteScalar
    /// <summary>
    /// 执行一条计算查询结果语句，返回查询结果（object）。
    /// </summary>
    /// <param name="SqlString">计算查询结果语句</param>
    /// <returns>查询结果（object）</returns>
    public static object ExecuteScalar(string SqlString)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            using (MySqlCommand cmd = new MySqlCommand(SqlString, connection))
            {
                try
                {
                    connection.Open();
                    object obj = cmd.ExecuteScalar();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (MySqlException e)
                {
                    connection.Close();
                    throw e;
                }
            }
        }
    }
    /// <summary>
    /// 执行一条计算查询结果语句，返回查询结果（object）。
    /// </summary>
    /// <param name="SqlString">计算查询结果语句</param>
    /// <returns>查询结果（object）</returns>
    public static object ExecuteScalar(string SqlString, params MySqlParameter[] cmdParms)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            using (MySqlCommand cmd = new MySqlCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, null, SqlString, cmdParms);
                    object obj = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                    if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                    {
                        return null;
                    }
                    else
                    {
                        return obj;
                    }
                }
                catch (MySqlException e)
                {
                    throw e;
                }
            }
        }
    }
    #endregion

    #region ExecuteReader
    /// <summary>
    /// 执行查询语句，返回MySqlDataReader (注意：调用该方法后，一定要对MySqlDataReader进行Close )
    /// </summary>
    /// <param name="strSql">查询语句</param>
    /// <returns>MySqlDataReader</returns>
    public static MySqlDataReader ExecuteReader(string strSql)
    {
        MySqlConnection connection = new MySqlConnection(connectionString);
        MySqlCommand cmd = new MySqlCommand(strSql, connection);
        MySqlDataReader myReader = null;
        try
        {
            connection.Open();
            myReader = cmd.ExecuteReader();

            return myReader;
        }
        catch (MySqlException e)
        {
            throw e;
        }
        finally
        {
            myReader.Close();
        }
    }
    /// <summary>
    /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
    /// </summary>
    /// <param name="strSql">查询语句</param>
    /// <returns>MySqlDataReader</returns>
    public static MySqlDataReader ExecuteReader(string SqlString, params MySqlParameter[] cmdParms)
    {
        MySqlConnection connection = new MySqlConnection(connectionString);
        MySqlCommand cmd = new MySqlCommand();
        MySqlDataReader myReader = null;
        try
        {
            PrepareCommand(cmd, connection, null, SqlString, cmdParms);
            myReader = cmd.ExecuteReader();
            cmd.Parameters.Clear();
            return myReader;
        }
        catch (MySqlException e)
        {
            throw e;
        }
        finally
        {
            myReader.Close();
            cmd.Dispose();
            connection.Close();

        }
    }
    #endregion

    #region ExecuteDataTable
    /// <summary>
    /// 执行查询语句，返回DataTable
    /// </summary>
    /// <param name="SqlString">查询语句</param>
    /// <returns>DataTable</returns>
    public static DataTable ExecuteDataTable(string SqlString)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            DataSet ds = new DataSet();
            try
            {
                connection.Open();
                MySqlDataAdapter command = new MySqlDataAdapter(SqlString, connection);
                command.Fill(ds, "ds");
            }
            catch (MySqlException ex)
            {
                throw new Exception(ex.Message);
            }
            return ds.Tables[0];
        }
    }
    /// <summary>
    /// 执行查询语句，返回DataSet
    /// </summary>
    /// <param name="SqlString">查询语句</param>
    /// <returns>DataTable</returns>
    public static DataTable ExecuteDataTable(string SqlString, params MySqlParameter[] cmdParms)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            MySqlCommand cmd = new MySqlCommand();
            PrepareCommand(cmd, connection, null, SqlString, cmdParms);
            using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                try
                {
                    da.Fill(ds, "ds");
                    cmd.Parameters.Clear();
                }
                catch (MySqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds.Tables[0];
            }
        }
    }
    //获取起始页码和结束页码
    public static DataTable ExecuteDataTable(string cmdText, int startResord, int maxRecord)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            DataSet ds = new DataSet();
            try
            {
                connection.Open();
                MySqlDataAdapter command = new MySqlDataAdapter(cmdText, connection);
                command.Fill(ds, startResord, maxRecord, "ds");
            }
            catch (MySqlException ex)
            {
                throw new Exception(ex.Message);
            }
            return ds.Tables[0];
        }
    }

    public string InsertByDataTable(DataTable dataTable)
    {
        string result = string.Empty;
        if (null == dataTable || dataTable.Rows.Count <= 0)
        {
            return "添加失败！DataTable暂无数据！";
        }
        if (string.IsNullOrEmpty(dataTable.TableName))
        {
            return "添加失败！请先设置DataTable的名称！";
        }
        // 构建INSERT语句
        StringBuilder sb = new StringBuilder();
        sb.Append("INSERT INTO " + dataTable.TableName + "(");
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            sb.Append(dataTable.Columns[i].ColumnName + ",");
        }
        sb.Remove(sb.ToString().LastIndexOf(','), 1);
        sb.Append(") VALUES ");
        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            sb.Append("(");
            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                sb.Append("'" + dataTable.Rows[i][j] + "',");
            }
            sb.Remove(sb.ToString().LastIndexOf(','), 1);
            sb.Append("),");
        }
        sb.Remove(sb.ToString().LastIndexOf(','), 1);
        sb.Append(";");
        int res = -1;
        using (MySqlConnection con = new MySqlConnection(connectionString))
        {
            con.Open();
            using (MySqlCommand cmd = new MySqlCommand(sb.ToString(), con))
            {
                try
                {
                    res = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    res = -1;
                    // Unknown column 'names' in 'field list' 
                    result = "操作失败！" + ex.Message.Replace("Unknown column", "未知列").Replace("in 'field list'", "存在字段集合中！");
                }
            }
        }
        if (res > 0)
        {
            result = "恭喜添加成功!";
        }
        return result;
    }

    #endregion

    //#region PageList Without Proc
    ///// <summary>
    ///// 获取分页数据 在不用存储过程情况下
    ///// </summary>
    ///// <param name="recordCount">总记录条数</param>
    ///// <param name="selectList">选择的列逗号隔开,支持top num</param>
    ///// <param name="tableName">表名字</param>
    ///// <param name="whereStr">条件字符 必须前加 and</param>
    ///// <param name="orderExpression">排序 例如 ID</param>
    ///// <param name="pageIdex">当前索引页</param>
    ///// <param name="pageSize">每页记录数</param>
    ///// <returns></returns>
    //public static DataTable getPager(out int recordCount, string selectList, string tableName, string whereStr, string orderExpression, int pageIdex, int pageSize)
    //{
    //    int rows = 0;
    //    DataTable dt = new DataTable();
    //    MatchCollection matchs = Regex.Matches(selectList, @"top\s+\d{1,}", RegexOptions.IgnoreCase);//含有top
    //    string sqlStr = sqlStr = string.Format("select {0} from {1} where 1=1 {2}", selectList, tableName, whereStr);
    //    if (!string.IsNullOrEmpty(orderExpression)) { sqlStr += string.Format(" Order by {0}", orderExpression); }
    //    if (matchs.Count > 0) //含有top的时候
    //    {
    //        DataTable dtTemp = ExecuteDataTable(sqlStr);
    //        rows = dtTemp.Rows.Count;
    //    }
    //    else //不含有top的时候
    //    {
    //        string sqlCount = string.Format("select count(*) from {0} where 1=1 {1} ", tableName, whereStr);
    //        //获取行数
    //        object obj = ExecuteScalar(sqlCount);
    //        if (obj != null)
    //        {
    //            rows = Convert.ToInt32(obj);
    //        }
    //    }
    //    dt = ExecuteDataTable(sqlStr, (pageIdex - 1) * pageSize, pageSize);
    //    recordCount = rows;
    //    return dt;
    //}
    //#endregion

    #region 创建command
    private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
    {
        if (conn.State != ConnectionState.Open)
            conn.Open();
        cmd.Connection = conn;
        cmd.CommandText = cmdText;
        if (trans != null)
            cmd.Transaction = trans;
        cmd.CommandType = CommandType.Text;//cmdType;
        if (cmdParms != null)
        {
            foreach (MySqlParameter parameter in cmdParms)
            {
                if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                (parameter.Value == null))
                {
                    parameter.Value = DBNull.Value;
                }
                cmd.Parameters.Add(parameter);
            }
        }
    }
    #endregion

    #region InsertByDataTable

   
    #endregion




}
