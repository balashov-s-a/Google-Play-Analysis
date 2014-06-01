namespace GooglePlay
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button1 = new System.Windows.Forms.Button();
            this.genreComboBox = new System.Windows.Forms.ComboBox();
            this.genreChartButton = new System.Windows.Forms.Button();
            this.commonChartButton = new System.Windows.Forms.Button();
            this.VerifyGenresButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea1);
            chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            chart1.Legends.Add(legend1);
            chart1.Location = new System.Drawing.Point(0, 0);
            chart1.Name = "chart1";
            chart1.Size = new System.Drawing.Size(559, 333);
            chart1.TabIndex = 0;
            chart1.Text = "chart1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.VerifyGenresButton);
            this.splitContainer1.Panel1.Controls.Add(this.button1);
            this.splitContainer1.Panel1.Controls.Add(this.genreComboBox);
            this.splitContainer1.Panel1.Controls.Add(this.genreChartButton);
            this.splitContainer1.Panel1.Controls.Add(this.commonChartButton);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(chart1);
            this.splitContainer1.Size = new System.Drawing.Size(559, 375);
            this.splitContainer1.SplitterDistance = 38;
            this.splitContainer1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(501, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(46, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Dump!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // genreComboBox
            // 
            this.genreComboBox.FormattingEnabled = true;
            this.genreComboBox.Location = new System.Drawing.Point(218, 8);
            this.genreComboBox.Name = "genreComboBox";
            this.genreComboBox.Size = new System.Drawing.Size(121, 21);
            this.genreComboBox.TabIndex = 2;
            // 
            // genreChartButton
            // 
            this.genreChartButton.Location = new System.Drawing.Point(345, 6);
            this.genreChartButton.Name = "genreChartButton";
            this.genreChartButton.Size = new System.Drawing.Size(103, 23);
            this.genreChartButton.TabIndex = 1;
            this.genreChartButton.Text = "genreChartButton";
            this.genreChartButton.UseVisualStyleBackColor = true;
            this.genreChartButton.Click += new System.EventHandler(this.genreChartButton_Click);
            // 
            // commonChartButton
            // 
            this.commonChartButton.Location = new System.Drawing.Point(12, 4);
            this.commonChartButton.Name = "commonChartButton";
            this.commonChartButton.Size = new System.Drawing.Size(120, 23);
            this.commonChartButton.TabIndex = 0;
            this.commonChartButton.Text = "commonChartButton";
            this.commonChartButton.UseVisualStyleBackColor = true;
            this.commonChartButton.Click += new System.EventHandler(this.commonChartButton_Click);
            // 
            // VerifyGenresButton
            // 
            this.VerifyGenresButton.Location = new System.Drawing.Point(170, 6);
            this.VerifyGenresButton.Name = "VerifyGenresButton";
            this.VerifyGenresButton.Size = new System.Drawing.Size(23, 23);
            this.VerifyGenresButton.TabIndex = 4;
            this.VerifyGenresButton.Text = "verify genres";
            this.VerifyGenresButton.UseVisualStyleBackColor = true;
            this.VerifyGenresButton.Click += new System.EventHandler(this.VerifyGenresButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 375);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(chart1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public static System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ComboBox genreComboBox;
        private System.Windows.Forms.Button genreChartButton;
        private System.Windows.Forms.Button commonChartButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button VerifyGenresButton;
    }
}

