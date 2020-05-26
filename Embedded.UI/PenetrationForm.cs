using System;
using System.Windows.Forms;

using System.Collections.Generic;
using System.ComponentModel;

using Shared;

namespace Embedded.UI
{

public partial class PenetrationForm : Form
{
    public delegate void PreviewAction();
    public delegate void CreateAction();
    public delegate void ScanForUpdateAction();
    public delegate void UpdateAction();
    public delegate void OnCloseFormAction();
    public delegate void DataRowsAddedAction(IEnumerable<DataGridViewRow> rows);

    static Properties.Settings Sets => Properties.Settings.Default;

    readonly System.Drawing.Color readonlyColor = System.Drawing.SystemColors.Control;

    public PenetrationForm()
    {        
        InitializeComponent();

        { // ПОСТРОЕНИЕ:
            dgvCreate.AutoGenerateColumns = false;
            dgvCreate.EnableHeadersVisualStyles = false;
            dgvCreate.Columns.Clear();
            dgvCreate.AutoSize = false;

            dgvCreate.RowsAdded += DgvFields_RowsAdded;
            dgvCreate.DataError += DgvFields_DataError;
        }

        { // ОБНОВЛЕНИЕ:
            dgvUpdate.AutoGenerateColumns = false;
            dgvUpdate.EnableHeadersVisualStyles = false;
            dgvUpdate.Columns.Clear();
            dgvUpdate.AutoSize = false;
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
            dgvCreate.Columns.Add(column);
        } 
    }

    public void setDataSource_Create(IBindingList bindList)
    {
        dgvCreate.DataSource = new BindingSource(bindList, null);
    }

    public void setDataSource_Update(IBindingList bindList)
    {
        dgvUpdate.DataSource = new BindingSource(bindList, null);
    }

    public void setBinding(string controlName, string controlProperty,
        object dataSource, string dataMember)
    {
        try
        {
            findControl(controlName, this).DataBindings.Add(
                new Binding(controlProperty, dataSource, dataMember));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Binding error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    public void setStatusText(string text)
    {
        lblStatus.Text = text;
        lblStatus.Visible = !string.IsNullOrEmpty(text);
    }

    public void setReadOnly()
    {
        btnAddToModel.Visible = 
        btnAddToModel.Enabled = false;
        dgvCreate.ReadOnly = true;
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
            rows.Add(dgvCreate.Rows[i]);
        }
        dataRowsAddedAction_?.Invoke(rows);
    }

    private void btnPreview_Click(object sender, EventArgs e)
    {
        try
        {
            previewAction_?.Invoke();
        }
        catch (Exception ex)
        {
            ex.ShowMessage();
        }        
    }

    private void btnAddToModel_Click(object sender, EventArgs e)
    {
        try
        {
            createAction_?.Invoke();
        }
        catch (Exception ex)
        {
            ex.ShowMessage();
        }
    }

    private void btnScan_Click(object sender, EventArgs e)
    {
        InvokeSafe(scanForUpdateAction_);
    }

    private void InvokeSafe(Delegate action)
    {
        try
        {
            action?.DynamicInvoke();
        }
        catch (Exception ex)
        {
            ex.ShowMessage();
        }
    }


    private void PenetrationForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        onCloseFormAction_?.Invoke();
    }

    private PreviewAction previewAction_;
    private CreateAction createAction_;
    private ScanForUpdateAction scanForUpdateAction_;
    private UpdateAction updateAction_;
    private OnCloseFormAction onCloseFormAction_;
    private DataRowsAddedAction dataRowsAddedAction_;


}
}
