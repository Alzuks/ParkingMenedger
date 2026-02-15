using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Parking.Operator.WinForms
{
    public partial class VehicleRegistrationForm : Form
    {
        public long? PassageId { get; set; }
        public string? PlateNorm { get; set; }

        public VehicleRegistrationForm()
        {
            InitializeComponent();



        }
    }
}
