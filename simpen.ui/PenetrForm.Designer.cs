namespace simpen.ui
{
    partial class PenetrForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpSets = new System.Windows.Forms.TableLayoutPanel();
            this.dgvFields = new System.Windows.Forms.DataGridView();
            this.Label = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.chbxEdit = new System.Windows.Forms.CheckBox();
            this.btnLocate = new System.Windows.Forms.Button();
            this.btnAddToModel = new System.Windows.Forms.Button();
            this.grbxParameters = new System.Windows.Forms.GroupBox();
            this.tlpSets.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFields)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.grbxParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpSets
            // 
            this.tlpSets.ColumnCount = 1;
            this.tlpSets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSets.Controls.Add(this.dgvFields, 0, 0);
            this.tlpSets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSets.Location = new System.Drawing.Point(3, 16);
            this.tlpSets.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSets.Name = "tlpSets";
            this.tlpSets.RowCount = 1;
            this.tlpSets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpSets.Size = new System.Drawing.Size(273, 182);
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
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvFields.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvFields.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvFields.Location = new System.Drawing.Point(0, 1);
            this.dgvFields.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.dgvFields.MultiSelect = false;
            this.dgvFields.Name = "dgvFields";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvFields.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dgvFields.RowHeadersVisible = false;
            this.dgvFields.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            this.dgvFields.RowTemplate.Height = 20;
            this.dgvFields.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvFields.Size = new System.Drawing.Size(273, 181);
            this.dgvFields.TabIndex = 2;
            this.dgvFields.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvFields_CellValueChanged);
            this.dgvFields.EnabledChanged += new System.EventHandler(this.dgvFields_EnabledChanged);
            // 
            // Label
            // 
            this.Label.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            this.Label.DefaultCellStyle = dataGridViewCellStyle7;
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
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tlpMain.Controls.Add(this.grbxParameters, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
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
            this.btnAddToModel.MinimumSize = new System.Drawing.Size(75, 0);
            this.btnAddToModel.Name = "btnAddToModel";
            this.btnAddToModel.Size = new System.Drawing.Size(75, 23);
            this.btnAddToModel.TabIndex = 6;
            this.btnAddToModel.Text = "Создать";
            this.btnAddToModel.UseVisualStyleBackColor = true;
            this.btnAddToModel.Visible = false;
            this.btnAddToModel.Click += new System.EventHandler(this.btnAddToModel_Click);
            // 
            // grbxParameters
            // 
            this.grbxParameters.Controls.Add(this.tlpSets);
            this.grbxParameters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbxParameters.Location = new System.Drawing.Point(3, 3);
            this.grbxParameters.Name = "grbxParameters";
            this.grbxParameters.Size = new System.Drawing.Size(279, 201);
            this.grbxParameters.TabIndex = 7;
            this.grbxParameters.TabStop = false;
            this.grbxParameters.Text = "Параметры:";
            // 
            // PenetrForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 271);
            this.Controls.Add(this.tlpMain);
            this.Name = "PenetrForm";
            this.Text = "Проходки круглые";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OpeningForm_FormClosed);
            this.tlpSets.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFields)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.grbxParameters.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tlpSets;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox chbxEdit;
        private System.Windows.Forms.DataGridView dgvFields;
        private System.Windows.Forms.DataGridViewTextBoxColumn Label;
        private System.Windows.Forms.DataGridViewTextBoxColumn Value;
        private System.Windows.Forms.GroupBox grbxParameters;
        private System.Windows.Forms.Button btnLocate;
        public System.Windows.Forms.Button btnAddToModel;
    }
}