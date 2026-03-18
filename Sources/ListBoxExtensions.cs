using System.Windows.Forms;
using System.Drawing;

namespace UUM_Hytroc
{
    public static class ListBoxExtensions
    {
        public static void SetItemTextColor(this ListBox lb, int index, Color color)
        {
            lb.BeginUpdate();
            lb.Items[index] = new ColoredListItem(lb.Items[index].ToString(), color);
            lb.EndUpdate();
        }
    }
}
