namespace Embedded.Openings.UI
{
    partial class OpeningFormOld
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpSets = new System.Windows.Forms.TableLayoutPanel();
            this.dgvFields = new System.Windows.Forms.DataGridView();
            this.Label = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tlpRefPoint = new System.Windows.Forms.TableLayoutPanel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.chbxRemoveContour = new System.Windows.Forms.CheckBox();
            this.chbxPolicyThrough = new System.Windows.Forms.CheckBox();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.chbxEdit = new System.Windows.Forms.CheckBox();
            this.btnLocate = new System.Windows.Forms.Button();
            this.btnAddToModel = new System.Windows.Forms.Button();
            this.flpMode = new System.Windows.Forms.FlowLayoutPanel();
            this.rbtnModeTask = new System.Windows.Forms.RadioButton();
            this.rbtnModeContour = new System.Windows.Forms.RadioButton();
            this.grbxParameters = new System.Windows.Forms.GroupBox();
            this.tlpSets.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFields)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tlpRefPoint.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flpMode.SuspendLayout();
            this.grbxParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpSets
            // 
            this.tlpSets.ColumnCount = 1;
            this.tlpSets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSets.Controls.Add(this.dgvFields, 0, 0);
            this.tlpSets.Controls.Add(this.tableLayoutPanel1, 0, 1);
            this.tlpSets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSets.Location = new System.Drawing.Point(3, 16);
            this.tlpSets.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSets.Name = "tlpSets";
            this.tlpSets.RowCount = 2;
            this.tlpSets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSets.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpSets.Size = new System.Drawing.Size(273, 159);
            this.tlpSets.TabIndex = 1;
            // 
            // dgvFields
            // 
            this.dgvFields.AllowUserToAddRows = false;
            this.dgvFields.AllowUserToDeleteRows = false;
            this.dgvFields.AllowUserToResizeColumns = false;
            this.dgvFields.AllowUserToResizeRows = false;
            this.dgvFields.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvFields.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvFields.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dgvFields.ColumnHeadersVisible = false;
            this.dgvFields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Label,
            this.Value});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFields.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFields.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvFields.Location = new System.Drawing.Point(0, 1);
            this.dgvFields.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.dgvFields.MultiSelect = false;
            this.dgvFields.Name = "dgvFields";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvFields.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvFields.RowHeadersVisible = false;
            this.dgvFields.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            this.dgvFields.RowTemplate.Height = 20;
            this.dgvFields.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFields.Size = new System.Drawing.Size(273, 109);
            this.dgvFields.TabIndex = 2;
            this.dgvFields.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFields_CellValueChanged);
            this.dgvFields.EnabledChanged += new System.EventHandler(this.dgvFields_EnabledChanged);
            // 
            // Label
            // 
            this.Label.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            this.Label.DefaultCellStyle = dataGridViewCellStyle1;
            this.Label.HeaderText = "Метка";
            this.Label.Name = "Label";
            this.Label.ReadOnly = true;
            this.Label.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Label.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Label.Width = 5;
            // 
            // Value
            // 
            this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Value.HeaderText = "Значение";
            this.Value.Name = "Value";
            this.Value.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tlpRefPoint, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.chbxRemoveContour, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.chbxPolicyThrough, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 110);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(273, 49);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // tlpRefPoint
            // 
            this.tlpRefPoint.AutoSize = true;
            this.tlpRefPoint.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlpRefPoint.ColumnCount = 6;
            this.tableLayoutPanel1.SetColumnSpan(this.tlpRefPoint, 2);
            this.tlpRefPoint.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRefPoint.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpRefPoint.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRefPoint.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpRefPoint.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpRefPoint.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tlpRefPoint.Controls.Add(this.textBox1, 1, 0);
            this.tlpRefPoint.Controls.Add(this.label1, 0, 0);
            this.tlpRefPoint.Controls.Add(this.label2, 2, 0);
            this.tlpRefPoint.Controls.Add(this.label3, 4, 0);
            this.tlpRefPoint.Controls.Add(this.textBox2, 3, 0);
            this.tlpRefPoint.Controls.Add(this.textBox3, 5, 0);
            this.tlpRefPoint.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpRefPoint.Location = new System.Drawing.Point(0, 0);
            this.tlpRefPoint.Margin = new System.Windows.Forms.Padding(0);
            this.tlpRefPoint.Name = "tlpRefPoint";
            this.tlpRefPoint.RowCount = 1;
            this.tlpRefPoint.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpRefPoint.Size = new System.Drawing.Size(273, 26);
            this.tlpRefPoint.TabIndex = 2;
            this.tlpRefPoint.Visible = false;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(20, 3);
            this.textBox1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(71, 20);
            this.textBox1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 6, 0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "X:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(94, 6);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 6, 0, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Y:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(185, 6);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 6, 0, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Z:";
            // 
            // textBox2
            // 
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox2.Location = new System.Drawing.Point(111, 3);
            this.textBox2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(71, 20);
            this.textBox2.TabIndex = 4;
            // 
            // textBox3
            // 
            this.textBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox3.Location = new System.Drawing.Point(202, 3);
            this.textBox3.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(71, 20);
            this.textBox3.TabIndex = 5;
            // 
            // chbxRemoveContour
            // 
            this.chbxRemoveContour.AutoSize = true;
            this.chbxRemoveContour.Checked = true;
            this.chbxRemoveContour.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbxRemoveContour.Location = new System.Drawing.Point(83, 29);
            this.chbxRemoveContour.Name = "chbxRemoveContour";
            this.chbxRemoveContour.Size = new System.Drawing.Size(103, 17);
            this.chbxRemoveContour.TabIndex = 3;
            this.chbxRemoveContour.Text = "удалять контур";
            this.chbxRemoveContour.UseVisualStyleBackColor = true;
            // 
            // chbxPolicyThrough
            // 
            this.chbxPolicyThrough.AutoSize = true;
            this.chbxPolicyThrough.Checked = true;
            this.chbxPolicyThrough.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbxPolicyThrough.Location = new System.Drawing.Point(3, 29);
            this.chbxPolicyThrough.Name = "chbxPolicyThrough";
            this.chbxPolicyThrough.Size = new System.Drawing.Size(74, 17);
            this.chbxPolicyThrough.TabIndex = 2;
            this.chbxPolicyThrough.Text = "насквозь";
            this.chbxPolicyThrough.UseVisualStyleBackColor = true;
            this.chbxPolicyThrough.CheckedChanged += new System.EventHandler(this.chbxThrough_CheckedChanged);
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tlpMain.Controls.Add(this.flpMode, 0, 0);
            this.tlpMain.Controls.Add(this.grbxParameters, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 3;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.Size = new System.Drawing.Size(285, 271);
            this.tlpMain.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.chbxEdit);
            this.flowLayoutPanel1.Controls.Add(this.btnLocate);
            this.flowLayoutPanel1.Controls.Add(this.btnAddToModel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 210);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(279, 58);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // chbxEdit
            // 
            this.chbxEdit.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chbxEdit, true);
            this.chbxEdit.Location = new System.Drawing.Point(3, 3);
            this.chbxEdit.Name = "chbxEdit";
            this.chbxEdit.Size = new System.Drawing.Size(162, 17);
            this.chbxEdit.TabIndex = 3;
            this.chbxEdit.Text = "редактировать параметры";
            this.chbxEdit.UseVisualStyleBackColor = true;
            this.chbxEdit.CheckedChanged += new System.EventHandler(this.chbxEdit_CheckedChanged);
            // 
            // btnLocate
            // 
            this.btnLocate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLocate.Location = new System.Drawing.Point(3, 32);
            this.btnLocate.Name = "btnLocate";
            this.btnLocate.Size = new System.Drawing.Size(75, 23);
            this.btnLocate.TabIndex = 7;
            this.btnLocate.Text = "Выбрать";
            this.btnLocate.UseVisualStyleBackColor = true;
            this.btnLocate.Click += new System.EventHandler(this.btnLocate_Click);
            // 
            // btnAddToModel
            // 
            this.btnAddToModel.AutoSize = true;
            this.btnAddToModel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddToModel.Location = new System.Drawing.Point(84, 32);
            this.btnAddToModel.Name = "btnAddToModel";
            this.btnAddToModel.Size = new System.Drawing.Size(94, 23);
            this.btnAddToModel.TabIndex = 6;
            this.btnAddToModel.Text = "Создать проём";
            this.btnAddToModel.UseVisualStyleBackColor = true;
            this.btnAddToModel.Visible = false;
            this.btnAddToModel.Click += new System.EventHandler(this.btnAddToModel_Click);
            // 
            // flpMode
            // 
            this.flpMode.AutoSize = true;
            this.flpMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flpMode.Controls.Add(this.rbtnModeTask);
            this.flpMode.Controls.Add(this.rbtnModeContour);
            this.flpMode.Dock = System.Windows.Forms.DockStyle.Top;
            this.flpMode.Location = new System.Drawing.Point(0, 0);
            this.flpMode.Margin = new System.Windows.Forms.Padding(0);
            this.flpMode.Name = "flpMode";
            this.flpMode.Size = new System.Drawing.Size(285, 23);
            this.flpMode.TabIndex = 6;
            // 
            // rbtnModeTask
            // 
            this.rbtnModeTask.AutoSize = true;
            this.rbtnModeTask.Checked = true;
            this.rbtnModeTask.Location = new System.Drawing.Point(3, 3);
            this.rbtnModeTask.Name = "rbtnModeTask";
            this.rbtnModeTask.Size = new System.Drawing.Size(84, 17);
            this.rbtnModeTask.TabIndex = 0;
            this.rbtnModeTask.TabStop = true;
            this.rbtnModeTask.Text = "по заданию";
            this.rbtnModeTask.UseVisualStyleBackColor = true;
            this.rbtnModeTask.CheckedChanged += new System.EventHandler(this.rbtnMode_CheckedChanged);
            // 
            // rbtnModeContour
            // 
            this.rbtnModeContour.AutoSize = true;
            this.rbtnModeContour.Location = new System.Drawing.Point(93, 3);
            this.rbtnModeContour.Name = "rbtnModeContour";
            this.rbtnModeContour.Size = new System.Drawing.Size(79, 17);
            this.rbtnModeContour.TabIndex = 1;
            this.rbtnModeContour.Text = "по контуру";
            this.rbtnModeContour.UseVisualStyleBackColor = true;
            this.rbtnModeContour.CheckedChanged += new System.EventHandler(this.rbtnMode_CheckedChanged);
            // 
            // grbxParameters
            // 
            this.grbxParameters.Controls.Add(this.tlpSets);
            this.grbxParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbxParameters.Location = new System.Drawing.Point(3, 26);
            this.grbxParameters.Name = "grbxParameters";
            this.grbxParameters.Size = new System.Drawing.Size(279, 178);
            this.grbxParameters.TabIndex = 7;
            this.grbxParameters.TabStop = false;
            this.grbxParameters.Text = "Параметры:";
            // 
            // OpeningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 271);
            this.Controls.Add(this.tlpMain);
            this.Name = "OpeningForm";
            this.Text = "Проёмы";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OpeningForm_FormClosed);
            this.tlpSets.ResumeLayout(false);
            this.tlpSets.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFields)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tlpRefPoint.ResumeLayout(false);
            this.tlpRefPoint.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flpMode.ResumeLayout(false);
            this.flpMode.PerformLayout();
            this.grbxParameters.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tlpSets;
        private System.Windows.Forms.CheckBox chbxPolicyThrough;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox chbxEdit;
        private System.Windows.Forms.FlowLayoutPanel flpMode;
        private System.Windows.Forms.RadioButton rbtnModeTask;
        private System.Windows.Forms.RadioButton rbtnModeContour;
        private System.Windows.Forms.DataGridView dgvFields;
        private System.Windows.Forms.DataGridViewTextBoxColumn Label;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private System.Windows.Forms.GroupBox grbxParameters;
        private System.Windows.Forms.Button btnLocate;
        private System.Windows.Forms.CheckBox chbxRemoveContour;
        public System.Windows.Forms.Button btnAddToModel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tlpRefPoint;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
    }
}