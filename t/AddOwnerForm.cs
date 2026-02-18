using System.Text.RegularExpressions;

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

    private void TryOk()
    {
        var surname = (tbSurname.Text ?? "").Trim();
        var firstName = (tbFirstName.Text ?? "").Trim();
        var lastName = (tbLastName.Text ?? "").Trim();
        var phone = NormalizePhone((tbPhone.Text ?? "").Trim());

        if (surname.Length < 2)
        {
            MessageBox.Show("Введите фамилию.");
            tbSurname.Focus();
            return;
        }

        if (firstName.Length < 2)
        {
            MessageBox.Show("Введите имя.");
            tbFirstName.Focus();
            return;
        }

        
        if (!string.IsNullOrWhiteSpace(phone) && phone.Length < 7)
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
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone
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

public sealed class OwnerCreateDto
{
    public string Surname { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }  
    public string? Phone { get; set; }
}
