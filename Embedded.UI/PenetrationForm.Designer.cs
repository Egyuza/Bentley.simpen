namespace Embedded.UI
{
    partial class PenetrationForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tlpSets = new System.Windows.Forms.TableLayoutPanel();
            this.dgvCreationTasks = new System.Windows.Forms.DataGridView();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.lblSelectionCount = new System.Windows.Forms.Label();
            this.chboxEdit = new System.Windows.Forms.CheckBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnAddToModel = new System.Windows.Forms.Button();
            this.lblBreak = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.grbxTasks = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabCreate = new System.Windows.Forms.TabPage();
            this.tabUpdate = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.trvUpdate = new System.Windows.Forms.TreeView();
            this.tlpSets.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCreationTasks)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.grbxTasks.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabCreate.SuspendLayout();
            this.tabUpdate.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpSets
            // 
            this.tlpSets.ColumnCount = 1;
            this.tlpSets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSets.Controls.Add(this.dgvCreationTasks, 0, 0);
            this.tlpSets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSets.Location = new System.Drawing.Point(3, 16);
            this.tlpSets.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSets.Name = "tlpSets";
            this.tlpSets.RowCount = 1;
            this.tlpSets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 154F));
            this.tlpSets.Size = new System.Drawing.Size(477, 154);
            this.tlpSets.TabIndex = 1;
            // 
            // dgvCreate
            // 
            this.dgvCreationTasks.AllowUserToAddRows = false;
            this.dgvCreationTasks.AllowUserToDeleteRows = false;
            this.dgvCreationTasks.AllowUserToResizeRows = false;
            this.dgvCreationTasks.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvCreationTasks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvCreationTasks.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCreationTasks.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvCreationTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvCreationTasks.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvCreationTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCreationTasks.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvCreationTasks.EnableHeadersVisualStyles = false;
            this.dgvCreationTasks.Location = new System.Drawing.Point(0, 1);
            this.dgvCreationTasks.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.dgvCreationTasks.MultiSelect = false;
            this.dgvCreationTasks.Name = "dgvCreate";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCreationTasks.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvCreationTasks.RowHeadersVisible = false;
            this.dgvCreationTasks.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            this.dgvCreationTasks.RowTemplate.Height = 20;
            this.dgvCreationTasks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCreationTasks.Size = new System.Drawing.Size(477, 153);
            this.dgvCreationTasks.TabIndex = 2;
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tlpMain.Controls.Add(this.grbxTasks, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(489, 284);
            this.tlpMain.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.lblSelectionCount);
            this.flowLayoutPanel1.Controls.Add(this.chboxEdit);
            this.flowLayoutPanel1.Controls.Add(this.btnPreview);
            this.flowLayoutPanel1.Controls.Add(this.btnAddToModel);
            this.flowLayoutPanel1.Controls.Add(this.lblBreak);
            this.flowLayoutPanel1.Controls.Add(this.lblStatus);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 182);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(483, 99);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 6);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 6, 0, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Выбрано:";
            // 
            // lblSelectionCount
            // 
            this.lblSelectionCount.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.lblSelectionCount, true);
            this.lblSelectionCount.Location = new System.Drawing.Point(61, 6);
            this.lblSelectionCount.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
            this.lblSelectionCount.Name = "lblSelectionCount";
            this.lblSelectionCount.Size = new System.Drawing.Size(13, 13);
            this.lblSelectionCount.TabIndex = 11;
            this.lblSelectionCount.Text = "0";
            // 
            // chboxEdit
            // 
            this.chboxEdit.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.chboxEdit, true);
            this.chboxEdit.Location = new System.Drawing.Point(6, 28);
            this.chboxEdit.Margin = new System.Windows.Forms.Padding(6, 6, 3, 3);
            this.chboxEdit.Name = "chboxEdit";
            this.chboxEdit.Size = new System.Drawing.Size(162, 17);
            this.chboxEdit.TabIndex = 8;
            this.chboxEdit.Text = "редактировать параметры";
            this.chboxEdit.UseVisualStyleBackColor = true;
            this.chboxEdit.Visible = false;
            // 
            // btnPreview
            // 
            this.btnPreview.AutoSize = true;
            this.btnPreview.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPreview.Location = new System.Drawing.Point(3, 54);
            this.btnPreview.MinimumSize = new System.Drawing.Size(75, 0);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(75, 23);
            this.btnPreview.TabIndex = 7;
            this.btnPreview.Text = "Показать";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnAddToModel
            // 
            this.btnAddToModel.AutoSize = true;
            this.btnAddToModel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddToModel.Location = new System.Drawing.Point(84, 54);
            this.btnAddToModel.MinimumSize = new System.Drawing.Size(75, 0);
            this.btnAddToModel.Name = "btnAddToModel";
            this.btnAddToModel.Size = new System.Drawing.Size(75, 23);
            this.btnAddToModel.TabIndex = 6;
            this.btnAddToModel.Text = "Создать";
            this.btnAddToModel.UseVisualStyleBackColor = true;
            this.btnAddToModel.Click += new System.EventHandler(this.btnAddToModel_Click);
            // 
            // lblBreak
            // 
            this.lblBreak.AutoSize = true;
            this.flowLayoutPanel1.SetFlowBreak(this.lblBreak, true);
            this.lblBreak.Location = new System.Drawing.Point(165, 57);
            this.lblBreak.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.lblBreak.Name = "lblBreak";
            this.lblBreak.Size = new System.Drawing.Size(0, 13);
            this.lblBreak.TabIndex = 12;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblStatus.Location = new System.Drawing.Point(6, 83);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(92, 13);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "Сатусная строка";
            this.lblStatus.Visible = false;
            // 
            // grbxTasks
            // 
            this.grbxTasks.Controls.Add(this.tlpSets);
            this.grbxTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbxTasks.Location = new System.Drawing.Point(3, 3);
            this.grbxTasks.Name = "grbxTasks";
            this.grbxTasks.Size = new System.Drawing.Size(483, 173);
            this.grbxTasks.TabIndex = 7;
            this.grbxTasks.TabStop = false;
            this.grbxTasks.Text = "Задания:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabCreate);
            this.tabControl1.Controls.Add(this.tabUpdate);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(497, 310);
            this.tabControl1.TabIndex = 2;
            // 
            // tabCreate
            // 
            this.tabCreate.Controls.Add(this.tlpMain);
            this.tabCreate.Location = new System.Drawing.Point(4, 22);
            this.tabCreate.Name = "tabCreate";
            this.tabCreate.Size = new System.Drawing.Size(489, 284);
            this.tabCreate.TabIndex = 0;
            this.tabCreate.Text = "Построение";
            this.tabCreate.UseVisualStyleBackColor = true;
            // 
            // tabUpdate
            // 
            this.tabUpdate.Controls.Add(this.tableLayoutPanel1);
            this.tabUpdate.Location = new System.Drawing.Point(4, 22);
            this.tabUpdate.Name = "tabUpdate";
            this.tabUpdate.Size = new System.Drawing.Size(489, 284);
            this.tabUpdate.TabIndex = 1;
            this.tabUpdate.Text = "Обновление";
            this.tabUpdate.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.trvUpdate, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(489, 284);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.label2);
            this.flowLayoutPanel2.Controls.Add(this.label3);
            this.flowLayoutPanel2.Controls.Add(this.btnScan);
            this.flowLayoutPanel2.Controls.Add(this.btnUpdate);
            this.flowLayoutPanel2.Controls.Add(this.label4);
            this.flowLayoutPanel2.Controls.Add(this.label5);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 211);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(483, 70);
            this.flowLayoutPanel2.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 6);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 6, 0, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Найдено:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.flowLayoutPanel2.SetFlowBreak(this.label3, true);
            this.label3.Location = new System.Drawing.Point(60, 6);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "0";
            // 
            // btnScan
            // 
            this.btnScan.AutoSize = true;
            this.btnScan.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnScan.Location = new System.Drawing.Point(3, 25);
            this.btnScan.MinimumSize = new System.Drawing.Size(75, 0);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(83, 23);
            this.btnScan.TabIndex = 13;
            this.btnScan.Text = "Сканировать";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.AutoSize = true;
            this.btnUpdate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUpdate.Location = new System.Drawing.Point(92, 25);
            this.btnUpdate.MinimumSize = new System.Drawing.Size(75, 0);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 6;
            this.btnUpdate.Text = "Обновить";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.flowLayoutPanel2.SetFlowBreak(this.label4, true);
            this.label4.Location = new System.Drawing.Point(173, 28);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label5.Location = new System.Drawing.Point(6, 54);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Сатусная строка";
            this.label5.Visible = false;
            // 
            // trvUpdate
            // 
            this.trvUpdate.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.trvUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvUpdate.Location = new System.Drawing.Point(3, 3);
            this.trvUpdate.Name = "trvUpdate";
            this.trvUpdate.Size = new System.Drawing.Size(483, 202);
            this.trvUpdate.TabIndex = 6;
            // 
            // PenetrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 310);
            this.Controls.Add(this.tabControl1);
            this.Name = "PenetrationForm";
            this.Text = "Проходки";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PenetrationForm_FormClosed);
            this.tlpSets.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCreationTasks)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.grbxTasks.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabCreate.ResumeLayout(false);
            this.tabUpdate.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tlpSets;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox grbxTasks;
        private System.Windows.Forms.Button btnAddToModel;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.CheckBox chboxEdit;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSelectionCount;
        private System.Windows.Forms.Label lblBreak;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabCreate;
        private System.Windows.Forms.TabPage tabUpdate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.TreeView trvUpdate;
        public System.Windows.Forms.DataGridView dgvCreationTasks;
    }
}