using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProyectoFinalED
{
    public class ProgressBarMorada : ProgressBar
    {
        public ProgressBarMorada()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;

            e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), rec);

            rec.Width = rec.Width * Value / Maximum;
            e.Graphics.FillRectangle(new SolidBrush(Color.MediumPurple), rec);
        }
    }
}
