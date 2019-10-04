using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;

namespace FreeJoyConfigurator
{

    public enum ReportID : byte
    {
        JOY_REPORT = 1,

    };

    class JoyReport
    {
        const byte ReportId = (byte) ReportID.JOY_REPORT;
        public Joystick Joystick;

        public JoyReport(HidReport hr)
        {
            Joystick = new Joystick();

            if (hr != null)
            {

                for (int i=0; i<128; i++)
                {
                    Joystick.Buttons[i] = (hr.Data[1 + (i & 0xF8)>>3] & (1<<i & 0x07)) > 0 ? true : false;
                }

                for (int i=0; i<8; i++)
                {
                    Joystick.Axis[i] = (ushort) (hr.Data[17 + 2 * i] << 8 |  hr.Data[18 + 2 * i]);
                }
                
            }
        }
    }
}
