using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class ButtonVM
    {
        public bool State { get; set; }
        public ButtonType Type { get; set; }

        public ButtonVM(bool state, ButtonType type)
        {
            this.State = state;
            this.Type = type;
        }
    }
}
