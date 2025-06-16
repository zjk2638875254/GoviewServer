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


//�ļ��ϴ�--��ʱû�и�
app.MapPost("/goview/oss/object/", async (HttpContext context) =>
{
    try
    {
        // ��ȡ���������ݣ�֧��JSON/XML/�ı��ȸ�ʽ��
        using var reader = new StreamReader(context.Request.Body);
        string requestBody = await reader.ReadToEndAsync();
        // ʾ��������JSON������
        var requestData = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
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

//���޸ģ�ɾ��ҳ��
app.MapGet("/page/delete/{item}", async (string item) =>
{
    try
    {
        if(sqlite_define.page_delete(sql_connection, item) != "success")
        {
            var response_error = new MessageResponse { Msg = "ɾ��ʧ�ܣ�sqlite_define.page_delete ִ��ʧ�ܣ� �����Ƿ����ҳ��id", Code = 401 };
            return Results.Ok(response_error);
        }
        var response_success = new MessageResponse { Msg = "�����ɹ�", Code = 200 };
        return Results.Ok(response_success);
    }
    catch (Exception ex)
    {
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
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
        for (int i = 0; i < requestData.Count; i++)
            page_data.Add(requestData[i]);

        if (sqlite_define.pages_edit(sql_connection, page_data) == "success")
        {
            // ������Ӧ״̬�����������
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            var response = new MessageResponse { Msg = "������޸�", Code = 200 };
            // ����JSON��Ӧ
            return Results.Ok(response);
        }
        else
        {
            var response = new MessageResponse { Msg = "����ʧ�ܣ�sqlite_define.pages_edit ִ��ʧ�ܣ� �����Ƿ����ҳ��id", Code = 401 };
            // ����JSON��Ӧ
            return Results.BadRequest(response);
        }
    }
    catch (Exception ex)
    {
        // ͳһ������
        Console.WriteLine($"��������ʱ��������{ex.ToString()}");
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
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

            var response = new CommonResponse { Msg = "�����ɹ�", Code = 200, Count = project_data.Count(), Data = project_data };
            // ����JSON��Ӧ
            return Results.Ok(response);
        }
        else
        {
            var response = new MessageResponse { Msg = "����ʧ�ܣ�sqlite_define.page_create ִ��ʧ�ܣ������Ƿ����ҳ��id", Code = 401 };
            // ����JSON��Ӧ
            return Results.BadRequest(response);
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"��������ʱ��������{ex.ToString()}");
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
    }
});
//���޸ģ���ȡ��Ŀlist����-�Ѳ���
app.MapGet("/project/list/", async () =>
{
    try
    {
        ArrayList project_list = sqlite_define.get_project_list(sql_connection);

        var response = new CommonResponse { Msg = "��ȡ�ɹ�", Code = 200, Count = project_list.Count, Data = project_list };
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"��������ʱ��������{ex.ToString()}");
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
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

        var response = new CommonResponse { Msg = "��ȡ�ɹ�", Code = 200, Count = page_data.Count, Data = sorted_pages };

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
    }
});
//���޸ģ���ȡҳ������-�Ѳ���
app.MapGet("/page/getData/{item}", async (string item) =>
{
    try
    {
        string page_id = item;
        string project_data = sqlite_define.get_page_data(sql_connection, item);

        var response = new CommonResponse { Msg = "��ȡ�ɹ�", Code = 200, Count = project_data.Length, Data = project_data };
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        var response = new MessageResponse { Msg = "Wrong with server:" + ex.Message, Code = 501 };
        return Results.BadRequest(response);
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

        string u_id = requestData["page_id"].ToString();
        string data = requestData["content"].ToString();
        bool res = sqlite_define.save_page_data(sql_connection, u_id, data);
        if (res == true)
        {

            var response = new MessageResponse { Msg = "����ɹ�", Code = 200 };
            return Results.Ok(response);
        }
        else
        {
            var response = new MessageResponse { Msg = "����ʧ��:Wrong in /page/savedata, sqlite_define.save_page_data ����ֵΪ false", Code = 401 };
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