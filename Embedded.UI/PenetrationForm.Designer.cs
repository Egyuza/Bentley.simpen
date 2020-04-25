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
            this.dgvFields = new System.Windows.Forms.DataGridView();
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
            this.tlpSets.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFields)).BeginInit();
            this.tlpMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.grbxTasks.SuspendLayout();
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
            this.tlpSets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 141F));
            this.tlpSets.Size = new System.Drawing.Size(406, 141);
            this.tlpSets.TabIndex = 1;
            // 
            // dgvFields
            // 
            this.dgvFields.AllowUserToAddRows = false;
            this.dgvFields.AllowUserToDeleteRows = false;
            this.dgvFields.AllowUserToResizeRows = false;
            this.dgvFields.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvFields.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvFields.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvFields.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvFields.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
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
            this.dgvFields.EnableHeadersVisualStyles = false;
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
            this.dgvFields.Size = new System.Drawing.Size(406, 140);
            this.dgvFields.TabIndex = 2;
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tlpMain.Controls.Add(this.grbxTasks, 0, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(418, 271);
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
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 169);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(412, 99);
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
            this.grbxTasks.Size = new System.Drawing.Size(412, 160);
            this.grbxTasks.TabIndex = 7;
            this.grbxTasks.TabStop = false;
            this.grbxTasks.Text = "Задания:";
            // 
            // PenetrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 271);
            this.Controls.Add(this.tlpMain);
            this.Name = "PenetrationForm";
            this.Text = "Проходки";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PenetrationForm_FormClosed);
            this.tlpSets.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFields)).EndInit();
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.grbxTasks.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tlpSets;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.DataGridView dgvFields;
        private System.Windows.Forms.GroupBox grbxTasks;
        private System.Windows.Forms.Button btnAddToModel;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.CheckBox chboxEdit;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSelectionCount;
        private System.Windows.Forms.Label lblBreak;
    }
}