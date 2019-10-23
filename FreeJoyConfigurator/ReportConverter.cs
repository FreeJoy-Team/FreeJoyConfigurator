using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Prism.Mvvm;
using System.Windows.Threading;

namespace FreeJoyConfigurator
{

    public enum ReportID : byte
    {
        JOY_REPORT = 1,
        CONFIG_IN_REPORT,
        CONFIG_OUT_REPORT,
    };

    public static class ReportConverter
    {

        public static void ReportToJoystick(ref Joystick joystick, HidReport hr)
        {
            if (hr != null)
            {
                for (int i=0; i<joystick.Buttons.Count; i++)
                {
                    joystick.Buttons[i].State = (hr.Data[1 + (i & 0xF8)>>3] & (1<<(i & 0x07))) > 0 ? true : false;
                }

                for (int i = 0; i < joystick.Axes.Count; i++)
                {
                    joystick.Axes[i].Value =  (ushort) (hr.Data[17 + 2 * i] << 8 |  hr.Data[16 + 2 * i]);
                }

                for (int i = 0; i < joystick.Axes.Count; i++)
                {
                    joystick.Axes[i].RawValue = (ushort)(hr.Data[37 + 2 * i] << 8 | hr.Data[36 + 2 * i]);
                }
            }
        }
    

        public static void ReportToConfig (ref DeviceConfig config, HidReport hr)
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

                for (int i = 0; i < config.PinConfig.Count; i++)
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
                config.AxisConfig[0].IsAutoCalib = Convert.ToBoolean(hr.Data[7]);
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
                config.AxisConfig[1].IsAutoCalib = Convert.ToBoolean(hr.Data[37]);
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
                config.AxisConfig[2].IsAutoCalib = Convert.ToBoolean(hr.Data[7]);
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
                config.AxisConfig[3].IsAutoCalib = Convert.ToBoolean(hr.Data[37]);
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
                config.AxisConfig[4].IsAutoCalib = Convert.ToBoolean(hr.Data[7]);
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
                config.AxisConfig[5].IsAutoCalib = Convert.ToBoolean(hr.Data[37]);
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
                config.AxisConfig[6].IsAutoCalib = Convert.ToBoolean(hr.Data[7]);
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
                config.AxisConfig[7].IsAutoCalib = Convert.ToBoolean(hr.Data[37]);
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
                    config.ButtonConfig[i].Type = (ButtonType)hr.Data[i + 1];
                }
            }
            else if (hr.Data[0] == 7)
            {
                for (int i = 0; i < 62; i++)
                {
                    config.ButtonConfig[i + 62].Type = (ButtonType)hr.Data[i + 1];
                }
            }
            else if (hr.Data[0] == 8)
            {
                for (int i = 0; i < 4; i++)
                {
                    config.ButtonConfig[i + 124].Type = (ButtonType)hr.Data[i + 1];
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

        public static List<HidReport> ConfigToReports (DeviceConfig config)
        {
            List<HidReport> hidReports = new List<HidReport>();
            byte[] buffer = new byte[64];
            byte[] chars;

            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = (byte) 0x01;
            buffer[2] = (byte)(config.FirmwareVersion & 0xFF);
            buffer[3] = (byte) (config.FirmwareVersion >> 8);            
            chars = Encoding.ASCII.GetBytes(config.DeviceName);
            Array.ConstrainedCopy(chars, 0, buffer, 4, (chars.Length > 10) ? 10 : chars.Length);
            buffer[14] = (byte)(config.ButtonDebounceMs & 0xFF);
            buffer[15] = (byte)(config.ButtonDebounceMs >> 8);
            buffer[16] = (byte)(config.TogglePressMs & 0xFF);
            buffer[17] = (byte)(config.TogglePressMs >> 8);
            buffer[18] = (byte)(config.EncoderPressMs & 0xFF);
            buffer[19] = (byte)(config.EncoderPressMs >> 8);
            buffer[20] = (byte)(config.ExchangePeriod & 0xFF);
            buffer[21] = (byte)(config.ExchangePeriod >> 8);           
            for (int i = 0; i < 30; i++)
            {
                buffer[i + 33] = (byte)config.PinConfig[i];
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x02;
            buffer[2] = (byte)(config.AxisConfig[0].CalibMin & 0xFF);
            buffer[3] = (byte)(config.AxisConfig[0].CalibMin >> 8);
            buffer[4] = (byte)(config.AxisConfig[0].CalibCenter & 0xFF);
            buffer[5] = (byte)(config.AxisConfig[0].CalibCenter >> 8);
            buffer[6] = (byte)(config.AxisConfig[0].CalibMax & 0xFF);
            buffer[7] = (byte)(config.AxisConfig[0].CalibMax >> 8);            
            buffer[8] = (byte)(config.AxisConfig[0].IsAutoCalib ? 0x01 : 0x00);
            buffer[9] = (byte)(config.AxisConfig[0].IsInverted ? 0x01 : 0x00);
            buffer[10] = (byte)(config.AxisConfig[0].FilterLevel);
            for (int i=0; i<10; i++)
            {
                buffer[i + 11] = config.AxisConfig[0].CurveShape[i];
            }
            buffer[32] = (byte)(config.AxisConfig[1].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[1].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[1].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[1].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[1].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[1].CalibMax >> 8);            
            buffer[38] = (byte)(config.AxisConfig[1].IsAutoCalib ? 0x01 : 0x00);
            buffer[39] = (byte)(config.AxisConfig[1].IsInverted ? 0x01 : 0x00);
            buffer[40] = (byte)(config.AxisConfig[1].FilterLevel);
            for (int i = 0; i < 10; i++)
            {
                buffer[i + 41] = config.AxisConfig[1].CurveShape[i];
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x03;
            buffer[2] = (byte)(config.AxisConfig[2].CalibMin & 0xFF);
            buffer[3] = (byte)(config.AxisConfig[2].CalibMin >> 8);
            buffer[4] = (byte)(config.AxisConfig[2].CalibCenter & 0xFF);
            buffer[5] = (byte)(config.AxisConfig[2].CalibCenter >> 8);
            buffer[6] = (byte)(config.AxisConfig[2].CalibMax & 0xFF);
            buffer[7] = (byte)(config.AxisConfig[2].CalibMax >> 8);
            buffer[8] = (byte)(config.AxisConfig[2].IsAutoCalib ? 0x01 : 0x00);
            buffer[9] = (byte)(config.AxisConfig[2].IsInverted ? 0x01 : 0x00);
            buffer[10] = (byte)(config.AxisConfig[2].FilterLevel);
            for (int i = 0; i < 10; i++)
            {
                buffer[i + 11] = config.AxisConfig[2].CurveShape[i];
            }
            buffer[32] = (byte)(config.AxisConfig[3].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[3].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[3].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[3].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[3].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[3].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[3].IsAutoCalib ? 0x01 : 0x00);
            buffer[39] = (byte)(config.AxisConfig[3].IsInverted ? 0x01 : 0x00);
            buffer[40] = (byte)(config.AxisConfig[3].FilterLevel);
            for (int i = 0; i < 10; i++)
            {
                buffer[i + 41] = config.AxisConfig[3].CurveShape[i];
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x04;
            buffer[2] = (byte)(config.AxisConfig[4].CalibMin & 0xFF);
            buffer[3] = (byte)(config.AxisConfig[4].CalibMin >> 8);
            buffer[4] = (byte)(config.AxisConfig[4].CalibCenter & 0xFF);
            buffer[5] = (byte)(config.AxisConfig[4].CalibCenter >> 8);
            buffer[6] = (byte)(config.AxisConfig[4].CalibMax & 0xFF);
            buffer[7] = (byte)(config.AxisConfig[4].CalibMax >> 8);
            buffer[8] = (byte)(config.AxisConfig[4].IsAutoCalib ? 0x01 : 0x00);
            buffer[9] = (byte)(config.AxisConfig[4].IsInverted ? 0x01 : 0x00);
            buffer[10] = (byte)(config.AxisConfig[4].FilterLevel);
            for (int i = 0; i < 10; i++)
            {
                buffer[i + 11] = config.AxisConfig[4].CurveShape[i];
            }
            buffer[32] = (byte)(config.AxisConfig[5].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[5].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[5].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[5].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[5].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[5].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[5].IsAutoCalib ? 0x01 : 0x00);
            buffer[39] = (byte)(config.AxisConfig[5].IsInverted ? 0x01 : 0x00);
            buffer[40] = (byte)(config.AxisConfig[5].FilterLevel);
            for (int i = 0; i < 10; i++)
            {
                buffer[i + 41] = config.AxisConfig[5].CurveShape[i];
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x05;
            buffer[2] = (byte)(config.AxisConfig[6].CalibMin & 0xFF);
            buffer[3] = (byte)(config.AxisConfig[6].CalibMin >> 8);
            buffer[4] = (byte)(config.AxisConfig[6].CalibCenter & 0xFF);
            buffer[5] = (byte)(config.AxisConfig[6].CalibCenter >> 8);
            buffer[6] = (byte)(config.AxisConfig[6].CalibMax & 0xFF);
            buffer[7] = (byte)(config.AxisConfig[6].CalibMax >> 8);
            buffer[8] = (byte)(config.AxisConfig[6].IsAutoCalib ? 0x01 : 0x00);
            buffer[9] = (byte)(config.AxisConfig[6].IsInverted ? 0x01 : 0x00);
            buffer[10] = (byte)(config.AxisConfig[6].FilterLevel);
            for (int i = 0; i < 10; i++)
            {
                buffer[i + 11] = config.AxisConfig[6].CurveShape[i];
            }
            buffer[32] = (byte)(config.AxisConfig[7].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[7].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[7].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[7].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[7].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[7].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[7].IsAutoCalib ? 0x01 : 0x00);
            buffer[39] = (byte)(config.AxisConfig[7].IsInverted ? 0x01 : 0x00);
            buffer[40] = (byte)(config.AxisConfig[7].FilterLevel);
            for (int i = 0; i < 10; i++)
            {
                buffer[i + 41] = config.AxisConfig[7].CurveShape[i];
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x06;
            for (int i=0; i<62; i++)
            {
                buffer[i + 2] = (byte) config.ButtonConfig[i].Type;
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x07;
            for (int i = 0; i < 62; i++)
            {
                buffer[i + 2] = (byte)config.ButtonConfig[i + 62].Type;
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x08;
            for (int i = 0; i < 4; i++)
            {
                buffer[i + 2] = (byte)config.ButtonConfig[i + 124].Type;
            }
            for (int i = 0; i < 12; i++)
            {
                buffer[i * 4 + 15] = (byte)config.EncoderConfig[i].PinA;
                buffer[i * 4 + 16] = (byte)config.EncoderConfig[i].PinB;
                buffer[i * 4 + 17] = (byte)config.EncoderConfig[i].PinC;
                buffer[i * 4 + 18] = (byte)config.EncoderConfig[i].Type;
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x09;
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0A;
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            return hidReports;
        }
    }
}
