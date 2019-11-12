using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MessageBoxServicing
{

    interface IMessageBoxService
    {
        MessageBoxResult ShowMessage(string text, string caption);
    }

    class MessageBoxService : IMessageBoxService
    {
        public MessageBoxResult ShowMessage(string text, string caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public MessageBoxResult ShowMessage(string text, string caption, MessageBoxButton button)
        {
            return MessageBox.Show(text, caption, button, MessageBoxImage.Information);
        }

        public MessageBoxResult ShowMessage(string text, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            return MessageBox.Show(text, caption, button, image);
        }
    }
}
