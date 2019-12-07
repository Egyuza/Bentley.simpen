using System;
using System.Windows.Forms;
using System.Data.SqlClient;

using Bentley.Internal.MicroStation.Elements;
using Bentley.MicroStation;
using System.Collections.Generic;

using BCOM = Bentley.Interop.MicroStationDGN;
using System.Data;

namespace simpen.ui
{

public partial class PenetrForm : Form
{
    enum FieldType
    {
        FLANGES,
        DIAMETR,
        LENGTH,
        KKS,
        DESCRIPTION
    }

    static Properties.Settings Sets {
        get
        {
            return   Properties.Settings.Default;
        }
    }

    Dictionary<IntPtr, PenetrTask> penTaskSelection = 
        new Dictionary<IntPtr, PenetrTask>();

    private BindingSource bindSource = new BindingSource();

    BCOM.TransientElementContainer transientContainer;

    public PenetrForm()
    {        
        InitializeComponent();            
        this.Text = "Проходки " + Addin.getVersion();
        // ------------------------------------------
        
        readDatabaseData();

        ViewHelper.getActiveView().UsesDisplaySet = true;

        dgvFields.AutoGenerateColumns = false;
        dgvFields.DataSource = bindSource;
        dgvFields.EnableHeadersVisualStyles = false;
        dgvFields.Columns.Clear();
        dgvFields.AutoSize = true;

        System.Drawing.Color readonlyColor = System.Drawing.SystemColors.Control;
        
        DataGridViewColumn column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = "Code";
        column.Name = "KKS код";
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvFields.Columns.Add(column);
        
        column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = "Name";
        column.Name = "Типоразмер";
        column.ReadOnly = true;
        column.CellTemplate.Style.BackColor = readonlyColor;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvFields.Columns.Add(column);

        column = new DataGridViewTextBoxColumn();
        //column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "Flanges";
        column.Name = "Фланцы";
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvFields.Columns.Add(column);
                        
        column = new DataGridViewTextBoxColumn();
        //column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "Diametr";
        column.Name = "Диаметр";
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvFields.Columns.Add(column);
        
        column = new DataGridViewTextBoxColumn();
        //column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "Length";
        column.Name = "Длина";
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvFields.Columns.Add(column);

        column = new DataGridViewTextBoxColumn();
        //column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "RefPointIndex";
        column.Name = "Точка установки";
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        column.MinimumWidth = 25;
        dgvFields.Columns.Add(column);        

        column = new DataGridViewTextBoxColumn();
        //column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "RefPoint1";
        column.Name = "Контр. точка 1";
        column.ReadOnly = true;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        column.CellTemplate.Style.BackColor = readonlyColor;
        dgvFields.Columns.Add(column);

        column = new DataGridViewTextBoxColumn();
        //column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "RefPoint2";
        column.Name = "Контр. точка 2";
        column.ReadOnly = true;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        column.CellTemplate.Style.BackColor = readonlyColor;
        dgvFields.Columns.Add(column);

        column = new DataGridViewTextBoxColumn();
        //column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "RefPoint3";
        column.Name = "Контр. точка 3";
        column.ReadOnly = true;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        column.CellTemplate.Style.BackColor = readonlyColor;
        dgvFields.Columns.Add(column);
        
        Addin.Instance.SelectionChangedEvent += Instance_SelectionChangedEvent;
        Addin.Instance.SelectedViewChangeEvent += Instance_SelectedViewChangeEvent;

        dgvFields.CellMouseDoubleClick += DgvFields_CellMouseDoubleClick;
        dgvFields.SelectionChanged += DgvFields_SelectionChanged; ;
        
        transientContainer = Addin.App.CreateTransientElementContainer1(
            null, 
            BCOM.MsdTransientFlags.DisplayFirst |
            BCOM.MsdTransientFlags.Overlay | BCOM.MsdTransientFlags.Snappable,
            (BCOM.MsdViewMask)ViewHelper.getActiveViewIndex(), 
            BCOM.MsdDrawingMode.Temporary);

        { // todo восстанавливаем сохранённые настройки:
            
        }
    }

    private void DgvFields_SelectionChanged(object sender, EventArgs e)
    {
        // Задача - выделить объект задания в модели для пользователя

        transientContainer?.Reset();

        foreach(DataGridViewRow row in dgvFields.SelectedRows)
        {
            PenetrTask task = (PenetrTask)dgvFields.Rows[row.Index].DataBoundItem;

            BCOM.ModelReference modelRef = 
                Addin.App.MdlGetModelReferenceFromModelRefP((int)task.modelRefP);
            BCOM.Element el = modelRef.GetElementByID(task.elemId);

            BCOM.View view = ViewHelper.getActiveView();

            // TODO рисовать кубик range штриховкой под цвет выделения...
            // !!! учесть, что задание может быть в референсе

            

            BCOM.Element box = ElementHelper.getElementRangeBox(el);
            box.Color = 0;
            transientContainer.AppendCopyOfElement(box);
            
            view.Redraw();
        }
    }

    private void DgvFields_CellMouseDoubleClick(
        object sender, DataGridViewCellMouseEventArgs e)
    {
        PenetrTask task = (PenetrTask)dgvFields.Rows[e.RowIndex].DataBoundItem;

        Element element = Element.ElementFactory(task.elemRefP, task.modelRefP);
        ViewHelper.zoomToElement(element.ElementID, (int)element.ModelRef);
    }

    private void Instance_SelectedViewChangeEvent(AddIn senderIn, 
        AddIn.SelectedViewChangeEventArgs eventArgsIn)
    {
        //eventArgsIn.NewView;
        //    Addin.App.ActiveDesignFile.View
    }

    // СОЗДАТЬ ОБЪЕКТ
    private void btnAddToModel_Click(object sender, EventArgs e)
    {
        //sendTaskData();
        //sendKeyin("add");

    // todo наверно есть ключ на запуск с ожиданием возврата от mdl
    // ! нельзя reload, т.к. не завершена работа mdl

      //  isReadyToPublishTrigger_ = false;

        // поиск фланцев:
        foreach (DataGridViewRow row in dgvFields.Rows)
        {
            PenetrTask task = (PenetrTask)row.DataBoundItem;
            
             Element element = Element.ElementFactory(task.elemRefP, task.modelRefP);

            BCOM.CellElement cell = Addin.App.ActiveModelReference.
                GetElementByID(task.elemId).AsCellElement();

            if (cell == null)
                continue;

            BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
            BCOM.Range3d scanRange = 
                Addin.App.Range3dScaleAboutCenter(cell.Range, 1.0);

            //criteria.IncludeOnlyCell();
            criteria.IncludeOnlyVisible();
            criteria.IncludeOnlyWithinRange(scanRange);                       
            
            BCOM.ElementEnumerator res = 
                Addin.App.ActiveModelReference.Scan(criteria);            
            while((res?.MoveNext()).Value) 
            {
                Addin.App.ActiveModelReference.SelectElement(res.Current, true);       
            }
        }
    }

    private void dgvFields_EnabledChanged(object sender, EventArgs e)
    {
        foreach (DataGridViewRow row in dgvFields.Rows)
        {
            row.Cells[0].Style.ForeColor = 
            row.Cells[1].Style.ForeColor = dgvFields.Enabled ? 
            System.Drawing.Color.Black : System.Drawing.Color.Gray;
        }
    }


    private void OpeningForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        Addin.Instance.SelectionChangedEvent -= Instance_SelectionChangedEvent;
        transientContainer.Reset();
        // todo sets save

    }

    private unsafe void Instance_SelectionChangedEvent(
        AddIn sender, AddIn.SelectionChangedEventArgs eventArgs)
    {
        // Element element = null;
        // Session.Instance.StartUndoGroup();
        // Session.Instance.EndUndoGroup();
        
        try
        {
            switch (eventArgs.Action)
            {
            case AddIn.SelectionChangedEventArgs.ActionKind.SetEmpty:
                penTaskSelection.Clear();
                bindSource.Clear();
                break;
            case AddIn.SelectionChangedEventArgs.ActionKind.Remove:
            {
                Element element = Element.FromFilePosition(
                    eventArgs.FilePosition, eventArgs.ModelReference.DgnModelRefPtr);
                
                if (penTaskSelection.ContainsKey(element.ElementRef))
                {
                    bindSource.Remove(penTaskSelection[element.ElementRef]);
                    penTaskSelection.Remove(element.ElementRef);
                }
                break;
            }
            case AddIn.SelectionChangedEventArgs.ActionKind.New:
            {
                Element element = Element.FromFilePosition(
                    eventArgs.FilePosition, eventArgs.ModelReference.DgnModelRefPtr);

                PenetrTask task;
                if (PenetrTask.getFromElement(element, out task))
                {
                    penTaskSelection.Add(element.ElementRef, task);
                    bindSource.Add(task);
                }

                break;
            }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void readDatabaseData()
    {
        BCOM.Workspace wspace = Addin.App.ActiveWorkspace;

        string server = wspace.ConfigurationVariableValue("AEP_SAVRD_SERVER");
        string db = wspace.ConfigurationVariableValue("AEP_SAVRD_BASE");

        // todo read vba settings:
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

            string userName = Environment.UserName;
            long userId = 0L;
            long projId = 0L;
            long catalogId = 0L;
            long depId = 0L;


            string sql = "select top 1 * from usr where usrLogin = '" +
                userName + "' order by usrID desc";
            using (SqlDataReader reader = 
                new SqlCommand(sql, connection).ExecuteReader())
            {
                if (reader != null && reader.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    userId = dt.Rows[0].Field<long>("usrID");
                    projId = dt.Rows[0].Field<long?>("projectID") ?? 0L;
                    catalogId = dt.Rows[0].Field<long?>("usrCatalogID") ?? 0L;
                    depId = dt.Rows[0].Field<long>("depID");
                }
            }

            SqlCommand command = new SqlCommand(sql, connection);  

            
            using (SqlDataReader reader = 
                new SqlCommand(sql, connection).ExecuteReader())
            {
                if (reader != null && reader.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    userId = dt.Rows[0].Field<long>("usrID");
                    projId = dt.Rows[0].Field<long?>("projectID") ?? 0L;
                    catalogId = dt.Rows[0].Field<long?>("usrCatalogID") ?? 0L;
                    depId = dt.Rows[0].Field<long>("depID");
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
}
}
