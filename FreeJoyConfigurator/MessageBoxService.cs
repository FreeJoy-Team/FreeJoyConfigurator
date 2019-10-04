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
        MessageBoxResult ShowMessage(string text, string caption, MessageType messageType);
    }

    class MessageBoxService : IMessageBoxService
    {
        public MessageBoxResult ShowMessage(string text, string caption, MessageType messageType)
        {
            return MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
