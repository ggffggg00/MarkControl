using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Text;
using WpfApp2;
using WpfApp2.DB.Models;


public class ProjectShortEntry
{
    public string name { get; set; }
    public int id { get; set; }

    public ProjectShortEntry(string name, int id)
    {
        this.name = name;
        this.id = id;
    }

    public override string ToString()
    {
        return this.name;
    }

}

public class DatabaseHelper
{
    private const string FILE_PATH = "./markdb.db";

    private bool isJustCreated = false;
    private SQLiteConnection connection { get; } = null;

    public DatabaseHelper()
    {
        if (!File.Exists(FILE_PATH))
        {
            SQLiteConnection.CreateFile(FILE_PATH);
            isJustCreated = true;
        }

        this.connection = new SQLiteConnection(@"Data Source=" + FILE_PATH + "; Version=3;");
        openConnection();
        if (isJustCreated)
        {

            firstInitialize();
        }


    }

    private void firstInitialize()
    {
        string query = " CREATE TABLE \"projects\" (\"id\"	INTEGER NOT NULL UNIQUE,\"name\"	TEXT NOT NULL,\"mark_count\"	INTEGER NOT NULL,\"image\"	TEXT NOT NULL,\"block_count\"	INTEGER NOT NULL, \"meta\"	TEXT NOT NULL DEFAULT '{}', PRIMARY KEY(\"id\")); ";
        SQLiteCommand cmd = new SQLiteCommand(query, connection);
        cmd.ExecuteNonQuery();
    }


    public void openConnection()
    {
        if (connection.State == System.Data.ConnectionState.Closed)
            connection.Open();
    }

    public void CloseConnection()
    {
        if (connection.State == System.Data.ConnectionState.Open)
            connection.Close();
    }



    public abstract class DBRequest<T1>
    {
        DatabaseHelper DBHelper;


        public DBRequest(DatabaseHelper DBHelper)
        {
            this.DBHelper = DBHelper;
        }

        public T1 execute()
        {
            SQLiteCommand command = new SQLiteCommand(getQueryStatement(), DBHelper.connection);
            return handleRequest(command);
        }

        protected abstract T1 handleRequest(SQLiteCommand command);
        abstract protected string getQueryStatement();

    }

}

public class ProjectsListRequest : DatabaseHelper.DBRequest<ObservableCollection<ProjectShortEntry>>
{

    public ProjectsListRequest(DatabaseHelper DBHelper) : base(DBHelper) { }

    protected override string getQueryStatement() { return "SELECT id, name FROM projects;"; }

    protected override ObservableCollection<ProjectShortEntry> handleRequest(SQLiteCommand command)
    {

        ObservableCollection<ProjectShortEntry> list = new ObservableCollection<ProjectShortEntry>();
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            object name = reader.GetFieldValue<object>(0);
            ProjectShortEntry entry = new ProjectShortEntry(reader.GetString(1), reader.GetInt32(0));
            list.Add(entry);
        }


        return list;
    }



}

public class CreateProjectRequest : DatabaseHelper.DBRequest<long>
{

    ProjectInitialData data;

    public CreateProjectRequest(DatabaseHelper DBHelper, ProjectInitialData projectData) : base(DBHelper)
    {
        this.data = projectData;
    }

    protected override string getQueryStatement()
    {

        //Читаем побайтово файл изображения и перегоняем в base64
        string base64image = Convert.ToBase64String(File.ReadAllBytes(data.imagePath));
        data.metaData.Add("eAccuracy", 0.001);
        data.metaData.Add("aAccuracy", 0.1);
        string jsonMetaData = Convert.ToBase64String(Encoding.UTF8.GetBytes(data.metaData.ToString()));

        var query = "INSERT INTO projects (name, mark_count, block_count, image, meta) VALUES " +
            "(\"" + data.title + "\"," +
             data.markCount + ", " +
              data.blockCount + "," +
              " \"" + base64image + "\", \"" + jsonMetaData + "\"); select last_insert_rowid();";

        Console.Out.WriteLine(query);

        return query ;


    }

    protected override long handleRequest(SQLiteCommand cmd)
    {
        object resp = cmd.ExecuteScalar();
        return (long)resp;
    }
}

public class CreateProjectTableRequest : DatabaseHelper.DBRequest<bool>
{

    long id;
    int marksCount;

    public CreateProjectTableRequest(DatabaseHelper DBHelper, long id, int marksCount) : base(DBHelper)
    {
        this.id = id;
        this.marksCount = marksCount;
    }

    protected override string getQueryStatement()
    {
        string query = " CREATE TABLE \"marks" + id.ToString() + "\" (" +
            "\"epoch\"	INTEGER NOT NULL UNIQUE"; 

        for(int i = 1; i<=marksCount; i++)
        {
           query +=  ",\"" + i.ToString() + "\"	REAL NOT NULL";
        }
        query += ");";

        return query;


    }

    protected override bool handleRequest(SQLiteCommand cmd)
    {
        cmd.ExecuteNonQuery();
        return true;
    }
}

public class GetProjectDataRequest : DatabaseHelper.DBRequest<ProjectData>
{

    int id;

    public GetProjectDataRequest(DatabaseHelper DBHelper, int id) : base(DBHelper) {
        this.id = id;
    }

    protected override string getQueryStatement() { return "SELECT * FROM projects LEFT JOIN marks" + id.ToString()+ " ON 1=1 WHERE id=" + id.ToString()+";"; }

    protected override ProjectData handleRequest(SQLiteCommand command)
    {

        ObservableCollection<ProjectShortEntry> list = new ObservableCollection<ProjectShortEntry>();
        SQLiteDataReader reader = command.ExecuteReader();
        var resp = ProjectData.conctruct(reader);
        reader.Close();

        return resp;
    }



}

public class UpdateProjectDataRequest : DatabaseHelper.DBRequest<bool>
{

    ProjectData data;

    public UpdateProjectDataRequest(DatabaseHelper DBHelper, ProjectData data) : base(DBHelper)
    {
        this.data = data;
    }

    protected override string getQueryStatement() {

        var bl = new StringBuilder();

        bl.Append("DELETE FROM marks").Append(data.id).Append(";");

        bl.Append("INSERT INTO marks").Append(data.id).Append(" VALUES ");

        for(int i = 0; i < data.epochCount; i++)
        {
            bl.Append("( ").Append(i).Append(", ");

            foreach(double val in data.marks[i].marks.Values)
            {
                bl.Append("\"").Append(val.ToString().Replace(",",".")).Append("\", ");
            }

            bl.Remove(bl.Length - 2, 2);
            bl.Append(" ),");

        }

        bl.Remove(bl.Length - 1, 1).Append(";");

        Console.Out.WriteLine(bl.ToString());

        return bl.ToString(); 
    }

    protected override bool handleRequest(SQLiteCommand command)
    {

        int affectedRows = command.ExecuteNonQuery();
        return affectedRows != 0;
    }

}

public class UpdateProjectMetaRequest : DatabaseHelper.DBRequest<bool>
{

    ProjectData data;

    public UpdateProjectMetaRequest(DatabaseHelper DBHelper, ProjectData data) : base(DBHelper)
    {
        this.data = data;
    }

    protected override string getQueryStatement()
    {

        var bl = new StringBuilder();



        bl.Append("UPDATE projects SET meta=\"");
        bl.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(data.metaData.ToString())));
        bl.Append("\" WHERE id = ").Append(data.id).Append("; ");

        return bl.ToString();
    }

    protected override bool handleRequest(SQLiteCommand command)
    {
        return command.ExecuteNonQuery() == 1;
    }



}