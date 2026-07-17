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

namespace Parking.Operator.WinForms
{

    public partial class AddStatus : Form
    {
        public WatchlistTypeCreateDto Result { get; private set; } = new();

        private string? _selectedColor;

        public AddStatus()
        {
            InitializeComponent();

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            btnOk.Click += (_, __) => TryOk();
            btnCancel.Click += (_, __) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            btnColor.Click += (_, __) => SelectColor();

            tbStatus.Focus();
        }
        private void SelectColor()
        {
            if (cdColor.ShowDialog(this) != DialogResult.OK)
                return;

            _selectedColor = ColorTranslator.ToHtml(cdColor.Color);

            btnColor.BackColor = cdColor.Color;
            btnColor.Text = _selectedColor;
        }

        private void TryOk()
        {
            var name = (tbStatus.Text ?? "").Trim();
            var code = (tbCode.Text ?? "").Trim();
            var text = (tbText.Text ?? "").Trim();

            if (name.Length < 2)
            {
                MessageBox.Show("Введите название статуса.");
                tbStatus.Focus();
                return;
            }

            if (code.Length < 2)
            {
                MessageBox.Show("Введите кодовое обозначение.");
                tbCode.Focus();
                return;
            }

            // Код лучше без пробелов, чтобы потом не ловить дурь в логике.
            if (code.Any(char.IsWhiteSpace))
            {
                MessageBox.Show("Кодовое обозначение не должно содержать пробелы.");
                tbCode.Focus();
                return;
            }

            Result = new WatchlistTypeCreateDto
            {
                Name = name,
                Code = code,

                AllowUnrestrictedAccess = chbUnrestrict.Checked,
                UnlimitedStay = chbUnlimit.Checked,

                OperatorMessage = string.IsNullOrWhiteSpace(text) ? null : text,
                StampColor = string.IsNullOrWhiteSpace(_selectedColor) ? null : _selectedColor
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

