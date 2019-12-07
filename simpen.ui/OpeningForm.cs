using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;

namespace simpen.ui
{

public partial class OpeningForm : Form
{
    enum Mode
    {
        BY_TASK,
        BY_CONTOUR,
        BY_POINTS
    }
    enum FieldType
    {
        HEIGHT,
        WIDTH,
        LENGTH,
        KKS
    }
        
    static Properties.Settings Sets {
        get
        {
            return   Properties.Settings.Default;
        }
    }

    static class CExpr
    {
        private const string pref = "opening";
        
        public const string 
            HEIGHT = pref + "Height",
            WIDTH = pref + "Width",
            DEPTH = pref + "Distance",
            KKS = pref + "KKS",
            IS_POLICY_THROUGH = pref + "IsThroughHole",
            IS_READY_TO_PUBLISH = pref + "IsReadyToPublish",
            IS_REQUIRED_REMOVE_CONTOUR = pref + "IsRequiredRemoveContour";
    }

    bool isReadyToPublishTrigger_;

    public OpeningForm()
    {
        InitializeComponent();

        Version version = Assembly.GetExecutingAssembly().GetName().Version;

        this.Text = "Проёмы " + Addin.getVersion();

        foreach (FieldType fieldType in Enum.GetValues(typeof(FieldType)))
        { 
            string fieldKey = 
                fieldType == FieldType.HEIGHT ? "Высота, мм" :
                fieldType == FieldType.WIDTH ? "Ширина, мм" :
                fieldType == FieldType.LENGTH ? "Глубина, мм" :
                fieldType == FieldType.KKS ? "KKS код" : 
                "";

            Debug.Assert(fieldKey.Length > 0, string.Format(
                "Не определён параметр {0}", fieldType.ToString()));

            dgvFields.Rows.Add(fieldKey);
        }

        { // сохранённые настройки:
            setMode((Mode)Sets.mode);
            chbxPolicyThrough.Checked = Sets.isPolicyThrough;
            chbxRemoveContour.Checked = Sets.isRequiredRemoveConture;
        }
                
        reload();
        runLocatingTool();
    }

    public void runLocatingTool()
    {
        sendTaskData();

        if (isTaskMode()) // НВС
        {
            sendKeyin("locate task");
        }    
        else if (isContourMode())
        {
            sendKeyin("locate contour");
        }
    }
    
    private Mode getMode()
    {
        return rbtnModeTask.Checked ? Mode.BY_TASK : Mode.BY_CONTOUR;
    }

    private void setMode(Mode mode)
    {
        rbtnModeTask.Checked = mode == Mode.BY_TASK;
        rbtnModeContour.Checked = mode == Mode.BY_CONTOUR;
    }

    private bool validateCell(DataGridViewCell cell)
    {
        string value = cell.Value?.ToString();
        value = value ?? "";
        string errText = "";

        switch ((FieldType)cell.RowIndex)
        {
        case FieldType.HEIGHT:
        case FieldType.WIDTH:
        {
            Regex regex = new Regex("^[0-9]+$");
            try
            {
                if (!regex.IsMatch(value) || int.Parse(value) == 0 ||
                    int.Parse(value) != (5.0 * Math.Round(double.Parse(value) / 5.0)))
                {
                    throw new FormatException();
                }
                cell.Value = double.Parse(value);
            }
            catch (FormatException)
            {
                errText = "должно быть натуральным числом, кратным 5 мм";
            }
            break;
        }
        case FieldType.LENGTH:
        {
            // знак значения задаёт направление вектора
            Regex regex = new Regex("^-?[0-9]+$");
            try
            {
                if (!regex.IsMatch(value) || int.Parse(value) == 0 ||
                    int.Parse(value) != (5.0*Math.Round(double.Parse(value)/5.0)))
                {
                    throw new FormatException();
                }
                cell.Value = double.Parse(value);
            }
            catch (FormatException)
            {
                errText = "должно быть целым числом, кратным 5 мм";
            }           
            break;
        }
        case FieldType.KKS:
        {
            Regex regex = new Regex("^[0-9]{2}[A-Z]{3}[0-9]{3}$");

            if (string.IsNullOrEmpty(value))
                errText = "не может быть пустым";

            // TODO проверка формата записи KKS

            //else if (!regex.IsMatch(value))
            //    errText = "не соответствует формату KKS: 00AAA000";

            /* TODO проверка на уникальность KKS   
            Public Sub checkPenCode(strCode As String)

                On Error GoTo err

                Dim i As Integer

                Dim iCheck As Integer
                iCheck = val(getConfigVar("EMBDB_CHECK_CODE"))

                If iCheck = 0 Then Exit Sub

                If frmPen Is Nothing Then Exit Sub

                strErrorLoc = ""
                frmPen.txtSys.ControlTipText = ""
                frmPen.txtSys.BackColor = &H80000005

                Dim strProj As String
                strProj = getConfigVar("PW_AEP_PROJECT")

                Dim cn As New ADODB.Connection

                cn.Open "Provider=SQLOLEDB.1" & _
                    ";Persist Security Info=False;Timeout=3" & _
                    ";User ID=" & strDBUser & _
                    ";Initial Catalog=AECOSIM" & _
                    ";Data Source=" & strDBServer, strDBUser, strDBPass

                Dim RS As New ADODB.Recordset
                Dim strSQL As String
                Dim strTip As String


                strSQL = "select * from view_i_APP_Embedded_Part"
                strSQL = strSQL & " where Code = '" & strCode & "'"
                If Len(strProj) > 0 Then strSQL = strSQL & " and proj = '" & strProj & "'"

                RS.Open strSQL, cn, adOpenForwardOnly, adLockReadOnly

                If Not RS.EOF Then
                    RS.MoveFirst
                    frmPen.txtSys.BackColor = &HC0C0FF

                    For i = 0 To RS.Fields.Count - 1
                        If Len(strTip) > 0 Then strTip = strTip & " ; "
                        If LCase(Right(RS.Fields(i).name, 2)) <> "id" And Len(RS.Fields(i).name) = 1 Then
                            strTip = strTip & RS.Fields(i).name & "=" & RS.Fields(i).value
                        ElseIf LCase(Right(RS.Fields(i).name, 2)) <> "id" Then
                            strTip = strTip & RS.Fields(i).value
                        End If

                        If RS.Fields(i).name = "X" Then strErrorLoc = RS.Fields(i).value
                        If RS.Fields(i).name = "Y" Then strErrorLoc = strErrorLoc & "," & RS.Fields(i).value
                        If RS.Fields(i).name = "Z" Then strErrorLoc = strErrorLoc & "," & RS.Fields(i).value


                    Next i

                    frmPen.txtSys.ControlTipText = strTip

                End If


                RS.Close
                Set RS = Nothing

                cn.Close
                Set cn = Nothing

                Exit Sub
            err:
            MsgBox err.Description

            End Sub

            */

            break;
        }
        }

        cell.ErrorText = errText;
        cell.Style.ForeColor = errText.Length > 0 ? 
            System.Drawing.Color.Red : System.Drawing.Color.Black;

        checkValidationState();

        return cell.ErrorText.Length == 0;
    }

    private bool checkValidationState()
    {
        bool isDataValid = true;
        dgvFields.Update();
        foreach (DataGridViewRow row in dgvFields.Rows)
        {   // проверка только по видимым строкам: ( кроме KKS)
            if (row.Visible && row.Cells[1].ErrorText.Length > 0 &&
                row.Index != (int)FieldType.KKS)
            {
                isDataValid = false;                 
                break;
            }
        }
        bool res = isDataValid && isReadyToPublishTrigger_;

        try
        {
            Addin.Instance.setCExpressionValue(CExpr.IS_READY_TO_PUBLISH, res);
        }
        catch (Exception)
        {
            Addin.Instance.MessageCenter.StatusPrompt = "<simpen.dll> не загружена";
        }

        btnAddToModel.Enabled = res;

        return res;
    }

    void rbtnMode_CheckedChanged(object sender, EventArgs e)
    {     
        RadioButton rb = (RadioButton)sender;
        rb.Font = new System.Drawing.Font(rb.Font, rb.Checked ? 
            System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular);

        refreshControlsDependsOnMode();
        runLocatingTool();
    }

    void refreshControlsDependsOnMode()
    {
        dgvFields.Rows[(int)FieldType.HEIGHT].Visible = 
        dgvFields.Rows[(int)FieldType.WIDTH].Visible = isTaskMode();
        dgvFields.Rows[(int)FieldType.LENGTH].Visible = !chbxPolicyThrough.Checked;

        chbxEdit.Checked = isContourMode();
        chbxRemoveContour.Visible = isContourMode();
       
        btnAddToModel.Visible = true;
        btnAddToModel.Enabled = false;
    }
    
    void chbxEdit_CheckedChanged(object sender, EventArgs e)
    {
        grbxParameters.Enabled = chbxEdit.Checked;
    }

    bool isTaskMode()
    {
        return getMode() == Mode.BY_TASK;
    }

    bool isContourMode()
    {
        return getMode() == Mode.BY_CONTOUR;
    }

    bool isPointsMode()
    {
        return getMode() == Mode.BY_POINTS;
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

        refreshControlsDependsOnMode();
        checkValidationState();
    }

    public void enableAddToModel()
    {
        isReadyToPublishTrigger_ = true;
        checkValidationState();        
    }

    public void readTaskData()
    {
        // TODO зачитываем геометрические параметры с шагом 5 мм;
        // построение элемента должно быть уже с учётом округления, 
        // + точка установки тоже корректируется

        // TODO проверить проходит ли валидация при зачитывании данных из задания

        readGeometryProperty(FieldType.HEIGHT);
        readGeometryProperty(FieldType.WIDTH);
        readGeometryProperty(FieldType.LENGTH);
        readProperty(FieldType.KKS);

        checkValidationState();
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

        sendProperty(FieldType.HEIGHT);
        sendProperty(FieldType.WIDTH);
        sendProperty(FieldType.LENGTH);
        sendProperty(FieldType.KKS);

        Addin.Instance.setCExpressionValue(CExpr.IS_POLICY_THROUGH,
            chbxPolicyThrough.Checked);

        if (isContourMode()) {
            Addin.Instance.setCExpressionValue(
            CExpr.IS_REQUIRED_REMOVE_CONTOUR, chbxRemoveContour.Checked);
        }

        checkValidationState();
    }


    private string getCExpression(FieldType fieldType)
    {
        // TODO ИСПЛ. только openingDistance, ...

        string result = fieldType == FieldType.HEIGHT ? CExpr.HEIGHT :
            fieldType == FieldType.WIDTH ? CExpr.WIDTH :
            fieldType == FieldType.LENGTH ? CExpr.DEPTH :
            fieldType == FieldType.KKS ? CExpr.KKS :
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
            case FieldType.HEIGHT:
            case FieldType.WIDTH:
            case FieldType.LENGTH:
                Addin.Instance.setCExpressionValue(cexpr, 0.0);
                break;
            default:
                Addin.Instance.setCExpressionValue(cexpr, "");
                break;
            }
        }
    }

    private void dgvFields_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 1)
            return;

        if (validateCell(dgvFields.Rows[e.RowIndex].Cells[1]))
        {
            sendProperty((FieldType)e.RowIndex);
        }
        dgvFields.RefreshEdit();
    }

    private void chbxThrough_CheckedChanged(object sender, EventArgs e)
    {
        dgvFields.Rows[(int)FieldType.LENGTH].Visible = !chbxPolicyThrough.Checked;

        Addin.Instance.setCExpressionValue(
            CExpr.IS_POLICY_THROUGH, chbxPolicyThrough.Checked);

        sendTaskData();
        sendKeyin("update preview");
    }
    
    private void btnLocate_Click(object sender, EventArgs e)
    {
        runLocatingTool();
    }

    private void OpeningForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        Sets.mode = (int)getMode();
        Sets.isPolicyThrough = chbxPolicyThrough.Checked;
        Sets.isRequiredRemoveConture = chbxRemoveContour.Checked;
        Sets.Save();
    }

    private void sendKeyin(string smallCmdName)
    {
        Addin.Instance.sendKeyin("openings " + smallCmdName);
    }

    public void updateAllOpenings()
    {
        
        
    }
}
}
