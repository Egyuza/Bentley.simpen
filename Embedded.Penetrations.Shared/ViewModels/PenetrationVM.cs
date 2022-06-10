using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Embedded.Penetrations.UI;

using Shared.Bentley;
using Shared;
using System.Data;

namespace Embedded.Penetrations.Shared
{
public class PenetrationVM : BentleyInteropBase
{
    private static PenetrationVM instance_;    
    private static bool isDebugMode_;

#if V8i
    private static Bentley.MicroStation.AddIn addin_;

    public static PenetrationVM getInstance(
        Bentley.MicroStation.AddIn addin, KeyinOptions options)
    {
        addin_ = addin;
        return loadInstace(new GroupByTaskModel(addin), options);
    }
#elif CONNECT
        private static Bentley.MstnPlatformNET.AddIn addin_;
    public static PenetrationVM getInstance(
        Bentley.MstnPlatformNET.AddIn addin, KeyinOptions options)
    {
        addin_ = addin;
        return loadInstace(new GroupByTaskModel(addin), options);
    }
#endif

    private static PenetrationVM loadInstace(
        GroupByTaskModel penModel, KeyinOptions options)
    {
        isDebugMode_ = options.IsDebug;

        if (PenConfigVariables.LogFolder.IsDefined)
        {
            Logger.setLogFolder(PenConfigVariables.LogFolder.Value);
        }
        if (PenConfigVariables.Log.IsDefined)
        {
            Logger.IsActive = bool.Parse(PenConfigVariables.Log.Value);
        }

        Logger.Log.Info(new string('=', 22) + " LOAD " + new string('=', 22));

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
        WindowHelper.show(form_);
    }

    private void initializeForm()
    {
        try
        {
            initialize_();
        }
        catch (Exception ex)
        {
            ex.Alert();
        }
    }

    private void initialize_()
    {
        if (form_ != null && !form_.IsDisposed) {
            return;
        }
        form_ = new PenetrationForm();
        form_.setAction_ShowErrorMessage(showErrorMessage_);

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

        form_.dgvCreationTasks.AutoGenerateColumns = false;
        form_.dgvCreationTasks.AutoSizeColumnsMode = 
            DataGridViewAutoSizeColumnsMode.AllCells;

        form_.setColumns(getColumns_(groupModel_.TaskTable));
        form_.setDataSource(groupModel_.TaskTable);

        groupModel_.AttrsXDoc = null;
        form_.setAction_LoadXmlAttrs(groupModel_.loadXmlAttrs);        

        form_.setBinding("lblSelectionCount", "Text",
            groupModel_, nameof(groupModel_.SelectionCount), 
            BindinUpdateMode.ControlOnly);

        form_.setAction_DataRowsAdded(rowsAdded_);
        form_.setAction_Create(addToModel_);
        form_.setAction_Preview(preview_);
        form_.setAction_OnCloseForm (groupModel_.clearContext);

        form_.setAction_StartPrimitive(singleModel_.startPrimitive);
        form_.setAction_StartDefault(singleModel_.startDefaultCommand);

        form_.setAction_ScanForUpdate(updateModel_.scanForUpdate);
        form_.setAction_Update(updateModel_.runUpdate);
        form_.setAction_UpdateNodeDoubleClick(updateModel_.updateNodeDoubleClick);

        form_.dgvCreationTasks.SelectionChanged += DgvCreationTasks_SelectionChanged;
        form_.dgvCreationTasks.CellMouseDoubleClick += DgvCreationTasks_CellMouseDoubleClick;

        form_.dgvCreationTasks.ShowRowErrors = true;
        form_.dgvCreationTasks.ReadOnly = true;
        form_.setAction_SetReadOnly(setReadOnly_);
        ensureReadOnlyColumns();

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

        form_.setAction_SingleSelecteFlangeType(singleModel_.setFlangeType);
        form_.setAction_SingleSelecteDiameterType(singleModel_.setDiameterType);
        form_.setAction_SingleSelecteLength(singleModel_.setLength);

        form_.cbxDiameter.DataSource = singleModel_.CurrentDiameters;
        form_.cbxFlanges.DataSource = 
            PenetrDataSource.Instance.getFlangeNumbersSort();
        form_.cbxFlanges.SelectedIndex = form_.cbxFlanges.Items.Count > 0 ? 0 : -1;
        form_.cbxDiameter.SelectedIndex = form_.cbxDiameter.Items.Count > 0 ? 0 : - 1;
                
        #if DEBUG
            // form_.tabControl1.TabPages.RemoveAt(0); // TODO временно до отладки и ввода в работу        
        #endif
    }

    private void showErrorMessage_(Exception ex)
    {
        ex.AddToMessageCenter();
    }

    private void ensureReadOnlyColumns()
    {
        var columns = form_.dgvCreationTasks.Columns;
        columns[GroupByTaskModel.FieldName.STATUS].ReadOnly = 
        columns[GroupByTaskModel.PropKey.NAME].ReadOnly = true;
    }

    private void setReadOnly_(bool readOnly)
    {
        form_.dgvCreationTasks.ReadOnly = readOnly;
        ensureReadOnlyColumns();
    }

    private void DgvCreationTasks_SelectionChanged(object sender, EventArgs e)
    {
        var selection = new List<DataRow>();
        foreach(DataGridViewRow row in form_.dgvCreationTasks.SelectedRows)
        {
            DataRow taskRow = row.GetDataRow();
            if (taskRow != null)
            {
                selection.Add(taskRow);
            }
        }
        groupModel_.changeSelection(selection);
    }
    private void DgvCreationTasks_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        DataGridViewRow dgvRow = form_.dgvCreationTasks.Rows[e.RowIndex];
        groupModel_.focusTaskElement(dgvRow.GetDataRow());
    }

    private void preview_()
    {
        groupModel_.preview();
        UpdateRowsStylesByStatus();
    }

    private void addToModel_()
    {
        groupModel_.addToModel();
        UpdateRowsStylesByStatus();
    }

    private void rowsAdded_(IEnumerable<DataGridViewRow> rows)
    {
        foreach (var row in rows)
        {
            var comboCell = row.Cells[GroupByTaskModel.FieldName.DIAMETER] as DataGridViewComboBoxCell;

            PenetrVueTask task = groupModel_.GetTask(row.GetDataRow());
            if (task == null)
                continue;

            IList<DiameterType> diameters = groupModel_.getDiametersList(task);
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

            DataRow dataRow = row.GetDataRow();
            if (!dataRow.HasErrors && !dataRow.AnyColumnHasError())
            {
                dataRow.SetField(GroupByTaskModel.FieldName.STATUS, "OK");
            }
            else
            {
                dataRow.SetField(GroupByTaskModel.FieldName.STATUS, "ERROR");
            }
        }
        UpdateRowsStylesByStatus();

        try
        {
            var tabCtrl = form_.tabCtrlMain;
            if (tabCtrl.SelectedTab.Name != "tabGroupByTask")
            {
                tabCtrl.SelectedTab = tabCtrl.TabPages["tabGroupByTask"];
                form_.Refresh();
            }
        }
        catch (Exception)
        {
        }
    }

    private void UpdateRowsStylesByStatus()
    {
        foreach (DataGridViewRow dgvRow in form_.dgvCreationTasks.Rows)
        {
            DataRow dataRow = dgvRow.GetDataRow();
            var cellStyle = dgvRow.DefaultCellStyle;

            string status = dataRow.Field<string>(
                GroupByTaskModel.FieldName.STATUS)?.ToUpper();

            switch (status)
            {
            case "OK":
                cellStyle.BackColor = System.Drawing.Color.White;
                break;
            case "DONE":
                cellStyle.BackColor = System.Drawing.Color.YellowGreen;
                break;
            case "ERROR":
                cellStyle.BackColor = System.Drawing.Color.Coral; break;
            case "WARN":
                cellStyle.BackColor = System.Drawing.Color.LightYellow; break;
            }
        }
    }

    private IEnumerable<DataGridViewColumn> getColumns_(DataTable dataTable)
    {
        var columns = new List<DataGridViewColumn>();

        foreach(DataColumn dataColumn in dataTable.Columns)
        {
            DataGridViewColumn column;

            switch (dataColumn.ColumnName)
            {
            case GroupByTaskModel.FieldName.FLANGES:
                var cmboxColumn = new DataGridViewComboBoxColumn();
                cmboxColumn.DataSource = groupModel_.getFlangeNumbersSort();
                column = cmboxColumn;
                break;
            case GroupByTaskModel.FieldName.DIAMETER:
                column = new DataGridViewComboBoxColumn();
                break;
            default:
                column = new DataGridViewTextBoxColumn();
                break;
            }
            column.DataPropertyName = dataColumn.ColumnName;
            column.Name = dataColumn.ColumnName;
            columns.Add(column);
        }
        return columns;
    }

    private GroupByTaskModel groupModel_;
    private UpdateModel updateModel_;
    private SingleModel singleModel_;
    private PenetrationForm form_;

}

//static class ColumnName
//{
//    public static readonly string CODE = "KKS код";
//    public static readonly string TYPE_SIZE = "Типоразмер";
//    public static readonly string FLANGES = "Фланцы";
//    public static readonly string DIAMETER = "Диаметр";
//    public static readonly string LENGTH = "Длина(см)";
//    public static readonly string REF_POINT1 = "RefPoint1";
//    public static readonly string REF_POINT2 = "RefPoint2";
//    public static readonly string REF_POINT3 = "RefPoint3";
//}

}
