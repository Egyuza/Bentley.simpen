using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;

namespace simpen.ui
{

public partial class PenetrForm : Form
{
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

    static class CExpr
    {
        private const string pref = "penPipe";
        public const string 
            FLANGES = pref + "Flanges",
            DIAMETR = pref + "Diametr",
            LENGTH = pref + "Length",
            KKS = pref + "KKS",
            DESCRIPTION = pref + "Description";
    }

    bool isReadyToPublishTrigger_;

    public PenetrForm()
    {
        InitializeComponent();
            
        Version version = Assembly.GetExecutingAssembly().GetName().Version;

        this.Text = string.Format("Проёмы v{0}.{1}.{2}",
            version.Major, version.Minor, version.Build);

        foreach (FieldType fieldType in Enum.GetValues(typeof(FieldType)))
        { 
            string fieldKey = 
                fieldType == FieldType.FLANGES ? "Фланцы, шт" :
                fieldType == FieldType.DIAMETR ? "Диаметр, мм" :
                fieldType == FieldType.LENGTH ? "Длина, мм" :
                fieldType == FieldType.KKS ? "KKS код" : 
                fieldType == FieldType.DESCRIPTION ? "Наименование" :
                "";

            Debug.Assert(fieldKey.Length > 0, string.Format(
                "Не определён параметр {0}", fieldType.ToString()));

            dgvFields.Rows.Add(fieldKey);
        }

        { // todo восстанавливаем сохранённые настройки:
            
        }
                
        reload();
        runLocatingTool();
    }

    public void runLocatingTool()
    {
        sendTaskData();
        sendKeyin("locate task");
    }

    void chbxEdit_CheckedChanged(object sender, EventArgs e)
    {
        grbxParameters.Enabled = chbxEdit.Checked;
    }

    void setEnabled(Control control, bool state)
    {
        foreach (Control child in control.Controls)
        {
            setEnabled(child, state);
        }
        control.Enabled = state;
    }

    // СОЗДАТЬ ОБЪЕКТ
    private void btnAddToModel_Click(object sender, EventArgs e)
    {
        sendTaskData();
        sendKeyin("add");

    // todo наверно есть ключ на запуск с ожиданием возврата от mdl
    // ! нельзя reload, т.к. не завершена работа mdl

        isReadyToPublishTrigger_ = false;
    }

    private void dgvFields_EnabledChanged(object sender, EventArgs e)
    {
        foreach (DataGridViewRow row in dgvFields.Rows)
        {
            row.Cells[0].Style.ForeColor = 
            row.Cells[1].Style.ForeColor = dgvFields.Enabled ? 
            System.Drawing.Color.Black : System.Drawing.Color.Gray;
        }
    }

    public void reload()
    {
        isReadyToPublishTrigger_ = false;
        foreach (DataGridViewRow row in dgvFields.Rows)
        {
            row.Cells[1].Value = "";
        }
        
        dgvFields.RefreshEdit();

        grbxParameters.Enabled = false;
        chbxEdit.Checked = false;
        btnAddToModel.Enabled = false;

        // todo
        //refreshControlsDependsOnMode();
        //checkValidationState();
    }

    public void enableAddToModel()
    {
        isReadyToPublishTrigger_ = true;

        // todo
        //checkValidationState();        
    }

    public void readTaskData()
    {
        // TODO зачитываем геометрические параметры с шагом 5 мм;
        // построение элемента должно быть уже с учётом округления, 
        // + точка установки тоже корректируется

        // TODO проверить проходит ли валидация при зачитывании данных из задания

        readGeometryProperty(FieldType.DIAMETR);
        readGeometryProperty(FieldType.LENGTH);
        readProperty(FieldType.FLANGES);
        readProperty(FieldType.KKS);
        readProperty(FieldType.DESCRIPTION);

        // TODO checkValidationState();
    }

    private void readProperty(FieldType fieldType)
    {
        DataGridViewCell cell = dgvFields.Rows[(int)fieldType].Cells[1];
        cell.Value = 
            Addin.Instance.getCExpressionValue(getCExpression(fieldType));
    }

    private void readGeometryProperty(FieldType fieldType)
    {
        // геометрические параметры с шагом в 5 мм;
        // TODO логировать корректировку и выводить информ. сообщение на форму?
        double value = 
            (double)Addin.Instance.getCExpressionValue(getCExpression(fieldType));
        dgvFields.Rows[(int)fieldType].Cells[1].Value = 
            (int)(Math.Round( value/5.0) * 5);
    }

    public void sendTaskData()
    {
        dgvFields.CommitEdit(DataGridViewDataErrorContexts.Commit); // ! важно

        sendProperty(FieldType.FLANGES);
        sendProperty(FieldType.DIAMETR);
        sendProperty(FieldType.LENGTH);
        sendProperty(FieldType.KKS);
        sendProperty(FieldType.DESCRIPTION);

            // TODO checkValidationState();
        }


    private string getCExpression(FieldType fieldType)
    {
        // TODO ИСПЛ. только openingDistance, ...

        string result = fieldType == FieldType.FLANGES ? CExpr.FLANGES :
            fieldType == FieldType.DIAMETR ? CExpr.DIAMETR :
            fieldType == FieldType.LENGTH ? CExpr.LENGTH :
            fieldType == FieldType.KKS ? CExpr.KKS :
            fieldType == FieldType.DESCRIPTION ? CExpr.DESCRIPTION :
            "";

        Debug.Assert(result.Length > 0, string.Format(
            "Для параметра {0} не задано соответствие CExpression",
            fieldType.ToString()));

        return result;
    }

    private void sendProperty(FieldType fieldType)
    {
        DataGridViewCell cell = dgvFields.Rows[(int)fieldType].Cells[1];
        string cexpr = getCExpression(fieldType);
        if (cell.ErrorText.Length == 0 && cell.Value != null)
        {
            Addin.Instance.setCExpressionValue(cexpr, cell.Value);
            sendKeyin("update preview");
        }
        else
        {
            switch (fieldType)
            {
            case FieldType.FLANGES:
            case FieldType.DIAMETR:
            case FieldType.LENGTH:
                Addin.Instance.setCExpressionValue(cexpr, 0.0);
                break;
            default:
                Addin.Instance.setCExpressionValue(cexpr, "");
                break;
            }
        }
    }

    private void dgvFields_CellValueChanged(
        object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 1)
            return;

        // TODO
        //if (validateCell(dgvFields.Rows[e.RowIndex].Cells[1]))
        //{
        //    sendProperty((FieldType)e.RowIndex);
        //}
        dgvFields.RefreshEdit();
    }
    
    private void btnLocate_Click(object sender, EventArgs e)
    {
        runLocatingTool();
    }

    private void OpeningForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        // todo sets save
    }

    private void sendKeyin(string smallCmdName)
    {
        Addin.Instance.sendKeyin("penPipe " + smallCmdName);
    }
    }
}
