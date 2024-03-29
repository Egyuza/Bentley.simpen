﻿using System;
using System.Windows.Forms;

using System.Collections.Generic;
using System.ComponentModel;

using Shared;
using System.Data;
using System.IO;

namespace Embedded.Penetrations.UI
{

//public enum BindinUpdateMode
//{
//    ControlOnly,
//    SourceOnly,
//    Both
//}

public partial class PenetrationForm : Form
{
    public delegate void ShowErrorMessageAction(Exception ex);

    public delegate void LoadXmlAttrsAction(string uri);
    public delegate void PreviewAction();
    public delegate void CreateAction();
    public delegate void ScanForUpdateAction(TreeView treeView);
    public delegate void UpdateAction();
    public delegate void UpdateNodeDoubleClickAction(TreeNode node);
    public delegate void StartDefaultAction();
    public delegate void StartPrimitiveAction();
    public delegate void SingleSelectFlangeTypeAction(long flangeType);
    public delegate void SingleSelectDiameterTypeAction(object diameterType);
    public delegate void SingleSelectLengthAction(int length);

    public delegate void OnCloseFormAction();
    public delegate void DataRowsAddedAction(IEnumerable<DataGridViewRow> rows);

    public delegate void SetReadOnlyAction(bool readOnly);

    private ShowErrorMessageAction showErrorMessageAction_;
    private LoadXmlAttrsAction loadXmlAttrsAction_;
    private PreviewAction previewAction_;
    private CreateAction createAction_;
    private ScanForUpdateAction scanForUpdateAction_;
    private UpdateNodeDoubleClickAction updateNodeDoubleClickAction_;
    private UpdateAction updateAction_;
    private StartPrimitiveAction startPrimitiveAction_;
    private StartDefaultAction startDefaultAction_;
    private SingleSelectFlangeTypeAction singleSelectFlangeTypeAction_;
    private SingleSelectDiameterTypeAction singleSelectDiameterTypeAction_;
    private SingleSelectLengthAction singleSelectLengthAction_;
    private OnCloseFormAction onCloseFormAction_;
    private DataRowsAddedAction dataRowsAddedAction_;
    private SetReadOnlyAction setReadOnlyAction_;

    static Properties.Settings Sets => Properties.Settings.Default;

    readonly System.Drawing.Color readonlyColor = System.Drawing.SystemColors.Control;

    public PenetrationForm()
    {        
        InitializeComponent();

        { // ПОСТРОЕНИЕ:
            dgvCreationTasks.AutoGenerateColumns = false;
            dgvCreationTasks.EnableHeadersVisualStyles = false;
            //dgvCreationTasks.Columns.Clear();
            //dgvCreationTasks.AutoSize = false;

            dgvCreationTasks.RowsAdded += DgvFields_RowsAdded;
            dgvCreationTasks.DataError += DgvFields_DataError;  

            numAngleX.setBinding("Enabled", chbxManualRotate, "Checked", BindinUpdateMode.ControlOnly);
            numAngleY.setBinding("Enabled", chbxManualRotate, "Checked", BindinUpdateMode.ControlOnly);
            numAngleZ.setBinding("Enabled", chbxManualRotate, "Checked", BindinUpdateMode.ControlOnly);

            numIncrement.setBinding("Enabled", chbxManualRotate, "Checked", BindinUpdateMode.ControlOnly);
            numAngleX.setBinding("Increment", numIncrement, "Value", BindinUpdateMode.ControlOnly);
            numAngleY.setBinding("Increment", numIncrement, "Value", BindinUpdateMode.ControlOnly);            
            numAngleZ.setBinding("Increment", numIncrement, "Value", BindinUpdateMode.ControlOnly);

            txtLength.setBinding(nameof(txtLength.ReadOnly), 
                chbxAutoLength, nameof(chbxAutoLength.Checked), 
                BindinUpdateMode.ControlOnly);
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

    public void setDataSource(DataTable table)
    {
        dgvCreationTasks.DataSource = table;    
    }

    public void setDataSource_Create(IBindingList bindList)
    {
        dgvCreationTasks.DataSource = new BindingSource(bindList, null);
    }

    //public void setDataSource_Update(IBindingList bindList)
    //{
    //    //trvUpdate.DataSource = new BindingSource(bindList, null);
    //}

    //public void setBinding(string controlName, string controlPropertyName,
    //    object dataSource, string dataSourceMember, BindinUpdateMode bindingMode)
    //{
    //    try
    //    {
    //        setBinding(findControl(controlName, this), controlPropertyName,
    //            dataSource, dataSourceMember, bindingMode);
    //    }
    //    catch (Exception ex)
    //    {
    //        ex.ShowMessage();
    //    }
    //}

    //public static void setBinding(Control control, string controlPropertyName,
    //    object dataSource, string dataSourceMember, BindinUpdateMode bindingMode)
    //{       
    //    try
    //    {
    //        var binding = new Binding(
    //            controlPropertyName, dataSource, dataSourceMember);
             
    //        switch (bindingMode)
    //        {
    //        case BindinUpdateMode.ControlOnly:
    //            binding.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
    //            binding.DataSourceUpdateMode = DataSourceUpdateMode.Never;
    //            break;
    //        case BindinUpdateMode.SourceOnly:
    //            binding.ControlUpdateMode = ControlUpdateMode.Never;
    //            binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
    //            break;
    //        default:
    //            binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
    //            binding.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;             
    //            break;
    //        }

    //        control.DataBindings.Add(binding);
    //    }
    //    catch (Exception ex)
    //    {
    //        ex.ShowMessage();
    //    }
    //}

    //private Control findControl(string name, Control owner)
    //{
    //    Control res = null;
    //    res = owner.Controls[name];
        
    //    if (res == null)
    //    {
    //        foreach (Control control in owner.Controls)
    //        {
    //            res = findControl(name, control);
    //            if (res != null) return res;
    //        }
    //    }
    //    return res;
    //}

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

    public void setAction_SetReadOnly(SetReadOnlyAction action)
    {
        setReadOnlyAction_ = action;
    }

    public void setAction_ShowErrorMessage(ShowErrorMessageAction action)
    {
        showErrorMessageAction_ = action;
    }

    public void setAction_LoadXmlAttrs(LoadXmlAttrsAction action)
    {
        loadXmlAttrsAction_ = action;
    }

    public void setAction_Preview(PreviewAction action)
    {
        previewAction_ = action;
    }

    public void setAction_Create(CreateAction action)
    {
        createAction_ = action;
    }

    public void setAction_ScanForUpdate(ScanForUpdateAction action)
    {
        scanForUpdateAction_ = action;
    }

    public void setAction_UpdateNodeDoubleClick(UpdateNodeDoubleClickAction action)
    {
       updateNodeDoubleClickAction_ = action;
    }

    public void setAction_Update(UpdateAction action)
    {
        updateAction_ = action;
    }

    public void setAction_StartDefault(StartDefaultAction action)
    {
        startDefaultAction_ = action;
    }

    public void setAction_StartPrimitive(StartPrimitiveAction action)
    {
        startPrimitiveAction_ = action;
    }

    public void setAction_SingleSelecteFlangeType(SingleSelectFlangeTypeAction action)
    {
        singleSelectFlangeTypeAction_ = action;
    }
    public void setAction_SingleSelecteDiameterType(SingleSelectDiameterTypeAction action)
    {
        singleSelectDiameterTypeAction_ = action;
    }

    public void setAction_SingleSelecteLength(SingleSelectLengthAction action)
    {
        singleSelectLengthAction_ = action;
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
            ShowErrorMessage(ex);
        }
    }

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
        int length;
        if (int.TryParse(txtLength.Text, out length))
        {
            InvokeSafe(singleSelectLengthAction_, length);
        }
    }

    private void PenetrationForm_KeyDown(object sender, KeyEventArgs e)
    {
        ;
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
        InvokeSafe(startDefaultAction_);
    }

    private void trvUpdate_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        InvokeSafe(updateNodeDoubleClickAction_, e.Node);
    }

    private void ShowErrorMessage(Exception ex)
    {
        showErrorMessageAction_.Invoke(ex);
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

    private void chboxEdit_CheckedChanged(object sender, EventArgs e)
    {
        InvokeSafe(setReadOnlyAction_, !chboxEdit.Checked);
    }
}
}
