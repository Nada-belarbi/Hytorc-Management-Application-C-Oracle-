using System.Windows.Forms;
using System.Drawing;

namespace UUM_Hytroc
{
    public class ColoredListItem
    {
        public string Text { get; set; }
        public Color TextColor { get; set; }

        public ColoredListItem(string text, Color textColor)
        {
            Text = text;
            TextColor = textColor;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
