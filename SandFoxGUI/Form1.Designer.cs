namespace SandFoxGUI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnInject = new Button();
            btnUnload = new Button();
            SuspendLayout();
            // 
            // btnInject
            // 
            btnInject.Location = new Point(43, 12);
            btnInject.Name = "btnInject";
            btnInject.Size = new Size(223, 43);
            btnInject.TabIndex = 0;
            btnInject.Text = "Inject";
            btnInject.UseVisualStyleBackColor = true;
            btnInject.Click += OnInjectButtonClick;
            // 
            // btnUnload
            // 
            btnUnload.Location = new Point(43, 61);
            btnUnload.Name = "btnUnload";
            btnUnload.Size = new Size(223, 43);
            btnUnload.TabIndex = 1;
            btnUnload.Text = "Unload";
            btnUnload.UseVisualStyleBackColor = true;
            btnUnload.Click += OnUnloadButtonClick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(306, 140);
            Controls.Add(btnInject);
            Controls.Add(btnUnload);
            Name = "Form1";
            Text = "SandFoxGUI";
            ResumeLayout(false);
        }

        private System.Windows.Forms.Button btnInject;
        private System.Windows.Forms.Button btnUnload;
    }
}

