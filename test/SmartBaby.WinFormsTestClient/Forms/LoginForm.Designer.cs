namespace SmartBaby.WinFormsTestClient.Forms
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnRegister;
        private Label lblStatus;
        private Label lblTitle;
        private Label lblEmail;
        private Label lblPassword;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtEmail = new TextBox();
            this.txtPassword = new TextBox();
            this.btnLogin = new Button();
            this.btnRegister = new Button();
            this.lblStatus = new Label();
            this.lblTitle = new Label();
            this.lblEmail = new Label();
            this.lblPassword = new Label();
            this.SuspendLayout();
            
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(80, 30);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(240, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "SmartBaby Test Client";
            
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new Point(50, 90);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new Size(39, 15);
            this.lblEmail.TabIndex = 1;
            this.lblEmail.Text = "Email:";
            
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new Point(50, 110);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new Size(300, 23);
            this.txtEmail.TabIndex = 2;
            this.txtEmail.Text = "gm@gm.com";
            
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new Point(50, 150);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new Size(60, 15);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Password:";
            
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new Point(50, 170);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new Size(300, 23);
            this.txtPassword.TabIndex = 4;
            this.txtPassword.Text = "Passwd123!";
            
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = Color.FromArgb(0, 120, 215);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.Location = new Point(50, 220);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new Size(140, 35);
            this.btnLogin.TabIndex = 5;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new EventHandler(this.btnLogin_Click);
            
            // 
            // btnRegister
            // 
            this.btnRegister.BackColor = Color.FromArgb(40, 40, 40);
            this.btnRegister.ForeColor = Color.White;
            this.btnRegister.Location = new Point(210, 220);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new Size(140, 35);
            this.btnRegister.TabIndex = 6;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = false;
            this.btnRegister.Click += new EventHandler(this.btnRegister_Click);
            
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new Point(50, 280);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(0, 15);
            this.lblStatus.TabIndex = 7;
            
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.ClientSize = new Size(400, 320);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblEmail);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "SmartBaby Login";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
