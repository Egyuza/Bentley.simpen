using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Embedded.Penetrations.UI;

using Shared.Bentley;
using Shared;

namespace Embedded.Penetrations.Shared
{
public class PenetrationVM : BentleyInteropBase
{
    private static PenetrationVM instance_;    
    private static bool isDebugMode_;

#if V8i
    private static Bentley.MicroStation.AddIn addin_;

    public static PenetrationVM getInstance(
        Bentley.MicroStation.AddIn addin, string unparsed)
    {
        addin_ = addin;
        return loadInstace(new GroupByTaskModel(addin), unparsed);
    }
#elif CONNECT
    private static Bentley.MstnPlatformNET.AddIn addin_;
    public static PenetrationVM getInstance(
        Bentley.MstnPlatformNET.AddIn addin, string unparsed)
    {
        addin_ = addin;
        return loadInstace(new GroupByTaskModel(addin), unparsed);
    }
#endif

    private static PenetrationVM loadInstace(
        GroupByTaskModel penModel, string unparsed)
    {
        var options = new List<string> (unparsed?.ToUpper().Split(' '));

        var wspace = App.ActiveWorkspace;
        if (wspace.IsConfigurationVariableDefined("AEP_EMB_PEN_LOG_FOLDER"))
        {
            Logger.setLogFolder(
                wspace.ConfigurationVariableValue("AEP_EMB_PEN_LOG_FOLDER"));
        }
        if (wspace.IsConfigurationVariableDefined("AEP_EMB_PEN_LOG"))
        {
            Logger.IsActive = bool.Parse(
                wspace.ConfigurationVariableValue("AEP_EMB_PEN_LOG"));
        }
        // доп.
        if (options.Contains("LOG"))
        {
            Logger.IsActive = true;
        }

        Logger.Log.Info(new string('=', 22) + " LOAD " + new string('=', 22));

        isDebugMode_ = options.Contains("DEBUG");
        if (isDebugMode_)
        {
            Logger.Log.Info("запущено в DEBUG режиме");
        }        

        instance_ = instance_ ?? new PenetrationVM(penModel);
        instance_.loadContext();
        return instance_;
    }


    public void loadContext()
    {
        groupModel_.loadContext();
    }

    private PenetrationVM(GroupByTaskModel model)
    {
        groupModel_ = model;
        groupModel_.PropertyChanged += PenModel__PropertyChanged;

        updateModel_ = new UpdateModel();
        singleModel_ = new SingleModel();

        initializeForm();
    }

    private void PenModel__PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var pname = e.PropertyName;
    }
    
    public void showForm()
    {
        initializeForm();
        WindowHelper.show(form_, "simpen::Penetration");
    }

    private void initializeForm()
    {
        if (form_ != null && !form_.IsDisposed) {
            return;
        }
        form_ = new PenetrationForm();
        form_.Text = $"Проходки v{AssemblyVersion.VersionStr}" 
            + (isDebugMode_ ? " [DEBUG]" : string.Empty);

        if (groupModel_.isProjectDefined)
        {
            // TODO определять имя проекта по его id
            form_.setStatusProject(groupModel_.ProjectId.ToString());
        }
        else
        {
            form_.setStatusProject(null);

            if (isDebugMode_)
            {
                form_.setStatusText("создание проходок доступно в DEBUG режиме");
            }
            else
            {
                form_.setStatusText("создание проходок не доступно");
                form_.setReadOnly();
            }
        }

        form_.setColumns(getColumns_());

        form_.setDataSource_Create(groupModel_.TaskSelection);
        form_.setBinding("lblSelectionCount", "Text",
            groupModel_, nameof(groupModel_.SelectionCount), 
            BindinUpdateMode.ControlOnly);

        form_.setDataRowsAddedAction(rowsAdded);
        form_.setPreviewAction(groupModel_.preview);
        form_.setCreateAction(groupModel_.addToModel);
        form_.setOnCloseFormAction (groupModel_.clearContext);


        form_.setStartPrimitiveAction(singleModel_.startPrimitive);
        form_.setStartDefaultAction(singleModel_.startDefaultCommand);

        form_.setScanForUpdateAction(updateModel_.scanForUpdate);
        form_.setUpdateAction(updateModel_.runUpdate);
        form_.setUpdateNodeDoubleClickAction(updateModel_.updateNodeDoubleClick);

        ///
        /// Single =========================
        ///
        var userTask = singleModel_.UserTask;

        form_.btnStartPrimitive.setBinding("Enabled",
            singleModel_, nameof(singleModel_.IsCodeValid), BindinUpdateMode.ControlOnly);

        form_.txtKKS.setBinding("Text",
            singleModel_, nameof(singleModel_.Code), BindinUpdateMode.SourceOnly);

        form_.txtTypeSize.setBinding("Text",
            singleModel_, nameof(singleModel_.TypeSize), BindinUpdateMode.ControlOnly); 

        form_.txtRefPoint.setBinding("Text",
            singleModel_, nameof(singleModel_.RefPointString), BindinUpdateMode.ControlOnly);

        form_.txtLength.setBinding("Text",
            singleModel_, nameof(singleModel_.LengthCm));

        form_.chbxAutoLength.setBinding("Checked",
            userTask, nameof(userTask.IsAutoLength), BindinUpdateMode.SourceOnly);

        form_.chbxManualRotate.setBinding("Checked",
            userTask, nameof(userTask.IsManualRotateMode), BindinUpdateMode.SourceOnly);

        form_.numAngleX.setBinding("Value", userTask, nameof(userTask.userAngleX));
        form_.numAngleY.setBinding("Value", userTask, nameof(userTask.userAngleY));
        form_.numAngleZ.setBinding("Value", userTask, nameof(userTask.userAngleZ));

        form_.setSingleSelecteFlangeType(singleModel_.setFlangeType);
        form_.setSingleSelecteDiameterType(singleModel_.setDiameterType);
        form_.setSingleSelecteLength(singleModel_.setLength);

        form_.cbxDiameter.DataSource = singleModel_.CurrentDiameters;
        form_.cbxFlanges.DataSource = 
            PenetrDataSource.Instance.getFlangeNumbersSort();
        form_.cbxFlanges.SelectedIndex = 0;
        form_.cbxDiameter.SelectedIndex = 0;

        form_.dgvCreationTasks.SelectionChanged += DgvCreationTasks_SelectionChanged;
        form_.dgvCreationTasks.CellMouseDoubleClick += DgvCreationTasks_CellMouseDoubleClick;
                
        #if DEBUG
            // form_.tabControl1.TabPages.RemoveAt(0); // TODO временно до отладки и ввода в работу        
        #endif
    }

    private void DgvCreationTasks_SelectionChanged(object sender, EventArgs e)
    {
        var selection = new List<PenetrVueTask>();
        foreach(DataGridViewRow row in form_.dgvCreationTasks.SelectedRows)
        {
            var task = (PenetrVueTask)row.DataBoundItem;
            if (task != null)
            {
                selection.Add(task);
            }
        }
        groupModel_.changeSelection(selection);
    }
    private void DgvCreationTasks_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        PenetrVueTask task = 
            (PenetrVueTask)form_.dgvCreationTasks.Rows[e.RowIndex].DataBoundItem;

        groupModel_.focusTaskElement(task);
    }

    private void rowsAdded(IEnumerable<DataGridViewRow> rows)
    {
        foreach (var row in rows)
        {
            var comboCell = 
                row.Cells[ColumnName.DIAMETER] as DataGridViewComboBoxCell;

            PenetrVueTask task = (PenetrVueTask)row.DataBoundItem;

            var diameters = groupModel_.getDiametersList(task);
            var diamStrList = new List<string>();
            DiameterType matchValue = null;
            foreach (DiameterType diamType in diameters)
            {        
                if (diamType.Equals(task.DiameterType))
                {
                    matchValue = diamType;
                    Logger.Log.Debug(
                        $"Найден диаметр в таблице диаметров: {diamType}" + 
                        $" для задания '{task}'");
                }
                diamStrList.Add(diamType.ToString());
            }
            
            comboCell.DataSource = diamStrList;
            if (matchValue != null) {
                Logger.Log.Debug($"Установка диаметра: {matchValue}");
                comboCell.Value = matchValue.ToString();
            }
            else {
                Logger.Log.Debug(
                    $"невалиное значение диаметра '{task.DiameterType}'" + 
                    $" для задания '{task}'" + 
                    $"\n валидные занчения: {diameters.ToStringEx()}");                

                comboCell.ErrorText = "невалидное значение диаметра";
            }
        }
    }

    private IEnumerable<DataGridViewColumn> getColumns_()
    {
        var columns = new List<DataGridViewColumn>();
        
        DataGridViewColumn column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = "Code";
        column.Name = ColumnName.CODE;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        columns.Add(column);
        
        column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = "Name";
        column.Name = ColumnName.TYPE_SIZE;
        column.ReadOnly = true;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        columns.Add(column);

        var cmboxColumn = new DataGridViewComboBoxColumn();
        column = cmboxColumn; // НВС
        cmboxColumn.DataPropertyName = "FlangesType";
        cmboxColumn.Name = ColumnName.FLANGES;
        cmboxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        cmboxColumn.DataSource = groupModel_.getFlangeNumbersSort();
        columns.Add(cmboxColumn);
        
        column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "DiameterTypeStr";
        column.Name = ColumnName.DIAMETER;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        columns.Add(column);
        
        column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = "Length";
        column.Name = ColumnName.LENGTH;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        columns.Add(column);

        column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = 
        column.Name = ColumnName.REF_POINT1;
        column.ReadOnly = true;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        columns.Add(column);

        //column = new DataGridViewTextBoxColumn();
        //column.DataPropertyName = "Test.First().Value";
        //column.Name = "new";
        //column.ReadOnly = true;
        //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        //columns.Add(column);

        return columns;
    }

    private GroupByTaskModel groupModel_;
    private UpdateModel updateModel_;
    private SingleModel singleModel_;
    private PenetrationForm form_;

}

static class ColumnName
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

}
