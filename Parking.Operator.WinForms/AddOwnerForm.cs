using System.Text.RegularExpressions;
using Parking.Operator.WinForms.Models;


namespace Parking.Operator.WinForms;

public partial class AddOwnerForm : Form
{
    public OwnerCreateDto Result { get; private set; } = new();

    public AddOwnerForm()
    {
        InitializeComponent();

        // ESC закрывает без сохранения
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        };

        // Кнопки
        btnOk.Click += (_, __) => TryOk();
        btnCancel.Click += (_, __) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        // UX: Enter = OK
        AcceptButton = btnOk;
        CancelButton = btnCancel;

        tbSurname.Focus();
    }

    public AddOwnerForm(OwnerItemDto owner) : this()
    {
        Text = "Редактировать владельца";

        tbSurname.Text = owner.Surname;
        tbFirstName.Text = owner.FirstName;
        tbLastName.Text = owner.LastName ?? "";
        tbPhone.Text = owner.Phone ?? "";
        tbAddress.Text = owner.ResidentialAddress ?? "";
    }
    private void TryOk()
    {
        var surname = (tbSurname.Text ?? "").Trim();
        var firstName = (tbFirstName.Text ?? "").Trim();
        var lastName = (tbLastName.Text ?? "").Trim();
        var phone = NormalizePhone((tbPhone.Text ?? "").Trim());
        var address = (tbAddress.Text ?? "").Trim();

        if (surname.Length < 2)
        {
            MessageBox.Show("Введите фамилию.");
            tbSurname.Focus();
            return;
        }
       
        if (!string.IsNullOrWhiteSpace(phone) && phone.Length < 5)
        {
            MessageBox.Show("Телефон слишком короткий.");
            tbPhone.Focus();
            return;
        }

        Result = new OwnerCreateDto
        {
            Surname = surname,
            FirstName = firstName,
            LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName,
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
            ResidentialAddress = string.IsNullOrWhiteSpace(address) ? null : address
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    private static string NormalizePhone(string s)
    {
        // оставляем цифры и +, всё остальное выкидываем
        var cleaned = Regex.Replace(s, @"[^\d+]", "");
        if (cleaned.Contains('+'))
            cleaned = "+" + cleaned.Replace("+", "");
        return cleaned;
    }
}
