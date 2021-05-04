
namespace Embedded.Openings.UI
{
    partial class OpeningForm
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
            System.Windows.Forms.ToolStrip toolStrip1;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.statusProject = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.attrsInfoLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.attrsInfoText = new System.Windows.Forms.ToolStripLabel();
            this.tlpForm = new System.Windows.Forms.TableLayoutPanel();
            this.tabCtrlMain = new System.Windows.Forms.TabControl();
            this.tabCreate = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnStartPrimitive = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.tabGroupByTask = new System.Windows.Forms.TabPage();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.grbxTasks = new System.Windows.Forms.GroupBox();
            this.tlpSets = new System.Windows.Forms.TableLayoutPanel();
            this.dgvCreationTasks = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnLoadXmlAttributes = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lblSelectionCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddToModel = new System.Windows.Forms.Button();
            this.btnPreview = new System.Windows.Forms.Button();
            this.chboxEdit = new System.Windows.Forms.CheckBox();
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStrip1.SuspendLayout();
            this.tlpForm.SuspendLayout();
            this.tabCtrlMain.SuspendLayout();
            this.tabCreate.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.tabGroupByTask.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.grbxTasks.SuspendLayout();
            this.tlpSets.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCreationTasks)).BeginInit();
            this.panel1.SuspendLayout();
            this.tabUpdate.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            toolStrip1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            toolStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.statusProject,
            this.toolStripSeparator2,
            this.attrsInfoLabel,
            this.toolStripProgressBar1,
            this.attrsInfoText});
            toolStrip1.Location = new System.Drawing.Point(0, 335);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(585, 28);
            toolStrip1.TabIndex = 4;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(50, 25);
            this.toolStripLabel1.Text = "Проект:";
            // 
            // statusProject
            // 
            this.statusProject.ForeColor = System.Drawing.Color.Red;
            this.statusProject.Name = "statusProject";
            this.statusProject.Size = new System.Drawing.Size(84, 25);
            this.statusProject.Text = "Не определён";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            // 
            // attrsInfoLabel
            // 
            this.attrsInfoLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.attrsInfoLabel.Name = "attrsInfoLabel";
            this.attrsInfoLabel.Size = new System.Drawing.Size(98, 25);
            this.attrsInfoLabel.Text = "Файл атрибутов:";
            this.attrsInfoLabel.Visible = false;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(133, 25);
            this.toolStripProgressBar1.Visible = false;
            // 
            // attrsInfoText
            // 
            this.attrsInfoText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.attrsInfoText.ForeColor = System.Drawing.Color.OliveDrab;
            this.attrsInfoText.Name = "attrsInfoText";
            this.attrsInfoText.Size = new System.Drawing.Size(16, 25);
            this.attrsInfoText.Text = "...";
            this.attrsInfoText.Visible = false;
            // 
            // tlpForm
            // 
            this.tlpForm.BackColor = System.Drawing.SystemColors.Control;
            this.tlpForm.ColumnCount = 1;
            this.tlpForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpForm.Controls.Add(this.tabCtrlMain, 0, 0);
            this.tlpForm.Controls.Add(toolStrip1, 0, 1);
            this.tlpForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpForm.Location = new System.Drawing.Point(0, 0);
            this.tlpForm.Margin = new System.Windows.Forms.Padding(0);
            this.tlpForm.Name = "tlpForm";
            this.tlpForm.RowCount = 2;
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpForm.Size = new System.Drawing.Size(585, 363);
            this.tlpForm.TabIndex = 5;
            // 
            // tabCtrlMain
            // 
            this.tabCtrlMain.Controls.Add(this.tabCreate);
            this.tabCtrlMain.Controls.Add(this.tabGroupByTask);
            this.tabCtrlMain.Controls.Add(this.tabUpdate);
            this.tabCtrlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabCtrlMain.Location = new System.Drawing.Point(0, 0);
            this.tabCtrlMain.Margin = new System.Windows.Forms.Padding(0);
            this.tabCtrlMain.Name = "tabCtrlMain";
            this.tabCtrlMain.SelectedIndex = 0;
            this.tabCtrlMain.Size = new System.Drawing.Size(585, 335);
            this.tabCtrlMain.TabIndex = 2;
            // 
            // tabCreate
            // 
            this.tabCreate.Controls.Add(this.tableLayoutPanel2);
            this.tabCreate.Location = new System.Drawing.Point(4, 25);
            this.tabCreate.Margin = new System.Windows.Forms.Padding(4);
            this.tabCreate.Name = "tabCreate";
            this.tabCreate.Padding = new System.Windows.Forms.Padding(4);
            this.tabCreate.Size = new System.Drawing.Size(577, 306);
            this.tabCreate.TabIndex = 2;
            this.tabCreate.Text = "Построение";
            this.tabCreate.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(569, 298);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel3.Controls.Add(this.btnStartPrimitive);
            this.flowLayoutPanel3.Controls.Add(this.button2);
            this.flowLayoutPanel3.Controls.Add(this.button3);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(0, 264);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(569, 34);
            this.flowLayoutPanel3.TabIndex = 5;
            // 
            // btnStartPrimitive
            // 
            this.btnStartPrimitive.AutoSize = true;
            this.btnStartPrimitive.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnStartPrimitive.Enabled = false;
            this.btnStartPrimitive.Location = new System.Drawing.Point(4, 4);
            this.btnStartPrimitive.Margin = new System.Windows.Forms.Padding(4);
            this.btnStartPrimitive.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnStartPrimitive.Name = "btnStartPrimitive";
            this.btnStartPrimitive.Size = new System.Drawing.Size(100, 26);
            this.btnStartPrimitive.TabIndex = 7;
            this.btnStartPrimitive.Text = "Установить";
            this.btnStartPrimitive.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.AutoSize = true;
            this.button2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button2.Location = new System.Drawing.Point(112, 4);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.MinimumSize = new System.Drawing.Size(100, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 26);
            this.button2.TabIndex = 13;
            this.button2.Text = "Применить";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.AutoSize = true;
            this.button3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button3.Location = new System.Drawing.Point(220, 4);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.MinimumSize = new System.Drawing.Size(100, 0);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(190, 26);
            this.button3.TabIndex = 14;
            this.button3.Text = "Выйти из редактирования";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Visible = false;
            // 
            // tabGroupByTask
            // 
            this.tabGroupByTask.Controls.Add(this.tlpMain);
            this.tabGroupByTask.Location = new System.Drawing.Point(4, 25);
            this.tabGroupByTask.Margin = new System.Windows.Forms.Padding(4);
            this.tabGroupByTask.Name = "tabGroupByTask";
            this.tabGroupByTask.Size = new System.Drawing.Size(577, 306);
            this.tabGroupByTask.TabIndex = 0;
            this.tabGroupByTask.Text = "Групповой по заданию";
            this.tabGroupByTask.UseVisualStyleBackColor = true;
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 1;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.grbxTasks, 0, 0);
            this.tlpMain.Controls.Add(this.panel1, 0, 1);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 2;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tlpMain.Size = new System.Drawing.Size(577, 306);
            this.tlpMain.TabIndex = 1;
            // 
            // grbxTasks
            // 
            this.grbxTasks.Controls.Add(this.tlpSets);
            this.grbxTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grbxTasks.Location = new System.Drawing.Point(1, 1);
            this.grbxTasks.Margin = new System.Windows.Forms.Padding(1);
            this.grbxTasks.Name = "grbxTasks";
            this.grbxTasks.Padding = new System.Windows.Forms.Padding(4);
            this.grbxTasks.Size = new System.Drawing.Size(575, 235);
            this.grbxTasks.TabIndex = 7;
            this.grbxTasks.TabStop = false;
            this.grbxTasks.Text = "Задания:";
            // 
            // tlpSets
            // 
            this.tlpSets.ColumnCount = 1;
            this.tlpSets.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSets.Controls.Add(this.dgvCreationTasks, 0, 0);
            this.tlpSets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpSets.Location = new System.Drawing.Point(4, 19);
            this.tlpSets.Margin = new System.Windows.Forms.Padding(0);
            this.tlpSets.Name = "tlpSets";
            this.tlpSets.RowCount = 1;
            this.tlpSets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpSets.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 198F));
            this.tlpSets.Size = new System.Drawing.Size(567, 212);
            this.tlpSets.TabIndex = 1;
            // 
            // dgvCreationTasks
            // 
            this.dgvCreationTasks.AllowUserToAddRows = false;
            this.dgvCreationTasks.AllowUserToDeleteRows = false;
            this.dgvCreationTasks.AllowUserToResizeRows = false;
            this.dgvCreationTasks.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvCreationTasks.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvCreationTasks.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCreationTasks.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvCreationTasks.ColumnHeadersHeight = 29;
            this.dgvCreationTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvCreationTasks.DefaultCellStyle = dataGridViewCellStyle5;
            this.dgvCreationTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCreationTasks.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvCreationTasks.EnableHeadersVisualStyles = false;
            this.dgvCreationTasks.Location = new System.Drawing.Point(0, 1);
            this.dgvCreationTasks.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.dgvCreationTasks.MultiSelect = false;
            this.dgvCreationTasks.Name = "dgvCreationTasks";
            this.dgvCreationTasks.ReadOnly = true;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvCreationTasks.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dgvCreationTasks.RowHeadersVisible = false;
            this.dgvCreationTasks.RowHeadersWidth = 51;
            this.dgvCreationTasks.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            this.dgvCreationTasks.RowTemplate.Height = 20;
            this.dgvCreationTasks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCreationTasks.Size = new System.Drawing.Size(567, 211);
            this.dgvCreationTasks.TabIndex = 2;
            this.dgvCreationTasks.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DgvFields_DataError);
            this.dgvCreationTasks.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.DgvFields_RowsAdded);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Controls.Add(this.lblSelectionCount);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnAddToModel);
            this.panel1.Controls.Add(this.btnPreview);
            this.panel1.Controls.Add(this.chboxEdit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 237);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(577, 69);
            this.panel1.TabIndex = 8;
            // 
            // btnLoadXmlAttributes
            // 
            this.btnLoadXmlAttributes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLoadXmlAttributes.AutoSize = true;
            this.btnLoadXmlAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLoadXmlAttributes.Location = new System.Drawing.Point(110, 0);
            this.btnLoadXmlAttributes.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.btnLoadXmlAttributes.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnLoadXmlAttributes.Name = "btnLoadXmlAttributes";
            this.btnLoadXmlAttributes.Size = new System.Drawing.Size(126, 26);
            this.btnLoadXmlAttributes.TabIndex = 13;
            this.btnLoadXmlAttributes.Text = "Файл атрибутов";
            this.btnLoadXmlAttributes.UseVisualStyleBackColor = true;
            this.btnLoadXmlAttributes.Click += new System.EventHandler(this.btnLoadXmlAttributes_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.AutoSize = true;
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.Location = new System.Drawing.Point(3, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.button1.MinimumSize = new System.Drawing.Size(100, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(104, 26);
            this.button1.TabIndex = 12;
            this.button1.Text = "Сканировать";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            // 
            // lblSelectionCount
            // 
            this.lblSelectionCount.AutoSize = true;
            this.lblSelectionCount.Location = new System.Drawing.Point(73, 7);
            this.lblSelectionCount.Margin = new System.Windows.Forms.Padding(0, 7, 4, 4);
            this.lblSelectionCount.Name = "lblSelectionCount";
            this.lblSelectionCount.Size = new System.Drawing.Size(15, 16);
            this.lblSelectionCount.TabIndex = 11;
            this.lblSelectionCount.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 16);
            this.label1.TabIndex = 10;
            this.label1.Text = "Выбрано:";
            // 
            // btnAddToModel
            // 
            this.btnAddToModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddToModel.AutoSize = true;
            this.btnAddToModel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddToModel.Location = new System.Drawing.Point(474, 40);
            this.btnAddToModel.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.btnAddToModel.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnAddToModel.Name = "btnAddToModel";
            this.btnAddToModel.Size = new System.Drawing.Size(100, 26);
            this.btnAddToModel.TabIndex = 6;
            this.btnAddToModel.Text = "Создать";
            this.btnAddToModel.UseVisualStyleBackColor = true;
            this.btnAddToModel.Click += new System.EventHandler(this.btnAddToModel_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPreview.AutoSize = true;
            this.btnPreview.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPreview.Location = new System.Drawing.Point(371, 40);
            this.btnPreview.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.btnPreview.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(100, 26);
            this.btnPreview.TabIndex = 7;
            this.btnPreview.Text = "Просмотр";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // chboxEdit
            // 
            this.chboxEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chboxEdit.AutoSize = true;
            this.chboxEdit.Location = new System.Drawing.Point(363, 3);
            this.chboxEdit.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.chboxEdit.Name = "chboxEdit";
            this.chboxEdit.Size = new System.Drawing.Size(211, 20);
            this.chboxEdit.TabIndex = 8;
            this.chboxEdit.Text = "разрешить редактирование";
            this.chboxEdit.UseVisualStyleBackColor = true;
            this.chboxEdit.CheckedChanged += new System.EventHandler(this.chboxEdit_CheckedChanged);
            // 
            // tabUpdate
            // 
            this.tabUpdate.Controls.Add(this.tableLayoutPanel1);
            this.tabUpdate.Location = new System.Drawing.Point(4, 25);
            this.tabUpdate.Margin = new System.Windows.Forms.Padding(4);
            this.tabUpdate.Name = "tabUpdate";
            this.tabUpdate.Size = new System.Drawing.Size(577, 306);
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
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(577, 306);
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
            this.flowLayoutPanel2.Location = new System.Drawing.Point(4, 217);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(569, 85);
            this.flowLayoutPanel2.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(8, 7, 0, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 16);
            this.label2.TabIndex = 10;
            this.label2.Text = "Найдено:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.flowLayoutPanel2.SetFlowBreak(this.label3, true);
            this.label3.Location = new System.Drawing.Point(77, 7);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 7, 4, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 16);
            this.label3.TabIndex = 11;
            this.label3.Text = "0";
            // 
            // btnScan
            // 
            this.btnScan.AutoSize = true;
            this.btnScan.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnScan.Location = new System.Drawing.Point(4, 31);
            this.btnScan.Margin = new System.Windows.Forms.Padding(4);
            this.btnScan.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(104, 26);
            this.btnScan.TabIndex = 13;
            this.btnScan.Text = "Сканировать";
            this.btnScan.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            this.btnUpdate.AutoSize = true;
            this.btnUpdate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUpdate.Location = new System.Drawing.Point(116, 31);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(4);
            this.btnUpdate.MinimumSize = new System.Drawing.Size(100, 0);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(100, 26);
            this.btnUpdate.TabIndex = 6;
            this.btnUpdate.Text = "Обновить";
            this.btnUpdate.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.flowLayoutPanel2.SetFlowBreak(this.label4, true);
            this.label4.Location = new System.Drawing.Point(224, 34);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 7, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 16);
            this.label4.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label5.Location = new System.Drawing.Point(8, 65);
            this.label5.Margin = new System.Windows.Forms.Padding(8, 4, 4, 4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(118, 16);
            this.label5.TabIndex = 9;
            this.label5.Text = "Сатусная строка";
            this.label5.Visible = false;
            // 
            // trvUpdate
            // 
            this.trvUpdate.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.trvUpdate.CheckBoxes = true;
            this.trvUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvUpdate.Location = new System.Drawing.Point(4, 4);
            this.trvUpdate.Margin = new System.Windows.Forms.Padding(4);
            this.trvUpdate.Name = "trvUpdate";
            this.trvUpdate.Size = new System.Drawing.Size(569, 205);
            this.trvUpdate.TabIndex = 6;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.button1);
            this.flowLayoutPanel1.Controls.Add(this.btnLoadXmlAttributes);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(1, 40);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(236, 29);
            this.flowLayoutPanel1.TabIndex = 14;
            // 
            // OpeningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(585, 363);
            this.Controls.Add(this.tlpForm);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "OpeningForm";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Closed);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            this.tlpForm.ResumeLayout(false);
            this.tabCtrlMain.ResumeLayout(false);
            this.tabCreate.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.tabGroupByTask.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.grbxTasks.ResumeLayout(false);
            this.tlpSets.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCreationTasks)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabUpdate.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpForm;
        public System.Windows.Forms.TabControl tabCtrlMain;
        private System.Windows.Forms.TabPage tabCreate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        public System.Windows.Forms.Button btnStartPrimitive;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TabPage tabGroupByTask;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chboxEdit;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnAddToModel;
        private System.Windows.Forms.GroupBox grbxTasks;
        private System.Windows.Forms.TableLayoutPanel tlpSets;
        public System.Windows.Forms.DataGridView dgvCreationTasks;
        private System.Windows.Forms.TabPage tabUpdate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TreeView trvUpdate;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel statusProject;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel attrsInfoLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripLabel attrsInfoText;
        public System.Windows.Forms.Label lblSelectionCount;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnLoadXmlAttributes;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}