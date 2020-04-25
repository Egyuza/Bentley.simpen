using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Embedded.UI;

using Shared.Bentley;

namespace Embedded.Penetrations.Shared
{
public class PenetrationVM
{
    private static PenetrationVM instance_;    
    private static bool isDebugMode_;

#if V8i
    private static Bentley.MicroStation.AddIn addin_;

    public static PenetrationVM getInstance(
        Bentley.MicroStation.AddIn addin, string unparsed)
    {
        addin_ = addin;
        isDebugMode_ = unparsed == null ? 
            false : unparsed.Equals("DEBUG", StringComparison.OrdinalIgnoreCase);

        return instance_ ?? (instance_ =
            new PenetrationVM(new PenetrationModel(addin)));
    }
#elif CONNECT
    private static Bentley.MstnPlatformNET.AddIn addin_;
    public static PenetrationVM getInstance(
        Bentley.MstnPlatformNET.AddIn addin, string unparsed)
    {
        addin_ = addin;
        isDebugMode_ = unparsed == null ? 
            false : unparsed.Equals("DEBUG", StringComparison.OrdinalIgnoreCase);

        return instance_ ?? (instance_ =
            new PenetrationVM(new PenetrationModel(addin)));
    }
#endif

    private PenetrationVM(PenetrationModel model)
    {
        penModel_ = model;
        penModel_.PropertyChanged += PenModel__PropertyChanged;

        initializeForm();
    }

    private void PenModel__PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var pname = e.PropertyName;
    }
    
    public void showForm()
    {
        initializeForm();
        WindowHelper.show(form_, "simpen::Penetration");
    }

    private void initializeForm()
    {
        if (form_ != null && !form_.IsDisposed) {
            return;
        }
        form_ = new PenetrationForm();
        form_.Text = "Проходки" /* TODO версия*/ 
            + (isDebugMode_ ? " [DEBUG]" : string.Empty);

        if (!isDebugMode_ && !penModel_.isProjectDefined)
        {
            form_.setStatusText(
                "Проект не определён - создание проходок не доступно");
            form_.setReadOnly();
        }

        form_.setColumns(getColumns_());

        form_.setDataSource(penModel_.TaskSelection);
        form_.setBinding("lblSelectionCount", "Text",
            penModel_, PenetrationModel.NP.SelectionCount);

        form_.setDataRowsAddedAction(rowsAdded);
        form_.setPreviewAction(penModel_.preview);
        form_.setCreateAction(penModel_.create);
    }

    private void rowsAdded(IEnumerable<DataGridViewRow> rows)
    {
        foreach (var row in rows)
        {
            var comboCell = 
                row.Cells[ColumnName.DIAMETER] as DataGridViewComboBoxCell;

            PenetrTask task = (PenetrTask)row.DataBoundItem;

            var diameters = penModel_.getDiametersList(task);
            var diamStrList = new List<string>();
            DiameterType matchValue = null;
            foreach (DiameterType diamType in diameters)
            {        
                if (diamType.number == 
                    DiameterType.Parse(task.DiameterTypeStr).number)
                {
                    matchValue = diamType;
                }
                diamStrList.Add(diamType.ToString());
            }
            
            comboCell.DataSource = diamStrList;
            if (matchValue != null) {
                comboCell.Value = matchValue.ToString();
            }
            else {
                comboCell.ErrorText = "невалидное значение диаметра";
            }
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
        column.DataPropertyName = "Name";
        column.Name = ColumnName.TYPE_SIZE;
        column.ReadOnly = true;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        columns.Add(column);

        var cmboxColumn = new DataGridViewComboBoxColumn();
        column = cmboxColumn; // НВС
        cmboxColumn.DataPropertyName = "FlangesType";
        cmboxColumn.Name = ColumnName.FLANGES;
        cmboxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        cmboxColumn.DataSource = penModel_.getFlangeNumbersSort();
        columns.Add(cmboxColumn);
        
        column = new DataGridViewComboBoxColumn();
        column.DataPropertyName = "DiameterTypeStr";
        column.Name = ColumnName.DIAMETER;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        columns.Add(column);
        
        column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = "Length";
        column.Name = ColumnName.LENGTH;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        columns.Add(column);

        column = new DataGridViewTextBoxColumn();
        column.DataPropertyName = 
        column.Name = ColumnName.REF_POINT1;
        column.ReadOnly = true;
        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        columns.Add(column);

        return columns;
    }

    private PenetrationModel penModel_;
    private PenetrationForm form_;

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
