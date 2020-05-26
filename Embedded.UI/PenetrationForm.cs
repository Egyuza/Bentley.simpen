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
    public delegate void OnCloseFormAction();
    public delegate void DataRowsAddedAction(IEnumerable<DataGridViewRow> rows);

    //enum ModeEnum
    //{
    //    Single,
    //    Multiple
    //}

    //enum FieldType
    //{
    //    FLANGES,
    //    DIAMETR,
    //    LENGTH,
    //    KKS,
    //    DESCRIPTION
    //}

    static Properties.Settings Sets => Properties.Settings.Default;

    readonly System.Drawing.Color readonlyColor = System.Drawing.SystemColors.Control;

    public PenetrationForm()
    {        
        InitializeComponent();

        dgvFields.AutoGenerateColumns = false;
        dgvFields.EnableHeadersVisualStyles = false;
        dgvFields.Columns.Clear();
        dgvFields.AutoSize = false;

        dgvFields.RowsAdded += DgvFields_RowsAdded;
        dgvFields.DataError += DgvFields_DataError;

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
            dgvFields.Columns.Add(column);
        } 
    }

    public void setDataSource(IBindingList bindList)
    {
        dgvFields.DataSource = new BindingSource(bindList, null);
    }

    //public void setCountBinding(object source, string dataMember)
    //{
    //    lblSelectedCount.DataBindings.Clear();
    //    lblSelectedCount.DataBindings.Add(new Binding("Text", source, dataMember));
    //}

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
        dgvFields.ReadOnly = true;
    }

    public void setPreviewAction(PreviewAction action)
    {
        previewAction_ = action;
    }

    public void setCreateAction(CreateAction action)
    {
        createAction_ = action;
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
            rows.Add(dgvFields.Rows[i]);
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

    private void PenetrationForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        onCloseFormAction_?.Invoke();
    }

    private PreviewAction previewAction_;
    private CreateAction createAction_;
    private OnCloseFormAction onCloseFormAction_;
    private DataRowsAddedAction dataRowsAddedAction_;
}
}
