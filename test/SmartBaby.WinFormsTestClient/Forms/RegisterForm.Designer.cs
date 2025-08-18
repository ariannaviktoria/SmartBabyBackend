namespace SmartBaby.WinFormsTestClient.Forms
{
    partial class RegisterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private TextBox txtFullName;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Button btnRegister;
        private Button btnCancel;
        private Label lblStatus;
        private Label lblTitle;
        private Label lblFullName;
        private Label lblEmail;
        private Label lblPassword;
        private Label lblConfirmPassword;

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
            this.txtFullName = new TextBox();
            this.txtEmail = new TextBox();
            this.txtPassword = new TextBox();
            this.txtConfirmPassword = new TextBox();
            this.btnRegister = new Button();
            this.btnCancel = new Button();
            this.lblStatus = new Label();
            this.lblTitle = new Label();
            this.lblFullName = new Label();
            this.lblEmail = new Label();
            this.lblPassword = new Label();
            this.lblConfirmPassword = new Label();
            this.SuspendLayout();
            
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(120, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(160, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Register User";
            
            // 
            // lblFullName
            // 
            this.lblFullName.AutoSize = true;
            this.lblFullName.Location = new Point(50, 70);
            this.lblFullName.Name = "lblFullName";
            this.lblFullName.Size = new Size(64, 15);
            this.lblFullName.TabIndex = 1;
            this.lblFullName.Text = "Full Name:";
            
            // 
            // txtFullName
            // 
            this.txtFullName.Location = new Point(50, 90);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Size = new Size(300, 23);
            this.txtFullName.TabIndex = 2;
            
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new Point(50, 130);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new Size(39, 15);
            this.lblEmail.TabIndex = 3;
            this.lblEmail.Text = "Email:";
            
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new Point(50, 150);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new Size(300, 23);
            this.txtEmail.TabIndex = 4;
            
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new Point(50, 190);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new Size(60, 15);
            this.lblPassword.TabIndex = 5;
            this.lblPassword.Text = "Password:";
            
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new Point(50, 210);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new Size(300, 23);
            this.txtPassword.TabIndex = 6;
            
            // 
            // lblConfirmPassword
            // 
            this.lblConfirmPassword.AutoSize = true;
            this.lblConfirmPassword.Location = new Point(50, 250);
            this.lblConfirmPassword.Name = "lblConfirmPassword";
            this.lblConfirmPassword.Size = new Size(107, 15);
            this.lblConfirmPassword.TabIndex = 7;
            this.lblConfirmPassword.Text = "Confirm Password:";
            
            // 
            // txtConfirmPassword
            // 
            this.txtConfirmPassword.Location = new Point(50, 270);
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.PasswordChar = '*';
            this.txtConfirmPassword.Size = new Size(300, 23);
            this.txtConfirmPassword.TabIndex = 8;
            
            // 
            // btnRegister
            // 
            this.btnRegister.BackColor = Color.FromArgb(0, 120, 215);
            this.btnRegister.ForeColor = Color.White;
            this.btnRegister.Location = new Point(50, 320);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new Size(140, 35);
            this.btnRegister.TabIndex = 9;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Click += new EventHandler(this.btnRegister_Click);
            
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = Color.FromArgb(40, 40, 40);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.Location = new Point(210, 320);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(140, 35);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new Point(50, 380);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(0, 15);
            this.lblStatus.TabIndex = 11;
            
            // 
            // RegisterForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.ClientSize = new Size(400, 420);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.txtConfirmPassword);
            this.Controls.Add(this.lblConfirmPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.txtFullName);
            this.Controls.Add(this.lblFullName);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RegisterForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Register New User";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
