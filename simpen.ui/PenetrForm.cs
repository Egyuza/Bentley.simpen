using System;
using System.Windows.Forms;
using System.Data.SqlClient;

using Bentley.Internal.MicroStation.Elements;
using Bentley.MicroStation;
using System.Collections.Generic;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

using System.Data;

using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace simpen.ui
{

public partial class PenetrForm : Form
{

    //[DllImport("stdmdlbltin.dll")]
    //public static extern int mdlCnv_masterToUOR(ref double uors, double masterUnits, int modelRef);

    //[DllImport("stdmdlbltin.dll")]
    //public static extern int mdlCnv_UORToMaster(ref double masterUnits, double uors, int modelRef);



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
        column.DataPropertyName = "FlangesType";
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
        //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
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
        
        selectionTranContainer = Addin.App.CreateTransientElementContainer1(
            null, 
            BCOM.MsdTransientFlags.DisplayFirst |
            BCOM.MsdTransientFlags.Overlay,
            (BCOM.MsdViewMask)ViewHelper.getActiveViewIndex(), 
            BCOM.MsdDrawingMode.Temporary);

        previewTranContainer = Addin.App.CreateTransientElementContainer1(
            null, 
            BCOM.MsdTransientFlags.DisplayFirst |
            BCOM.MsdTransientFlags.Overlay | BCOM.MsdTransientFlags.Snappable,
            (BCOM.MsdViewMask)ViewHelper.getActiveViewIndex(), 
            BCOM.MsdDrawingMode.Temporary);


        { // TODO восстанавливаем сохранённые настройки:
            
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

            // TODO рисовать кубик range штриховкой под цвет выделения...
            // !!! учесть, что задание может быть в референсе !!!

            List<long> itemsIds = new List<long> {task.elemId};
            // добавляем фланцы:
            foreach (PenetrTaskFlange flangeTask in task.Flanges) 
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
                previewTranContainer.Reset();
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
            // todo обработать
            MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void readDatabaseData()
    {
        BCOM.Workspace wspace = Addin.App.ActiveWorkspace;

        string server = wspace.IsConfigurationVariableDefined("AEP_SAVRD_SERVER") ?
            wspace.ConfigurationVariableValue("AEP_SAVRD_SERVER") : "vibe1";
            
        string db = wspace.IsConfigurationVariableDefined("AEP_SAVRD_BASE") ?
            wspace.ConfigurationVariableValue("AEP_SAVRD_BASE") : "parts";
            
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

            projId = wspace.IsConfigurationVariableDefined("EMBDB_PROJECT_ID") ?
                long.Parse(wspace.ConfigurationVariableValue("EMBDB_PROJECT_ID")) : 
                0; // offtake project id
                // 0 - no project
          
            penData.Clear();
            using (SqlDataReader reader = 
                new SqlCommand("select * from view_pendiam2", connection).ExecuteReader())
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

        foreach (TFCOM.TFFrameList frameList in getFrames())
        {
            previewTranContainer.AppendCopyOfElement(
                frameList.AsTFFrame.Get3DElement());
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

        var activeModel = Addin.App.ActiveModelReference;
        foreach (TFCOM.TFFrameList frameList in getFrames())
        {            
            {
                // ! без этого кода не срабатывает перфорация в стенке/плите
                // судя по всему инициализирует обновление объектов, с которыми
                // взаимодействует frame
            
                Addin.AppTF.ModelReferenceUpdateAutoOpeningsByFrame(
                    activeModel, frameList.AsTFFrame, true, false, 
                    TFCOM.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone);
            }

            Addin.AppTF.ModelReferenceAddFrameList(
                Addin.App.ActiveModelReference, frameList);
            
            //TFCOM.TFFrameList outFrameList;

            //var elem = activeModel.GetLastValidGraphicalElement();          
            //int elPos = elem.FilePosition;

            //Addin.AppTF.ModelReferenceReadFrameListToMasterByFilePos(
            //    activeModel, out outFrameList, elPos); 

            //outFrameList.AsTFFrame.SetPerforatorsAreActive(false);

            //Addin.AppTF.ModelReferenceRewriteFrameList(
            //    Addin.App.ActiveModelReference, outFrameList);

            

            //Addin.AppTF.ModelReferenceReadFrameListToMasterByFilePos(
            //    activeModel, out outFrameList, elPos);

            //outFrameList.AsTFFrame.SetPerforatorsAreActive(true);

            //Addin.AppTF.ModelReferenceRewriteFrameList(
            //    Addin.App.ActiveModelReference, outFrameList);
        }
      
    }

    List<TFCOM.TFFrameList> getFrames()
    {
        var res = new List<TFCOM.TFFrameList>();
        
        foreach (DataGridViewRow row in dgvFields.Rows)
        {
            PenetrTask task = (PenetrTask)row.DataBoundItem;
            TFCOM.TFFrameList frameList = createFrameList(task);

            if (frameList != null) // TODO исключение?
            {
                res.Add(frameList);
            }            
        }    

        return res;
    }

    TFCOM.TFFrameList createFrameList(PenetrTask task)
    {
        task.scanCollisions();        
        if (task.isCompoundExistsInPlace) 
            return null;

        var res = penData.AsEnumerable().First(x => 
        x.Field<long>("prjID") == projId &&
        x.Field<long>("flanNumber") == task.FlangesType &&
        x.Field<long>("diamNumber") == task.Diametr);

        if (res == null) {
            return null;
        }

        BCOM.ModelReference taskModel = 
            Addin.App.MdlGetModelReferenceFromModelRefP((int)task.modelRefP);

        double task_toUOR = taskModel.UORsPerMasterUnit;
        double task_subPerMaster = taskModel.SubUnitsPerMasterUnit;
        double task_unit3 = taskModel.UORsPerStorageUnit;
        double task_unit4 = taskModel.UORsPerSubUnit;

        BCOM.ModelReference activeModel = Addin.App.ActiveModelReference;               

        double toUOR = activeModel.UORsPerMasterUnit;
        double subPerMaster = activeModel.SubUnitsPerMasterUnit;
        double unit3 = activeModel.UORsPerStorageUnit;
        double unit4 = activeModel.UORsPerSubUnit;
        
        double pipeInsideDiam = res.Field<float>("pipeDiam") - 
            res.Field<float>("pipeThick") * 2;
        pipeInsideDiam /= subPerMaster;

        double pipeOutsideDiam = res.Field<float>("pipeDiam")/subPerMaster;

        double flangeInsideDiam = (res.Field<float>("pipeDiam") - 
            res.Field<float>("pipeThick") * 2)/subPerMaster;

        double flangeOutsideDiam = res.Field<double>("flangeWidth")/subPerMaster;
        double flangeThick = res.Field<double>("flangeThick")/subPerMaster;

        double length = task.Length *10 / subPerMaster;

        //BCOM.Point3d origin = task.Location;
        //BCOM.Point3d vec = Addin.App.Point3dFromXYZ(0, 0, 1);        
        //BCOM.Point3d topCenter = Addin.App.Point3dAddScaled(origin, vec, length );

        //BCOM.Matrix3d rot = Addin.App.Matrix3dIdentity();
        //BCOM.EllipseElement ellips = Addin.App.CreateEllipseElement2(null, 
        //    origin,  pipeInsideDiam/2, pipeInsideDiam/2, rot );

        //ellips.Color = 0;
        //transientContainer.AppendCopyOfElement(ellips);

        //BCOM.ConeElement cone = Addin.App.CreateConeElement2(null, 
        //    pipeInsideDiam/2, origin, topCenter);
        //cone.Color = 0;

        var solids = Addin.App.SmartSolid;

        BCOM.SmartSolidElement cylindrInside =
            solids.CreateCylinder(null, pipeInsideDiam / 2, length);

        BCOM.SmartSolidElement cylindrOutside =
            solids.CreateCylinder(null, pipeOutsideDiam / 2, length);
        
        var cylindr = solids.SolidSubtract(cylindrOutside, cylindrInside);

        var elements = new Dictionary<BCOM.SmartSolidElement, double>();
        elements.Add(cylindr, length/2);

        for (int i = 0; i < task.FlangesCount; ++i)
        {
            BCOM.SmartSolidElement flangeCylindr = solids.SolidSubtract(
                solids.CreateCylinder(null, flangeOutsideDiam / 2, flangeThick), 
                solids.CreateCylinder(null, pipeOutsideDiam / 2, flangeThick));            
            
            double shift = 0;
            if (task.FlangesCount == 1)
            {
                shift = Addin.App.Vector3dEqualTolerance(task.singleFlangeSide,
                    Addin.App.Vector3dFromXY(0, 1), 0.001) ? 
                        0.0    + flangeThick / 2:
                        length - flangeThick / 2;
            }
            else
            {
                shift = i == 0 ? 0.0 : length;
                shift += Math.Pow(-1, i) * flangeThick / 2;
            }

            elements.Add(flangeCylindr, shift);
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

        BCOM.Point3d taskLocation =  taskModel.IsAttachment ? 
            Addin.App.Point3dScale(task.Location, task_subPerMaster) :
            task.Location;

        TFCOM.TFFrameList frameList = Addin.AppTF.CreateTFFrame();
        foreach (BCOM.SmartSolidElement elem in elements.Keys)
        {
            elem.Color = 0; // TODO
            elem.Rotate(Addin.App.Point3dZero(), Math.PI/2, 0, 0);

            BCOM.Point3d offset = Addin.App.Point3dAddScaled(
                Addin.App.Point3dZero(), Addin.App.Point3dFromXYZ(0, 1, 0), elements[elem]);
            elem.Move(offset);
                        
            elem.Transform(taskTran);
            elem.Move(taskLocation);
            //if (taskModel == activeModel)
            //    elem.Move(task.Location);
            //else
            //{
            //    elem.Move(
            //        Addin.App.Point3dScale(task.Location, task_subPerMaster));
            //}

            frameList.AsTFFrame.Add3DElement(elem);
        }
   
        // ПЕРФОРАТОР
        BCOM.EllipseElement perfoEl = 
            Addin.App.CreateEllipseElement2(null, Addin.App.Point3dZero(), 
                pipeInsideDiam/2, pipeInsideDiam/2, 
                Addin.App.Matrix3dFromVectorAndRotationAngle(
                    Addin.App.Point3dFromXY(1, 0), Math.PI/2), 
                BCOM.MsdFillMode.Filled);
        {
            BCOM.Point3d offset = Addin.App.Point3dAddScaled(
                Addin.App.Point3dZero(), 
                Addin.App.Point3dFromXYZ(0, 1, 0), length/2);
            perfoEl.Move(offset);
        }
        perfoEl.Transform(taskTran);
        perfoEl.Move(taskLocation);      

        BCOM.Point3d perfoVec = Addin.App.Point3dFromXY(0,1);

        TFCOM.TFPerforatorList perfoList = Addin.AppTF.CreateTFPerforator();
        perfoList.InitFromElement(perfoEl, perfoVec, length/2 * 1.01, taskTran);
        perfoList.SetSweepMode(
            TFCOM.TFdPerforatorSweepMode.tfdPerforatorSweepModeBi);
        //perfoList.SetSenseDist(1.01 * length / 2);
        perfoList.SetPolicy(
            TFCOM.TFdPerforatorPolicy.tfdPerforatorPolicyThroughHoleWithinSenseDist);
        
        frameList.AsTFFrame.SetPerforatorList(perfoList);
        frameList.AsTFFrame.SetSenseDistance2(length/2);
        frameList.AsTFFrame.SetName("Penetration"); // ранее было 'EmbeddedPart'
        frameList.AsTFFrame.SetPerforatorsAreActive(true);
        frameList.Synchronize();

        return frameList;
    }

}
}
