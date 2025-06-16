using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Collections.Generic;

//using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.Transactions;
using static System.Net.Mime.MediaTypeNames;

namespace GoViewServer
{
    public class sqlite_define
    {
        //初始化
        //CREATE TABLE IF NOT EXISTS project_property (project_id TEXT, project_name TEXT);
        //CREATE TABLE IF NOT EXISTS project_list (page_id TEXT, project_id TEXT, project_name TEXT);

        //根据 page_id 在 project_list 表查找 project_name;在 project_name 表中删除 page_id 行
        public static string page_delete(SQLiteConnection connection, string page_id)
        {
            try
            {
                string projectNameResult = null;
                string pageNumberResult = null;
                string queryPageNumber = "SELECT project_name FROM project_list WHERE page_id = @id";

                using (SQLiteCommand cmdPageNumber = new SQLiteCommand(queryPageNumber, connection))
                {
                    cmdPageNumber.Parameters.AddWithValue("@id", page_id);

                    using (SQLiteDataReader reader = cmdPageNumber.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // 获取值并转换为string（虽然SQLite返回的本来就是string类型）
                            projectNameResult = reader["project_name"].ToString();
                            //Console.WriteLine($"[page_number表] 找到记录：project_name = {projectNameResult}");
                        }
                        else
                        {
                            //Console.WriteLine($"[page_number表] 未找到ID为 {page_id} 的记录");
                        }
                    }
                }

                // 第二个查询：从project_name表获取page_number
                string queryProjectName = "SELECT page_number FROM '" + projectNameResult + "' WHERE page_id = @id";

                using (SQLiteCommand cmdProjectName = new SQLiteCommand(queryProjectName, connection))
                {
                    cmdProjectName.Parameters.AddWithValue("@id", page_id);

                    using (SQLiteDataReader reader = cmdProjectName.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // 获取值并转换为string
                            pageNumberResult = reader["page_number"].ToString();
                            //Console.WriteLine($"[project_name表] 找到记录：page_number = {pageNumberResult}");
                        }
                        else
                        {
                            //Console.WriteLine($"[project_name表] 未找到ID为 {page_id} 的记录");
                        }
                    }
                }

                SQLiteTransaction transaction = connection.BeginTransaction();

                // 3. 删除project_name表中的记录
                string deleteQuery = "DELETE FROM '" + projectNameResult + "' WHERE page_id = @id";
                using (SQLiteCommand cmd = new SQLiteCommand(deleteQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@id", page_id);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    //Console.WriteLine($"已删除 {rowsAffected} 条记录");
                }

                // 4. 更新page_number表中大于limit的值
                string updateQuery = "UPDATE '" + projectNameResult + "' SET page_number = page_number - 1 WHERE page_number > @limit";
                using (SQLiteCommand cmd = new SQLiteCommand(updateQuery, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@limit", pageNumberResult);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    //Console.WriteLine($"已更新 {rowsAffected} 条记录");
                }

                transaction.Commit();
                //Console.WriteLine("事务已提交");


                return "success";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        //根据 page_id 在 page_list 表查找 project_name;在 project_name 表中修改 page_id 行
        public static string pages_edit(SQLiteConnection connection, ArrayList pages_data)
        {
            try
            {
                for (int i = 0; i < pages_data.Count; i++)
                {
                    Dictionary<string, string> single_page = (Dictionary<string, string>)pages_data[i];
                    string page_id = single_page["page_id"].ToString();
                    string indexImage = single_page["indexImage"].ToString();
                    string page_name = single_page["page_name"].ToString();
                    string remarks = single_page["remarks"].ToString();
                    string page_number = single_page["page_number"].ToString();

                    string sql = "SELECT project_name FROM project_list WHERE page_id = '" + page_id + "';";
                    using var command_select = new SQLiteCommand(sql, connection);
                    SQLiteDataReader reader_select = command_select.ExecuteReader();
                    {
                        if (reader_select.Read())
                        {
                            // 获取查询结果（假设project_id是整数类型）
                            string project_name = reader_select.GetString(reader_select.GetOrdinal("project_name"));
                            string query = "UPDATE " + project_name + " SET indexImage = '" + indexImage + "', page_name = '" + page_name + "', remarks = '" + remarks + "', page_number = '" + page_number + "' WHERE page_id = '" + page_id + "'; ";
                            using var command = new SQLiteCommand(query, connection);
                            using var reader = command.ExecuteReader();
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
                return "success";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ex.ToString();
            }
        }

        //创建页面，根据项目id在 project_property 查找项目名 project_name ，在 project_name 中新增数据，在 page_list 中新增 project_id 和 page_id 关系
        public static string page_create(SQLiteConnection connection, string project_id, string page_id, string index_image, string page_name, string remarks, string page_number, string createTime)
        {
            try
            {
                string sql = "SELECT project_name FROM project_property WHERE project_id = '" + project_id + "';";
                using var command_select = new SQLiteCommand(sql, connection);
                using (SQLiteDataReader reader_select = command_select.ExecuteReader())
                {
                    if (reader_select.Read())
                    {
                        // 获取查询结果
                        string project_name = reader_select.GetString(reader_select.GetOrdinal("project_name"));
                        string query = "INSERT INTO " + project_name + " (page_id, indexImage, page_name, remarks, createTime, page_number) VALUES ('" + page_id + "' , '" + index_image + "' , '" + page_name + "' , '" + remarks + "' , '" + createTime + "','" + page_number + "');";
                        SQLiteCommand command1 = new SQLiteCommand(query, connection);
                        command1.ExecuteReader();

                        query = "INSERT INTO project_list (page_id, project_id, project_name) VALUES ('" + page_id + "','" + project_id + "','" + project_name + "');";
                        SQLiteCommand command2 = new SQLiteCommand(query, connection);
                        command2.ExecuteReader();
                        return "success";
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return ex.ToString();
            }
        }
        //在 project_property 表查找所有项目
        public static ArrayList get_project_list(SQLiteConnection connection)
        {
            ArrayList projectList = new ArrayList();
            try
            {
                string query = "SELECT project_id, project_name FROM project_property;";

                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // 创建字典并填充数据
                        Dictionary<string, object> projectDict = new Dictionary<string, object>();
                        //string index_image, string page_name, string remarks
                        projectDict["project_id"] = reader["project_id"].ToString();
                        projectDict["project_name"] = reader["project_name"].ToString();
                        projectList.Add(projectDict);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return projectList;
        }
        //在 project_name 表查找 project_id 对应的 project_name, 在 project_name 表查找所有页面
        public static ArrayList get_page_list(SQLiteConnection connection, string project_id)
        {
            ArrayList page_list = new ArrayList();
            try
            {
                string query = "SELECT project_name FROM project_property " + "WHERE project_id = '" + project_id + "'; ";

                using (SQLiteCommand cmd_select = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader_select = cmd_select.ExecuteReader())
                {
                    while (reader_select.Read())
                    {
                        string project_name = reader_select.GetString(reader_select.GetOrdinal("project_name"));
                        // 获取查询结果
                        string sql = "SELECT * FROM " + project_name + ";";
                        SQLiteCommand cmd = new SQLiteCommand(sql, connection);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // 创建字典并填充数据
                                Dictionary<string, object> page_dict = new Dictionary<string, object>();
                                page_dict["page_id"] = reader["page_id"].ToString();
                                page_dict["index_image"] = reader["indexImage"].ToString();
                                page_dict["page_name"] = reader["page_name"].ToString();
                                page_dict["remarks"] = reader["remarks"].ToString();
                                page_dict["page_number"] = reader["page_number"].ToString();
                                page_list.Add(page_dict);
                            }
                        }
                    }
                }
                return page_list;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return page_list;
        }
        //在 page_list 表查找 page_id 对应的 project_name, 在 project_name 表查找page_id对应的数据
        public static string get_page_data(SQLiteConnection connection, string page_id)
        {
            string result = "";
            try
            {
                string query = "SELECT project_name FROM project_list WHERE page_id = '" + page_id + "';";
                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string project_name = reader["project_name"].ToString();
                        query = "SELECT data FROM " + project_name + " WHERE page_id = '" + page_id + "';";
                        using (SQLiteCommand cmd_final = new SQLiteCommand(query, connection))
                        using (SQLiteDataReader reader_final = cmd_final.ExecuteReader())
                        {
                            if (reader_final.Read())
                            {
                                result = reader_final["data"].ToString();
                            }
                        }
                    }
                }
                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return result;
            }
        }
        //在 page_list 表查找 page_id 对应的 project_name, 在 project_name 表更新page_id对应的数据
        public static bool save_page_data(SQLiteConnection connection, string page_id, string data)
        {
            string result = "";
            try
            {
                string query = "SELECT project_name FROM project_list WHERE page_id = '" + page_id + "';";
                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string project_name = reader["project_name"].ToString();
                        query = "UPDATE " + project_name + " SET data = '" + data + "' WHERE page_id = '" + page_id + "';";
                        using (SQLiteCommand cmd_final = new SQLiteCommand(query, connection))
                        {
                            cmd_final.ExecuteReader();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

    }
}
