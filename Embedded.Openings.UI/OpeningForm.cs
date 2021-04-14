using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Shared;

namespace Embedded.Openings.UI
{
public partial class OpeningForm : Form
{
    public delegate void PreviewAction();
    public delegate void CreateAction();

    public delegate void StartDefaultAction();

    public delegate void OnCloseFormAction();
    public delegate void DataRowsAddedAction(IEnumerable<DataGridViewRow> rows);

    private PreviewAction previewAction_;
    private CreateAction createAction_;
    private StartDefaultAction startDefaultAction_;
    private OnCloseFormAction onCloseFormAction_;
    private DataRowsAddedAction dataRowsAddedAction_;

    static Properties.Settings Sets => Properties.Settings.Default;

    readonly System.Drawing.Color readonlyColor = System.Drawing.SystemColors.Control;

    public OpeningForm()
    {
        InitializeComponent();
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

    public void setDataSource_Create(DataTable table)
    {
        dgvCreationTasks.DataSource = table;
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

    public void setStartDefaultAction(StartDefaultAction action)
    {
        startDefaultAction_ = action;
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
        //InvokeSafe(scanForUpdateAction_, trvUpdate);
        btnUpdate.Enabled = trvUpdate.Nodes.Count > 0;
    }

    private void btnUpdate_Click(object sender, EventArgs e)
    {
        //InvokeSafe(updateAction_);
        // перезапускаем сканирование
        btnScan_Click(btnScan, e);
    }

    private void btnStartPrimitive_Click(object sender, EventArgs e)
    {
        //InvokeSafe(startPrimitiveAction_);
    }

    private void Form_Closed(object sender, FormClosedEventArgs e)
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

    private void Form_KeyDown(object sender, KeyEventArgs e)
    {
        ;
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
        InvokeSafe(startDefaultAction_);
    }
}
}
