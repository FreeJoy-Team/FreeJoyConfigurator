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
        FIRMWARE_REPORT,
    };

    public static class ReportConverter
    {
        const byte BUTTON_TYPE_MASK = 0x1F;
        const byte SHIFT_MASK = 0xE0;

        public static void ReportToJoystick(ref Joystick joystick, HidReport hr)
        {
            if (hr != null)
            {
                for (int i = 0; i < joystick.LogicalButtons.Count; i++)
                {
                    joystick.LogicalButtons[i].State = (hr.Data[1 + (i & 0xF8) >> 3] & (1 << (i & 0x07))) > 0 ? true : false;
                }

                for (int i = 0; i < joystick.Axes.Count; i++)
                {
                    joystick.Axes[i].Value =  (short) (hr.Data[17 + 2 * i] << 8 |  hr.Data[16 + 2 * i]);
                }

                for (int i = 0; i < joystick.Povs.Count; i++)
                {
                    joystick.Povs[i].State =  hr.Data[32 + i];
                }

                for (int i = 0; i < joystick.Axes.Count; i++)
                {
                    joystick.Axes[i].RawValue = (short)(hr.Data[37 + 2 * i] << 8 | hr.Data[36 + 2 * i]);
                }
               
                for (int i=0; i<8; i++)
                {
                    joystick.PhysicalButtons[hr.Data[52]+i].State = hr.Data[53+i] > 0 ? true : false;
                }
            }
        }
    

        public static void ReportToConfig (ref DeviceConfig config, HidReport hr)
        {
            if (hr.Data[0] == 1)
            {
                char[] chars = new char[20];

                config.FirmwareVersion = (ushort)(hr.Data[2] << 8 | hr.Data[1]);
                for (int i=0;i<20;i++)
                {
                    
                    chars[i] = (char)hr.Data[i + 3];
                    if (chars[i] == 0) break;   // end of string
                }
                config.DeviceName = new string(chars);
                config.DeviceName.TrimEnd('\0');
                config.ButtonDebounceMs = (ushort)(hr.Data[24] << 8 | hr.Data[23]);
                config.TogglePressMs = (ushort)(hr.Data[26] << 8 | hr.Data[25]);
                config.EncoderPressMs = (ushort)(hr.Data[28] << 8 | hr.Data[27]);
                config.ExchangePeriod = (ushort)(hr.Data[30] << 8 | hr.Data[29]);

                for (int i = 0; i < config.PinConfig.Count; i++)
                {
                    config.PinConfig[i] = (PinType)hr.Data[i + 32];
                }
                
            }
            else if (hr.Data[0] == 2)
            {
                config.AxisConfig[0] = new AxisConfig();
                config.AxisConfig[0].CalibMin = (short)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[0].CalibCenter = (short)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[0].CalibMax = (short)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[0].IsOutEnabled = Convert.ToBoolean(hr.Data[7] & 0x01);
                config.AxisConfig[0].IsMagnetOffset = Convert.ToBoolean(hr.Data[7] & 0x02);
                config.AxisConfig[0].IsInverted = Convert.ToBoolean(hr.Data[7] & 0x04);
                config.AxisConfig[0].FilterLevel = (byte)(hr.Data[7]>>3);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[0].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[8 + i]);
                }               
                config.AxisConfig[0].Resolution = hr.Data[19];
                config.AxisConfig[0].DeadZone = hr.Data[20];
                config.AxisConfig[0].SourceMain = (AxisSourceType)(hr.Data[21]);
                config.AxisConfig[0].Function = (AxisFunction)(hr.Data[22] & 0x07);
                config.AxisConfig[0].SourceSecondary = (AxisType)(hr.Data[22]>>3);
                config.AxisConfig[0].DecrementButton = (sbyte)(hr.Data[23]+1);
                config.AxisConfig[0].CenterButton = (sbyte)(hr.Data[24]+1);
                config.AxisConfig[0].IncrementButton = (sbyte)(hr.Data[25]+1);
                config.AxisConfig[0].Step = hr.Data[26];

                config.AxisConfig[1] = new AxisConfig();
                config.AxisConfig[1].CalibMin = (short)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[1].CalibCenter = (short)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[1].CalibMax = (short)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[1].IsOutEnabled = Convert.ToBoolean(hr.Data[37] & 0x01);
                config.AxisConfig[1].IsMagnetOffset = Convert.ToBoolean(hr.Data[37] & 0x02);
                config.AxisConfig[1].IsInverted = Convert.ToBoolean(hr.Data[37] & 0x04);
                config.AxisConfig[1].FilterLevel = (byte)(hr.Data[37] >> 3);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[1].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[38 + i]);
                }
                config.AxisConfig[1].Resolution = hr.Data[49];
                config.AxisConfig[1].DeadZone = hr.Data[50];
                config.AxisConfig[1].SourceMain = (AxisSourceType)hr.Data[51];
                config.AxisConfig[1].Function = (AxisFunction)(hr.Data[52] & 0x07);
                config.AxisConfig[1].SourceSecondary = (AxisType)(hr.Data[52]>>3);
                config.AxisConfig[1].DecrementButton = (sbyte)(hr.Data[53]+1);
                config.AxisConfig[1].CenterButton = (sbyte)(hr.Data[54]+1);
                config.AxisConfig[1].IncrementButton = (sbyte)(hr.Data[55]+1);
                config.AxisConfig[1].Step = hr.Data[56];

            }
            else if (hr.Data[0] == 3)
            {
                config.AxisConfig[2] = new AxisConfig();
                config.AxisConfig[2].CalibMin = (short)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[2].CalibCenter = (short)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[2].CalibMax = (short)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[2].IsOutEnabled = Convert.ToBoolean(hr.Data[7] & 0x01);
                config.AxisConfig[2].IsMagnetOffset = Convert.ToBoolean(hr.Data[7] & 0x02);
                config.AxisConfig[2].IsInverted = Convert.ToBoolean(hr.Data[7] & 0x04);
                config.AxisConfig[2].FilterLevel = (byte)(hr.Data[7] >> 3);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[2].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[8 + i]);
                }
                config.AxisConfig[2].Resolution = hr.Data[19];
                config.AxisConfig[2].DeadZone = hr.Data[20];
                config.AxisConfig[2].SourceMain = (AxisSourceType)hr.Data[21];
                config.AxisConfig[2].Function = (AxisFunction)(hr.Data[22] & 0x07);
                config.AxisConfig[2].SourceSecondary = (AxisType)(hr.Data[22] >> 3);
                config.AxisConfig[2].DecrementButton = (sbyte)(hr.Data[23] + 1);
                config.AxisConfig[2].CenterButton = (sbyte)(hr.Data[24] + 1);
                config.AxisConfig[2].IncrementButton = (sbyte)(hr.Data[25] + 1);
                config.AxisConfig[2].Step = hr.Data[26];

                config.AxisConfig[3] = new AxisConfig();
                config.AxisConfig[3].CalibMin = (short)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[3].CalibCenter = (short)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[3].CalibMax = (short)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[3].IsOutEnabled = Convert.ToBoolean(hr.Data[37] & 0x01);
                config.AxisConfig[3].IsMagnetOffset = Convert.ToBoolean(hr.Data[37] & 0x02);
                config.AxisConfig[3].IsInverted = Convert.ToBoolean(hr.Data[37] & 0x04);
                config.AxisConfig[3].FilterLevel = (byte)(hr.Data[37] >> 3);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[3].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[38 + i]);
                }
                config.AxisConfig[3].Resolution = hr.Data[49];
                config.AxisConfig[3].DeadZone = hr.Data[50];
                config.AxisConfig[3].SourceMain = (AxisSourceType)hr.Data[51];
                config.AxisConfig[3].Function = (AxisFunction)(hr.Data[52] & 0x07);
                config.AxisConfig[3].SourceSecondary = (AxisType)(hr.Data[52] >> 3);
                config.AxisConfig[3].DecrementButton = (sbyte)(hr.Data[53] + 1);
                config.AxisConfig[3].CenterButton = (sbyte)(hr.Data[54] + 1);
                config.AxisConfig[3].IncrementButton = (sbyte)(hr.Data[55] + 1);
                config.AxisConfig[3].Step = hr.Data[56];
            }
            else if (hr.Data[0] == 4)
            {
                config.AxisConfig[4] = new AxisConfig();
                config.AxisConfig[4].CalibMin = (short)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[4].CalibCenter = (short)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[4].CalibMax = (short)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[4].IsOutEnabled = Convert.ToBoolean(hr.Data[7] & 0x01);
                config.AxisConfig[4].IsMagnetOffset = Convert.ToBoolean(hr.Data[7] & 0x02);
                config.AxisConfig[4].IsInverted = Convert.ToBoolean(hr.Data[7] & 0x04);
                config.AxisConfig[4].FilterLevel = (byte)(hr.Data[7] >> 3);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[4].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[8 + i]);
                }
                config.AxisConfig[4].Resolution = hr.Data[19];
                config.AxisConfig[4].DeadZone = hr.Data[20];
                config.AxisConfig[4].SourceMain = (AxisSourceType)hr.Data[21];
                config.AxisConfig[4].Function = (AxisFunction)(hr.Data[22] & 0x07);
                config.AxisConfig[4].SourceSecondary = (AxisType)(hr.Data[22] >> 3);
                config.AxisConfig[4].DecrementButton = (sbyte)(hr.Data[23] + 1);
                config.AxisConfig[4].CenterButton = (sbyte)(hr.Data[24] + 1);
                config.AxisConfig[4].IncrementButton = (sbyte)(hr.Data[25] + 1);
                config.AxisConfig[4].Step = hr.Data[26];

                config.AxisConfig[5] = new AxisConfig();
                config.AxisConfig[5].CalibMin = (short)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[5].CalibCenter = (short)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[5].CalibMax = (short)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[5].IsOutEnabled = Convert.ToBoolean(hr.Data[37] & 0x01);
                config.AxisConfig[5].IsMagnetOffset = Convert.ToBoolean(hr.Data[37] & 0x02);
                config.AxisConfig[5].IsInverted = Convert.ToBoolean(hr.Data[37] & 0x04);
                config.AxisConfig[5].FilterLevel = (byte)(hr.Data[37] >> 3);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[5].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[38 + i]);
                }
                config.AxisConfig[5].Resolution = hr.Data[49];
                config.AxisConfig[5].DeadZone = hr.Data[50];
                config.AxisConfig[5].SourceMain = (AxisSourceType)hr.Data[51];
                config.AxisConfig[5].Function = (AxisFunction)(hr.Data[52] & 0x07);
                config.AxisConfig[5].SourceSecondary = (AxisType)(hr.Data[52] >> 3);
                config.AxisConfig[5].DecrementButton = (sbyte)(hr.Data[53] + 1);
                config.AxisConfig[5].CenterButton = (sbyte)(hr.Data[54] + 1);
                config.AxisConfig[5].IncrementButton = (sbyte)(hr.Data[55] + 1);
                config.AxisConfig[5].Step = hr.Data[56];
            }
            else if (hr.Data[0] == 5)
            {
                config.AxisConfig[6] = new AxisConfig();
                config.AxisConfig[6].CalibMin = (short)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[6].CalibCenter = (short)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[6].CalibMax = (short)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[6].IsOutEnabled = Convert.ToBoolean(hr.Data[7] & 0x01);
                config.AxisConfig[6].IsMagnetOffset = Convert.ToBoolean(hr.Data[7] & 0x02);
                config.AxisConfig[6].IsInverted = Convert.ToBoolean(hr.Data[7] & 0x04);
                config.AxisConfig[6].FilterLevel = (byte)(hr.Data[7] >> 3);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[6].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[8 + i]);
                }
                config.AxisConfig[6].Resolution = hr.Data[19];
                config.AxisConfig[6].DeadZone = hr.Data[20];
                config.AxisConfig[6].SourceMain = (AxisSourceType)hr.Data[21];
                config.AxisConfig[6].Function = (AxisFunction)(hr.Data[22] & 0x07);
                config.AxisConfig[6].SourceSecondary = (AxisType)(hr.Data[22] >> 3);
                config.AxisConfig[6].DecrementButton = (sbyte)(hr.Data[23] + 1);
                config.AxisConfig[6].CenterButton = (sbyte)(hr.Data[24] + 1);
                config.AxisConfig[6].IncrementButton = (sbyte)(hr.Data[25] + 1);
                config.AxisConfig[6].Step = hr.Data[26];

                config.AxisConfig[7] = new AxisConfig();
                config.AxisConfig[7].CalibMin = (short)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[7].CalibCenter = (short)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[7].CalibMax = (short)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[7].IsOutEnabled = Convert.ToBoolean(hr.Data[37] & 0x01);
                config.AxisConfig[7].IsMagnetOffset = Convert.ToBoolean(hr.Data[37] & 0x02);
                config.AxisConfig[7].IsInverted = Convert.ToBoolean(hr.Data[37] & 0x04);
                config.AxisConfig[7].FilterLevel = (byte)(hr.Data[37] >> 3);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[7].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[38 + i]);
                }
                config.AxisConfig[7].Resolution = hr.Data[49];
                config.AxisConfig[7].DeadZone = hr.Data[50];
                config.AxisConfig[7].SourceMain = (AxisSourceType)hr.Data[51];
                config.AxisConfig[7].Function = (AxisFunction)(hr.Data[52] & 0x07);
                config.AxisConfig[7].SourceSecondary = (AxisType)(hr.Data[52] >> 3);
                config.AxisConfig[7].DecrementButton = (sbyte)(hr.Data[53] + 1);
                config.AxisConfig[7].CenterButton = (sbyte)(hr.Data[54] + 1);
                config.AxisConfig[7].IncrementButton = (sbyte)(hr.Data[55] + 1);
                config.AxisConfig[7].Step = hr.Data[56];

            }
            else if (hr.Data[0] == 6)
            {
                // buttons group 1
                for (int i=0;i<31;i++)
                {
                    config.ButtonConfig[i].PhysicalNumber = (sbyte)(hr.Data[2*i + 1] + 1);
                    config.ButtonConfig[i].ShiftModificator = (ShiftType)((hr.Data[2 * i + 2] & SHIFT_MASK)>>5);
                    config.ButtonConfig[i].Type = (ButtonType)(hr.Data[2 * i + 2] & BUTTON_TYPE_MASK);
                }
            }
            else if (hr.Data[0] == 7)
            {
                // buttons group 2
                for (int i = 0; i < 31; i++)
                {
                    config.ButtonConfig[i + 31].PhysicalNumber = (sbyte)(hr.Data[2 * i + 1] + 1);
                    config.ButtonConfig[i + 31].ShiftModificator = (ShiftType)((hr.Data[2 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 31].Type = (ButtonType)(hr.Data[2 * i + 2] & BUTTON_TYPE_MASK);
                }
            }
            else if (hr.Data[0] == 8)
            {
                // buttons group 3
                for (int i = 0; i < 31; i++)
                {
                    config.ButtonConfig[i + 62].PhysicalNumber = (sbyte)(hr.Data[2 * i + 1] + 1);
                    config.ButtonConfig[i + 62].ShiftModificator = (ShiftType)((hr.Data[2 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 62].Type = (ButtonType)(hr.Data[2 * i + 2] & BUTTON_TYPE_MASK);
                }
            }
            else if (hr.Data[0] == 9)
            {
                // buttons group 4
                for (int i = 0; i < 31; i++)
                {
                    config.ButtonConfig[i + 93].PhysicalNumber = (sbyte)(hr.Data[2 * i + 1] + 1);
                    config.ButtonConfig[i + 93].ShiftModificator = (ShiftType)((hr.Data[2 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 93].Type = (ButtonType)(hr.Data[2 * i + 2] & BUTTON_TYPE_MASK);
                }
            }
            else if (hr.Data[0] == 10)
            {
                // buttons group 5
                for (int i = 0; i < 4; i++)
                {
                    config.ButtonConfig[i + 124].PhysicalNumber = (sbyte)(hr.Data[2 * i + 1] + 1);
                    config.ButtonConfig[i + 124].ShiftModificator = (ShiftType)((hr.Data[2 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 124].Type = (ButtonType)(hr.Data[2 * i + 2] & BUTTON_TYPE_MASK);
                }

                // axes to buttons group 1
                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[0].Points[i] = (sbyte)hr.Data[9 + i];
                }
                config.AxisToButtonsConfig[0].ButtonsCnt = (byte)hr.Data[22];
                config.AxisToButtonsConfig[0].IsEnabled = (hr.Data[23] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[1].Points[i] = (sbyte)hr.Data[24 + i];
                }
                config.AxisToButtonsConfig[1].ButtonsCnt = (byte)hr.Data[37];
                config.AxisToButtonsConfig[1].IsEnabled = (hr.Data[38] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[2].Points[i] = (sbyte)hr.Data[39 + i];
                }
                config.AxisToButtonsConfig[2].ButtonsCnt = (byte)hr.Data[52];
                config.AxisToButtonsConfig[2].IsEnabled = (hr.Data[53] > 0) ? true : false;

            }
            else if (hr.Data[0] == 11)
            {
                // axes to buttons group 2
                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[3].Points[i] = (sbyte)hr.Data[1 + i];
                }
                config.AxisToButtonsConfig[3].ButtonsCnt = (byte)hr.Data[14];
                config.AxisToButtonsConfig[3].IsEnabled = (hr.Data[15] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[4].Points[i] = (sbyte)hr.Data[16 + i];
                }
                config.AxisToButtonsConfig[4].ButtonsCnt = (byte)hr.Data[29];
                config.AxisToButtonsConfig[4].IsEnabled = (hr.Data[30] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[5].Points[i] = (sbyte)hr.Data[31 + i];
                }
                config.AxisToButtonsConfig[5].ButtonsCnt = (byte)hr.Data[44];
                config.AxisToButtonsConfig[5].IsEnabled = (hr.Data[45] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[6].Points[i] = (sbyte)hr.Data[46 + i];
                }
                config.AxisToButtonsConfig[6].ButtonsCnt = (byte)hr.Data[59];
                config.AxisToButtonsConfig[6].IsEnabled = (hr.Data[60] > 0) ? true : false;

            }
            else if (hr.Data[0] == 12)
            {
                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[7].Points[i] = (sbyte)hr.Data[1 + i];
                }
                config.AxisToButtonsConfig[7].ButtonsCnt = (byte)hr.Data[14];
                config.AxisToButtonsConfig[7].IsEnabled = (hr.Data[15] > 0) ? true : false;

                for (int i = 0; i < 4; i++)
                {
                    config.ShiftRegistersConfig[i].Type = (ShiftRegisterType)hr.Data[4 * i + 16];
                    config.ShiftRegistersConfig[i].ButtonCnt = (byte)hr.Data[4 * i + 17];
                }

                for (int i = 0; i < 5; i++)
                {
                    config.ShiftModificatorConfig[i].Button = (sbyte)(hr.Data[32 + i]+1);
                    //config.ShiftModificatorConfig[i].Mode = (ShiftMode)hr.Data[33 + i * 2];
                }

                config.Vid = (ushort)((ushort) (hr.Data[38] << 8) | (ushort) hr.Data[37]);
                config.Pid = (ushort)((ushort)(hr.Data[40] << 8) | (ushort)hr.Data[39]);
            }
        }

        public static List<HidReport> ConfigToReports (DeviceConfig config)
        {
            List<HidReport> hidReports = new List<HidReport>();
            byte[] buffer = new byte[64];
            byte[] chars;


            // Report 1
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = (byte) 0x01;
            buffer[2] = (byte)(config.FirmwareVersion & 0xFF);
            buffer[3] = (byte) (config.FirmwareVersion >> 8);            
            chars = Encoding.ASCII.GetBytes(config.DeviceName);
            Array.ConstrainedCopy(chars, 0, buffer, 4, (chars.Length > 20) ? 20 : chars.Length);
            buffer[24] = (byte)(config.ButtonDebounceMs & 0xFF);
            buffer[25] = (byte)(config.ButtonDebounceMs >> 8);
            buffer[26] = (byte)(config.TogglePressMs & 0xFF);
            buffer[27] = (byte)(config.TogglePressMs >> 8);
            buffer[28] = (byte)(config.EncoderPressMs & 0xFF);
            buffer[29] = (byte)(config.EncoderPressMs >> 8);
            buffer[30] = (byte)(config.ExchangePeriod & 0xFF);
            buffer[31] = (byte)(config.ExchangePeriod >> 8);           
            for (int i = 0; i < 30; i++)
            {
                buffer[i + 33] = (byte)config.PinConfig[i];
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 2
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x02;
            buffer[2] = (byte)(config.AxisConfig[0].CalibMin & 0xFF);
            buffer[3] = (byte)(config.AxisConfig[0].CalibMin >> 8);
            buffer[4] = (byte)(config.AxisConfig[0].CalibCenter & 0xFF);
            buffer[5] = (byte)(config.AxisConfig[0].CalibCenter >> 8);
            buffer[6] = (byte)(config.AxisConfig[0].CalibMax & 0xFF);
            buffer[7] = (byte)(config.AxisConfig[0].CalibMax >> 8);
            buffer[8] = (byte)(config.AxisConfig[0].IsOutEnabled ? 0x01 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[0].IsMagnetOffset ? 0x02 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[0].IsInverted ? 0x04 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[0].FilterLevel<<3);
            for (int i=0; i<11; i++)
            {
                buffer[i + 9] = (byte)config.AxisConfig[0].CurveShape[i].Y;
            }
            buffer[20] = (byte)(config.AxisConfig[0].Resolution);
            buffer[21] = (byte)(config.AxisConfig[0].DeadZone);
            buffer[22] = (byte)(config.AxisConfig[0].SourceMain);
            buffer[23] = (byte)(config.AxisConfig[0].Function);
            buffer[23] |= (byte)((byte)(config.AxisConfig[0].SourceSecondary) << 3);
            buffer[24] = (byte)(config.AxisConfig[0].DecrementButton - 1);
            buffer[25] = (byte)(config.AxisConfig[0].CenterButton - 1);
            buffer[26] = (byte)(config.AxisConfig[0].IncrementButton - 1);
            buffer[27] = (byte)(config.AxisConfig[0].Step);

            buffer[32] = (byte)(config.AxisConfig[1].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[1].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[1].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[1].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[1].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[1].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[1].IsOutEnabled ? 0x01 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[1].IsMagnetOffset ? 0x02 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[1].IsInverted ? 0x04 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[1].FilterLevel << 3);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 39] = (byte)config.AxisConfig[1].CurveShape[i].Y;
            }
            buffer[50] = (byte)(config.AxisConfig[1].Resolution);
            buffer[51] = (byte)(config.AxisConfig[1].DeadZone);
            buffer[52] = (byte)(config.AxisConfig[1].SourceMain);
            buffer[53] = (byte)(config.AxisConfig[1].Function);
            buffer[53] |= (byte)((byte)(config.AxisConfig[1].SourceSecondary) << 3);
            buffer[54] = (byte)(config.AxisConfig[1].DecrementButton - 1);
            buffer[55] = (byte)(config.AxisConfig[1].CenterButton - 1);
            buffer[56] = (byte)(config.AxisConfig[1].IncrementButton - 1);
            buffer[57] = (byte)(config.AxisConfig[1].Step);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 3
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x03;
            buffer[2] = (byte)(config.AxisConfig[2].CalibMin & 0xFF);
            buffer[3] = (byte)(config.AxisConfig[2].CalibMin >> 8);
            buffer[4] = (byte)(config.AxisConfig[2].CalibCenter & 0xFF);
            buffer[5] = (byte)(config.AxisConfig[2].CalibCenter >> 8);
            buffer[6] = (byte)(config.AxisConfig[2].CalibMax & 0xFF);
            buffer[7] = (byte)(config.AxisConfig[2].CalibMax >> 8);
            buffer[8] = (byte)(config.AxisConfig[2].IsOutEnabled ? 0x01 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[2].IsMagnetOffset ? 0x02 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[2].IsInverted ? 0x04 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[2].FilterLevel << 3);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 9] = (byte)config.AxisConfig[2].CurveShape[i].Y;
            }
            buffer[20] = (byte)(config.AxisConfig[2].Resolution);
            buffer[21] = (byte)(config.AxisConfig[2].DeadZone);
            buffer[22] = (byte)(config.AxisConfig[2].SourceMain);
            buffer[23] = (byte)(config.AxisConfig[2].Function);
            buffer[23] |= (byte)((byte)(config.AxisConfig[2].SourceSecondary) << 3);
            buffer[24] = (byte)(config.AxisConfig[2].DecrementButton - 1);
            buffer[25] = (byte)(config.AxisConfig[2].CenterButton - 1);
            buffer[26] = (byte)(config.AxisConfig[2].IncrementButton - 1);
            buffer[27] = (byte)(config.AxisConfig[2].Step);

            buffer[32] = (byte)(config.AxisConfig[3].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[3].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[3].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[3].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[3].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[3].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[3].IsOutEnabled ? 0x01 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[3].IsMagnetOffset ? 0x02 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[3].IsInverted ? 0x04 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[3].FilterLevel << 3);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 39] = (byte)config.AxisConfig[3].CurveShape[i].Y;
            }
            buffer[50] = (byte)(config.AxisConfig[3].Resolution);
            buffer[51] = (byte)(config.AxisConfig[3].DeadZone);
            buffer[52] = (byte)(config.AxisConfig[3].SourceMain);
            buffer[53] = (byte)(config.AxisConfig[3].Function);
            buffer[53] |= (byte)((byte)(config.AxisConfig[3].SourceSecondary) << 3);
            buffer[54] = (byte)(config.AxisConfig[3].DecrementButton - 1);
            buffer[55] = (byte)(config.AxisConfig[3].CenterButton - 1);
            buffer[56] = (byte)(config.AxisConfig[3].IncrementButton - 1);
            buffer[57] = (byte)(config.AxisConfig[3].Step);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 4
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x04;
            buffer[2] = (byte)(config.AxisConfig[4].CalibMin & 0xFF);
            buffer[3] = (byte)(config.AxisConfig[4].CalibMin >> 8);
            buffer[4] = (byte)(config.AxisConfig[4].CalibCenter & 0xFF);
            buffer[5] = (byte)(config.AxisConfig[4].CalibCenter >> 8);
            buffer[6] = (byte)(config.AxisConfig[4].CalibMax & 0xFF);
            buffer[7] = (byte)(config.AxisConfig[4].CalibMax >> 8);
            buffer[8] = (byte)(config.AxisConfig[4].IsOutEnabled ? 0x01 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[4].IsMagnetOffset ? 0x02 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[4].IsInverted ? 0x04 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[4].FilterLevel << 3);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 9] = (byte)config.AxisConfig[4].CurveShape[i].Y;
            }
            buffer[20] = (byte)(config.AxisConfig[4].Resolution);
            buffer[21] = (byte)(config.AxisConfig[4].DeadZone);
            buffer[22] = (byte)(config.AxisConfig[4].SourceMain);
            buffer[23] = (byte)(config.AxisConfig[4].Function);
            buffer[23] |= (byte)((byte)(config.AxisConfig[4].SourceSecondary) << 3);
            buffer[24] = (byte)(config.AxisConfig[4].DecrementButton - 1);
            buffer[25] = (byte)(config.AxisConfig[4].CenterButton - 1);
            buffer[26] = (byte)(config.AxisConfig[4].IncrementButton - 1);
            buffer[27] = (byte)(config.AxisConfig[4].Step);

            buffer[32] = (byte)(config.AxisConfig[5].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[5].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[5].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[5].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[5].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[5].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[5].IsOutEnabled ? 0x01 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[5].IsMagnetOffset ? 0x02 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[5].IsInverted ? 0x04 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[5].FilterLevel << 3);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 39] = (byte)config.AxisConfig[5].CurveShape[i].Y;
            }
            buffer[50] = (byte)(config.AxisConfig[5].Resolution);
            buffer[51] = (byte)(config.AxisConfig[5].DeadZone);
            buffer[52] = (byte)(config.AxisConfig[5].SourceMain);
            buffer[53] = (byte)(config.AxisConfig[5].Function);
            buffer[53] |= (byte)((byte)(config.AxisConfig[5].SourceSecondary) << 3);
            buffer[54] = (byte)(config.AxisConfig[5].DecrementButton - 1);
            buffer[55] = (byte)(config.AxisConfig[5].CenterButton - 1);
            buffer[56] = (byte)(config.AxisConfig[5].IncrementButton - 1);
            buffer[57] = (byte)(config.AxisConfig[5].Step);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 5
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x05;
            buffer[2] = (byte)(config.AxisConfig[6].CalibMin & 0xFF);
            buffer[3] = (byte)(config.AxisConfig[6].CalibMin >> 8);
            buffer[4] = (byte)(config.AxisConfig[6].CalibCenter & 0xFF);
            buffer[5] = (byte)(config.AxisConfig[6].CalibCenter >> 8);
            buffer[6] = (byte)(config.AxisConfig[6].CalibMax & 0xFF);
            buffer[7] = (byte)(config.AxisConfig[6].CalibMax >> 8);
            buffer[8] = (byte)(config.AxisConfig[6].IsOutEnabled ? 0x01 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[6].IsMagnetOffset ? 0x02 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[6].IsInverted ? 0x04 : 0x00);
            buffer[8] |= (byte)(config.AxisConfig[6].FilterLevel << 3);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 9] = (byte)config.AxisConfig[6].CurveShape[i].Y;
            }
            buffer[20] = (byte)(config.AxisConfig[6].Resolution);
            buffer[21] = (byte)(config.AxisConfig[6].DeadZone);
            buffer[22] = (byte)(config.AxisConfig[6].SourceMain);
            buffer[23] = (byte)(config.AxisConfig[6].Function);
            buffer[23] |= (byte)((byte)(config.AxisConfig[6].SourceSecondary) << 2);
            buffer[24] = (byte)(config.AxisConfig[6].DecrementButton - 1);
            buffer[25] = (byte)(config.AxisConfig[6].CenterButton - 1);
            buffer[26] = (byte)(config.AxisConfig[6].IncrementButton - 1);
            buffer[27] = (byte)(config.AxisConfig[6].Step);

            buffer[32] = (byte)(config.AxisConfig[7].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[7].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[7].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[7].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[7].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[7].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[7].IsOutEnabled ? 0x01 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[7].IsMagnetOffset ? 0x02 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[7].IsInverted ? 0x04 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[7].FilterLevel << 3);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 39] = (byte)config.AxisConfig[7].CurveShape[i].Y;
            }
            buffer[50] = (byte)(config.AxisConfig[7].Resolution);
            buffer[51] = (byte)(config.AxisConfig[7].DeadZone);
            buffer[52] = (byte)(config.AxisConfig[7].SourceMain);
            buffer[53] = (byte)(config.AxisConfig[7].Function);
            buffer[53] |= (byte)((byte)(config.AxisConfig[7].SourceSecondary) << 2);
            buffer[54] = (byte)(config.AxisConfig[7].DecrementButton - 1);
            buffer[55] = (byte)(config.AxisConfig[7].CenterButton - 1);
            buffer[56] = (byte)(config.AxisConfig[7].IncrementButton - 1);
            buffer[57] = (byte)(config.AxisConfig[7].Step);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 6
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x06;
            for (int i=0; i<31; i++)
            {
                buffer[2*i + 2] = (byte) (config.ButtonConfig[i].PhysicalNumber - 1);
                buffer[2*i + 3] = (byte)config.ButtonConfig[i].Type;
                buffer[2*i + 3] |= (byte)((byte)config.ButtonConfig[i].ShiftModificator << 5);
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 7
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x07;
            for (int i = 0; i < 31; i++)
            {
                buffer[2 * i + 2] = (byte)(config.ButtonConfig[i + 31].PhysicalNumber-1);
                buffer[2 * i + 3] = (byte)config.ButtonConfig[i + 31].Type;
                buffer[2 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 31].ShiftModificator << 5);
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 8
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x08;
            for (int i = 0; i < 31; i++)
            {
                buffer[2 * i + 2] = (byte)(config.ButtonConfig[i + 62].PhysicalNumber - 1);
                buffer[2 * i + 3] = (byte)config.ButtonConfig[i + 62].Type;
                buffer[2 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 62].ShiftModificator << 5);
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 9
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x09;
            for (int i = 0; i < 31; i++)
            {
                buffer[2 * i + 2] = (byte)(config.ButtonConfig[i + 93].PhysicalNumber - 1);
                buffer[2 * i + 3] = (byte)config.ButtonConfig[i + 93].Type;
                buffer[2 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 93].ShiftModificator << 5);
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 10
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0A;
            for (int i = 0; i < 4; i++)
            {
                buffer[2 * i + 2] = (byte)(config.ButtonConfig[i + 124].PhysicalNumber - 1);
                buffer[2 * i + 3] = (byte)config.ButtonConfig[i + 124].Type;
                buffer[2 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 124].ShiftModificator << 5);
            }
            // axes to buttons 1
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 10] = (byte)config.AxisToButtonsConfig[0].Points[i];
            }
            buffer[23] = (byte)config.AxisToButtonsConfig[0].ButtonsCnt;
            buffer[24] = (byte)(config.AxisToButtonsConfig[0].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 25] = (byte)config.AxisToButtonsConfig[1].Points[i];
            }
            buffer[38] = (byte)config.AxisToButtonsConfig[1].ButtonsCnt;
            buffer[39] = (byte)(config.AxisToButtonsConfig[1].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 40] = (byte)config.AxisToButtonsConfig[2].Points[i];
            }
            buffer[53] = (byte)config.AxisToButtonsConfig[2].ButtonsCnt;
            buffer[54] = (byte)(config.AxisToButtonsConfig[2].IsEnabled ? 0x01 : 0x00);
            
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 11
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0B;

            // axes to buttons
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 2] = (byte)config.AxisToButtonsConfig[3].Points[i];
            }
            buffer[15] = (byte)config.AxisToButtonsConfig[3].ButtonsCnt;
            buffer[16] = (byte)(config.AxisToButtonsConfig[3].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 17] = (byte)config.AxisToButtonsConfig[4].Points[i];
            }
            buffer[30] = (byte)config.AxisToButtonsConfig[4].ButtonsCnt;
            buffer[31] = (byte)(config.AxisToButtonsConfig[4].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 32] = (byte)config.AxisToButtonsConfig[5].Points[i];
            }
            buffer[45] = (byte)config.AxisToButtonsConfig[5].ButtonsCnt;
            buffer[46] = (byte)(config.AxisToButtonsConfig[5].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 47] = (byte)config.AxisToButtonsConfig[6].Points[i];
            }
            buffer[60] = (byte)config.AxisToButtonsConfig[6].ButtonsCnt;
            buffer[61] = (byte)(config.AxisToButtonsConfig[6].IsEnabled ? 0x01 : 0x00);            
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 12
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0C;

            for (int i = 0; i < 13; i++)
            {
                buffer[i + 2] = (byte)config.AxisToButtonsConfig[7].Points[i];
            }
            buffer[15] = (byte)config.AxisToButtonsConfig[7].ButtonsCnt;
            buffer[16] = (byte)(config.AxisToButtonsConfig[7].IsEnabled ? 0x01 : 0x00);

            for (int i = 0; i < 4; i++)
            {
                buffer[i * 4 + 17] = (byte) config.ShiftRegistersConfig[i].Type;
                buffer[i * 4 + 18] = (byte) config.ShiftRegistersConfig[i].ButtonCnt;
                buffer[i * 4 + 19] = 0;
                buffer[i * 4 + 20] = 0;
            }


            for (int i = 0; i < 5; i++)
            {
                buffer[i + 33] = (byte)(config.ShiftModificatorConfig[i].Button-1);
                //buffer[2 * i + 34] = (byte)config.ShiftModificatorConfig[i].Mode;
            }

            buffer[38] = (byte)(config.Vid & 0xFF);
            buffer[39] = (byte)(config.Vid >> 8);
            buffer[40] = (byte)(config.Pid & 0xFF);
            buffer[41] = (byte)(config.Pid >> 8);          

            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            return hidReports;
        }
    }
}
