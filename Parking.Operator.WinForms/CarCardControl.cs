using Parking.Operator.WinForms.Models;

namespace Parking.Operator.WinForms;

public partial class CarCardControl : UserControl
{
    public event EventHandler<long>? CardClick;
    public event EventHandler<long>? CardDoubleClick;

    public long passageId { get; private set; }

    public CarCardControl()
    {
        InitializeComponent();
        
        lblDebt.BackColor = Color.FromArgb(180, 120, 0, 0);

        // Чтобы клик срабатывал по любому месту карточки
        WireClickHandlers(this);
        WireClickHandlers(pnlBorder);
        WireClickHandlers(pnlContent);
        WireClickHandlers(pbPhoto);
        WireClickHandlers(lblPlate);
        WireClickHandlers(lblDebt);
    }

    private void WireClickHandlers(Control c)
    {
        c.Click += (_, __) => CardClick?.Invoke(this, passageId);
        c.DoubleClick += (_, __) => CardDoubleClick?.Invoke(this, passageId);
    }

    public void Bind(CarCardDto dto)
    {
        passageId = dto.passageId;

        lblPlate.Text = (dto.Plate);

        // Долг
        lblDebt.Visible = dto.Debt > 0;
        if (dto.Debt > 0)
            lblDebt.Text = $"ДОЛГ: {dto.Debt:0.00}";

        // Цвет рамки по статусу
        pnlBorder.BackColor =
            dto.IsVip ? Color.Gold :
            dto.Debt > 0 ? Color.Red :
            dto.IsExpiring ? Color.YellowGreen :
            Color.LimeGreen;

        // Фото пока не грузим тут — это будет отдельный метод SetImage
        // pbPhoto.Image = ...
    }

    public void SetImage(Image? image)
    {
        // Важно: освобождаем старую картинку, иначе утечки памяти
        var old = pbPhoto.Image;
        pbPhoto.Image = image;
        old?.Dispose();
    }

    public void Clear()
    {
        passageId = 0;
        lblPlate.Text = "-";
        lblDebt.Visible = false;
        pnlBorder.BackColor = Color.Gray;
        SetImage(null);
    }
}

