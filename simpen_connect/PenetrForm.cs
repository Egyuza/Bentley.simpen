using System;
using System.Windows.Forms;
using System.Data.SqlClient;

using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using System.Collections.Generic;

using BMI = Bentley.MstnPlatformNET.InteropServices;
using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;
using Bentley.Building.Api;
using Bentley.Interop.TFCom;

using System.Data;

using System.Linq;
using System.Linq.Expressions;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;
using Bentley.GeometryNET;

using Shared;
using Shared.Penetrations;

namespace simpen_cn
{

public partial class PenetrForm : Form
{
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

    DataTable penData = new DataTable();

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

    ModeEnum mode = ModeEnum.Single;

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
        this.Text = "Проходки " + Addin.getVersion();
        // ------------------------------------------
        
        readDatabaseData();
        
        
        //ViewHelper.getActiveView().UsesDisplaySet = true;
        
        dgvFields.AutoGenerateColumns = false;
        dgvFields.DataSource = bindSource;
        dgvFields.EnableHeadersVisualStyles = false;
        dgvFields.Columns.Clear();
        dgvFields.AutoSize = false;

        System.Drawing.Color readonlyColor = System.Drawing.SystemColors.Control;
         
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

        //column = new DataGridViewTextBoxColumn();
        column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "FlangesType";
        column.Name = ColumnName.FLANGES;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        List<long> flanNumbers = penData.AsEnumerable().
            Where(x => projId != 0 ? x.Field<long>("prjId") == projId : 
                x.Field<long>("depID") == depId)
            .Select(x => x.Field<long>("flanNumber")).Distinct().ToList();
        flanNumbers.Sort();
        (column as DataGridViewComboBoxColumn).DataSource = flanNumbers;        
        dgvFields.Columns.Add(column);
        
        column = new DataGridViewComboBoxColumn();
        //column.ValueType = typeof(DiameterType);
        column.DataPropertyName = "DiameterTypeStr";
        //(column as DataGridViewComboBoxColumn).DisplayMember = "NumberDisplay";
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
        
        BCOM.Point3d zero = Addin.App.Point3dZero();
        BCOM.LineElement line = Addin.App.CreateLineElement2(null, zero, zero);
        line.Color = 255;

        selectionTranContainer = Addin.App.CreateTransientElementContainer1(
            line, 
            BCOM.MsdTransientFlags.DisplayFirst |
            BCOM.MsdTransientFlags.Overlay,
            BCOM.MsdViewMask.AllViews, 
            BCOM.MsdDrawingMode.Temporary);
        
        previewTranContainer = Addin.App.CreateTransientElementContainer1(
            line, 
            BCOM.MsdTransientFlags.DisplayFirst |
            BCOM.MsdTransientFlags.Overlay | BCOM.MsdTransientFlags.Snappable | BCOM.MsdTransientFlags.IncludeInPlot,
            BCOM.MsdViewMask.AllViews, 
            BCOM.MsdDrawingMode.Temporary);

        { // TODO восстанавливаем сохранённые настройки:
            
        }
    }

        private void Instance_SelectedViewChangedEvent(AddIn sender, AddIn.SelectedViewChangedEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private void DgvFields_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        DataGridViewRow gridRow = dgvFields.Rows[e.RowIndex];
        PenetrTask task = (PenetrTask)gridRow.DataBoundItem;

        var lengthCell = gridRow.Cells[ColumnName.LENGTH];
        lengthCell.ReadOnly = false;

        // тип фланцев == 3 - расстояние насквозь

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
        //  TODO implement
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

    private void DgvFields_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
    {
        for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; ++i)
        {
            DataGridViewRow gridRow = dgvFields.Rows[i];
            PenetrTask task = (PenetrTask)gridRow.DataBoundItem;
            var dataRows = penData.AsEnumerable().
                Where(x => projId != 0 ? x.Field<long>("prjId") == projId : 
                    x.Field<long>("depID") == depId).
                Where(x => x.Field<long>("flanNumber") == task.FlangesType).
                OrderBy(x => x.Field<long>("diamNumber")).
                ThenByDescending(x => x.Field<float>("pipeDiam")).
                ThenByDescending(x => x.Field<float>("pipeThick"));

            var comboCell = gridRow.Cells["Диаметр"] as DataGridViewComboBoxCell;
            
            //var diameterList = new List<DiameterType>();
            var diameterList = new List<string>();

            DiameterType matchValue = null;
            foreach (DataRow dataRow in dataRows)
            {        
                var diamType = new DiameterType(
                    dataRow.Field<long>("diamNumber"),
                    dataRow.Field<float>("pipeDiam"),
                    dataRow.Field<float>("pipeThick"));

                if (diamType.number == 
                    DiameterType.Parse(task.DiameterTypeStr).number)
                {
                    matchValue = diamType;
                }
                //comboCell.Items.Add(diamType);
                diameterList.Add(diamType.ToString());
            }
            
            comboCell.DataSource = diameterList;
            //comboCell.ValueMember = "number";
            //comboCell.DisplayMember = "NumberDisplay";

            if (matchValue != null) {
                comboCell.Value = matchValue.ToString();
            }
            else {
                comboCell.ErrorText = "не валидное значение диаметра";
            }

            if (task.TFFormsIntersected.Count == 0)
            {
                gridRow.ReadOnly = true;
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
                Addin.App.MdlGetModelReferenceFromModelRefP((long)task.modelRefP);

            BCOM.View view = ViewHelper.getActiveView();

            // TODO рисовать кубик range штриховкой под цвет выделения...
            // !!! учесть, что задание может быть в референсе !!!

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

    // СОЗДАТЬ ОБЪЕКТ
    
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

        // todo sets save
    }

    private void Instance_SelectionChangedEvent(
        AddIn sender, AddIn.SelectionChangedEventArgs eventArgs)
    {
        // Element element = null;
        // Session.Instance.StartUndoGroup();
        // Session.Instance.EndUndoGroup();
        
        Dictionary<IntPtr, Element> selectionSet = new Dictionary<IntPtr, Element>();
        uint nums = SelectionSetManager.NumSelected();
        for (uint i = 0; i < nums; ++i)
        {
            Element element = null;
            DgnModelRef modelRef = null;

            if (StatusInt.Success ==
                SelectionSetManager.GetElement(i, ref element, ref modelRef) &&
                element.ElementType == MSElementType.CellHeader)
            {
                selectionSet.Add(element.GetNativeElementRef(), element);
            }
        }

        try
        {
            switch ((int)eventArgs.Action)
            {
            case (int)AddIn.SelectionChangedEventArgs.ActionKind.SetEmpty:
                penTaskSelection.Clear();
                bindSource.Clear();
                previewTranContainer.Reset();
                break;
            case (int)AddIn.SelectionChangedEventArgs.ActionKind.SetChanged:
            {
                // remove unselected
                foreach (IntPtr ptr in penTaskSelection.Keys)
                {
                    if (!selectionSet.ContainsKey(ptr))
                    {
                        bindSource.Remove(penTaskSelection[ptr]);
                        penTaskSelection.Remove(ptr);
                    }
                }
                // add new
                foreach (Element element in selectionSet.Values)
                {
                    IntPtr elementRef = element.GetNativeElementRef();
                    PenetrTask task;
                    if (PenetrTask.getFromElement(element, out task) &&
                        !penTaskSelection.ContainsKey(elementRef))
                    {
                        penTaskSelection.Add(elementRef, task);
                        bindSource.Add(task);
                    }
                }
                break;
            }
            case 7: // ActionKind.Remove
            {
                foreach (IntPtr ptr in penTaskSelection.Keys)
                {
                    if (!selectionSet.ContainsKey(ptr))
                    {
                        bindSource.Remove(penTaskSelection[ptr]);
                        penTaskSelection.Remove(ptr);
                    }
                }
                break;
            }
            case 5: // ActionKind.New:
            {
                foreach (Element element in selectionSet.Values)
                {
                    PenetrTask task;
                    if (PenetrTask.getFromElement(element, out task))
                    {
                        penTaskSelection.Add(element.GetNativeElementRef(), task);
                        bindSource.Add(task);
                    }
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

    private void readDatabaseData() 
    {   // логика взята из оригинального simpen от Л.Вибе        

        BCOM.Workspace wspace = Addin.App.ActiveWorkspace;

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
    
        projId = wspace.IsConfigurationVariableDefined("EMBDB_PROJECT_ID") ?
        long.Parse(wspace.ConfigurationVariableValue("EMBDB_PROJECT_ID")) : 
        0; // offtake project id
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
                linked, userName);
            using (SqlDataReader reader = 
                new SqlCommand(sql, connection).ExecuteReader())
            {
                if (reader != null && reader.HasRows)
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);

                    userId = dt.Rows[0].Field<long>("usrID");
                    catalogId = dt.Rows[0].Field<long?>("usrCatalogID") ?? 0L;
                    depId = dt.Rows[0].Field<long>("depID");
                }
            }

            bool resHasRows = false;
            using (SqlDataReader reader = new SqlCommand(
                string.Format(
                    "select distinct flanNumber from {0}pendiam where {1} = {2}",
                    linked,
                    projId > 0 ? "prjID" : "depID", 
                    projId > 0 ? projId : depId ), 
                connection).ExecuteReader())
            {
                // todo caption project
                resHasRows = reader.HasRows;
            }

            if (!resHasRows)
            {
                depId = 0;
            }
            
            penData.Clear();
            using (SqlDataReader reader = new SqlCommand(
                string.Format("select * from {0}view_pendiam2", linked), 
                connection).ExecuteReader())
            {
                if (reader != null && reader.HasRows)
                {
                    penData.Load(reader);
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
        catch (Exception ex) // todo
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


        //C:\Program Files\Bentley\AECOsim CONNECT Edition\AECOsimBuildingDesigner\Mdlapps\BldDesigner.dll
/*
BldDesigner.dll
cellEdit.dll
tfc.dll
tfdgschedules.dll
tfhandler.dll
 */
    [DllImport("tfc.dll")]
     [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    private static extern int mdlTFModelRef_updateAutoOpeningsByFrame(
        [MarshalAs(UnmanagedType.Interface), In] BCOM.ModelReference ModelRef,
        [MarshalAs(UnmanagedType.Interface), In, Out] ref TFCOM.TFFrame Frame,
        [In] bool AssocGG,
        [In] bool OperationDelete,
        [In] TFCOM.TFdFramePerforationPolicy OverridePolicy);

    [DllImport("tfc.dll")]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    private static extern int mdlTFModelRef_addFrame(
        long ModelRefP,
        [MarshalAs(UnmanagedType.Interface), In, Out] ref TFCOM.TFFrame Frame);
        //long modelRef, 
        //[MarshalAs(UnmanagedType.Interface), In, Out] ref TFCOM.TFFrame frame, 
        //int assocGG, int operationDelete, int overridePolicy);

        /*
            StatusInt       mdlTFModelRef_updateAutoOpeningsByFrame  
            ( 
            DgnModelRefP       pThis , 
            TFFrame*       pFrame , 
            BoolInt       assocGG , 
            BoolInt       operationDelete , 
            FramePerforationPolicyEnum       overridePolicy  
            ); 
        */

        private class Perforatibility : TFCOM.TFHandlerFormPerforatability
        {
            public bool FormIsPerforatable(TFFormRecipe FormIn)
            {
                return true;
            }
        }



    //private unsafe void unsafeTest()
    //{
        
    //}

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
            //foreach (var pair in getFramesData2())
            //{
            //    var tfApi = new Bentley.Building.Api.TFApplicationList().AsTFApplication;
            //    var modelRef = Session.Instance.GetActiveDgnModelRef();

            //    var frameList = pair.Key;
            //    tfApi.ModelReferenceAddFrameList(modelRef, ref frameList, 0);
            //}


            foreach (var pair in getFramesData())
            {
                TFCOM.TFFrameList frameList = pair.Key;
                PenetrTask task = pair.Value;
            
                TFCOM.TFFrame frame = null;

                return;

                //BCOM.Element newElement = activeModel.GetLastValidGraphicalElement();

                {
                    // ! без этого кода не срабатывает перфорация в стенке/плите
                    // судя по всему инициализирует обновление объектов, с которыми
                    // взаимодействует frame

                  
                    //var modelRef = Session.Instance.GetActiveDgnModelRef();

                    try
                    {
                      //  BCOM.Element bcomElem = frameList.AsTFFrame.Get3DElement();

                        //frameList.Synchronize();
                        //frameList.AsTFFrame.SetPerforatorsAreActive(true);
                                                

    // mdlTFApplication_modelReferenceUpdateAutoOpeningsByFrame                    

                        //var newElement = activeModel.GetLastValidGraphicalElement();
                        //frameList.InitFromElement(newElement);


                       // frame = frameList.AsTFFrame;

                frame = frameList.AsTFFrame;

               // Addin.AppTF.ModelReferenceAddFrame(activeModel, ref frame);


                Addin.AppTF.AddFormPerforatabilityHandler(new Perforatibility());

                Addin.AppTF.ModelReferenceAddFrameList(activeModel, ref frameList);

                frameList.Synchronize();
                var frameListClass = frameList as TFCOM.TFFrameListClass;

                {           
                    BCOM.Element bcomElem;                    
                    frameListClass.GetElement(out bcomElem);                
                         
                    var tfApi = new Bentley.Building.Api.TFApplicationList().AsTFApplication;
                    var modelRef = Session.Instance.GetActiveDgnModelRef();

                    Element ielement = Element.GetFromElementRef((IntPtr)bcomElem.MdlElementRef());
                    modelRef.GetFromElementRef((IntPtr)bcomElem.MdlElementRef());

                    ITFFrameList iframeList;
                    tfApi.CreateTFFrame(0, out iframeList);
                    iframeList.InitFromElement(ielement, "");
                    iframeList.Synchronize("");
                    //iframeList.ConsolidateOverrides("");

                    //iframeList.Duplicate(out iframeList, "");




                    ITFFrame iframe = iframeList.AsTFFrame;
                    int stat = tfApi.ModelReferenceUpdateAutoOpeningsByFrame(modelRef, 
                        ref iframe, true, false, Bentley.Building.Api.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone, 0); 
                    tfApi.ModelReferenceAddFrame(modelRef, ref iframe, 0);
                    ;
                                        
                   // tfApi.ModelReferenceAddFrameList(modelRef, ref iframeList, 0);
                }

               // frameListClass.SetPerforatorsAreActive(true, "");
                
                //Addin.AppTF.ModelReferenceUpdateAutoOpeningsByFrame(
                //    activeModel, frameListClass.AsTFFrame, true, false, 
                //    TFCOM.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone);
                
                
                //BCOM.Element bcomElem = activeModel.GetLastValidGraphicalElement();
                //frameList.InitFromElement(bcomElem);
                //frame = frameList.AsTFFrame;
                
                //frame = frameList.AsTFFrame;

                //mdlTFModelRef_updateAutoOpeningsByFrame(
                //    activeModel, ref frame, true, false, 
                //    TFCOM.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone);
                
                        //tfApp.ModelReferenceRewriteElement(modelRef, element, 0);
                       // // task.TFFormsIntersected[0].AsTFElement.GetElement(out bcomElem);
                       // Element.GetFromElementRef((IntPtr)bcomElem.MdlElementRef());
                       // Element element = Element.GetFromElementRefAndModelRef(
                       //     (IntPtr)bcomElem.MdlElementRef(), (IntPtr)activeModel.MdlModelRefP());

                       // ITFFrameList iframeList;
                       // int status = tfApp.ModelReferenceReadFrameListToMasterById(modelRef,
                       //     out iframeList, element.ElementId, 0);


                       // var iframe = iframeList.AsTFFrame;

                       // status  = tfApp.ModelReferenceUpdateAutoOpeningsByFrame(modelRef,
                       //     ref iframe, true, true, Bentley.Building.Api.TFdFramePerforationPolicy.tfdFramePerforationPolicyStrict, 0);
                                                   
                       //status = tfApp.ModelReferenceRewriteFrameList(modelRef, iframeList, 0);

                       //Addin.AppTF.ModelReferenceRewriteFrame(activeModel, frame);


                       // TFCOM.TFPerforatorList perfoList;
                       // frame.GetPerforatorList(out perfoList);

                       // TFCOM.TFItemList ChangedForms = Addin.AppTF.CreateTFItem();
                        
                       // string options = "";
                       // TFCOM._TFFormRecipeList recipeList; // = frameList.AsTFFrame.GetFormRecipeList();
                       // task.TFFormsIntersected[0].AsTFElement.GetFormRecipeList(out recipeList);
                        
                       // recipeList.UpdateOpeningByPerforatorList(out ChangedForms, perfoList, true, true, "", "");



                       //tfApp.ModelReferenceRewriteElement(modelRef, element, 0);
                        
                    //void ModelReferenceUpdateAutoOpeningsByFrame(ModelReference ModelRef, ref TFFrame Frame, bool AssocGG, bool OperationDelete, TFdFramePerforationPolicy OverridePolicy, string Options);
                        //Addin.AppTF.ModelReferenceUpdateAutoOpeningsByFrame(
                        //    activeModel, frameList.AsTFFrame, true, false, 
                        //    TFCOM.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone);
                    }
                    catch (Exception ex)
                    {
                        ex.ShowMessage(); // TODO
                    }
                }             

                //// добавление в модель
                //Addin.AppTF.ModelReferenceAddFrame(
                //    Addin.App.ActiveModelReference, frame);
                  
                //setDataGroupInstance(newElement, task);
            }
        }
        catch (Exception ex)
        {
            //throw;
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

    Dictionary<ITFFrameList, PenetrTask> getFramesData2()
    {
        var res = new Dictionary<ITFFrameList, PenetrTask>();
        
        foreach (DataGridViewRow row in dgvFields.Rows)
        {
            PenetrTask task = (PenetrTask)row.DataBoundItem;
            ITFFrameList frameList = createFrameList2(task);

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
        task.scanInfo();
        if (task.isCompoundExistsInPlace || task.TFFormsIntersected.Count == 0)
            return null;

        BCOM.ModelReference taskModel =
            Addin.App.MdlGetModelReferenceFromModelRefP((long)task.modelRefP);

        double task_toUOR = taskModel.UORsPerMasterUnit;
        double task_subPerMaster = taskModel.SubUnitsPerMasterUnit;
        double task_unit3 = taskModel.UORsPerStorageUnit;
        double task_unit4 = taskModel.UORsPerSubUnit;

        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;

        double toUOR = activeModel.UORsPerMasterUnit;
        double subPerMaster = activeModel.SubUnitsPerMasterUnit;
        double unit3 = activeModel.UORsPerStorageUnit;
        double unit4 = activeModel.UORsPerSubUnit;

        // todo убрать от сюда поиск данных
        var res = penData.AsEnumerable().First(x =>
            (projId != 0 ? x.Field<long>("prjId") == projId :
                    x.Field<long>("depID") == depId) &&
            x.Field<long>("prjID") == projId &&
            x.Field<long>("flanNumber") == task.FlangesType &&
            x.Field<long>("diamNumber") == DiameterType.Parse(task.DiameterTypeStr).number);

        if (res == null)
        {
            return null;
        }

        double pipeInsideDiam = res.Field<float>("pipeDiam").ToDouble() -
            res.Field<float>("pipeThick").ToDouble() * 2;
        pipeInsideDiam /= subPerMaster;

        double pipeOutsideDiam =
            res.Field<float>("pipeDiam").ToDouble() / subPerMaster;

        double flangeInsideDiam = pipeOutsideDiam;
        double flangeOutsideDiam =
            res.Field<double>("flangeWidth") / subPerMaster;
        double flangeThick = res.Field<double>("flangeThick") / subPerMaster;

        double length = task.Length * 10 / subPerMaster;

        var solids = Addin.App.SmartSolid;

        // ! длина трубы меньше размера проходки на толщину фланца


        BCOM.SmartSolidElement cylindrInside =
            solids.CreateCylinder(null, pipeInsideDiam / 2, length - flangeThick);

        BCOM.SmartSolidElement cylindrOutside =
            solids.CreateCylinder(null, pipeOutsideDiam / 2, length - flangeThick);

        var cylindr = solids.SolidSubtract(cylindrOutside, cylindrInside);

        var elements = new Dictionary<BCOM.Element, double>();
        var projections = new Dictionary<BCOM.Element, double>();

        elements.Add(cylindr, length / 2);

        for (int i = 0; i < task.FlangesCount; ++i)
        {
            BCOM.SmartSolidElement flangeCylindr = solids.SolidSubtract(
                solids.CreateCylinder(null, flangeOutsideDiam / 2, flangeThick),
                solids.CreateCylinder(null, pipeOutsideDiam / 2, flangeThick));

            double shift = 0;
            if (task.FlangesCount == 1)
            {
                bool isNearest = Addin.App.Vector3dEqualTolerance(task.singleFlangeSide,
                    Addin.App.Vector3dFromXY(0, 1), 0.1); // 0.001

                // 1 мм - для видимости фланцев на грани стены/плиты 
                shift = isNearest ?
                        0.0 + flangeThick / 2 - 1 :
                        length - flangeThick / 2 + 1;

                double projShift = isNearest ? 0.0 : length;

                var zero = Addin.App.Point3dZero();
                BCOM.Point3d[] verts = { zero, zero, zero, zero, zero };
                double k = pipeInsideDiam / 2 * Math.Cos(Math.PI / 4);
                verts[0].X = -k;
                verts[0].Y = -k;
                verts[1].X = k;
                verts[1].Y = k;
                verts[2] = ElementHelper.getMiddlePoint(verts[0], verts[1]);
                verts[3] = verts[0];
                verts[3].Y *= -1;
                verts[4] = verts[1];
                verts[4].Y *= -1;
                projections.Add(Addin.App.CreateLineElement1(null, verts), projShift);
            }
            else
            {
                shift = i == 0 ? 0.0 : length;

                var zero = Addin.App.Point3dZero();
                BCOM.Point3d[] verts = { zero, zero, zero, zero, zero };
                double k = pipeInsideDiam / 2 * Math.Cos(Math.PI / 4);
                verts[0].X = -k;
                verts[0].Y = -k;
                verts[1].X = k;
                verts[1].Y = k;
                verts[2] = ElementHelper.getMiddlePoint(verts[0], verts[1]);
                verts[3] = verts[0];
                verts[3].Y *= -1;
                verts[4] = verts[1];
                verts[4].Y *= -1;
                projections.Add(Addin.App.CreateLineElement1(null, verts), shift);

                // для самих фланцев:
                // 0.5 - для видимости фланцев на грани стены/плиты 
                shift += Math.Pow(-1, i) * (flangeThick / 2 - 1); //0.02);
            }

            elements.Add(flangeCylindr, shift);
        }

        { // точка вставки
            var pt = Addin.App.Point3dZero();
            BCOM.Element refPoint =
                Addin.App.CreateLineElement2(null, pt, pt);

            projections.Add(refPoint, 0.0);
            //            projections.Add(refPoint, 
            //task.RefPointIndex == RefPointPosEnum.START ? 0.0 :
            //task.RefPointIndex == RefPointPosEnum.CENTER ? length/2 : 
            //length);
        }

        //{ // построение через профиль и путь
        //    BCOM.LineElement line = Addin.App.CreateLineElement2(null, 
        //        Addin.App.Point3dZero(), Addin.App.Point3dFromXYZ(0, 1, 1));

        //    BCOM.EllipseElement circle = Addin.App.CreateEllipseElement2(null, 
        //        Addin.App.Point3dZero(), pipeOutsideDiam/2, pipeOutsideDiam/2,
        //        Addin.App.Matrix3dIdentity());

        //    elements.Clear();
        //    elements.Add(solids.SweepProfileAlongPath(circle, line),task.Location);
        //}

        BCOM.Transform3d taskTran =
            Addin.App.Transform3dFromMatrix3d(task.Rotation);

        //BCOM.Point3d taskLocation =  taskModel.IsAttachment ? 
        //    Addin.App.Point3dScale(task.Location, task_subPerMaster) :
        //    task.Location;
        BCOM.Point3d taskLocation = task.Location;

        BCOM.Level level = ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_NAME);
        BCOM.Level levelSymb =
            ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_SYMB_NAME);
        BCOM.Level levelRefPoint =
            ElementHelper.getOrCreateLevel(PenetrTask.LEVEL_POINT_NAME);

        TFCOM.TFFrameList frameList = Addin.AppTF.CreateTFFrame();

        foreach (var pair in elements)
        {
            BCOM.Element elem = pair.Key;
            double shift = pair.Value;

            elem.Color = 0; // TODO
            elem.Rotate(Addin.App.Point3dZero(), Math.PI / 2, 0, 0);

            BCOM.Point3d offset = Addin.App.Point3dAddScaled(
                Addin.App.Point3dZero(), Addin.App.Point3dFromXYZ(0, 1, 0), shift);
            elem.Move(offset);

            elem.Transform(taskTran);
            elem.Move(taskLocation);

            elem.Level = level;
            ElementHelper.setSymbologyByLevel(elem);

            frameList.AsTFFrame.Add3DElement(elem);
        }

        frameList.AsTFFrame.Get3DElement().Level = level;
        ElementHelper.setSymbologyByLevel(frameList.AsTFFrame.Get3DElement());

       // TFCOM.TFProjectionList projListOrig = Addin.AppTF.CreateTFProjection();
       // projListOrig.Init();
       // foreach (var pair in projections)
       // {
       //     BCOM.Element elem = pair.Key;
       //     double shift = pair.Value;

       //     elem.Color = 0; // TODO
       //     elem.Rotate(Addin.App.Point3dZero(), Math.PI / 2, 0, 0);

       //     BCOM.Point3d offset = Addin.App.Point3dAddScaled(
       //         Addin.App.Point3dZero(), Addin.App.Point3dFromXYZ(0, 1, 0), shift);
       //     elem.Move(offset);

       //     elem.Transform(taskTran);
       //     elem.Move(taskLocation);

       //     elem.Level = (elem.Type == BCOM.MsdElementType.Line) ?
       //         levelRefPoint : levelSymb;
       //     ElementHelper.setSymbologyByLevel(elem);

       //     if (elem.Type == BCOM.MsdElementType.Line)
       //     {
       //         // точка вставки - линия с нулевой длинной           
       //         elem.Level = levelRefPoint;
       //     }

       //     var elemProjList = Addin.AppTF.CreateTFProjection();
       //     elemProjList.AsTFProjection.SetEmbeddedElement(elem);
       //     projListOrig.Append(elemProjList);
       // }

       //frameList.AsTFFrame.SetProjectionList(projListOrig);


        { // DGN_PLATFORM_NET :

            Addin.AppTF.ModelReferenceAddFrameList(activeModel, ref frameList);
           
            //frameList.Synchronize();
            var frameListClass = frameList as TFCOM.TFFrameListClass;

            BCOM.Element bcomElem;
            frameListClass.GetElement(out bcomElem);          
                         
            var tfApi = new Bentley.Building.Api.TFApplicationList().AsTFApplication;
            var modelRef = Session.Instance.GetActiveDgnModelRef();
            var model = Session.Instance.GetActiveDgnModel();

            Element ielement = Element.GetFromElementRef((IntPtr)bcomElem.MdlElementRef());
            modelRef.GetFromElementRef((IntPtr)bcomElem.MdlElementRef());
                       

            ITFFrameList iframeList;
            tfApi.CreateTFFrame(0, out iframeList);
            iframeList.InitFromElement(ielement, "");
            iframeList.Synchronize("");

            DPoint3d origin = task.Location.ToDPoint();
            origin.ScaleInPlace(toUOR);

            DMatrix3d matrix = DMatrix3d.FromRows(
                task.Rotation.RowX.ToDVector(), task.Rotation.RowY.ToDVector(),
                task.Rotation.RowZ.ToDVector());

            DTransform3d dTran = DTransform3d.FromMatrixAndTranslation(matrix, origin);
            TransformInfo tranInfo = new TransformInfo(dTran);

            double pipeInsideRadius = pipeOutsideDiam/2 * toUOR;
            double dgnLength = length * toUOR;

            var ellips = new EllipseElement(model, null, 
                DEllipse3d.FromCenterRadiusNormal(DPoint3d.Zero, pipeInsideRadius, 
                DVector3d.FromXY(0, 1)));        
            
            ellips.ApplyTransform(tranInfo);
            
            
            {  // ПЕРФОРАТОР:
                ITFPerforatorList perfoList;
                tfApi.CreateTFPerforator(0, out perfoList);
                var dir = DVector3d.FromXY(1, 0);
                var tran = DTransform3d.Identity;
                perfoList.InitFromElement(ellips, ref dir, length*toUOR, ref tran, "");
                perfoList.AsTFPerforator.SetIsVisible(true, 0);
                perfoList.SetSweepMode(Bentley.Building.Api.TFdPerforatorSweepMode.tfdPerforatorSweepModeBi, "");
                perfoList.SetPolicy(Bentley.Building.Api.TFdPerforatorPolicy.tfdPerforatorPolicyThroughHoleWithinSenseDist, "");
        
                iframeList.AsTFFrame.SetPerforatorList(ref perfoList, 0);
                iframeList.AsTFFrame.SetSenseDistance2(length, 0);
                iframeList.AsTFFrame.SetPerforatorsAreActive(true, 0);

                var frame = iframeList.AsTFFrame;
                tfApi.ModelReferenceUpdateAutoOpeningsByFrame(modelRef,
                    ref frame, true, false, Bentley.Building.Api.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone, 0); 
            }

            { // ПРОЕКЦИОННАЯ ГЕОМЕТРИЯ
                ITFProjectionList projList, projList1, projList2, projList3;
                tfApi.CreateTFProjection(0, out projList);
                tfApi.CreateTFProjection(0, out projList1);
                tfApi.CreateTFProjection(0, out projList2);
                tfApi.CreateTFProjection(0, out projList3);
                                
                var zero = DPoint3d.Zero;
                DPoint3d[] verts = { zero, zero, zero, zero, zero };
                double k = pipeInsideRadius * Math.Cos(Math.PI / 4);
                verts[0].X = -k;
                verts[0].Z = -k;
                verts[1].X = k;
                verts[1].Z = k;
                verts[3] = verts[0];
                verts[3].Z *= -1;
                verts[4] = verts[1];
                verts[4].Z *= -1;

                LineStringElement cross1 = new LineStringElement(model, null, verts);
                for (int i = 0; i < verts.Count(); ++i)
                {
                    verts[i].Y = dgnLength;
                }
                LineStringElement cross2 = new LineStringElement(model, null, verts);
                
                cross1.ApplyTransform(tranInfo);
                cross2.ApplyTransform(tranInfo);                

                projList1.AsTFProjection.SetEmbeddedElement(cross1, 0);
                projList2.AsTFProjection.SetEmbeddedElement(cross2, 0);

                LineElement refPoint =
                    new LineElement(model, null, new DSegment3d(zero, zero));
                
                refPoint.ApplyTransform(tranInfo);
                ElementPropertiesSetter setter = new ElementPropertiesSetter();
                setter.SetWeight(7);
                setter.Apply(refPoint);

                projList3.AsTFProjection.SetEmbeddedElement(refPoint, 0);

                projList.Append(projList1, "");
                projList.Append(projList2, "");
                projList.Append(projList3, "");
                iframeList.AsTFFrame.SetProjectionList(projList, 0);
            }

            int stat = tfApi.ModelReferenceRewriteFrameList(modelRef, iframeList, 0); 

            frameListClass = frameList as TFCOM.TFFrameListClass;
            frameListClass.GetElement(out bcomElem);     

            setDataGroupInstance(bcomElem, task);
        }


        return frameList;
    }


    ITFFrameList createFrameList2(PenetrTask task)
    {
        task.scanInfo();
        if (task.isCompoundExistsInPlace || task.TFFormsIntersected.Count == 0)
            return null;

        BCOM.ModelReference taskModel =
            Addin.App.MdlGetModelReferenceFromModelRefP((long)task.modelRefP);

        var tfApi = new Bentley.Building.Api.TFApplicationList().AsTFApplication;

        double task_toUOR = taskModel.UORsPerMasterUnit;
        double task_subPerMaster = taskModel.SubUnitsPerMasterUnit;
        double task_unit3 = taskModel.UORsPerStorageUnit;
        double task_unit4 = taskModel.UORsPerSubUnit;

        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;

        double toUOR = activeModel.UORsPerMasterUnit;
        double subPerMaster = activeModel.SubUnitsPerMasterUnit;
        double unit3 = activeModel.UORsPerStorageUnit;
        double unit4 = activeModel.UORsPerSubUnit;

        // todo убрать от сюда поиск данных
        var res = penData.AsEnumerable().First(x =>
            (projId != 0 ? x.Field<long>("prjId") == projId :
                    x.Field<long>("depID") == depId) &&
            x.Field<long>("prjID") == projId &&
            x.Field<long>("flanNumber") == task.FlangesType &&
            x.Field<long>("diamNumber") == DiameterType.Parse(task.DiameterTypeStr).number);

        if (res == null)
        {
            return null;
        }

        double pipeInsideDiam = (res.Field<float>("pipeDiam").ToDouble() -
            res.Field<float>("pipeThick").ToDouble() * 2) * toUOR;
        pipeInsideDiam /= subPerMaster;

        double pipeOutsideDiam =
            res.Field<float>("pipeDiam").ToDouble() / subPerMaster * toUOR;

        double flangeInsideDiam = pipeOutsideDiam;
        double flangeOutsideDiam =
            res.Field<double>("flangeWidth") / subPerMaster * toUOR;
        double flangeThick = res.Field<double>("flangeThick") / subPerMaster * toUOR;

        double length = task.Length *task_subPerMaster; // * 10 / subPerMaster;
        
        double pipeInsideRadius = pipeInsideDiam / 2;
        double pipeOutsideRadius = pipeOutsideDiam / 2;
        
        //ITFBrepList brepList;
        //tfApi.CreateTFBrep(0, out brepList);

        DPoint3d origin = task.Location.ToDPoint();
        origin.ScaleInPlace(toUOR);

        var rot = task.Rotation;
        var matrix = DMatrix3d.Zero;
        // DMatrix3d.FromColumns(
        //rot.RowX.ToDVector(), rot.RowX.ToDVector(), rot.RowX.ToDVector());

        //ITFFormRecipeList recipeList;
        
        var model = Session.Instance.GetActiveDgnModel();
        var modelRef = Session.Instance.GetActiveDgnModelRef();


        //CellHeaderElement taskEl = Element.GetFromElementRef(task.elemRefP) as CellHeaderElement;
        //DPoint3d taskOrigin;
        //taskEl.GetSnapOrigin(out taskOrigin);

        var ellips = new EllipseElement(model, null, 
            DEllipse3d.FromCenterRadiusNormal(origin, pipeOutsideRadius, DVector3d.FromXY(1, 0)));
        
        var cone = new ConeElement(model, null, pipeOutsideRadius, pipeOutsideRadius,
            origin, DPoint3d.FromXYZ(origin.X + length, origin.Y, origin.Z), matrix, true);
        
        var ellips2 = new EllipseElement(model, null, DEllipse3d.FromCenterRadiusNormal(
                origin, pipeInsideRadius, DVector3d.FromXY(1, 0)));

        var cone2 = new ConeElement(model, null, pipeInsideRadius, pipeInsideRadius,
            DPoint3d.FromXYZ(origin.X, origin.Y, origin.Z),
            DPoint3d.FromXYZ(origin.X + length, origin.Y, origin.Z), matrix, true);

        //int status = brepList.InitCylinder(pipeInsideRadius*task_subPerMaster, 
        //    (length - flangeThick)*task_subPerMaster, ref origin,
        //    ref matrix, "");
            
        //ITFElementList elemList;
        //tfApi.CreateTFElement(0, out elemList);
        // Bentley.GeometryNET.Common.CircularCylinder

        

        //ITFBrepList coneBrepList, cone2BrepList, resBrepList;
        //tfApi.CreateTFBrep(0, out coneBrepList);
        //tfApi.CreateTFBrep(0, out cone2BrepList);
        //coneBrepList.InitFromElement(cone, modelRef, "");
        //cone2BrepList.InitFromElement(cone2, modelRef, "");
        
        //coneBrepList.AsTFBrep.InitCylinder(pipeInsideRadius, length, ref origin,
        //    ref matrix, 0);

        ITFItemList itemList;
        tfApi.CreateTFItem(0, out itemList);
        

        //var sweepDir = DVector3d.FromXY(1, 0);
        //coneBrepList.AsTFBrep.Drop(out resBrepList, cone2BrepList, 0);
        //sweepDir.NegateInPlace();
       // coneBrepList.AsTFBrep.Cut(out resBrepList, cone2BrepList,  ref sweepDir, length + 150, false, 0);
        //coneBrepList.AsTFBrep.SweepByVector3(ref sweepDir, length + 300, 
        //    pipeOutsideRadius - pipeInsideRadius, 0, 0);

        //Array arr = new System.Collections.ArrayList().ToArray();

        //coneBrepList.AsTFBrep.Cut2(out resBrepList, cone2BrepList.AsTFBrep, ref sweepDir,
        //Bentley.Building.Api.TFdBrepCutMethod.tfdBrepCutMethod_Outside,
        //Bentley.Building.Api.TFdBrepCutDirection.tfdBrepCutDirection_Both,
        //Bentley.Building.Api.TFdBrepCutDepth.tfdBrepCutDepth_UpToSolid, length,
        //arr, 0, false, Bentley.Building.Api.TFdBrepCutDepth.tfdBrepCutDepth_UpToSolid, length,
        //arr, 0, false, 0, 0, 0.00005, 0);

        //lement resElement;
        //resBrepList.GetElement(out resElement, 0, "");
        //coneBrepList.GetElement(out resElement, 0, "");

        ITFFrameList frameList;
        tfApi.CreateTFFrame(0, out frameList);
        frameList.AsTFFrame.Add3DElement(cone, 0);
        //frameList.AsTFFrame.Add3DElement(cone2, 0);
        //frameList.AsTFFrame.Add3DElement(resElement, 0);    
        
        //ITFFrameList openingFrameList;
        //tfApi.CreateTFFrame(0, out openingFrameList);
        //openingFrameList.AsTFFrame.Add3DElement(cone2, 0);

        //ITFFormRecipeList openRecipeList;
        ////tfApi.CreateTFFormRecipeArc
        //ITFFormRecipe openRecipe;
        //openingFrameList.AsTFFrame.GetFormRecipeList(0, out openRecipeList);
        //openRecipe = openRecipeList.AsTFFormRecipe;

        //ITFItemList featureList;
        //frameList.AsTFFrame.AddOpeningsToForm(out featureList, ref openRecipe, "", 0);


        ITFPerforatorList perfoList;
        tfApi.CreateTFPerforator(0, out perfoList);
        var dir = DVector3d.FromXY(1, 0);
        var tran = DTransform3d.Identity;
        perfoList.InitFromElement(ellips, ref dir, length, ref tran, "");
        perfoList.AsTFPerforator.SetIsVisible(false, 0);
        perfoList.SetSweepMode(Bentley.Building.Api.TFdPerforatorSweepMode.tfdPerforatorSweepModeBi, "");
        perfoList.SetPolicy(Bentley.Building.Api.TFdPerforatorPolicy.tfdPerforatorPolicyThroughHoleWithinSenseDist, "");
        

        frameList.AsTFFrame.SetPerforatorList(ref perfoList, 0);
        frameList.AsTFFrame.SetSenseDistance2(length/100, 0);
        frameList.AsTFFrame.SetPerforatorsAreActive(true, 0);

                
        int stat = tfApi.ModelReferenceAddFrameList(modelRef, ref frameList, 0); 
        var frame = frameList.AsTFFrame;

        stat = tfApi.ModelReferenceUpdateAutoOpeningsByFrame(modelRef,
            ref frame, true, false, Bentley.Building.Api.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone, 0); 


        //Element cylindr;
        //brepList.GetElement(out cylindr, 0, "");        

        //cylindr.AddToModel();

        //Element tfElement;
        //Element perfo = null;
        //Element dp_2d = null;
        //int value;
        //tfApi.ModelReferenceAddElement(ref cylindr, Session.Instance.GetActiveDgnModel(),
        //    0, 0, out value);        

        //tfApi.ModelReferenceConstructFrameElement(Session.Instance.GetActiveDgnModel(),
        //    0, ref cylindr, ref perfo, ref origin, ref dp_2d, "name", null,
        //    1, 0, false, null, false, task.Length, 0.00005, 0, out tfElement);

        //frameList.InitFromElement(cylindr, "");

       // frameList.AsTFFrame.Add3DElement(cylindr, 0);
                
        frameList.Synchronize("");       
        //tfApi.ModelReferenceAddFrameList(modelRef, ref frameList, 0);
        return frameList;
    }

    private static void setDataGroupInstance(
        BCOM.Element bcomElement, PenetrTask task)
    {
        //BCOM.Element bcomElement = frameList.AsTFFrame.Get3DElement();

        Element element = Element.GetFromElementRefAndModelRef(
            (IntPtr)bcomElement.MdlElementRef(), 
            (IntPtr)bcomElement.ModelReference.MdlModelRefP());

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
            foreach (DataGroupProperty property in catalogEditHandle.Properties)
            {
                if (property?.Xpath == "EmbPart/@PartCode") 
                {
                    catalogEditHandle.SetValue(property, task.Code);
                }
                else if (property?.Xpath == "EmbPart/@CatalogName")
                {
                    catalogEditHandle.SetValue(property, task.Name);
                }
            }
            catalogEditHandle.Rewrite(0);
        }
    }
        
    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
