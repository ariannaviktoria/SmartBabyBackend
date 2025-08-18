using SmartBaby.Core.DTOs;
using System.Text;
using System.Text.Json;

namespace SmartBaby.WinFormsTestClient.Forms;

public partial class LoginForm : Form
{
    private readonly HttpClient _httpClient;
    private const string API_BASE_URL = "https://localhost:55362/api/";

    public LoginForm()
    {
        InitializeComponent();
        
        // Configure HttpClient for development with SSL bypass
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        
        _httpClient = new HttpClient(handler);
        _httpClient.BaseAddress = new Uri(API_BASE_URL);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    private async void btnLogin_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            MessageBox.Show("Email is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Password is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnLogin.Enabled = false;
        btnRegister.Enabled = false;
        lblStatus.Text = "Logging in...";
        lblStatus.ForeColor = Color.Blue;

        try
        {
            var loginDto = new LoginDto
            {
                Email = txtEmail.Text.Trim(),
                Password = txtPassword.Text
            };

            var json = JsonSerializer.Serialize(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("auth/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenDto = JsonSerializer.Deserialize<TokenDto>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                
                if (tokenDto != null && !string.IsNullOrEmpty(tokenDto.Token))
                {
                    lblStatus.Text = "Login successful!";
                    lblStatus.ForeColor = Color.Green;
                    
                    // Open the main analysis form
                    var mainForm = new MainAnalysisForm(tokenDto.Token, txtEmail.Text.Trim());
                    this.Hide();
                    mainForm.ShowDialog();
                    this.Close();
                }
                else
                {
                    lblStatus.Text = "Invalid response from server";
                    lblStatus.ForeColor = Color.Red;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                lblStatus.Text = $"Login failed: {response.StatusCode}";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Login failed: {errorContent}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Login error occurred";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show($"Login error: {ex.Message}\n\nMake sure the SmartBaby API is running on https://localhost:55362", 
                          "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnLogin.Enabled = true;
            btnRegister.Enabled = true;
        }
    }

    private void btnRegister_Click(object sender, EventArgs e)
    {
        using var registerForm = new RegisterForm(_httpClient);
        if (registerForm.ShowDialog() == DialogResult.OK)
        {
            lblStatus.Text = "Registration successful! You can now login.";
            lblStatus.ForeColor = Color.Green;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _httpClient?.Dispose();
        }
        base.Dispose(disposing);
    }
}
