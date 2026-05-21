namespace TinyTrack.WinForms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    // Bu blokta form kaynaklarını birlikte temizliyoruz.
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    // Bu blokta Designer'ın görebileceği temel form ayarlarını birlikte hazırlıyoruz.
    private void InitializeComponent()
    {
        SuspendLayout();
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(9F, 23F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(241, 246, 250);
        ClientSize = new Size(1280, 820);
        Font = new Font("Segoe UI", 10F);
        MinimumSize = new Size(920, 680);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "TinyTrack ERP";
        Load += MainForm_Load;
        ResumeLayout(false);
    }
}
