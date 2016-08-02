namespace NetworkDynamics
{
    partial class MatrixForm
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
            this.rtbMat = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rtbMat
            // 
            this.rtbMat.Font = new System.Drawing.Font("Open Sans", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbMat.Location = new System.Drawing.Point(12, 12);
            this.rtbMat.Name = "rtbMat";
            this.rtbMat.Size = new System.Drawing.Size(724, 430);
            this.rtbMat.TabIndex = 0;
            this.rtbMat.Text = "";
            this.rtbMat.WordWrap = false;
            // 
            // MatrixForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(752, 455);
            this.Controls.Add(this.rtbMat);
            this.Name = "MatrixForm";
            this.Text = "MatrixForm";
            this.SizeChanged += new System.EventHandler(this.MatrixForm_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbMat;
    }
}