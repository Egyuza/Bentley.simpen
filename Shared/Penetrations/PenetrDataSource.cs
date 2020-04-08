using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;
using System.Linq;

using BCOM = Bentley.Interop.MicroStationDGN;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#endif

#if CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
using Bentley.GeometryNET;
#endif

namespace Shared.Penetrations
{
public class PenetrDataSource
{
    //private static PenetrData instance_;
    //public static PenetrData getInstance()
    //{
    //    return new PenetrData();
    //}
    public PenetrDataSource() 
    {
        refresh();
    }

    public long ProjectId { get { return projId_; } }
    string ProjectName
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    DataTable data_;
    readonly string userName_ = Environment.UserName;
    long userId_ = 0L;
    long projId_ = 0L;
    long catalogId_ = 0L;
    long depId_ = 0L;

    private void refresh() 
    {   // логика взята из оригинального simpen от Л.Вибе        
        data_ = new DataTable();

        BCOM.Workspace wspace = App.ActiveWorkspace;

        string server = wspace.IsConfigurationVariableDefined("AEP_SAVRD_SERVER") ?
            wspace.ConfigurationVariableValue("AEP_SAVRD_SERVER") : "vibe1.sp.spbaep.ru";
                    
        { // ОТЛАДКА
            // server = "badserver";
        }

        string passServer = 
            wspace.IsConfigurationVariableDefined("AEP_SAVRD_PASS_SERVER") ?
            wspace.ConfigurationVariableValue("AEP_SAVRD_PASS_SERVER") : 
            "pw-srv.sp.spbaep.ru";
        
        string db = wspace.IsConfigurationVariableDefined("AEP_SAVRD_BASE") ?
            wspace.ConfigurationVariableValue("AEP_SAVRD_BASE") : "parts";          
    
        projId_ = wspace.IsConfigurationVariableDefined("EMBDB_PROJECT_ID") ?
            long.Parse(wspace.ConfigurationVariableValue("EMBDB_PROJECT_ID")) : 
            0L;
        // offtake project id
        // 0 - no project

        // TODO read vba settings:
        string user = "so2user";
        string pwd = "so2user";

        string connectionString = string.Format( 
            "Persist Security Info=False;" + 
            "Timeout=3;" + 
            "Data Source={0};" + 
            "Initial Catalog={1};" + 
            "User ID={2};" + 
            "Password={3}",
            server, db, user, pwd);
            
        SqlConnection connection = null;

        try
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }
        catch (SqlException)
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
                connection = null;
            }
        }
        
        try
        {
            string linked = string.Empty;
            if (connection == null && server != passServer)
            {
                // если не доступен первый сервер, то пробуем через linkedserver
                var connBldr = new SqlConnectionStringBuilder(connectionString);

                { // ОТЛАДКА
                    //connBldr.DataSource = "vibe1.sp.spbaep.ru";
                }

                linked = string.Format("[{0}].[{1}].[dbo].",
                    connBldr.DataSource, connBldr.InitialCatalog);

                connBldr.DataSource = passServer;
                connBldr.InitialCatalog = string.Empty;
                connBldr.UserID = "oimread";
                connBldr.Password = connBldr.UserID;

                connection = new SqlConnection(connBldr.ToString());
                connection.Open();
            }

            string sql = string.Format("select top 1 * from {0}usr" + 
                " where usrLogin = '{1}' order by usrID desc", 
                linked, userName_);
            using (SqlDataReader reader = 
                new SqlCommand(sql, connection).ExecuteReader())
            {
                if (reader != null && reader.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    userId_ = dt.Rows[0].Field<long>("usrID");
                    catalogId_ = dt.Rows[0].Field<long?>("usrCatalogID") ?? 0L;
                    depId_ = dt.Rows[0].Field<long>("depID");
                }
            }

            bool resHasRows = false;
            using (SqlDataReader reader = new SqlCommand(
                string.Format(
                    "select distinct flanNumber from {0}pendiam where {1} = {2}",
                    linked,
                    projId_ > 0 ? "prjID" : "depID", 
                    projId_ > 0 ? projId_ : depId_ ), 
                connection).ExecuteReader())
            {
                // todo caption project
                resHasRows = reader.HasRows;
            }

            if (!resHasRows)
            {
                depId_ = 0;
            }
            
            data_.Clear();
            using (SqlDataReader reader = new SqlCommand(
                string.Format("select * from {0}view_pendiam2", linked), 
                connection).ExecuteReader())
            {
                if (reader != null && reader.HasRows)
                {
                    data_.Load(reader);
                }
            }

            //if (projId > 0)
            //{
            //    reader =  new SqlCommand("select distinct flanNumber " + 
            //        "from pendiam where prjID = " + projId,
            //        connection).ExecuteReader();
            //}
            //else
            //{
                
            //}

            //command = new SqlCommand(sql, connection);
            //reader = command.ExecuteReader();
               
            //if (!reader.HasRows)
            //{
            //    reader.Close();
            //    reader = new SqlCommand("select distinct flanNumber " + 
            //        "from pendiam where prjID = " + projId, 
            //        connection).ExecuteReader();

            //    reader.
            //}

            //dt = new DataTable();
            //dt.Load(reader);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            if (connection != null)
            {
                connection.Close();
                connection.Dispose();
            }
        }
    }

    public List<DiameterType> getDiameters(long flangesTypeIndex)
    {
        var dataRows = data_.AsEnumerable().
            Where(x => projId_ != 0 ? x.Field<long>("prjId") == projId_ : 
                x.Field<long>("depID") == depId_).
            Where(x => x.Field<long>("flanNumber") == flangesTypeIndex).
            OrderBy(x => x.Field<long>("diamNumber")).
            ThenByDescending(x => x.Field<float>("pipeDiam")).
            ThenByDescending(x => x.Field<float>("pipeThick"));

        var list = new List<DiameterType>();

        foreach (DataRow dataRow in dataRows)
        {        
            var diamType = new DiameterType(
                dataRow.Field<long>("diamNumber"),
                dataRow.Field<float>("pipeDiam"),
                dataRow.Field<float>("pipeThick"));
            list.Add(diamType);
        }
        return list;
    }

    public List<long> getFlangeNumbersSort()
    {
        List<long> flanNumbers = data_.AsEnumerable().
            Where(x => projId_ != 0 ? x.Field<long>("prjId") == projId_ : 
                x.Field<long>("depID") == depId_)
            .Select(x => x.Field<long>("flanNumber")).Distinct().ToList();
        flanNumbers.Sort();
        return flanNumbers;
    }

    public PenetrInfo getPenInfo(long flangesTypeIndex, long diameterIndex)
    {
        DataRow row = data_.AsEnumerable().First(x => 
            (projId_ != 0 ? x.Field<long>("prjId") == projId_ : 
                    x.Field<long>("depID") == depId_) &&
            x.Field<long>("prjID") == projId_ &&
            x.Field<long>("flanNumber") == flangesTypeIndex &&
            x.Field<long>("diamNumber") == diameterIndex);
        
        return row == null ? null :
            new PenetrInfo(
                row.Field<float>("pipeDiam").ToDouble(), 
                row.Field<float>("pipeThick").ToDouble(), 
                row.Field<double>("flangeWidth"),
                row.Field<double>("flangeThick"));
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
