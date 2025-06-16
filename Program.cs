//using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;
using GoViewServer;
using System.Text.Json;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System;


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:3104");

var app = builder.Build();

string currentDir = Directory.GetCurrentDirectory();
string db_path = currentDir + "\\data_base\\go_view.db3";
Console.WriteLine(db_path);
string db_connectionString = $"Data Source={db_path};Version=3;";
SQLiteConnection sql_connection = new SQLiteConnection(db_connectionString);
sql_connection.Open();


//文件上传--暂时没有改
app.MapPost("/goview/oss/object/", async (HttpContext context) =>
{
    try
    {
        // 读取请求体数据（支持JSON/XML/文本等格式）
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        // 示例：解析JSON请求体
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
        // 构建响应
        var response = new
        {
            Status = "Success",
            ReceivedAt = DateTime.UtcNow,
            RequestData = requestData,
            Environment = new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.VersionString
            }
        };

        // 设置响应状态码和内容类型
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";

        // 返回JSON响应
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    catch (Exception ex)
    {
        // 统一错误处理
        Console.WriteLine($"处理请求时发生错误：{ex.ToString()}");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            Status = "Error",
            Message = "服务器处理请求时发生错误",
            Details = ex.Message
        }));
    }
});

//已修改：删除页面
app.MapGet("/page/delete/{item}", async (string item) =>
{
    try
    {
        if(sqlite_define.page_delete(sql_connection, item) != "success")
        {
            var response_error = new MessageResponse { Msg = "删除失败，sqlite_define.page_delete 执行失败， 请检查是否存在页面id", Code = 401 };
            return Results.Ok(response_error);
        }
        var response_success = new MessageResponse { Msg = "操作成功", Code = 200 };
        return Results.Ok(response_success);
    }
    catch (Exception ex)
    {
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
    }
});
//已修改：修改页面-已测试
app.MapPost("/page/edit", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        // 示例：解析JSON请求体
        var requestData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(requestBody);

        ArrayList page_data = new ArrayList();
        for (int i = 0; i < requestData.Count; i++)
            page_data.Add(requestData[i]);

        if (sqlite_define.pages_edit(sql_connection, page_data) == "success")
        {
            // 设置响应状态码和内容类型
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            var response = new MessageResponse { Msg = "已完成修改", Code = 200 };
            // 返回JSON响应
            return Results.Ok(response);
        }
        else
        {
            var response = new MessageResponse { Msg = "操作失败，sqlite_define.pages_edit 执行失败， 请检查是否存在页面id", Code = 401 };
            // 返回JSON响应
            return Results.BadRequest(response);
        }
    }
    catch (Exception ex)
    {
        // 统一错误处理
        Console.WriteLine($"处理请求时发生错误：{ex.ToString()}");
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
    }
});
//已修改：创建页面-已测试
app.MapPost("/page/create", async (HttpContext context) =>
{
    try
    {
        // 读取请求体数据（支持JSON/XML/文本等格式）
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        // 示例：解析JSON请求体
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);

        DateTime now = DateTime.UtcNow;
        // 格式化为连续数字字符串（包含毫秒）
        //string time_id = now.ToString("yyyyMMddHHmmssfff");
        string formattedTime = now.ToString("yyyy-MM-dd HH:mm:ss");
        string page_id = requestData["page_id"].ToString();
        string project_id = requestData["project_id"].ToString();
        string indexImage = requestData["indexImage"].ToString();
        string page_name = requestData["page_name"].ToString();
        string remarks = requestData["remarks"].ToString();
        string page_number = requestData["page_number"].ToString();
        if (sqlite_define.page_create(sql_connection, project_id, page_id, indexImage, page_name, remarks, page_number, formattedTime) == "success")
        {
            // 设置响应状态码和内容类型
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            Dictionary<string, string> project_data = new Dictionary<string, string> { };
            project_data["project_id"] = project_id;
            project_data["page_id"] = page_id;
            project_data["page_name"] = page_name;
            project_data["state"] = "-1";
            project_data["createTime"] = formattedTime;
            project_data["createUserId"] = "1";
            project_data["isDelete"] = "-1";
            project_data["indexImage"] = indexImage;
            project_data["remarks"] = remarks;

            var response = new CommonResponse { Msg = "创建成功", Code = 200, Count = project_data.Count(), Data = project_data };
            // 返回JSON响应
            return Results.Ok(response);
        }
        else
        {
            var response = new MessageResponse { Msg = "操作失败，sqlite_define.page_create 执行失败，请检查是否存在页面id", Code = 401 };
            // 返回JSON响应
            return Results.BadRequest(response);
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"处理请求时发生错误：{ex.ToString()}");
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
    }
});
//已修改：获取项目list集合-已测试
app.MapGet("/project/list/", async () =>
{
    try
    {
        ArrayList project_list = sqlite_define.get_project_list(sql_connection);

        var response = new CommonResponse { Msg = "获取成功", Code = 200, Count = project_list.Count, Data = project_list };
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"处理请求时发生错误：{ex.ToString()}");
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
    }
});
//已修改：获取页面list集合-以测试
app.MapGet("/page/list/{item}", async (string item) =>
{
    try
    {
        string project_id = item;
        ArrayList page_data = sqlite_define.get_page_list(sql_connection, project_id);
        ArrayList sorted_pages = function_define.sort_arraylist(page_data);

        var response = new CommonResponse { Msg = "获取成功", Code = 200, Count = page_data.Count, Data = sorted_pages };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
    }
});
//已修改：获取页面数据-已测试
app.MapGet("/page/getData/{item}", async (string item) =>
{
    try
    {
        string page_id = item;
        string project_data = sqlite_define.get_page_data(sql_connection, item);

        var response = new CommonResponse { Msg = "获取成功", Code = 200, Count = project_data.Length, Data = project_data };
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
    }
});
//已修改：保存页面数据-已测试
app.MapPost("/page/savedata", async (HttpContext context) =>
{
    try
    {
        // 读取请求体数据（支持JSON/XML/文本等格式）
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        // 示例：解析JSON请求体
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);

        string u_id = requestData["page_id"].ToString();
        string data = requestData["content"].ToString();
        bool res = sqlite_define.save_page_data(sql_connection, u_id, data);
        if (res == true)
        {

            var response = new MessageResponse { Msg = "保存成功", Code = 200 };
            return Results.Ok(response);
        }
        else
        {
            var response = new MessageResponse { Msg = "保存失败:Wrong in /page/savedata, sqlite_define.save_page_data 返回值为 false", Code = 401 };
            return Results.BadRequest(response);
        }
    }
    catch (Exception ex)
    {
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
    }
});

app.Run();

public class MessageResponse
{
    public string Msg { get; set; }
    public int Code { get; set; }
}

public class CommonResponse : MessageResponse
{
    public string Msg { get; set; }
    public int Code { get; set; }
    public int Count { get; set; }
    public object Data { get; set; }
}