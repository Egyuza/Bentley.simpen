using System;
using System.Windows.Forms;

using System.Collections.Generic;
using System.ComponentModel;

using Shared;

namespace Embedded.UI
{

public enum BindinUpdateMode
{
    ControlOnly,
    SourceOnly,
    Both
}

public partial class PenetrationForm : Form
{
    public delegate void PreviewAction();
    public delegate void CreateAction();
    public delegate void ScanForUpdateAction(TreeView treeView);
    public delegate void UpdateAction();
    public delegate void StartPrimitiveAction();
    public delegate void SingleSelectFlangeTypeAction(long flangeType);
    public delegate void SingleSelectDiameterTypeAction(object diameterType);
    public delegate void SingleSelectLengthAction(int length);    

    public delegate void OnCloseFormAction();
    public delegate void DataRowsAddedAction(IEnumerable<DataGridViewRow> rows);

    static Properties.Settings Sets => Properties.Settings.Default;

    readonly System.Drawing.Color readonlyColor = System.Drawing.SystemColors.Control;

    public PenetrationForm()
    {        
        InitializeComponent();

        { // ПОСТРОЕНИЕ:
            dgvCreationTasks.AutoGenerateColumns = false;
            dgvCreationTasks.EnableHeadersVisualStyles = false;
            dgvCreationTasks.Columns.Clear();
            dgvCreationTasks.AutoSize = false;

            dgvCreationTasks.RowsAdded += DgvFields_RowsAdded;
            dgvCreationTasks.DataError += DgvFields_DataError;  
        }

        { // ОБНОВЛЕНИЕ:
            trvUpdate.Nodes.Clear();
            btnUpdate.Enabled = false;
        }

        { // TODO восстанавливаем сохранённые настройки:
            
        }
    }

    public void setColumns(IEnumerable<DataGridViewColumn> columns)
    {
        foreach (DataGridViewColumn column in columns)
        {
            if (column.ReadOnly)
            {
                column.CellTemplate.Style.BackColor = readonlyColor;
            }
            dgvCreationTasks.Columns.Add(column);
        } 
    }

    public void setDataSource_Create(IBindingList bindList)
    {
        dgvCreationTasks.DataSource = new BindingSource(bindList, null);
    }

    //public void setDataSource_Update(IBindingList bindList)
    //{
    //    //trvUpdate.DataSource = new BindingSource(bindList, null);
    //}

    public void setBinding(string controlName, string controlPropertyName,
        object dataSource, string dataSourceMember, BindinUpdateMode bindingMode)
    {
        try
        {
            setBinding(findControl(controlName, this), controlPropertyName,
                dataSource, dataSourceMember, bindingMode);
        }
        catch (Exception ex)
        {
            ex.ShowMessage();
        }
    }

    public static void setBinding(Control control, string controlPropertyName,
        object dataSource, string dataSourceMember, BindinUpdateMode bindingMode)
    {       
        try
        {
            var binding = new Binding(
                controlPropertyName, dataSource, dataSourceMember);
             
            switch (bindingMode)
            {
            case BindinUpdateMode.ControlOnly:
                binding.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                binding.DataSourceUpdateMode = DataSourceUpdateMode.Never;
                break;
            case BindinUpdateMode.SourceOnly:
                binding.ControlUpdateMode = ControlUpdateMode.Never;
                binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
                break;
            default:
                binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
                binding.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;             
                break;
            }

            control.DataBindings.Add(binding);
        }
        catch (Exception ex)
        {
            ex.ShowMessage();
        }
    }

    private Control findControl(string name, Control owner)
    {
        Control res = null;
        res = owner.Controls[name];
        
        if (res == null)
        {
            foreach (Control control in owner.Controls)
            {
                res = findControl(name, control);
                if (res != null) return res;
            }
        }
        return res;
    }

    public void setStatusProject(string projectName)
    {
        if (string.IsNullOrEmpty(projectName))
        {
            statusProject.Text = "Не определён";
            statusProject.ForeColor = System.Drawing.Color.Red;
        }
        else
        {
            statusProject.Text = projectName;
            statusProject.ForeColor = System.Drawing.Color.Black;
        }        
    }

    public void setStatusText(string text)
    {
        statusInfo.Text = text;
        statusInfo.Visible = !string.IsNullOrEmpty(text);
    }

    public void setReadOnly()
    {
        btnAddToModel.Visible = 
        btnAddToModel.Enabled = false;
        dgvCreationTasks.ReadOnly = true;
    }

    public void setPreviewAction(PreviewAction action)
    {
        previewAction_ = action;
    }

    public void setCreateAction(CreateAction action)
    {
        createAction_ = action;
    }

    public void setScanForUpdateAction(ScanForUpdateAction action)
    {
        scanForUpdateAction_ = action;
    }

    public void setUpdateAction(UpdateAction action)
    {
        updateAction_ = action;
    }

    public void setStartPrimitiveAction(StartPrimitiveAction action)
    {
        startPrimitiveAction_ = action;
    }

    public void setSingleSelecteFlangeType(SingleSelectFlangeTypeAction action)
    {
        singleSelectFlangeTypeAction_ = action;
    }
    public void setSingleSelecteDiameterType(SingleSelectDiameterTypeAction action)
    {
        singleSelectDiameterTypeAction_ = action;
    }

    public void setSingleSelecteLength(SingleSelectLengthAction action)
    {
        singleSelectLengthAction_ = action;
    }

    public void setDataRowsAddedAction(DataRowsAddedAction action)
    {
        dataRowsAddedAction_ = action;
    }

    public void setOnCloseFormAction(OnCloseFormAction action)
    {
        onCloseFormAction_ = action;
    }

    private void DgvFields_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
        // Во время добавления строки возникают ошибки раньше, чем 
        // мы можем сформировать список допустимых значений для ячеек combobox
        
        //  TODO implement ?
    }

    private void DgvFields_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
    {
        var rows = new List<DataGridViewRow>();
        for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; ++i)
        {
            rows.Add(dgvCreationTasks.Rows[i]);
        }

        InvokeSafe(dataRowsAddedAction_, rows);
    }

    private void btnPreview_Click(object sender, EventArgs e)
    {
        InvokeSafe(previewAction_);     
    }

    private void btnAddToModel_Click(object sender, EventArgs e)
    {
        InvokeSafe(createAction_);
    }

    private void btnScan_Click(object sender, EventArgs e)
    {
        InvokeSafe(scanForUpdateAction_, trvUpdate);
        btnUpdate.Enabled = trvUpdate.Nodes.Count > 0;
    }

    private void btnUpdate_Click(object sender, EventArgs e)
    {
        InvokeSafe(updateAction_);
        // перезапускаем сканирование
        btnScan_Click(btnScan, e);
    }

    private void btnStartPrimitive_Click(object sender, EventArgs e)
    {
        InvokeSafe(startPrimitiveAction_);
    }

    private void PenetrationForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        InvokeSafe(onCloseFormAction_);
    }

    private void InvokeSafe(Delegate action, params object[] args)
    {
        try
        {
            action?.DynamicInvoke(args);
        }
        catch (Exception ex)
        {
            ex.ShowMessage();
        }
    }

    private PreviewAction previewAction_;
    private CreateAction createAction_;
    private ScanForUpdateAction scanForUpdateAction_;
    private UpdateAction updateAction_;
    private StartPrimitiveAction startPrimitiveAction_;
    private SingleSelectFlangeTypeAction singleSelectFlangeTypeAction_;
    private SingleSelectDiameterTypeAction singleSelectDiameterTypeAction_;
    private SingleSelectLengthAction singleSelectLengthAction_;
    private OnCloseFormAction onCloseFormAction_;
    private DataRowsAddedAction dataRowsAddedAction_;

    private void dgvCreationTasks_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {

    }

    private void cbxFlanges_SelectedValueChanged(object sender, EventArgs e)
    {
        InvokeSafe(singleSelectFlangeTypeAction_, cbxFlanges.SelectedValue);
    }

    private void cbxDiameter_SelectedValueChanged(object sender, EventArgs e)
    {
        InvokeSafe(singleSelectDiameterTypeAction_, cbxDiameter.SelectedValue);
    }

    private void txtLength_TextChanged(object sender, EventArgs e)
    {
        InvokeSafe(singleSelectLengthAction_, int.Parse(txtLength.Text));
    }
}
}
