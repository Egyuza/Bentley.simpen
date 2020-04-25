using System;
using System.Windows.Forms;
using System.Data.SqlClient;

using Bentley.Internal.MicroStation.Elements;
using Bentley.MicroStation;
using System.Collections.Generic;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;
using Bentley.Interop.TFCom;

using System.Data;

using System.Linq;
using System.Linq.Expressions;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;
using Bentley.Building.DataGroupSystem.EC;
using Bentley.MicroStation.XmlInstanceApi;

using Shared;
using Shared.Penetrations;

namespace simpen.ui
{

public partial class PenetrForm : Form
{
    //[DllImport("stdmdlbltin.dll")]
    //public static extern int mdlCnv_masterToUOR(ref double uors, double masterUnits, int modelRef);

    //[DllImport("stdmdlbltin.dll")]
    //public static extern int mdlCnv_UORToMaster(ref double masterUnits, double uors, int modelRef);

    enum ModeEnum
    {
        Single,
        Multiple
    }

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

    // DataTable penData = new DataTable();    
    PenetrDataSource penData;

    //Dictionary<long, List<long>>  

    readonly string userName = Environment.UserName;
    long userId = 0L;
    long projId = 0L;
    long catalogId = 0L;
    long depId = 0L;

    Dictionary<IntPtr, PenetrTask> penTaskSelection = 
        new Dictionary<IntPtr, PenetrTask>();

    private BindingSource bindSource = new BindingSource();

    BCOM.TransientElementContainer selectionTranContainer;
    BCOM.TransientElementContainer previewTranContainer;

    ModeEnum mode = ModeEnum.Single; // TODO ?

    readonly System.Drawing.Color readonlyColor = System.Drawing.SystemColors.Control;

    private static class ColumnName
    {
        public static readonly string CODE = "KKS код";
        public static readonly string TYPE_SIZE = "Типоразмер";
        public static readonly string FLANGES = "Фланцы";
        public static readonly string DIAMETER = "Диаметр";
        public static readonly string LENGTH = "Длина(см)";
        public static readonly string REF_POINT1 = "RefPoint1";
        public static readonly string REF_POINT2 = "RefPoint2";
        public static readonly string REF_POINT3 = "RefPoint3";
    }

    public PenetrForm()
    {        
        InitializeComponent();            
        this.Text = "Проходки " + Addin.getVersion(); // TODO разделить версии Проходок и Проёмов

        if (Keyins.Penetrations.DEBUG_MODE)
        {
            this.Text += " [DEBUG]";
        }
        // ------------------------------------------
        
        penData = new PenetrDataSource();

        if (!Keyins.Penetrations.DEBUG_MODE && penData.ProjectId == 0)
        {
            lblStatus.Text = "Проект не определён - создание проходок не доступно";
            lblStatus.Visible = true;
            btnAddToModel.Visible = false;
            btnAddToModel.Enabled = false;
        }

        dgvFields.AutoGenerateColumns = false;
        dgvFields.DataSource = bindSource;
        dgvFields.EnableHeadersVisualStyles = false;
        dgvFields.Columns.Clear();
        dgvFields.AutoSize = false;
               
         
        DataGridViewColumn column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = "Code";
        column.Name = ColumnName.CODE;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvFields.Columns.Add(column);
        
        column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = "Name";
        column.Name = ColumnName.TYPE_SIZE;
        column.ReadOnly = true;
        column.CellTemplate.Style.BackColor = readonlyColor;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvFields.Columns.Add(column);

        column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "FlangesType";
        column.Name = ColumnName.FLANGES;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        List<long> flanNumbers = penData.getFlangeNumbersSort();
        (column as DataGridViewComboBoxColumn).DataSource = flanNumbers;        
        dgvFields.Columns.Add(column);
        
        column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "DiameterTypeStr";
        column.Name = ColumnName.DIAMETER;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;        
        dgvFields.Columns.Add(column);
        
        column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = "Length";
        column.Name = ColumnName.LENGTH;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvFields.Columns.Add(column);
        
        column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = 
        column.Name = ColumnName.REF_POINT1;
        column.ReadOnly = true;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        column.CellTemplate.Style.BackColor = readonlyColor;
        dgvFields.Columns.Add(column);

        //column = new DataGridViewTextBoxColumn();
        //column.DataPropertyName = 
        //column.Name = ColumnName.REF_POINT2;
        //column.ReadOnly = true;
        //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        //column.CellTemplate.Style.BackColor = readonlyColor;
        //dgvFields.Columns.Add(column);

        //column = new DataGridViewTextBoxColumn();
        //column.DataPropertyName = 
        //column.Name = ColumnName.REF_POINT3;
        //column.ReadOnly = true;
        //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        //column.CellTemplate.Style.BackColor = readonlyColor;
        //dgvFields.Columns.Add(column);
        
        Addin.Instance.SelectionChangedEvent += Instance_SelectionChangedEvent;

        dgvFields.EditingControlShowing += DgvFields_EditingControlShowing;
        dgvFields.DataError += DgvFields_DataError;
        dgvFields.CellValueChanged += DgvFields_CellValueChanged;
        dgvFields.CellMouseDoubleClick += DgvFields_CellMouseDoubleClick;
        dgvFields.SelectionChanged += DgvFields_SelectionChanged;
        dgvFields.RowsAdded += DgvFields_RowsAdded;
        dgvFields.RowsRemoved += DgvFields_RowsRemoved;
        
        selectionTranContainer = Addin.App.CreateTransientElementContainer1(
            null, 
            BCOM.MsdTransientFlags.DisplayFirst |
            BCOM.MsdTransientFlags.Overlay,
            BCOM.MsdViewMask.AllViews, 
            BCOM.MsdDrawingMode.Temporary);
        
        previewTranContainer = Addin.App.CreateTransientElementContainer1(
            null, 
            BCOM.MsdTransientFlags.DisplayFirst |
            BCOM.MsdTransientFlags.Overlay | BCOM.MsdTransientFlags.Snappable | BCOM.MsdTransientFlags.IncludeInPlot,
            BCOM.MsdViewMask.AllViews, 
            BCOM.MsdDrawingMode.Temporary);

        { // TODO восстанавливаем сохранённые настройки:
            
        }
    }


    private void DgvFields_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        DataGridViewRow gridRow = dgvFields.Rows[e.RowIndex];
        PenetrTask task = (PenetrTask)gridRow.DataBoundItem;

        var lengthCell = gridRow.Cells[ColumnName.LENGTH];
        lengthCell.ReadOnly = false;

        // тип фланцев == 3 - расстояние насквозь (толщина фланцев не вычитается!)

        if (task.FlangesType == 3)
        {
            double thickness;
            if (task.getTFFormThickness(out thickness))
            {
                lengthCell.ReadOnly = true;
                task.Length = (int)thickness/10;
            }
        }
    }

    private void DgvFields_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
        //  TODO implement ?
    }

    private void DgvFields_EditingControlShowing(object sender, 
        DataGridViewEditingControlShowingEventArgs e)
    {
        ComboBox cmbBx = e.Control as ComboBox;        

        if (cmbBx != null)
        {
            cmbBx.DropDown -= new EventHandler(ComboBoxCell_DropDown);
            cmbBx.DropDown += new EventHandler(ComboBoxCell_DropDown);
 
            cmbBx.DropDownClosed -= new EventHandler(ComboBoxCell_DropDownClosed);
            cmbBx.DropDownClosed += new EventHandler(ComboBoxCell_DropDownClosed);
        }
    }

    private void ComboBoxCell_DropDown(object sender, EventArgs e)
    {
        // When the drop down list appears, change the DisplayMember property of the ComboBox
        // to 'TypeAndDescription' to show the description
        DataGridViewComboBoxEditingControl cmbBx = sender as DataGridViewComboBoxEditingControl;
        //if (cmbBx != null)
        //    cmbBx.DisplayMember = "TypeAndDescription";
    }
 
    private void ComboBoxCell_DropDownClosed(object sender, EventArgs e)
    {
        // When the drop down list is closed, change the DisplayMember property of the ComboBox
        // back to 'Type' to hide the description
        DataGridViewComboBoxEditingControl cmbBx = sender as DataGridViewComboBoxEditingControl;
        //if (cmbBx != null)
        //    cmbBx.DisplayMember = "Type";
    } 

    private void updateRowsCountInfo()
    {
        lblSelectedCount.Text = bindSource.Count.ToString();
    }

    private void DgvFields_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
    {
        updateRowsCountInfo();
    }

    private void DgvFields_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
    {
        updateRowsCountInfo();

        for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; ++i)
        {
            DataGridViewRow gridRow = dgvFields.Rows[i];
            PenetrTask task = (PenetrTask)gridRow.DataBoundItem;

            List<DiameterType> diameters = penData.getDiameters(task.FlangesType);

            //var dataRows = penData.AsEnumerable().
            //    Where(x => projId != 0 ? x.Field<long>("prjId") == projId : 
            //        x.Field<long>("depID") == depId).
            //    Where(x => x.Field<long>("flanNumber") == task.FlangesType).
            //    OrderBy(x => x.Field<long>("diamNumber")).
            //    ThenByDescending(x => x.Field<float>("pipeDiam")).
            //    ThenByDescending(x => x.Field<float>("pipeThick"));

            var comboCell = gridRow.Cells["Диаметр"] as DataGridViewComboBoxCell;
            
            var diametersStringList = new List<string>();

            DiameterType matchValue = null;
            foreach (DiameterType diamType in diameters)
            {        
                if (diamType.number == 
                    DiameterType.Parse(task.DiameterTypeStr).number)
                {
                    matchValue = diamType;
                }
                diametersStringList.Add(diamType.ToString());
            }
            
            // TODO
            comboCell.DataSource = diametersStringList;
            if (matchValue != null) {
                comboCell.Value = matchValue.ToString();
            }
            else {
                comboCell.ErrorText = "не валидное значение диаметра";
            }

            if (!Keyins.Penetrations.DEBUG_MODE && 
                task.TFFormsIntersected.Count == 0)
            {
                gridRow.ReadOnly = true;
                gridRow.DefaultCellStyle.BackColor = readonlyColor;
            }
        }
    }

    private void DgvFields_SelectionChanged(object sender, EventArgs e)
    {
        // выделить(подсветить) объект задания в модели для пользователя
        selectionTranContainer?.Reset();  

        foreach(DataGridViewRow row in dgvFields.SelectedRows)
        {
            PenetrTask task = (PenetrTask)dgvFields.Rows[row.Index].DataBoundItem;

            BCOM.ModelReference modelRef = 
                Addin.App.MdlGetModelReferenceFromModelRefP((int)task.modelRefP);

            BCOM.View view = ViewHelper.getActiveView();

            List<long> itemsIds = new List<long> {task.elemId};
            // добавляем фланцы:
            foreach (PenetrTaskFlange flangeTask in task.FlangesGeom) 
            {
                itemsIds.Add(flangeTask.elemId);
            }

            foreach (long id in itemsIds)
            {
                BCOM.Element el = modelRef.GetElementByID(id);
                el.Color = 2; // зелёный
                el.LineWeight = 5;

                // для ОТЛАДКИ *************************************************
                    //BCOM.Transform3d taskTran;
                    //{
                    //    BCOM.Point3d origin = (el as BCOM.CellElement).Origin;

                    //    var shift = Addin.App.Vector3dSubtractPoint3dPoint3d(
                    //        Addin.App.Point3dZero(), task.Location);

                    //    el.Move(Addin.App.Point3dFromXYZ(shift.X, shift.Y, shift.Z));

                    //    taskTran = Addin.App.Transform3dInverse(
                    //        Addin.App.Transform3dFromMatrix3d(task.Rotation));

                    //    el.Transform(taskTran);
                    //    el.Move(task.Location);
                    //}
                //**************************************************************

                selectionTranContainer.AppendCopyOfElement(el);
            }
            
            view.Redraw();
        }
    }

    private void DgvFields_CellMouseDoubleClick(
        object sender, DataGridViewCellMouseEventArgs e)
    {
        PenetrTask task = (PenetrTask)dgvFields.Rows[e.RowIndex].DataBoundItem;
        ViewHelper.zoomToElement(
            ElementHelper.getElementCOM(task.elemRefP, task.modelRefP));
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
        selectionTranContainer?.Reset();
        previewTranContainer?.Reset();

        // TODO sets save
    }

    private void Instance_SelectionChangedEvent(
        AddIn sender, AddIn.SelectionChangedEventArgs eventArgs)
    {
        try
        {
            switch (eventArgs.Action)
            {
            case AddIn.SelectionChangedEventArgs.ActionKind.SetEmpty:
                penTaskSelection.Clear();
                bindSource.Clear();
                previewTranContainer.Reset();
                break;
            case AddIn.SelectionChangedEventArgs.ActionKind.Remove:
            {
                Element element = ElementHelper.getElement(eventArgs);
                
                if (penTaskSelection.ContainsKey(element.ElementRef))
                {
                    bindSource.Remove(penTaskSelection[element.ElementRef]);
                    penTaskSelection.Remove(element.ElementRef);
                }
                break;
            }
            case AddIn.SelectionChangedEventArgs.ActionKind.New:
            {
                Element element = ElementHelper.getElement(eventArgs);

                PenetrTask task;
                if (PenetrTask.getFromElement(element, out task))
                {
                    if (penTaskSelection.ContainsKey(element.ElementRef))
                    {
                        penTaskSelection.Remove(element.ElementRef);
                        bindSource.Remove(task);
                    }
                    penTaskSelection.Add(element.ElementRef, task);
                    bindSource.Add(task);
                }

                break;
            }
            }
        }
        catch (Exception ex)
        {
            // todo обработать
            MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    //private void readDatabaseData() 
    //{   // логика взята из оригинального simpen от Л.Вибе        

    //    BCOM.Workspace wspace = Addin.App.ActiveWorkspace;

    //    string server = wspace.IsConfigurationVariableDefined("AEP_SAVRD_SERVER") ?
    //        wspace.ConfigurationVariableValue("AEP_SAVRD_SERVER") : "vibe1.sp.spbaep.ru";
                    
    //    { // ОТЛАДКА
    //        // server = "badserver";
    //    }

    //    string passServer = 
    //        wspace.IsConfigurationVariableDefined("AEP_SAVRD_PASS_SERVER") ?
    //        wspace.ConfigurationVariableValue("AEP_SAVRD_PASS_SERVER") : 
    //        "pw-srv.sp.spbaep.ru";
        
    //    string db = wspace.IsConfigurationVariableDefined("AEP_SAVRD_BASE") ?
    //        wspace.ConfigurationVariableValue("AEP_SAVRD_BASE") : "parts";          
    
    //    projId = wspace.IsConfigurationVariableDefined("EMBDB_PROJECT_ID") ?
    //    long.Parse(wspace.ConfigurationVariableValue("EMBDB_PROJECT_ID")) : 
    //    0; // offtake project id
    //    // 0 - no project

    //    // TODO read vba settings:
    //    string user = "so2user";
    //    string pwd = "so2user";

    //    string connectionString = string.Format( 
    //        "Persist Security Info=False;" + 
    //        "Timeout=3;" + 
    //        "Data Source={0};" + 
    //        "Initial Catalog={1};" + 
    //        "User ID={2};" + 
    //        "Password={3}",
    //        server, db, user, pwd);
            
    //    SqlConnection connection = null;

    //    try
    //    {
    //        connection = new SqlConnection(connectionString);
    //        connection.Open();
    //    }
    //    catch (SqlException)
    //    {
    //        if (connection != null)
    //        {
    //            connection.Close();
    //            connection.Dispose();
    //            connection = null;
    //        }
    //    }
        
    //    try
    //    {
    //        string linked = string.Empty;
    //        if (connection == null && server != passServer)
    //        {
    //            // если не доступен первый сервер, то пробуем через linkedserver
    //            var connBldr = new SqlConnectionStringBuilder(connectionString);

    //            { // ОТЛАДКА
    //                //connBldr.DataSource = "vibe1.sp.spbaep.ru";
    //            }

    //            linked = string.Format("[{0}].[{1}].[dbo].",
    //                connBldr.DataSource, connBldr.InitialCatalog);

    //            connBldr.DataSource = passServer;
    //            connBldr.InitialCatalog = string.Empty;
    //            connBldr.UserID = "oimread";
    //            connBldr.Password = connBldr.UserID;

    //            connection = new SqlConnection(connBldr.ToString());
    //            connection.Open();
    //        }

    //        string sql = string.Format("select top 1 * from {0}usr" + 
    //            " where usrLogin = '{1}' order by usrID desc", 
    //            linked, userName);
    //        using (SqlDataReader reader = 
    //            new SqlCommand(sql, connection).ExecuteReader())
    //        {
    //            if (reader != null && reader.HasRows)
    //            {
    //                DataTable dt = new DataTable();
    //                dt.Load(reader);

    //                userId = dt.Rows[0].Field<long>("usrID");
    //                catalogId = dt.Rows[0].Field<long?>("usrCatalogID") ?? 0L;
    //                depId = dt.Rows[0].Field<long>("depID");
    //            }
    //        }

    //        bool resHasRows = false;
    //        using (SqlDataReader reader = new SqlCommand(
    //            string.Format(
    //                "select distinct flanNumber from {0}pendiam where {1} = {2}",
    //                linked,
    //                projId > 0 ? "prjID" : "depID", 
    //                projId > 0 ? projId : depId ), 
    //            connection).ExecuteReader())
    //        {
    //            // todo caption project
    //            resHasRows = reader.HasRows;
    //        }

    //        if (!resHasRows)
    //        {
    //            depId = 0;
    //        }
            
    //        penData.Clear();
    //        using (SqlDataReader reader = new SqlCommand(
    //            string.Format("select * from {0}view_pendiam2", linked), 
    //            connection).ExecuteReader())
    //        {
    //            if (reader != null && reader.HasRows)
    //            {
    //                penData.Load(reader);
    //            }
    //        }

    //        //if (projId > 0)
    //        //{
    //        //    reader =  new SqlCommand("select distinct flanNumber " + 
    //        //        "from pendiam where prjID = " + projId,
    //        //        connection).ExecuteReader();
    //        //}
    //        //else
    //        //{
                
    //        //}

    //        //command = new SqlCommand(sql, connection);
    //        //reader = command.ExecuteReader();
               
    //        //if (!reader.HasRows)
    //        //{
    //        //    reader.Close();
    //        //    reader = new SqlCommand("select distinct flanNumber " + 
    //        //        "from pendiam where prjID = " + projId, 
    //        //        connection).ExecuteReader();

    //        //    reader.
    //        //}

    //        //dt = new DataTable();
    //        //dt.Load(reader);
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show(ex.Message);
    //    }
    //    finally
    //    {
    //        if (connection != null)
    //        {
    //            connection.Close();
    //            connection.Dispose();
    //        }
    //    }
    //}


    private void btnPreview_Click(object sender, EventArgs e)
    {
        previewTranContainer.Reset();

        try
        {
            foreach (TFCOM.TFFrameList frameList in getFramesData().Keys)
            {
                previewTranContainer.AppendCopyOfElement(
                        frameList.AsTFFrame.Get3DElement());

                var projList = frameList.AsTFFrame.GetProjectionList();
                
                BCOM.Element projEl = null;
                do {
                    try
                    {
                        projList.AsTFProjection.GetElement(out projEl);
                        if(projEl != null)
                            previewTranContainer.AppendCopyOfElement(projEl);
                    }
                    catch (Exception)
                    {
                    }                                 
                } while ((projList = projList.GetNext()) != null);

            }

        }
        catch (Exception ex) // TODO
        {
            // ex.ShowMessage();
        }
    }

//#define FILEPOS_EOF                     0
//#define FILEPOS_CURRENT                 1
//#define FILEPOS_FIRST_ELE               2
//#define FILEPOS_NEXT_ELE                3
//#define FILEPOS_WORKING_SET             4
//#define FILEPOS_COMPONENT               5
//#define FILEPOS_NEXT_NEW_ELEMENT        6

    private void btnAddToModel_Click(object sender, EventArgs e)
    {
        previewTranContainer.Reset();

        BCOM.Level activeLevel = Addin.App.ActiveSettings.Level;
        BCOM.LineStyle activeLineStyle = Addin.App.ActiveSettings.LineStyle;
        int activeLineWeight = Addin.App.ActiveSettings.LineWeight;
        int activeColor = Addin.App.ActiveSettings.Color;

        var activeModel = Addin.App.ActiveModelReference;
        try
        {
            foreach (var pair in getFramesData())
            {
                TFCOM.TFFrameList frameList = pair.Key;
                PenetrTask task = pair.Value;
            
                {
                    // ! без этого кода не срабатывает перфорация в стенке/плите
                    // судя по всему инициализирует обновление объектов, с которыми
                    // взаимодействует frame
            
                    Addin.AppTF.ModelReferenceUpdateAutoOpeningsByFrame(
                        activeModel, frameList.AsTFFrame, true, false, 
                        TFCOM.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone);
                }
                // добавление в модель
                Addin.AppTF.ModelReferenceAddFrameList(
                    Addin.App.ActiveModelReference, frameList);

                BCOM.Element newElement = activeModel.GetLastValidGraphicalElement();  
                setDataGroupInstance(newElement, task);

{ // TODO ОТЛАДКА:
    //XmlInstanceSchemaManager modelSchema =
    //        new XmlInstanceSchemaManager((IntPtr)newElement.ModelReference.MdlModelRefP());
        
    //    XmlInstanceApi api = XmlInstanceApi.CreateApi(modelSchema);
    //    IList<string> instances = api.ReadInstances((IntPtr)newElement.MdlElementRef());

    //    foreach (string inst in instances)
    //    {
    //        string instId = XmlInstanceApi.GetInstanceIdFromXmlInstance(inst);   
    //    }
}

            }
        }
        catch (Exception ex)
        {
            ex.ShowMessage();
        }
        finally
        {
            Addin.App.ActiveSettings.Level = activeLevel;
            Addin.App.ActiveSettings.LineStyle = activeLineStyle;
            Addin.App.ActiveSettings.LineWeight = activeLineWeight;
            Addin.App.ActiveSettings.Color = activeColor;
        }
    }

    Dictionary<TFCOM.TFFrameList, PenetrTask> getFramesData()
    {
        var res = new Dictionary<TFCOM.TFFrameList, PenetrTask>();
        
        foreach (DataGridViewRow row in dgvFields.Rows)
        {
            PenetrTask task = (PenetrTask)row.DataBoundItem;
            TFCOM.TFFrameList frameList = createFrameList(task);

            if (frameList != null) // TODO исключение?
            {
                res.Add(frameList, task);
            }
        }
        return res;
    }

    /*    
     Declare Function mdlMinDist_betweenElms Lib "stdmdlbltin.dll" ( 
         ByRef point1 As Point3d , ByRef point2 As Point3d , ByRef distance As Double , 
         ByVal edP1 As Long , ByVal edP2 As Long , ByRef closestPoint As Point3d , 
         ByVal inputTolerance As Double ) As Long 
     */

    [DllImport("stdmdlbltin.dll")]
    public static extern int mdlMinDist_betweenElms( 
        ref BCOM.Point3d point1 , ref BCOM.Point3d point2, ref double distance, 
        int edP1, int edP2,  ref BCOM.Point3d closestPoint, 
        double inputTolerance);

    TFCOM.TFFrameList createFrameList(PenetrTask task)
    {
        BCOM.Level level = ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_NAME);
        BCOM.Level levelSymb = 
            ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_SYMB_NAME);
        BCOM.Level levelRefPoint = 
            ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_POINT_NAME);

        long diamIndex = DiameterType.Parse(task.DiameterTypeStr).number;  
        PenetrInfo penInfo = penData.getPenInfo(task.FlangesType, diamIndex); 

        TFCOM.TFFrameList frameList =
            PenetrHelper.createFrameList(task, penInfo, level);

        PenetrHelper.addProjection(ref frameList, 
            task, penInfo, levelSymb, levelRefPoint);

        // TODO видимость контура перфоратора можно в конфиг. переменную
        PenetrHelper.addPerforator(ref frameList, task, penInfo, levelSymb, false);

        return frameList;
    }

    //TFCOM.TFFrameList createFrameList_Old(PenetrTask task)
    //{
    //    task.scanInfo();
        
    //    if (!Keyins.Penetrations.DEBUG_MODE) {
    //        if (task.isCompoundExistsInPlace || task.TFFormsIntersected.Count == 0) 
    //            return null;
    //    }
            
    //    BCOM.ModelReference taskModel = 
    //        Addin.App.MdlGetModelReferenceFromModelRefP((int)task.modelRefP);
    //        taskModel.MdlModelRefP();
    //    double task_toUOR = taskModel.UORsPerMasterUnit;
    //    double task_subPerMaster = taskModel.SubUnitsPerMasterUnit;
    //    double task_unit3 = taskModel.UORsPerStorageUnit;
    //    double task_unit4 = taskModel.UORsPerSubUnit;

    //    BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;               

    //    double toUOR = activeModel.UORsPerMasterUnit;
    //    double subPerMaster = activeModel.SubUnitsPerMasterUnit;
    //    double unit3 = activeModel.UORsPerStorageUnit;
    //    double unit4 = activeModel.UORsPerSubUnit;

    //    if (Keyins.Penetrations.DEBUG_MODE) { // ОТЛАДКА:
    //        BCOM.LineElement line = 
    //            Addin.App.CreateLineElement2(null, task.Location, task.ProjectPoint);
    //        line.Color = 3;
    //        line.LineWeight = 5;
    //        previewTranContainer.AppendCopyOfElement(line);
    //    }
       
    //    long diamIndex = DiameterType.Parse(task.DiameterTypeStr).number;        
    //    PenetrInfo penInfo = penData.getPenInfo(task.FlangesType, diamIndex);        
        
    //    double pipeInsideDiam = penInfo.pipeDiameterInside / subPerMaster;
    //    double pipeOutsideDiam = penInfo.pipeDiameterOutside / subPerMaster;

    //    double flangeInsideDiam = penInfo.flangeDiameterInside / subPerMaster;
    //    double flangeOutsideDiam = penInfo.flangeDiameterOutside / subPerMaster;
    //    double flangeThick = penInfo.flangeThick / subPerMaster;

    //    double length = task.Length *10 / subPerMaster;

    //    var solids = Addin.App.SmartSolid;

    //    // ! длина трубы меньше размера проходки на толщину фланца
    //    // ! ЕСЛИ ФЛАНЕЦ ЕСТЬ

    //    double delta = task.FlangesCount == 0 ? 0 : 
    //        task.FlangesCount * flangeThick / 2;        

    //    BCOM.SmartSolidElement cylindrInside =
    //        solids.CreateCylinder(null, pipeInsideDiam / 2, length - delta);

    //    BCOM.SmartSolidElement cylindrOutside =
    //        solids.CreateCylinder(null, pipeOutsideDiam / 2, length - delta);
        
    //    var cylindr = solids.SolidSubtract(cylindrOutside, cylindrInside);

    //    var elements = new Dictionary<BCOM.Element, double>();
    //    var projections = new Dictionary<BCOM.Element, double>();

    //    {
    //        double shift  = task.FlangesCount == 1 ? delta : 0;
    //        shift *= task.isSingleFlangeFirst() ? 1 : -1;
    //        elements.Add(cylindr, (length + shift)/2);
    //    }
        
    //    { // Перекрестия: всегда в плоскости стены
    //        var zero = Addin.App.Point3dZero();
    //        projections.Add(
    //            ElementHelper.createCrossRound(ref zero, pipeInsideDiam), 
    //            0.0); 
    //        projections.Add(ElementHelper.createCircle(ref zero, pipeInsideDiam),
    //            0.0);         
    //        projections.Add(
    //            ElementHelper.createCrossRound(ref zero, pipeInsideDiam), 
    //            length);
    //        projections.Add(ElementHelper.createCircle(ref zero, pipeInsideDiam),
    //            length);
    //    }

    //    { // Точка установки:
    //        var pt = Addin.App.Point3dZero();
    //        BCOM.Element refPoint = 
    //            Addin.App.CreateLineElement2(null, pt, pt);
            
    //        projections.Add(refPoint, 0.0);
    //    }

    //    // Фланцы:
    //    for (int i = 0; i < task.FlangesCount; ++i)
    //    {
    //        BCOM.SmartSolidElement flangeCylindr = solids.SolidSubtract(
    //            solids.CreateCylinder(null, flangeOutsideDiam / 2, flangeThick), 
    //            solids.CreateCylinder(null, pipeOutsideDiam / 2, flangeThick));            
            
    //        double shift = 0;
    //        if (task.FlangesCount == 1)
    //        {
    //            bool isNearest = Addin.App.Vector3dEqualTolerance(task.singleFlangeSide,
    //                Addin.App.Vector3dFromXYZ(0, 0, -1), 0.1); // 0.001
                
    //            // 0.5 - для видимости фланцев на грани стены/плиты 
    //            shift = isNearest ?
    //                    0.0    + flangeThick / 2 - 1: // 0.02:
    //                    length - flangeThick / 2 + 1; // 0.02;
    //        }
    //        else
    //        {
    //            shift = i == 0 ? 0.0 : length;               
    //            // для самих фланцев:
    //            // 0.5 - для видимости фланцев на грани стены/плиты 
    //            shift += Math.Pow(-1, i) * (flangeThick/2 - 1); //0.02);
    //        }
    //        elements.Add(flangeCylindr, shift);
    //    }
        


    //    //**********************************************************************
    //    //{ // ПОСТРОЕНИЕ ЧЕРЕЗ ПРОФИЛЬ И ПУТЬ
    //    // *********************************************************************
    //    //    BCOM.LineElement line = Addin.App.CreateLineElement2(null, 
    //    //        Addin.App.Point3dZero(), Addin.App.Point3dFromXYZ(0, 1, 1));

    //    //    BCOM.EllipseElement circle = Addin.App.CreateEllipseElement2(null, 
    //    //        Addin.App.Point3dZero(), pipeOutsideDiam/2, pipeOutsideDiam/2,
    //    //        Addin.App.Matrix3dIdentity());

    //    //    elements.Clear();
    //    //    elements.Add(solids.SweepProfileAlongPath(circle, line),task.Location);
    //    //}        

    //    BCOM.Transform3d taskTran;
    //    {
    //        taskTran = Addin.App.Transform3dFromMatrix3d(task.Rotation);

    //        //taskTran = Addin.App.Transform3dInverse(
    //        //    Addin.App.Transform3dFromMatrix3d(task.Rotation)); 

    //        //taskTran = Addin.App.Transform3dFromMatrix3d(
    //        //    Addin.App.Matrix3dIdentity()); 
    //    }



    //    /*        
    //    Ориентация тела построения solid - вдоль оси Z
    //        Z              _____  
    //        ^  Y             |
    //        | /'             |
    //        !/___> X       __*__ 
    //    */

    //    double correctAboutX;
    //    double correctAboutY;
    //    if (task.TaskType == PenetrTask.TaskObjectType.PipeEquipment)
    //    {
    //        /* Ориентация проходки перед применением матрицы поворота задания
    //         * должна быть вдоль оси  X
    //         * 
    //            Y             |         |
    //            ^             • ========|
    //            !___> X       |         |
    //        */

    //        correctAboutX = 0;
    //        correctAboutY = Math.PI/2;
    //        //rawVector = Addin.App.Point3dFromXYZ(1, 0, 0);
    //    }
    //    else
    //    {
    //        /* Ориентация проходки перед применением матрицы поворота задания
    //         * должна быть вдоль оси Y
    //         *                 _____
    //            Y                |
    //            ^                |
    //            !___> X        __*__
    //        */

    //        correctAboutX = -Math.PI/2;
    //        correctAboutY = 0;
    //        //rawVector = Addin.App.Point3dFromXYZ(0, 1, 0);
    //    }

    //    //BCOM.Point3d taskLocation =  taskModel.IsAttachment ? 
    //    //    Addin.App.Point3dScale(task.Location, task_subPerMaster) :
    //    //    task.Location;
    //    BCOM.Point3d taskLocation = task.Location;

    //    BCOM.Level level = ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_NAME);
    //    BCOM.Level levelSymb = 
    //        ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_SYMB_NAME);
    //    BCOM.Level levelRefPoint = 
    //        ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_POINT_NAME);

    //    TFCOM.TFFrameList frameList = Addin.AppTF.CreateTFFrame();

    //    foreach (var pair in elements)
    //    {
    //        BCOM.Element elem = pair.Key;
    //        double shift = pair.Value;
            
    //        elem.Color = 0; // TODO
    //        BCOM.Point3d offset = Addin.App.Point3dAddScaled(
    //            Addin.App.Point3dZero(), Addin.App.Point3dFromXYZ(0, 0, 1), shift);
    //        elem.Move(offset);

    //        elem.Rotate(Addin.App.Point3dZero(), correctAboutX, correctAboutY, 0);
            
    //        elem.Transform(taskTran);
    //        elem.Move(taskLocation);

    //        elem.Level = level;
    //        ElementHelper.setSymbologyByLevel(elem);

    //        frameList.AsTFFrame.Add3DElement(elem);
    //    }

    //    //frameList.AsTFFrame.Get3DElement().Level = level;
    //    //ElementHelper.setSymbologyByLevel(frameList.AsTFFrame.Get3DElement());

    //    TFCOM.TFProjectionList projList = Addin.AppTF.CreateTFProjection();
    //    projList.Init();
    //    foreach (var pair in projections)
    //    {
    //        BCOM.Element elem = pair.Key;
    //        double shift = pair.Value;

    //        elem.Color = 0; // TODO

    //        BCOM.Point3d offset = Addin.App.Point3dAddScaled(
    //            Addin.App.Point3dZero(), Addin.App.Point3dFromXYZ(0, 0, 1), shift);
    //        elem.Move(offset);

    //        elem.Rotate(Addin.App.Point3dZero(), correctAboutX, correctAboutY, 0);
            
    //        elem.Transform(taskTran);
    //        elem.Move(taskLocation);

    //        elem.Level = (elem.Type == BCOM.MsdElementType.Line) ?
    //            levelRefPoint : levelSymb;
    //        ElementHelper.setSymbologyByLevel(elem);

    //        if (elem.Type == BCOM.MsdElementType.Line) {
    //            // точка вставки - линия с нулевой длинной           
    //            elem.Level = levelRefPoint;
    //        }

    //        var elemProjList = Addin.AppTF.CreateTFProjection();
    //        elemProjList.AsTFProjection.SetEmbeddedElement(elem);
    //        projList.Append(elemProjList);
    //    }
        
    //    frameList.AsTFFrame.SetProjectionList(projList);
        
    //    // ПЕРФОРАТОР
    //    BCOM.EllipseElement perfoEl = 
    //        Addin.App.CreateEllipseElement2(null, Addin.App.Point3dZero(), 
    //            pipeInsideDiam/2, pipeInsideDiam/2, 
    //            Addin.App.Matrix3dIdentity(), BCOM.MsdFillMode.Filled);
    //    {
    //        BCOM.Point3d offset = Addin.App.Point3dAddScaled(
    //            Addin.App.Point3dZero(), 
    //            Addin.App.Point3dFromXYZ(0, 0, 1), length/2);
    //        perfoEl.Move(offset);
    //        perfoEl.Rotate(Addin.App.Point3dZero(), correctAboutX, correctAboutY, 0);
    //    }
    //    perfoEl.Level = levelSymb;
    //    ElementHelper.setSymbologyByLevel(perfoEl);
    //    perfoEl.Transform(taskTran);
    //    perfoEl.Move(taskLocation);      

    //    BCOM.Point3d perfoVec = perfoEl.Normal;        

    //    TFCOM.TFPerforatorList perfoList = Addin.AppTF.CreateTFPerforator();
    //    var tranIdentity = Addin.App.Transform3dIdentity();


    //    perfoList.InitFromElement(perfoEl, perfoVec, length/2 * 1.01, tranIdentity);
    //    perfoList.SetSweepMode(
    //        TFCOM.TFdPerforatorSweepMode.tfdPerforatorSweepModeBi);
    //    //perfoList.SetSenseDist(1.01 * length / 2);
    //    perfoList.SetPolicy(
    //        TFCOM.TFdPerforatorPolicy.tfdPerforatorPolicyThroughHoleWithinSenseDist);
        
    //    frameList.AsTFFrame.SetPerforatorList(perfoList);
    //    frameList.AsTFFrame.SetSenseDistance2(length/2);
    //    frameList.AsTFFrame.SetName("Penetration"); // ранее было 'EmbeddedPart'
    //    frameList.AsTFFrame.SetPerforatorsAreActive(true);
    //    frameList.Synchronize();

    //    return frameList;
    //}

    private static void setDataGroupInstance(
        BCOM.Element bcomElement, PenetrTask task)
    {
        Element element = ElementHelper.getElement(bcomElement);
        if (element == null)
            return;
        
        using (var catalogEditHandle = new CatalogEditHandle(element, true, true))
        {
            if (catalogEditHandle == null || 
                catalogEditHandle.CatalogInstanceName != null)
            {
                return;
            }

            catalogEditHandle.InsertDataGroupCatalogInstance("EmbeddedPart", "Embedded Part");
            catalogEditHandle.UpdateInstanceDataDefaults();
            
            DataGroupProperty code = null;
            DataGroupProperty name = null;

            foreach (DataGroupProperty property in catalogEditHandle.GetProperties())
            {
                if (property?.Xpath == "EmbPart/@PartCode") 
                    code = property;
                else if (property?.Xpath == "EmbPart/@CatalogName")
                    name = property;
            }

            if (code != null)
                catalogEditHandle.SetValue(code, task.Code);
            else {
                code = new DataGroupProperty("PartCode", task.Code, false, true);
                code.SchemaName = "EmbPart";
                code.Xpath = "EmbPart/@PartCode";
                catalogEditHandle.Properties.Add(code);
            }
            catalogEditHandle.SetValue(code, task.Code);

            if (name != null)
                catalogEditHandle.SetValue(name, task.Name);
            else {
                name = new DataGroupProperty("CatalogName", task.Name, false, true);
                name.SchemaName = "EmbPart";
                name.Xpath = "EmbPart/@CatalogName";
                catalogEditHandle.Properties.Add(name);
            }
            catalogEditHandle.Properties.Add(name);            
            catalogEditHandle.Rewrite((int)BCOM.MsdDrawingMode.Normal);

            // TODO решить проблему вылета при команде Modify DataGroup Instance
        }
    }
    }
}
