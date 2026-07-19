using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Parking.Operator.WinForms.Models;

namespace Parking.Operator.WinForms;

public partial class LoginForm : Form
{
    private readonly ApiClient _api;

    public LoginResultDto? Result { get; private set; }
    public bool IsSuper { get; private set; }

    public LoginForm(ApiClient api)
    {
        InitializeComponent();

        _api = api;

        AcceptButton = btnOk;

        cbRole.DropDownStyle = ComboBoxStyle.DropDownList;
        tbPass.UseSystemPasswordChar = true;

        Shown += async (_, __) => await LoadRolesAsync();
        btnOk.Click += async (_, __) => await LoginAsync();
    }

    private async Task LoadRolesAsync()
    {
        try
        {
            var roles = await _api.GetLoginRolesAsync(CancellationToken.None);

            cbRole.DataSource = null;
            cbRole.DisplayMember = nameof(LoginRoleDto.Name);
            cbRole.ValueMember = nameof(LoginRoleDto.RoleId);
            cbRole.DataSource = roles;

            var operatorRole = roles.FirstOrDefault(r => r.Code == "OPERATOR");
            if (operatorRole != null)
                cbRole.SelectedValue = operatorRole.RoleId;
        }
        catch (ApiException ex)
        {
            MessageBox.Show(
                string.IsNullOrWhiteSpace(ex.Body) ? ex.Message : ex.Body,
                "Ошибка API");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка");
        }
    }

    private async Task LoginAsync()
    {
        if (cbRole.SelectedValue is not long roleId)
        {
            MessageBox.Show("Выберите тип входа.");
            return;
        }

        var pass = (tbPass.Text ?? "").Trim();

        if (string.IsNullOrWhiteSpace(pass))
        {
            MessageBox.Show("Введите пароль.");
            tbPass.Focus();
            return;
        }

        try
        {
            var result = await _api.LoginAsync(new LoginRequestDto
            {
                RoleId = roleId,
                Passcode = pass
            }, CancellationToken.None);

            if (result == null)
                return;

            Result = result;

            var code = (result.RoleCode ?? "").Trim().ToUpperInvariant();

            IsSuper =
                code == "SUPER" ||
                code == "ADMIN" ||
                result.CanEditDictionaries;

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ApiException ex)
        {
            MessageBox.Show(
                string.IsNullOrWhiteSpace(ex.Body) ? ex.Message : ex.Body,
                "Ошибка входа");

            tbPass.SelectAll();
            tbPass.Focus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка");
        }
    }
}