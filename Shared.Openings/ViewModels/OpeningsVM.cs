using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;


using Shared.Bentley;
using Shared;
using Embedded.Openings.Shared.Models;
using Embedded.Openings.UI;
using System.Data;

namespace Embedded.Openings.Shared.ViewModels
{
public class OpeningsVM : BentleyInteropBase
{
    private static OpeningsVM instance_;    
    private static bool isDebugMode_;

#if V8i
    private static Bentley.MicroStation.AddIn addin_;

    public static OpeningsVM getInstance(
        Bentley.MicroStation.AddIn addin, string unparsed)
    {
        addin_ = addin;
        return loadInstace(unparsed);
    }
#elif CONNECT
    private static Bentley.MstnPlatformNET.AddIn addin_;
    public static PenetrationVM getInstance(
        Bentley.MstnPlatformNET.AddIn addin, string unparsed)
    {
        addin_ = addin;
        return loadInstace(unparsed);
    }
#endif

    private static OpeningsVM loadInstace(string unparsed)
    {
        var options = new List<string> (unparsed?.ToUpper().Split(' '));

        var wspace = App.ActiveWorkspace;
        if (wspace.IsConfigurationVariableDefined("AEP_EMB_OPENINGS_LOG_FOLDER"))
        {
            Logger.setLogFolder(
                wspace.ConfigurationVariableValue("AEP_EMB_OPENINGS_LOG_FOLDER"));
        }
        if (wspace.IsConfigurationVariableDefined("AEP_EMB_OPENINGS_LOG"))
        {
            Logger.IsActive = bool.Parse(
                wspace.ConfigurationVariableValue("AEP_EMB_OPENINGS_LOG"));
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

        instance_ = instance_ ?? new OpeningsVM();
        instance_.loadContext();
        return instance_;
    }


    public void loadContext()
    {
        groupModel_.loadContext();
    }

    private OpeningsVM()
    {
        groupModel_ = new GroupByTaskModel(addin_);
        groupModel_.PropertyChanged += GroupModel__PropertyChanged;

        initializeForm();
    }

    private void GroupModel__PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var pname = e.PropertyName;
    }
    
    public void showForm()
    {
        initializeForm();
        WindowHelper.show(form_, "Embedded::Openings");
    }

    private void initializeForm()
    {
        if (form_ != null && !form_.IsDisposed) {
            return;
        }
        form_ = new OpeningForm();

        form_.tabCtrlMain.TabPages.RemoveByKey("tabCreate");
        form_.tabCtrlMain.TabPages.RemoveByKey("tabUpdate");

        form_.Text = $"Проёмы v{AssemblyVersion.VersionStr}" 
            + (isDebugMode_ ? " [DEBUG]" : string.Empty);

        form_.setOnCloseFormAction (groupModel_.clearContext);
        form_.setDataSource_Create(groupModel_.TaskTable);

            //form_.setDataRowsAddedAction(rowsAdded);
        form_.setPreviewAction(groupModel_.preview);
        form_.setCreateAction(groupModel_.addToModel);

            //form_.dgvCreationTasks.SelectionChanged += DgvCreationTasks_SelectionChanged;
            //form_.dgvCreationTasks.CellMouseDoubleClick += DgvCreationTasks_CellMouseDoubleClick;

        #if DEBUG
            // form_.tabControl1.TabPages.RemoveAt(0); // TODO временно до отладки и ввода в работу        
        #endif
    }

    private void DgvCreationTasks_SelectionChanged(object sender, EventArgs e)
    {
        var selection = new List<OpeningTask>();
        foreach(DataGridViewRow row in form_.dgvCreationTasks.SelectedRows)
        {
            var task = (OpeningTask)row.DataBoundItem;
            if (task != null)
            {
                selection.Add(task);
            }
        }
        groupModel_.changeSelection(selection);
    }
    private void DgvCreationTasks_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        OpeningTask task = 
            (OpeningTask)form_.dgvCreationTasks.Rows[e.RowIndex].DataBoundItem;

        groupModel_.focusTaskElement(task);
    }

    private void rowsAdded(IEnumerable<DataGridViewRow> rows)
    {
        foreach (var row in rows)
        {
           
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

        column.Name = ColumnName.TYPE_SIZE;
        column.ReadOnly = true;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        columns.Add(column);

        //var cmboxColumn = new DataGridViewComboBoxColumn();
        //column = cmboxColumn; // НВС
        //cmboxColumn.DataPropertyName = "FlangesType";
        //cmboxColumn.Name = ColumnName.FLANGES;
        //cmboxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        //cmboxColumn.DataSource = groupModel_.getFlangeNumbersSort();
        //columns.Add(cmboxColumn);
        
        //column = new DataGridViewComboBoxColumn();
        //column.DataPropertyName = "DiameterTypeStr";
        //column.Name = ColumnName.DIAMETER;
        //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        //columns.Add(column);
        
        //column = new DataGridViewTextBoxColumn();
        //column.DataPropertyName = "Length";
        //column.Name = ColumnName.LENGTH;
        //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        //columns.Add(column);

        //column = new DataGridViewTextBoxColumn();
        //column.DataPropertyName = 
        //column.Name = ColumnName.REF_POINT1;
        //column.ReadOnly = true;
        //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        //columns.Add(column);

        //column = new DataGridViewTextBoxColumn();
        //column.DataPropertyName = "Test.First().Value";
        //column.Name = "new";
        //column.ReadOnly = true;
        //column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        //columns.Add(column);

        return columns;
    }

    private GroupByTaskModel groupModel_;
    private OpeningForm form_;

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
