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
        CONFIG_REPORT,
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

    class ConfigReport
    {
        const byte ReportId = (byte)ReportID.CONFIG_REPORT;

        public ConfigReport (ref DeviceConfig config, HidReport hr)
        {
            if (hr.Data[0] == 1)
            {
                char[] chars = new char[10];

                config.FirmwareVersion = (ushort)(hr.Data[2] << 8 | hr.Data[1]);
                for (int i=0;i<10;i++)
                {
                    chars[i] = (char)hr.Data[i + 3];
                }
                config.DeviceName = new string(chars);
                config.ButtonDebounceMs = (ushort)(hr.Data[14] << 8 | hr.Data[13]);
                config.TogglePressMs = (ushort)(hr.Data[16] << 8 | hr.Data[15]);
                config.EncoderPressMs = (ushort)(hr.Data[18] << 8 | hr.Data[17]);
                config.ExchangePeriod = (ushort)(hr.Data[20] << 8 | hr.Data[19]);

                for (int i = 0; i < config.PinConfig.Length; i++)
                {
                    config.PinConfig[i] = (PinType)hr.Data[i + 32];
                }
            }
            else if (hr.Data[0] == 2)
            {
                config.AxisConfig[0] = new AxisConfig();
                config.AxisConfig[0].CalibMin = (ushort)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[0].CalibCenter = (ushort)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[0].CalibMax = (ushort)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[0].AutoCalib = Convert.ToBoolean(hr.Data[7]);
                config.AxisConfig[0].IsInverted = Convert.ToBoolean(hr.Data[8]);
                config.AxisConfig[0].FilterLevel = (AxisConfig.FilterLvl)hr.Data[9];
                for (int i = 0; i < 10; i++)
                {
                    config.AxisConfig[0].CurveShape[i] = hr.Data[10 + i];
                }

                config.AxisConfig[1] = new AxisConfig();
                config.AxisConfig[1].CalibMin = (ushort)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[1].CalibCenter = (ushort)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[1].CalibMax = (ushort)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[1].AutoCalib = Convert.ToBoolean(hr.Data[37]);
                config.AxisConfig[1].IsInverted = Convert.ToBoolean(hr.Data[38]);
                config.AxisConfig[1].FilterLevel = (AxisConfig.FilterLvl)hr.Data[39];
                for (int i = 0; i < 10; i++)
                {
                    config.AxisConfig[1].CurveShape[i] = hr.Data[40 + i];
                }

            }
            else if (hr.Data[0] == 3)
            {
                config.AxisConfig[2] = new AxisConfig();
                config.AxisConfig[2].CalibMin = (ushort)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[2].CalibCenter = (ushort)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[2].CalibMax = (ushort)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[2].AutoCalib = Convert.ToBoolean(hr.Data[7]);
                config.AxisConfig[2].IsInverted = Convert.ToBoolean(hr.Data[8]);
                config.AxisConfig[2].FilterLevel = (AxisConfig.FilterLvl)hr.Data[9];
                for (int i = 0; i < 10; i++)
                {
                    config.AxisConfig[2].CurveShape[i] = hr.Data[10 + i];
                }

                config.AxisConfig[3] = new AxisConfig();
                config.AxisConfig[3].CalibMin = (ushort)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[3].CalibCenter = (ushort)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[3].CalibMax = (ushort)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[3].AutoCalib = Convert.ToBoolean(hr.Data[37]);
                config.AxisConfig[3].IsInverted = Convert.ToBoolean(hr.Data[38]);
                config.AxisConfig[3].FilterLevel = (AxisConfig.FilterLvl)hr.Data[39];
                for (int i = 0; i < 10; i++)
                {
                    config.AxisConfig[3].CurveShape[i] = hr.Data[40 + i];
                }               
            }
            else if (hr.Data[0] == 4)
            {
                config.AxisConfig[4] = new AxisConfig();
                config.AxisConfig[4].CalibMin = (ushort)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[4].CalibCenter = (ushort)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[4].CalibMax = (ushort)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[4].AutoCalib = Convert.ToBoolean(hr.Data[7]);
                config.AxisConfig[4].IsInverted = Convert.ToBoolean(hr.Data[8]);
                config.AxisConfig[4].FilterLevel = (AxisConfig.FilterLvl)hr.Data[9];
                for (int i = 0; i < 10; i++)
                {
                    config.AxisConfig[4].CurveShape[i] = hr.Data[10 + i];
                }

                config.AxisConfig[5] = new AxisConfig();
                config.AxisConfig[5].CalibMin = (ushort)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[5].CalibCenter = (ushort)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[5].CalibMax = (ushort)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[5].AutoCalib = Convert.ToBoolean(hr.Data[37]);
                config.AxisConfig[5].IsInverted = Convert.ToBoolean(hr.Data[38]);
                config.AxisConfig[5].FilterLevel = (AxisConfig.FilterLvl)hr.Data[39];
                for (int i = 0; i < 10; i++)
                {
                    config.AxisConfig[5].CurveShape[i] = hr.Data[40 + i];
                }
            }
            else if (hr.Data[0] == 5)
            {
                config.AxisConfig[6] = new AxisConfig();
                config.AxisConfig[6].CalibMin = (ushort)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[6].CalibCenter = (ushort)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[6].CalibMax = (ushort)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[6].AutoCalib = Convert.ToBoolean(hr.Data[7]);
                config.AxisConfig[6].IsInverted = Convert.ToBoolean(hr.Data[8]);
                config.AxisConfig[6].FilterLevel = (AxisConfig.FilterLvl)hr.Data[9];
                for (int i = 0; i < 10; i++)
                {
                    config.AxisConfig[6].CurveShape[i] = hr.Data[10 + i];
                }

                config.AxisConfig[7] = new AxisConfig();
                config.AxisConfig[7].CalibMin = (ushort)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[7].CalibCenter = (ushort)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[7].CalibMax = (ushort)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[7].AutoCalib = Convert.ToBoolean(hr.Data[37]);
                config.AxisConfig[7].IsInverted = Convert.ToBoolean(hr.Data[38]);
                config.AxisConfig[7].FilterLevel = (AxisConfig.FilterLvl)hr.Data[39];
                for (int i = 0; i < 10; i++)
                {
                    config.AxisConfig[7].CurveShape[i] = hr.Data[40 + i];
                }
            }
            else if (hr.Data[0] == 6)
            {
                for (int i=0;i<62;i++)
                {
                    config.Buttons[i] = (ButtonType)hr.Data[i + 1];
                }
            }
            else if (hr.Data[0] == 7)
            {
                for (int i = 0; i < 62; i++)
                {
                    config.Buttons[i + 62] = (ButtonType)hr.Data[i + 1];
                }
            }
            else if (hr.Data[0] == 8)
            {
                for (int i = 0; i < 4; i++)
                {
                    config.Buttons[i + 124] = (ButtonType)hr.Data[i + 1];
                }

                for (int i = 0; i<12; i++)
                {
                    config.EncoderConfig[i] = new EncoderConfig();
                    config.EncoderConfig[i].PinA = hr.Data[i*4 + 14];
                    config.EncoderConfig[i].PinB = hr.Data[i * 4 + 15];
                    config.EncoderConfig[i].PinC = hr.Data[i * 4 + 16];
                    config.EncoderConfig[i].Type = (EncoderConfig.EncoderType) hr.Data[i * 4 + 17];
                }
            }
            else if (hr.Data[0] == 9)
            {

            }
            else if (hr.Data[0] == 10)
            {

            }
        }
    }
}
