using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Shared;

namespace Embedded.Openings.UI
{
public partial class OpeningForm : Form
{
    public delegate void LoadXmlAttrsAction(string uri);
    public delegate void PreviewAction();
    public delegate void CreateAction();
    public delegate void SetReadOnlyAction(bool readOnly);

    public delegate void StartDefaultAction();

    public delegate void OnCloseFormAction();
    public delegate void DataRowsAddedAction(IEnumerable<DataGridViewRow> rows);

    private LoadXmlAttrsAction loadXmlAttrsAction_;
    private PreviewAction previewAction_;
    private CreateAction createAction_;
    private StartDefaultAction startDefaultAction_;
    private OnCloseFormAction onCloseFormAction_;
    private DataRowsAddedAction dataRowsAddedAction_;
    private SetReadOnlyAction setReadOnlyAction_;

    static Properties.Settings Sets => Properties.Settings.Default;

    readonly System.Drawing.Color readonlyColor = System.Drawing.SystemColors.Control;

    public OpeningForm()
    {
        InitializeComponent();
        dgvCreationTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        //dgvCreationTasks.autosize
        //dgvCreationTasks.RowPostPaint += DgvCreationTasks_RowPostPaint;
    }

    private void DgvCreationTasks_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
    {
        dgvCreationTasks.AutoResizeColumns();
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

    public void setDataSource(DataTable table)
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

    public void setAttrsInfoText(string text)
    {
        attrsInfoText.Text = text;
        bool isVisible = !string.IsNullOrEmpty(text);
        attrsInfoLabel.Visible =
        attrsInfoText.Visible = isVisible;
    }

    public void setReadOnly()
    {
        btnAddToModel.Visible = 
        btnAddToModel.Enabled = false;
        dgvCreationTasks.ReadOnly = true;
    }

    public void setAction_SetReadOnly(SetReadOnlyAction action)
    {
        setReadOnlyAction_ = action;
    }

    public void setAction_Preview(PreviewAction action)
    {
        previewAction_ = action;
    }

    public void setAction_LoadXmlAttrs(LoadXmlAttrsAction action)
    {
        loadXmlAttrsAction_ = action;
    }

    public void setAction_Create(CreateAction action)
    {
        createAction_ = action;
    }

    public void setAction_StartDefault(StartDefaultAction action)
    {
        startDefaultAction_ = action;
    }


    public void setAction_DataRowsAdded(DataRowsAddedAction action)
    {
        dataRowsAddedAction_ = action;
    }

    public void setAction_OnCloseForm(OnCloseFormAction action)
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
        //dgvCreationTasks.PerformLayout();
        //    //dgvCreationTasks.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

        //foreach (DataGridViewColumn column in dgvCreationTasks.Columns)
        //{
        //    column.Width = Math.Max(
        //        column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.DisplayedCells, true),
        //        column.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, false)
        //    );
        //}

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

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
        InvokeSafe(startDefaultAction_);
    }

    private void chboxEdit_CheckedChanged(object sender, EventArgs e)
    {
        InvokeSafe(setReadOnlyAction_, !chboxEdit.Checked);
    }

    private void btnLoadXmlAttributes_Click(object sender, EventArgs e)
    {
        OpenFileDialog fileDialog = new OpenFileDialog()
        {
            Filter = "Xml files (*.xml)|*.xml"
        };
        DialogResult result = fileDialog.ShowDialog();
        if (result == DialogResult.OK)
        {
            InvokeSafe(loadXmlAttrsAction_, fileDialog.FileName);
            string shortName = Path.GetFileName(fileDialog.FileName);
            setAttrsInfoText(shortName);
        }
    }
}
}
