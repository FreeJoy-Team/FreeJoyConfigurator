using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeJoyConfigurator
{
    public class Joystick
    {
        public UInt16[] Axis;
        public bool[] Buttons;
        public byte[] Povs;

        public Joystick()
        {
            Axis = new ushort[8];
            Buttons = new bool[128];
            Povs = new byte[4];
        }

    }
}
