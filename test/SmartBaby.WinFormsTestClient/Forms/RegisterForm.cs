using SmartBaby.Core.DTOs;
using System.Text;
using System.Text.Json;

namespace SmartBaby.WinFormsTestClient.Forms;

public partial class RegisterForm : Form
{
    private readonly HttpClient _httpClient;

    public RegisterForm(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
    }

    private async void btnRegister_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtFullName.Text))
        {
            MessageBox.Show("Full name is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            MessageBox.Show("Email is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text.Length < 6)
        {
            MessageBox.Show("Password must be at least 6 characters", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (txtPassword.Text != txtConfirmPassword.Text)
        {
            MessageBox.Show("Passwords do not match", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnRegister.Enabled = false;
        btnCancel.Enabled = false;
        lblStatus.Text = "Registering...";
        lblStatus.ForeColor = Color.Blue;

        try
        {
            var userDto = new UserDto
            {
                FullName = txtFullName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Password = txtPassword.Text
            };

            var json = JsonSerializer.Serialize(userDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("auth/register", content);
            
            if (response.IsSuccessStatusCode)
            {
                lblStatus.Text = "Registration successful!";
                lblStatus.ForeColor = Color.Green;
                MessageBox.Show("Registration successful! You can now login with your credentials.", 
                              "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                lblStatus.Text = $"Registration failed: {response.StatusCode}";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Registration failed: {errorContent}", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Registration error occurred";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show($"Registration error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnRegister.Enabled = true;
            btnCancel.Enabled = true;
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }
}
