namespace SmartBaby.WinFormsTestClient.Forms
{
    partial class MainAnalysisForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        private Label lblUserInfo;
        private Label lblStatus;
        private Label lblConnectionStatus;
        private Label lblCameraStatus;
        private Label lblSessionId;
        private Label lblCurrentAnalysis;
        private Label lblEmotionResult;
        private Label lblCryResult;
        private Label lblFusionResult;
        private Label lblAlertLevel;
        private Button btnStartAnalysis;
        private Button btnStopAnalysis;
        private Button btnCameraPreview;
        private Button btnClearLog;
        private CheckBox chkCameraPreview;
        private PictureBox pictureBoxCamera;
        private TextBox txtAnalysisLog;
        private GroupBox groupBoxStatus;
        private GroupBox groupBoxResults;
        private GroupBox groupBoxCamera;
        private GroupBox groupBoxLog;

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
            this.lblUserInfo = new Label();
            this.lblStatus = new Label();
            this.lblConnectionStatus = new Label();
            this.lblCameraStatus = new Label();
            this.lblSessionId = new Label();
            this.lblCurrentAnalysis = new Label();
            this.lblEmotionResult = new Label();
            this.lblCryResult = new Label();
            this.lblFusionResult = new Label();
            this.lblAlertLevel = new Label();
            this.btnStartAnalysis = new Button();
            this.btnStopAnalysis = new Button();
            this.btnCameraPreview = new Button();
            this.btnClearLog = new Button();
            this.chkCameraPreview = new CheckBox();
            this.pictureBoxCamera = new PictureBox();
            this.txtAnalysisLog = new TextBox();
            this.groupBoxStatus = new GroupBox();
            this.groupBoxResults = new GroupBox();
            this.groupBoxCamera = new GroupBox();
            this.groupBoxLog = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCamera)).BeginInit();
            this.groupBoxStatus.SuspendLayout();
            this.groupBoxResults.SuspendLayout();
            this.groupBoxCamera.SuspendLayout();
            this.groupBoxLog.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // lblUserInfo
            // 
            this.lblUserInfo.AutoSize = true;
            this.lblUserInfo.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblUserInfo.Location = new Point(20, 15);
            this.lblUserInfo.Name = "lblUserInfo";
            this.lblUserInfo.Size = new Size(200, 21);
            this.lblUserInfo.TabIndex = 0;
            this.lblUserInfo.Text = "Logged in as: [User]";
            
            // 
            // groupBoxStatus
            // 
            this.groupBoxStatus.Controls.Add(this.lblStatus);
            this.groupBoxStatus.Controls.Add(this.lblConnectionStatus);
            this.groupBoxStatus.Controls.Add(this.lblCameraStatus);
            this.groupBoxStatus.Controls.Add(this.lblSessionId);
            this.groupBoxStatus.Location = new Point(20, 50);
            this.groupBoxStatus.Name = "groupBoxStatus";
            this.groupBoxStatus.Size = new Size(350, 120);
            this.groupBoxStatus.TabIndex = 1;
            this.groupBoxStatus.TabStop = false;
            this.groupBoxStatus.Text = "System Status";
            
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new Point(15, 25);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(120, 15);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Status: Initializing...";
            
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Location = new Point(15, 45);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new Size(120, 15);
            this.lblConnectionStatus.TabIndex = 1;
            this.lblConnectionStatus.Text = "SignalR: Connecting...";
            
            // 
            // lblCameraStatus
            // 
            this.lblCameraStatus.AutoSize = true;
            this.lblCameraStatus.Location = new Point(15, 65);
            this.lblCameraStatus.Name = "lblCameraStatus";
            this.lblCameraStatus.Size = new Size(100, 15);
            this.lblCameraStatus.TabIndex = 2;
            this.lblCameraStatus.Text = "Camera: Initializing...";
            
            // 
            // lblSessionId
            // 
            this.lblSessionId.AutoSize = true;
            this.lblSessionId.Location = new Point(15, 85);
            this.lblSessionId.Name = "lblSessionId";
            this.lblSessionId.Size = new Size(80, 15);
            this.lblSessionId.TabIndex = 3;
            this.lblSessionId.Text = "Session: None";
            
            // 
            // groupBoxResults
            // 
            this.groupBoxResults.Controls.Add(this.lblCurrentAnalysis);
            this.groupBoxResults.Controls.Add(this.lblEmotionResult);
            this.groupBoxResults.Controls.Add(this.lblCryResult);
            this.groupBoxResults.Controls.Add(this.lblFusionResult);
            this.groupBoxResults.Controls.Add(this.lblAlertLevel);
            this.groupBoxResults.Location = new Point(390, 50);
            this.groupBoxResults.Name = "groupBoxResults";
            this.groupBoxResults.Size = new Size(350, 120);
            this.groupBoxResults.TabIndex = 2;
            this.groupBoxResults.TabStop = false;
            this.groupBoxResults.Text = "Analysis Results";
            
            // 
            // lblCurrentAnalysis
            // 
            this.lblCurrentAnalysis.AutoSize = true;
            this.lblCurrentAnalysis.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblCurrentAnalysis.Location = new Point(15, 25);
            this.lblCurrentAnalysis.Name = "lblCurrentAnalysis";
            this.lblCurrentAnalysis.Size = new Size(100, 15);
            this.lblCurrentAnalysis.TabIndex = 0;
            this.lblCurrentAnalysis.Text = "Latest: None";
            
            // 
            // lblEmotionResult
            // 
            this.lblEmotionResult.AutoSize = true;
            this.lblEmotionResult.Location = new Point(15, 45);
            this.lblEmotionResult.Name = "lblEmotionResult";
            this.lblEmotionResult.Size = new Size(100, 15);
            this.lblEmotionResult.TabIndex = 1;
            this.lblEmotionResult.Text = "Emotion: -";
            
            // 
            // lblCryResult
            // 
            this.lblCryResult.AutoSize = true;
            this.lblCryResult.Location = new Point(15, 65);
            this.lblCryResult.Name = "lblCryResult";
            this.lblCryResult.Size = new Size(60, 15);
            this.lblCryResult.TabIndex = 2;
            this.lblCryResult.Text = "Crying: -";
            
            // 
            // lblFusionResult
            // 
            this.lblFusionResult.AutoSize = true;
            this.lblFusionResult.Location = new Point(15, 85);
            this.lblFusionResult.Name = "lblFusionResult";
            this.lblFusionResult.Size = new Size(60, 15);
            this.lblFusionResult.TabIndex = 3;
            this.lblFusionResult.Text = "Overall: -";
            
            // 
            // lblAlertLevel
            // 
            this.lblAlertLevel.AutoSize = true;
            this.lblAlertLevel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblAlertLevel.Location = new Point(200, 85);
            this.lblAlertLevel.Name = "lblAlertLevel";
            this.lblAlertLevel.Size = new Size(80, 15);
            this.lblAlertLevel.TabIndex = 4;
            this.lblAlertLevel.Text = "Alert: Normal";
            
            // 
            // btnStartAnalysis
            // 
            this.btnStartAnalysis.BackColor = Color.FromArgb(0, 120, 215);
            this.btnStartAnalysis.Enabled = false;
            this.btnStartAnalysis.ForeColor = Color.White;
            this.btnStartAnalysis.Location = new Point(20, 190);
            this.btnStartAnalysis.Name = "btnStartAnalysis";
            this.btnStartAnalysis.Size = new Size(120, 40);
            this.btnStartAnalysis.TabIndex = 3;
            this.btnStartAnalysis.Text = "Start Analysis";
            this.btnStartAnalysis.UseVisualStyleBackColor = false;
            this.btnStartAnalysis.Click += new EventHandler(this.btnStartAnalysis_Click);
            
            // 
            // btnStopAnalysis
            // 
            this.btnStopAnalysis.BackColor = Color.FromArgb(200, 50, 50);
            this.btnStopAnalysis.Enabled = false;
            this.btnStopAnalysis.ForeColor = Color.White;
            this.btnStopAnalysis.Location = new Point(160, 190);
            this.btnStopAnalysis.Name = "btnStopAnalysis";
            this.btnStopAnalysis.Size = new Size(120, 40);
            this.btnStopAnalysis.TabIndex = 4;
            this.btnStopAnalysis.Text = "Stop Analysis";
            this.btnStopAnalysis.UseVisualStyleBackColor = false;
            this.btnStopAnalysis.Click += new EventHandler(this.btnStopAnalysis_Click);
            
            // 
            // groupBoxCamera
            // 
            this.groupBoxCamera.Controls.Add(this.pictureBoxCamera);
            this.groupBoxCamera.Controls.Add(this.btnCameraPreview);
            this.groupBoxCamera.Controls.Add(this.chkCameraPreview);
            this.groupBoxCamera.Location = new Point(390, 190);
            this.groupBoxCamera.Name = "groupBoxCamera";
            this.groupBoxCamera.Size = new Size(350, 280);
            this.groupBoxCamera.TabIndex = 5;
            this.groupBoxCamera.TabStop = false;
            this.groupBoxCamera.Text = "Camera Preview";
            
            // 
            // pictureBoxCamera
            // 
            this.pictureBoxCamera.BackColor = Color.Black;
            this.pictureBoxCamera.BorderStyle = BorderStyle.FixedSingle;
            this.pictureBoxCamera.Location = new Point(15, 25);
            this.pictureBoxCamera.Name = "pictureBoxCamera";
            this.pictureBoxCamera.Size = new Size(320, 240);
            this.pictureBoxCamera.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBoxCamera.TabIndex = 0;
            this.pictureBoxCamera.TabStop = false;
            
            // 
            // btnCameraPreview
            // 
            this.btnCameraPreview.Enabled = false;
            this.btnCameraPreview.Location = new Point(15, 275);
            this.btnCameraPreview.Name = "btnCameraPreview";
            this.btnCameraPreview.Size = new Size(100, 30);
            this.btnCameraPreview.TabIndex = 1;
            this.btnCameraPreview.Text = "Start Preview";
            this.btnCameraPreview.UseVisualStyleBackColor = true;
            this.btnCameraPreview.Click += new EventHandler(this.btnCameraPreview_Click);
            
            // 
            // chkCameraPreview
            // 
            this.chkCameraPreview.AutoSize = true;
            this.chkCameraPreview.Location = new Point(130, 280);
            this.chkCameraPreview.Name = "chkCameraPreview";
            this.chkCameraPreview.Size = new Size(120, 19);
            this.chkCameraPreview.TabIndex = 2;
            this.chkCameraPreview.Text = "Continuous Preview";
            this.chkCameraPreview.UseVisualStyleBackColor = true;
            
            // 
            // groupBoxLog
            // 
            this.groupBoxLog.Controls.Add(this.txtAnalysisLog);
            this.groupBoxLog.Controls.Add(this.btnClearLog);
            this.groupBoxLog.Location = new Point(20, 250);
            this.groupBoxLog.Name = "groupBoxLog";
            this.groupBoxLog.Size = new Size(350, 220);
            this.groupBoxLog.TabIndex = 6;
            this.groupBoxLog.TabStop = false;
            this.groupBoxLog.Text = "Analysis Log";
            
            // 
            // txtAnalysisLog
            // 
            this.txtAnalysisLog.BackColor = Color.FromArgb(30, 30, 30);
            this.txtAnalysisLog.ForeColor = Color.White;
            this.txtAnalysisLog.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            this.txtAnalysisLog.Location = new Point(15, 25);
            this.txtAnalysisLog.Multiline = true;
            this.txtAnalysisLog.Name = "txtAnalysisLog";
            this.txtAnalysisLog.ReadOnly = true;
            this.txtAnalysisLog.ScrollBars = ScrollBars.Vertical;
            this.txtAnalysisLog.Size = new Size(320, 150);
            this.txtAnalysisLog.TabIndex = 0;
            
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new Point(15, 185);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new Size(100, 25);
            this.btnClearLog.TabIndex = 1;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new EventHandler(this.btnClearLog_Click);
            
            // 
            // MainAnalysisForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.White;
            this.ClientSize = new Size(760, 490);
            this.Controls.Add(this.groupBoxLog);
            this.Controls.Add(this.groupBoxCamera);
            this.Controls.Add(this.btnStopAnalysis);
            this.Controls.Add(this.btnStartAnalysis);
            this.Controls.Add(this.groupBoxResults);
            this.Controls.Add(this.groupBoxStatus);
            this.Controls.Add(this.lblUserInfo);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainAnalysisForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "SmartBaby Real-Time Analysis";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCamera)).EndInit();
            this.groupBoxStatus.ResumeLayout(false);
            this.groupBoxStatus.PerformLayout();
            this.groupBoxResults.ResumeLayout(false);
            this.groupBoxResults.PerformLayout();
            this.groupBoxCamera.ResumeLayout(false);
            this.groupBoxCamera.PerformLayout();
            this.groupBoxLog.ResumeLayout(false);
            this.groupBoxLog.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
