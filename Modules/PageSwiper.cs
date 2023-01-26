using System.Windows.Controls;

namespace FFmpegGUI.Modules
{
    internal class PageSwiper
    {
        private Frame _frame;

        public PageSwiper(Frame frame)
        {
            Initialize(frame);
        }

        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public void SetPanel(object panel)
        {
            _frame.Content = panel;
        }
    }
}
