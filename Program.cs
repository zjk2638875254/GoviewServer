using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SQLite3;
using System.Text.Json;
using SQLite3;
using System.Collections;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:3104");

var app = builder.Build();

string currentDir = Directory.GetCurrentDirectory();
//DirectoryInfo dirInfo = new DirectoryInfo(currentDir);
//string parentDir = dirInfo.Parent?.FullName;
//string db_path = parentDir + "\\data_base\\go_view.db3";
string db_path = currentDir + "\\data_base\\go_view.db3";
Console.WriteLine(db_path);
string db_connectionString = $"Data Source={db_path};Version=3;";
SQLiteConnection sql_connection = new SQLiteConnection(db_connectionString);
sql_connection.Open();

//�ļ��ϴ�--��ʱû�и�
app.MapPost("/goview/oss/object/", async (HttpContext context) =>
{
    try
    {
        File.AppendAllText("D://ServerTest//log.txt", "\r\n" + "�ļ��ϴ�://goview//oss/object/");
        // ��ȡ���������ݣ�֧��JSON/XML/�ı��ȸ�ʽ��
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        File.AppendAllText("D://ServerTest//log.txt", "\r\n" + requestBody);
        // ʾ��������JSON������
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
        File.AppendAllText("D://ServerTest//log.txt", "\r\n" + requestData.ToString());
        File.AppendAllText("D://ServerTest//log.txt", "\r\n" + "�ļ��ϴ�:End");
        // ������Ӧ
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

        // ������Ӧ״̬�����������
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";

        // ����JSON��Ӧ
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    catch (Exception ex)
    {
        // ͳһ������
        Console.WriteLine($"��������ʱ��������{ex.ToString()}");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            Status = "Error",
            Message = "��������������ʱ��������",
            Details = ex.Message
        }));
    }
});
//δ�޸ģ�ɾ����Ŀ
app.MapGet("/project/delete/{item}", async (string item) =>
{
    try
    {
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(item);

        sqlite_define.page_delete(sql_connection, item);
        var response = new ApiResponse { Msg = "�����ɹ�", Code = 200 };
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});
//���޸ģ�ɾ��ҳ��
app.MapGet("/page/delete/{item}", async (string item) =>
{
    try
    {
        sqlite_define.page_delete(sql_connection, item);
        var response = new ApiResponse { Msg = "�����ɹ�", Code = 200 };
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});
//���޸ģ��޸�ҳ��-�Ѳ���
app.MapPost("/page/edit", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        // ʾ��������JSON������
        var requestData = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(requestBody);

        ArrayList page_data = new ArrayList();
        for(int i=0;i< requestData.Count;i++)
            page_data.Add(requestData[i]);
        
        if (sqlite_define.pages_edit(sql_connection, page_data) == "success")
        {
            // ������Ӧ״̬�����������
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            var response = new ApiResponse { Msg = "�����ɹ�", Code = 200 };
            // ����JSON��Ӧ
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        else
        {
            // ������Ӧ״̬�����������
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            var response = new ApiResponse { Msg = "����ʧ��", Code = 200 };
            // ����JSON��Ӧ
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
    catch (Exception ex)
    {
        // ͳһ������
        Console.WriteLine($"��������ʱ��������{ex.ToString()}");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            Status = "Error",
            Message = "��������������ʱ��������",
            Details = ex.Message
        }));
    }
});
//���޸ģ�����ҳ��-�Ѳ���
app.MapPost("/page/create", async (HttpContext context) =>
{
    try
    {
        // ��ȡ���������ݣ�֧��JSON/XML/�ı��ȸ�ʽ��
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        // ʾ��������JSON������
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);

        DateTime now = DateTime.UtcNow;
        // ��ʽ��Ϊ���������ַ������������룩
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
            // ������Ӧ״̬�����������
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
            var response = new ProjectResponse { msg = "�����ɹ�", code = 200 ,data = project_data };
            // ����JSON��Ӧ
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        
    }
    catch (Exception ex)
    {
        // ͳһ������
        Console.WriteLine($"��������ʱ��������{ex.ToString()}");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            Status = "Error",
            Message = "��������������ʱ��������",
            Details = ex.Message
        }));
    }
});
//���޸ģ���ȡ��Ŀlist����-�Ѳ���
app.MapGet("/project/list/", async () =>
{
    try
    {
        File.AppendAllText("D://ServerTest//log.txt", "\r\n" + "��ȡ��Ŀlist����:goview/api/goview/project/list/");
        File.AppendAllText("D://ServerTest//log.txt", "\r\n");
        File.AppendAllText("D://ServerTest//log.txt", "\r\n" + "��ȡ��Ŀlist����:End");

        ArrayList project_list = sqlite_define.get_project_list(sql_connection);
        var response = new ProjectListResponse { msg = "��ȡ�ɹ�", code = 200, count = project_list.Count, data = project_list };
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});
//���޸ģ���ȡҳ��list����-�Բ���
app.MapGet("/page/list/{item}", async (string item) =>
{
    try
    {
        string project_id = item;
        ArrayList page_data = sqlite_define.get_page_list(sql_connection, project_id);
        ArrayList sorted_pages = function_define.sort_arraylist(page_data);

        var response = new ProjectListResponse { msg = "��ȡ�ɹ�", code = 200, count = page_data.Count, data = sorted_pages };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});
//���޸ģ���ȡҳ������-�Ѳ���
app.MapGet("/page/getData/{item}", async (string item) =>
{
    try
    {
        string page_id = item;
        string project_data = sqlite_define.get_page_data(sql_connection, item);
        var response = new ProjectDataResponse { msg = "��ȡ�ɹ�", code = 200, data = project_data };
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database error: {ex.Message}");
    }
});
//���޸ģ�����ҳ������-�Ѳ���
app.MapPost("/page/savedata", async (HttpContext context) =>
{
    try
    {
        // ��ȡ���������ݣ�֧��JSON/XML/�ı��ȸ�ʽ��
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        // ʾ��������JSON������
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);

        string u_id = requestData["id"].ToString();
        string data = requestData["content"].ToString();
        bool res = sqlite_define.save_page_data(sql_connection, u_id, data);
        if(res == true)
        {
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                msg = "�����ɹ�",
                code = 200,
                data = data
            }));
        }
        else
        {
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                msg = "�������ɹ�",
                code = 201,
                data = data
            }));
        }
    }
    catch (Exception ex)
    {
        // ͳһ������
        Console.WriteLine($"��������ʱ��������{ex.ToString()}");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            Status = "Error",
            Message = "��������������ʱ��������",
            Details = ex.Message
        }));
    }
});

app.Run();

public class ApiResponse
{
    public string Msg { get; set; }
    public int Code { get; set; }
}

public class ProjectResponse
{
    public string msg { get; set; }
    public int code { get; set; } 
    public Dictionary<string, string> data { get; set; }
}

public class ProjectListResponse
{
    public string msg { get; set; }
    public int code { get; set; }
    public int count { get; set; }
    public ArrayList data { get; set; }
}

public class ProjectDataResponse
{
    public string msg { get; set; }
    public int code { get; set; }
    public string data { get; set; }
}