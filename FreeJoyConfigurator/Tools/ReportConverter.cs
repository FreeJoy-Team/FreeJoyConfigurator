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
                for (int i = 0; i < joystick.Axes.Count; i++)
                {
                    joystick.Axes[i].RawValue = (short)(hr.Data[1 + 2 * i] << 8 | hr.Data[0 + 2 * i]);
                }

                for (int i = 0; i < 64; i++)
                {
                    joystick.PhysicalButtons[hr.Data[16] + i].State = (hr.Data[17 + ((i & 0xF8) >> 3)] & (1 << (i & 0x07))) > 0 ? true : false;
                }

                for (int i = 0; i < 5; i++)
                {
                    joystick.ShiftButtons[i].State = (hr.Data[25] & (1 << i)) > 0 ? true : false;
                }

                for (int i = 0; i < joystick.Axes.Count; i++)
                {
                    joystick.Axes[i].Value = (short)(hr.Data[27 + 2 * i] << 8 | hr.Data[26 + 2 * i]);
                }

                for (int i = 0; i < joystick.Povs.Count; i++)
                {
                    joystick.Povs[i].State = hr.Data[42 + i];
                }

                for (int i = 0; i < joystick.LogicalButtons.Count; i++)
                {
                    joystick.LogicalButtons[i].State = (hr.Data[46 + ((i & 0xF8) >> 3)] & (1 << (i & 0x07))) > 0 ? true : false;
                }


            }
        }


        public static void ReportToConfig(ref DeviceConfig config, HidReport hr)
        {
            if (hr.Data[0] == 1)
            {
                char[] chars = new char[26];

                config.FirmwareVersion = (ushort)(hr.Data[2] << 8 | hr.Data[1]);
                for (int i = 0; i < chars.Length; i++)
                {

                    chars[i] = (char)hr.Data[i + 3];
                    if (chars[i] == 0) break;   // end of string
                }
                config.DeviceName = new string(chars);
                config.DeviceName.TrimEnd('\0');
                config.ButtonDebounceMs = (ushort)(hr.Data[30] << 8 | hr.Data[29]);
                config.EncoderPressMs = (byte)hr.Data[31];
                config.ExchangePeriod = (byte)hr.Data[32];

                for (int i = 0; i < config.PinConfig.Count; i++)
                {
                    config.PinConfig[i] = (PinType)hr.Data[i + 33];
                }

            }
            else if (hr.Data[0] == 2)
            {
                config.AxisConfig[0] = new AxisConfig();
                config.AxisConfig[0].CalibMin = (short)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[0].CalibCenter = (short)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[0].CalibMax = (short)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[0].IsOutEnabled = Convert.ToBoolean(hr.Data[7] & 0x01);
                config.AxisConfig[0].IsInverted = Convert.ToBoolean(hr.Data[7] & 0x02);
                config.AxisConfig[0].Function = (AxisFunction)((hr.Data[7] & 0x1C) >> 2);
                config.AxisConfig[0].FilterLevel = (byte)((hr.Data[7] & 0xE0) >> 5);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[0].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[8 + i]);
                }
                config.AxisConfig[0].Resolution = (byte)((hr.Data[19] & 0x0F) + 1);
                config.AxisConfig[0].Channel = (byte)(hr.Data[19] >> 4);
                config.AxisConfig[0].Deadband = (byte)(hr.Data[20] & 0x7F);
                config.AxisConfig[0].IsDynamicDeadband = (hr.Data[20] & 0x80) > 0 ? true : false;
                config.AxisConfig[0].SourceMain = (AxisSourceType)(hr.Data[21]);
                config.AxisConfig[0].SourceSecondary = (AxisType)(hr.Data[22] & 0x07);
                config.AxisConfig[0].OffsetAngle = (hr.Data[22] >> 3) * 15;
                config.AxisConfig[0].Button1 = (sbyte)(hr.Data[23] + 1);
                config.AxisConfig[0].Button2 = (sbyte)(hr.Data[24] + 1);
                config.AxisConfig[0].Button3 = (sbyte)(hr.Data[25] + 1);
                config.AxisConfig[0].Divider = hr.Data[26];
                config.AxisConfig[0].I2cAddress = (AxisAddressType)hr.Data[27];
                config.AxisConfig[0].Button1_Type = (AxisButtonFullType)(hr.Data[28] & 0x07 );
                config.AxisConfig[0].Button2_Type = (AxisButtonCutType)((hr.Data[28]>>3) & 0x03);
                config.AxisConfig[0].Button3_Type = (AxisButtonFullType)(hr.Data[28]>>5);
                config.AxisConfig[0].Prescaler = (byte)(hr.Data[29]);

                config.AxisConfig[1] = new AxisConfig();
                config.AxisConfig[1].CalibMin = (short)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[1].CalibCenter = (short)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[1].CalibMax = (short)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[1].IsOutEnabled = Convert.ToBoolean(hr.Data[37] & 0x01);
                config.AxisConfig[1].IsInverted = Convert.ToBoolean(hr.Data[37] & 0x02);
                config.AxisConfig[1].Function = (AxisFunction)((hr.Data[37] & 0x1C) >> 2);
                config.AxisConfig[1].FilterLevel = (byte)((hr.Data[37] & 0xE0) >> 5);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[1].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[38 + i]);
                }
                config.AxisConfig[1].Resolution = (byte)((hr.Data[49] & 0x0F) + 1);
                config.AxisConfig[1].Channel = (byte)(hr.Data[49] >> 4);
                config.AxisConfig[1].Deadband = (byte)(hr.Data[50] & 0x7F);
                config.AxisConfig[1].IsDynamicDeadband = (hr.Data[50] & 0x80) > 0 ? true : false;
                config.AxisConfig[1].SourceMain = (AxisSourceType)hr.Data[51];
                config.AxisConfig[1].SourceSecondary = (AxisType)(hr.Data[52] & 0x07);
                config.AxisConfig[1].OffsetAngle = (hr.Data[52] >> 3) * 15;
                config.AxisConfig[1].Button1 = (sbyte)(hr.Data[53] + 1);
                config.AxisConfig[1].Button2 = (sbyte)(hr.Data[54] + 1);
                config.AxisConfig[1].Button3 = (sbyte)(hr.Data[55] + 1);
                config.AxisConfig[1].Divider = hr.Data[56];
                config.AxisConfig[1].I2cAddress = (AxisAddressType)hr.Data[57];
                config.AxisConfig[1].Button1_Type = (AxisButtonFullType)(hr.Data[58] & 0x07);
                config.AxisConfig[1].Button2_Type = (AxisButtonCutType)((hr.Data[58] >> 3) & 0x03);
                config.AxisConfig[1].Button3_Type = (AxisButtonFullType)(hr.Data[58] >> 5);
                config.AxisConfig[1].Prescaler = (byte)(hr.Data[59]);

            }
            else if (hr.Data[0] == 3)
            {
                config.AxisConfig[2] = new AxisConfig();
                config.AxisConfig[2].CalibMin = (short)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[2].CalibCenter = (short)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[2].CalibMax = (short)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[2].IsOutEnabled = Convert.ToBoolean(hr.Data[7] & 0x01);
                config.AxisConfig[2].IsInverted = Convert.ToBoolean(hr.Data[7] & 0x02);
                config.AxisConfig[2].Function = (AxisFunction)((hr.Data[7] & 0x1C) >> 2);
                config.AxisConfig[2].FilterLevel = (byte)((hr.Data[7] & 0xE0) >> 5);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[2].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[8 + i]);
                }
                config.AxisConfig[2].Resolution = (byte)((hr.Data[19] & 0x0F) + 1);
                config.AxisConfig[2].Channel = (byte)(hr.Data[19] >> 4);
                config.AxisConfig[2].Deadband = (byte)(hr.Data[20] & 0x7F);
                config.AxisConfig[2].IsDynamicDeadband = (hr.Data[20] & 0x80) > 0 ? true : false;
                config.AxisConfig[2].SourceMain = (AxisSourceType)(hr.Data[21]);
                config.AxisConfig[2].SourceSecondary = (AxisType)(hr.Data[22] & 0x07);
                config.AxisConfig[2].OffsetAngle = (hr.Data[22] >> 3) * 15;
                config.AxisConfig[2].Button1 = (sbyte)(hr.Data[23] + 1);
                config.AxisConfig[2].Button2 = (sbyte)(hr.Data[24] + 1);
                config.AxisConfig[2].Button3 = (sbyte)(hr.Data[25] + 1);
                config.AxisConfig[2].Divider = hr.Data[26];
                config.AxisConfig[2].I2cAddress = (AxisAddressType)hr.Data[27];
                config.AxisConfig[2].Button1_Type = (AxisButtonFullType)(hr.Data[28] & 0x07);
                config.AxisConfig[2].Button2_Type = (AxisButtonCutType)((hr.Data[28] >> 3) & 0x03);
                config.AxisConfig[2].Button3_Type = (AxisButtonFullType)(hr.Data[28] >> 5);
                config.AxisConfig[2].Prescaler = (byte)(hr.Data[29]);

                config.AxisConfig[3] = new AxisConfig();
                config.AxisConfig[3].CalibMin = (short)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[3].CalibCenter = (short)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[3].CalibMax = (short)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[3].IsOutEnabled = Convert.ToBoolean(hr.Data[37] & 0x01);
                config.AxisConfig[3].IsInverted = Convert.ToBoolean(hr.Data[37] & 0x02);
                config.AxisConfig[3].Function = (AxisFunction)((hr.Data[37] & 0x1C) >> 2);
                config.AxisConfig[3].FilterLevel = (byte)((hr.Data[37] & 0xE0) >> 5);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[3].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[38 + i]);
                }
                config.AxisConfig[3].Resolution = (byte)((hr.Data[49] & 0x0F) + 1);
                config.AxisConfig[3].Channel = (byte)(hr.Data[49] >> 4);
                config.AxisConfig[3].Deadband = (byte)(hr.Data[50] & 0x7F);
                config.AxisConfig[3].IsDynamicDeadband = (hr.Data[50] & 0x80) > 0 ? true : false;
                config.AxisConfig[3].SourceMain = (AxisSourceType)hr.Data[51];
                config.AxisConfig[3].SourceSecondary = (AxisType)(hr.Data[52] & 0x07);
                config.AxisConfig[3].OffsetAngle = (hr.Data[52] >> 3) * 15;
                config.AxisConfig[3].Button1 = (sbyte)(hr.Data[53] + 1);
                config.AxisConfig[3].Button2 = (sbyte)(hr.Data[54] + 1);
                config.AxisConfig[3].Button3 = (sbyte)(hr.Data[55] + 1);
                config.AxisConfig[3].Divider = hr.Data[56];
                config.AxisConfig[3].I2cAddress = (AxisAddressType)hr.Data[57];
                config.AxisConfig[3].Button1_Type = (AxisButtonFullType)(hr.Data[58] & 0x07);
                config.AxisConfig[3].Button2_Type = (AxisButtonCutType)((hr.Data[58] >> 3) & 0x03);
                config.AxisConfig[3].Button3_Type = (AxisButtonFullType)(hr.Data[58] >> 5);
                config.AxisConfig[3].Prescaler = (byte)(hr.Data[59]);
            }
            else if (hr.Data[0] == 4)
            {
                config.AxisConfig[4] = new AxisConfig();
                config.AxisConfig[4].CalibMin = (short)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[4].CalibCenter = (short)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[4].CalibMax = (short)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[4].IsOutEnabled = Convert.ToBoolean(hr.Data[7] & 0x01);
                config.AxisConfig[4].IsInverted = Convert.ToBoolean(hr.Data[7] & 0x02);
                config.AxisConfig[4].Function = (AxisFunction)((hr.Data[7] & 0x1C) >> 2);
                config.AxisConfig[4].FilterLevel = (byte)((hr.Data[7] & 0xE0) >> 5);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[4].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[8 + i]);
                }
                config.AxisConfig[4].Resolution = (byte)((hr.Data[19] & 0x0F) + 1);
                config.AxisConfig[4].Channel = (byte)(hr.Data[19] >> 4);
                config.AxisConfig[4].Deadband = (byte)(hr.Data[20] & 0x7F);
                config.AxisConfig[4].IsDynamicDeadband = (hr.Data[20] & 0x80) > 0 ? true : false;
                config.AxisConfig[4].SourceMain = (AxisSourceType)(hr.Data[21]);
                config.AxisConfig[4].SourceSecondary = (AxisType)(hr.Data[22] & 0x07);
                config.AxisConfig[4].OffsetAngle = (hr.Data[22] >> 3) * 15;
                config.AxisConfig[4].Button1 = (sbyte)(hr.Data[23] + 1);
                config.AxisConfig[4].Button2 = (sbyte)(hr.Data[24] + 1);
                config.AxisConfig[4].Button3 = (sbyte)(hr.Data[25] + 1);
                config.AxisConfig[4].Divider = hr.Data[26];
                config.AxisConfig[4].I2cAddress = (AxisAddressType)hr.Data[27];
                config.AxisConfig[4].Button1_Type = (AxisButtonFullType)(hr.Data[28] & 0x07);
                config.AxisConfig[4].Button2_Type = (AxisButtonCutType)((hr.Data[28] >> 3) & 0x03);
                config.AxisConfig[4].Button3_Type = (AxisButtonFullType)(hr.Data[28] >> 5);
                config.AxisConfig[4].Prescaler = (byte)(hr.Data[29]);

                config.AxisConfig[5] = new AxisConfig();
                config.AxisConfig[5].CalibMin = (short)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[5].CalibCenter = (short)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[5].CalibMax = (short)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[5].IsOutEnabled = Convert.ToBoolean(hr.Data[37] & 0x01);
                config.AxisConfig[5].IsInverted = Convert.ToBoolean(hr.Data[37] & 0x02);
                config.AxisConfig[5].Function = (AxisFunction)((hr.Data[37] & 0x1C) >> 2);
                config.AxisConfig[5].FilterLevel = (byte)((hr.Data[37] & 0xE0) >> 5);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[5].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[38 + i]);
                }
                config.AxisConfig[5].Resolution = (byte)((hr.Data[49] & 0x0F) + 1);
                config.AxisConfig[5].Channel = (byte)(hr.Data[49] >> 4);
                config.AxisConfig[5].Deadband = (byte)(hr.Data[50] & 0x7F);
                config.AxisConfig[5].IsDynamicDeadband = (hr.Data[50] & 0x80) > 0 ? true : false;
                config.AxisConfig[5].SourceMain = (AxisSourceType)hr.Data[51];
                config.AxisConfig[5].SourceSecondary = (AxisType)(hr.Data[52] & 0x07);
                config.AxisConfig[5].OffsetAngle = (hr.Data[52] >> 3) * 15;
                config.AxisConfig[5].Button1 = (sbyte)(hr.Data[53] + 1);
                config.AxisConfig[5].Button2 = (sbyte)(hr.Data[54] + 1);
                config.AxisConfig[5].Button3 = (sbyte)(hr.Data[55] + 1);
                config.AxisConfig[5].Divider = hr.Data[56];
                config.AxisConfig[5].I2cAddress = (AxisAddressType)hr.Data[57];
                config.AxisConfig[5].Button1_Type = (AxisButtonFullType)(hr.Data[58] & 0x07);
                config.AxisConfig[5].Button2_Type = (AxisButtonCutType)((hr.Data[58] >> 3) & 0x03);
                config.AxisConfig[5].Button3_Type = (AxisButtonFullType)(hr.Data[58] >> 5);
                config.AxisConfig[5].Prescaler = (byte)(hr.Data[59]);
            }
            else if (hr.Data[0] == 5)
            {
                config.AxisConfig[6] = new AxisConfig();
                config.AxisConfig[6].CalibMin = (short)(hr.Data[2] << 8 | hr.Data[1]);
                config.AxisConfig[6].CalibCenter = (short)(hr.Data[4] << 8 | hr.Data[3]);
                config.AxisConfig[6].CalibMax = (short)(hr.Data[6] << 8 | hr.Data[5]);
                config.AxisConfig[6].IsOutEnabled = Convert.ToBoolean(hr.Data[7] & 0x01);
                config.AxisConfig[6].IsInverted = Convert.ToBoolean(hr.Data[7] & 0x02);
                config.AxisConfig[6].Function = (AxisFunction)((hr.Data[7] & 0x1C) >> 2);
                config.AxisConfig[6].FilterLevel = (byte)((hr.Data[7] & 0xE0) >> 5);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[6].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[8 + i]);
                }
                config.AxisConfig[6].Resolution = (byte)((hr.Data[19] & 0x0F) + 1);
                config.AxisConfig[6].Channel = (byte)(hr.Data[19] >> 4);
                config.AxisConfig[6].Deadband = (byte)(hr.Data[20] & 0x7F);
                config.AxisConfig[6].IsDynamicDeadband = (hr.Data[20] & 0x80) > 0 ? true : false;
                config.AxisConfig[6].SourceMain = (AxisSourceType)(hr.Data[21]);
                config.AxisConfig[6].SourceSecondary = (AxisType)(hr.Data[22] & 0x07);
                config.AxisConfig[6].OffsetAngle = (hr.Data[22] >> 3) * 15;
                config.AxisConfig[6].Button1 = (sbyte)(hr.Data[23] + 1);
                config.AxisConfig[6].Button2 = (sbyte)(hr.Data[24] + 1);
                config.AxisConfig[6].Button3 = (sbyte)(hr.Data[25] + 1);
                config.AxisConfig[6].Divider = hr.Data[26];
                config.AxisConfig[6].I2cAddress = (AxisAddressType)hr.Data[27];
                config.AxisConfig[6].Button1_Type = (AxisButtonFullType)(hr.Data[28] & 0x07);
                config.AxisConfig[6].Button2_Type = (AxisButtonCutType)((hr.Data[28] >> 3) & 0x03);
                config.AxisConfig[6].Button3_Type = (AxisButtonFullType)(hr.Data[28] >> 5);
                config.AxisConfig[6].Prescaler = (byte)(hr.Data[29]);

                config.AxisConfig[7] = new AxisConfig();
                config.AxisConfig[7].CalibMin = (short)(hr.Data[32] << 8 | hr.Data[31]);
                config.AxisConfig[7].CalibCenter = (short)(hr.Data[34] << 8 | hr.Data[33]);
                config.AxisConfig[7].CalibMax = (short)(hr.Data[36] << 8 | hr.Data[35]);
                config.AxisConfig[7].IsOutEnabled = Convert.ToBoolean(hr.Data[37] & 0x01);
                config.AxisConfig[7].IsInverted = Convert.ToBoolean(hr.Data[37] & 0x02);
                config.AxisConfig[7].Function = (AxisFunction)((hr.Data[37] & 0x1C) >> 2);
                config.AxisConfig[7].FilterLevel = (byte)((hr.Data[37] & 0xE0) >> 5);
                for (int i = 0; i < 11; i++)
                {
                    config.AxisConfig[7].CurveShape[i] = new System.Windows.Point(i, (sbyte)hr.Data[38 + i]);
                }
                config.AxisConfig[7].Resolution = (byte)((hr.Data[49] & 0x0F) + 1);
                config.AxisConfig[7].Channel = (byte)(hr.Data[49] >> 4);
                config.AxisConfig[7].Deadband = (byte)(hr.Data[50] & 0x7F);
                config.AxisConfig[7].IsDynamicDeadband = (hr.Data[50] & 0x80) > 0 ? true : false;
                config.AxisConfig[7].SourceMain = (AxisSourceType)hr.Data[51];
                config.AxisConfig[7].SourceSecondary = (AxisType)(hr.Data[52] & 0x07);
                config.AxisConfig[7].OffsetAngle = (hr.Data[52] >> 3) * 15;
                config.AxisConfig[7].Button1 = (sbyte)(hr.Data[53] + 1);
                config.AxisConfig[7].Button2 = (sbyte)(hr.Data[54] + 1);
                config.AxisConfig[7].Button3 = (sbyte)(hr.Data[55] + 1);
                config.AxisConfig[7].Divider = hr.Data[56];
                config.AxisConfig[7].I2cAddress = (AxisAddressType)hr.Data[57];
                config.AxisConfig[7].Button1_Type = (AxisButtonFullType)(hr.Data[58] & 0x07);
                config.AxisConfig[7].Button2_Type = (AxisButtonCutType)((hr.Data[58] >> 3) & 0x03);
                config.AxisConfig[7].Button3_Type = (AxisButtonFullType)(hr.Data[58] >> 5);
                config.AxisConfig[7].Prescaler = (byte)(hr.Data[59]);

            }
            else if (hr.Data[0] == 6)
            {
                // buttons group 1
                for (int i = 0; i < 20; i++)
                {
                    config.ButtonConfig[i].PhysicalNumber = (sbyte)(hr.Data[3 * i + 1] + 1);
                    config.ButtonConfig[i].ShiftModificator = (ShiftType)((hr.Data[3 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i].Type = (ButtonType)(hr.Data[3 * i + 2] & BUTTON_TYPE_MASK);

                    config.ButtonConfig[i].IsInverted = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x01);
                    config.ButtonConfig[i].IsDisabled = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x02);
                    config.ButtonConfig[i].ButtonDelayNumber = (TimerType)((hr.Data[3 * i + 3] & 0x1C) >> 2);
                    config.ButtonConfig[i].ButtonToggleNumber = (TimerType)((hr.Data[3 * i + 3] & 0xE0) >> 5);
                }

                config.ButtonTimer1Ms = (ushort)(hr.Data[62] << 8 | hr.Data[61]);
            }
            else if (hr.Data[0] == 7)
            {
                // buttons group 2
                for (int i = 0; i < 20; i++)
                {
                    config.ButtonConfig[i + 20].PhysicalNumber = (sbyte)(hr.Data[3 * i + 1] + 1);
                    config.ButtonConfig[i + 20].ShiftModificator = (ShiftType)((hr.Data[3 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 20].Type = (ButtonType)(hr.Data[3 * i + 2] & BUTTON_TYPE_MASK);

                    config.ButtonConfig[i + 20].IsInverted = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x01);
                    config.ButtonConfig[i + 20].IsDisabled = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x02);
                    config.ButtonConfig[i + 20].ButtonDelayNumber = (TimerType)((hr.Data[3 * i + 3] & 0x1C) >> 2);
                    config.ButtonConfig[i + 20].ButtonToggleNumber = (TimerType)((hr.Data[3 * i + 3] & 0xE0) >> 5);
                }

                config.ButtonTimer2Ms = (ushort)(hr.Data[62] << 8 | hr.Data[61]);
            }
            else if (hr.Data[0] == 8)
            {
                // buttons group 3
                for (int i = 0; i < 20; i++)
                {
                    config.ButtonConfig[i + 40].PhysicalNumber = (sbyte)(hr.Data[3 * i + 1] + 1);
                    config.ButtonConfig[i + 40].ShiftModificator = (ShiftType)((hr.Data[3 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 40].Type = (ButtonType)(hr.Data[3 * i + 2] & BUTTON_TYPE_MASK);

                    config.ButtonConfig[i + 40].IsInverted = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x01);
                    config.ButtonConfig[i + 40].IsDisabled = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x02);
                    config.ButtonConfig[i + 40].ButtonDelayNumber = (TimerType)((hr.Data[3 * i + 3] & 0x1C) >> 2);
                    config.ButtonConfig[i + 40].ButtonToggleNumber = (TimerType)((hr.Data[3 * i + 3] & 0xE0) >> 5);
                }

                config.ButtonTimer3Ms = (ushort)(hr.Data[62] << 8 | hr.Data[61]);
            }
            else if (hr.Data[0] == 9)
            {
                // buttons group 4
                for (int i = 0; i < 20; i++)
                {
                    config.ButtonConfig[i + 60].PhysicalNumber = (sbyte)(hr.Data[3 * i + 1] + 1);
                    config.ButtonConfig[i + 60].ShiftModificator = (ShiftType)((hr.Data[3 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 60].Type = (ButtonType)(hr.Data[3 * i + 2] & BUTTON_TYPE_MASK);

                    config.ButtonConfig[i + 60].IsInverted = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x01);
                    config.ButtonConfig[i + 60].IsDisabled = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x02);
                    config.ButtonConfig[i + 60].ButtonDelayNumber = (TimerType)((hr.Data[3 * i + 3] & 0x1C) >> 2);
                    config.ButtonConfig[i + 60].ButtonToggleNumber = (TimerType)((hr.Data[3 * i + 3] & 0xE0) >> 5);
                }
            }
            else if (hr.Data[0] == 10)
            {
                // buttons group 5
                for (int i = 0; i < 20; i++)
                {
                    config.ButtonConfig[i + 80].PhysicalNumber = (sbyte)(hr.Data[3 * i + 1] + 1);
                    config.ButtonConfig[i + 80].ShiftModificator = (ShiftType)((hr.Data[3 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 80].Type = (ButtonType)(hr.Data[3 * i + 2] & BUTTON_TYPE_MASK);

                    config.ButtonConfig[i + 80].IsInverted = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x01);
                    config.ButtonConfig[i + 80].IsDisabled = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x02);
                    config.ButtonConfig[i + 80].ButtonDelayNumber = (TimerType)((hr.Data[3 * i + 3] & 0x1C) >> 2);
                    config.ButtonConfig[i + 80].ButtonToggleNumber = (TimerType)((hr.Data[3 * i + 3] & 0xE0) >> 5);
                }
            }
            else if (hr.Data[0] == 11)
            {
                // buttons group 6
                for (int i = 0; i < 20; i++)
                {
                    config.ButtonConfig[i + 100].PhysicalNumber = (sbyte)(hr.Data[3 * i + 1] + 1);
                    config.ButtonConfig[i + 100].ShiftModificator = (ShiftType)((hr.Data[3 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 100].Type = (ButtonType)(hr.Data[3 * i + 2] & BUTTON_TYPE_MASK);

                    config.ButtonConfig[i + 100].IsInverted = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x01);
                    config.ButtonConfig[i + 100].IsDisabled = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x02);
                    config.ButtonConfig[i + 100].ButtonDelayNumber = (TimerType)((hr.Data[3 * i + 3] & 0x1C) >> 2);
                    config.ButtonConfig[i + 100].ButtonToggleNumber = (TimerType)((hr.Data[3 * i + 3] & 0xE0) >> 5);
                }
            }
            else if (hr.Data[0] == 12)
            {
                // buttons group 7
                for (int i = 0; i < 8; i++)
                {
                    config.ButtonConfig[i + 120].PhysicalNumber = (sbyte)(hr.Data[3 * i + 1] + 1);
                    config.ButtonConfig[i + 120].ShiftModificator = (ShiftType)((hr.Data[3 * i + 2] & SHIFT_MASK) >> 5);
                    config.ButtonConfig[i + 120].Type = (ButtonType)(hr.Data[3 * i + 2] & BUTTON_TYPE_MASK);

                    config.ButtonConfig[i + 120].IsInverted = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x01);
                    config.ButtonConfig[i + 120].IsDisabled = Convert.ToBoolean(hr.Data[3 * i + 3] & 0x02);
                    config.ButtonConfig[i + 120].ButtonDelayNumber = (TimerType)((hr.Data[3 * i + 3] & 0x1C) >> 2);
                    config.ButtonConfig[i + 120].ButtonToggleNumber = (TimerType)((hr.Data[3 * i + 3] & 0xE0) >> 5);
                }

                // axes to buttons group 1
                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[0].Points[i] = (byte)hr.Data[25 + i];
                }
                config.AxisToButtonsConfig[0].ButtonsCnt = (byte)hr.Data[38];
                config.AxisToButtonsConfig[0].IsEnabled = (hr.Data[39] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[1].Points[i] = (byte)hr.Data[40 + i];
                }
                config.AxisToButtonsConfig[1].ButtonsCnt = (byte)hr.Data[53];
                config.AxisToButtonsConfig[1].IsEnabled = (hr.Data[54] > 0) ? true : false;

            }
            else if (hr.Data[0] == 13)
            {
                // axes to buttons group 2
                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[2].Points[i] = (byte)hr.Data[1 + i];
                }
                config.AxisToButtonsConfig[2].ButtonsCnt = (byte)hr.Data[14];
                config.AxisToButtonsConfig[2].IsEnabled = (hr.Data[15] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[3].Points[i] = (byte)hr.Data[16 + i];
                }
                config.AxisToButtonsConfig[3].ButtonsCnt = (byte)hr.Data[29];
                config.AxisToButtonsConfig[3].IsEnabled = (hr.Data[30] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[4].Points[i] = (byte)hr.Data[31 + i];
                }
                config.AxisToButtonsConfig[4].ButtonsCnt = (byte)hr.Data[44];
                config.AxisToButtonsConfig[4].IsEnabled = (hr.Data[45] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[5].Points[i] = (byte)hr.Data[46 + i];
                }
                config.AxisToButtonsConfig[5].ButtonsCnt = (byte)hr.Data[59];
                config.AxisToButtonsConfig[5].IsEnabled = (hr.Data[60] > 0) ? true : false;

            }
            else if (hr.Data[0] == 14)
            {
                // axes to buttons group 3
                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[6].Points[i] = (byte)hr.Data[1 + i];
                }
                config.AxisToButtonsConfig[6].ButtonsCnt = (byte)hr.Data[14];
                config.AxisToButtonsConfig[6].IsEnabled = (hr.Data[15] > 0) ? true : false;

                for (int i = 0; i < 13; i++)
                {
                    config.AxisToButtonsConfig[7].Points[i] = (byte)hr.Data[16 + i];
                }
                config.AxisToButtonsConfig[7].ButtonsCnt = (byte)hr.Data[29];
                config.AxisToButtonsConfig[7].IsEnabled = (hr.Data[30] > 0) ? true : false;


                for (int i = 0; i < 4; i++)
                {
                    config.ShiftRegistersConfig[i].Type = (ShiftRegisterType)hr.Data[4 * i + 31];
                    config.ShiftRegistersConfig[i].ButtonCnt = (byte)hr.Data[4 * i + 32];
                }

                for (int i = 0; i < 5; i++)
                {
                    config.ShiftModificatorConfig[i].Button = (sbyte)(hr.Data[47 + i] + 1);
                }

                config.Vid = (ushort)((ushort)(hr.Data[53] << 8) | (ushort)hr.Data[52]);
                config.Pid = (ushort)((ushort)(hr.Data[55] << 8) | (ushort)hr.Data[54]);
                config.IsDynamicConfig = (hr.Data[56] > 0) ? true : false;

            }
            else if (hr.Data[0] == 15)
            {
                for (int i = 0; i < 3; i++)
                {
                    config.LedPwmConfig.DutyCycle[i] = hr.Data[1 + i];
                }

                for (int i = 0; i < 24; i++)
                {
                    config.LedConfig[i].InputNumber = (sbyte)(hr.Data[2 * i + 11] + 1);
                    config.LedConfig[i].Type = (LedType)(hr.Data[2 * i + 12] & 0x07);
                }

                
            }
            else if (hr.Data[0] == 16)
            {
                for (int i = 0; i < 16; i++)
                {
                    config.EncodersConfig[i].Type = (EncoderType)(hr.Data[i+1]);
                }
            }
        }

        public static List<HidReport> ConfigToReports(DeviceConfig config)
        {
            List<HidReport> hidReports = new List<HidReport>();
            byte[] buffer = new byte[64];
            byte[] chars;


            // Report 1
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = (byte)0x01;
            buffer[2] = (byte)(config.FirmwareVersion & 0xFF);
            buffer[3] = (byte)(config.FirmwareVersion >> 8);
            chars = Encoding.ASCII.GetBytes(config.DeviceName);
            Array.ConstrainedCopy(chars, 0, buffer, 4, (chars.Length > 26) ? 26 : chars.Length);
            buffer[30] = (byte)(config.ButtonDebounceMs & 0xFF);
            buffer[31] = (byte)(config.ButtonDebounceMs >> 8);
            buffer[32] = (byte)(config.EncoderPressMs);
            buffer[33] = (byte)(config.ExchangePeriod);
            for (int i = 0; i < 30; i++)
            {
                buffer[i + 34] = (byte)config.PinConfig[i];
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
            buffer[8] |= (byte)(config.AxisConfig[0].IsInverted ? 0x02 : 0x00);
            buffer[8] |= (byte)((byte)config.AxisConfig[0].Function << 2);
            buffer[8] |= (byte)(config.AxisConfig[0].FilterLevel << 5);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 9] = (byte)config.AxisConfig[0].CurveShape[i].Y;
            }
            buffer[20] = (byte)(config.AxisConfig[0].Resolution-1);
            buffer[20] |= (byte)(config.AxisConfig[0].Channel << 4);
            buffer[21] = (byte)(config.AxisConfig[0].Deadband);
            buffer[21] |= (byte)(config.AxisConfig[0].IsDynamicDeadband ? 0x80 : 0x00);
            buffer[22] = (byte)(config.AxisConfig[0].SourceMain);
            buffer[23] = (byte)(config.AxisConfig[0].SourceSecondary);
            buffer[23] |= (byte)((config.AxisConfig[0].OffsetAngle / 15) << 3);
            buffer[24] = (byte)(config.AxisConfig[0].Button1 - 1);
            buffer[25] = (byte)(config.AxisConfig[0].Button2 - 1);
            buffer[26] = (byte)(config.AxisConfig[0].Button3 - 1);
            buffer[27] = (byte)(config.AxisConfig[0].Divider);
            buffer[28] = (byte)(config.AxisConfig[0].I2cAddress);
            buffer[29] = (byte)(config.AxisConfig[0].Button1_Type);
            buffer[29] |= (byte)((byte)config.AxisConfig[0].Button2_Type<<3);
            buffer[29] |= (byte)((byte)config.AxisConfig[0].Button3_Type<<5);
            buffer[30] = (byte)(config.AxisConfig[0].Prescaler);

            buffer[32] = (byte)(config.AxisConfig[1].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[1].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[1].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[1].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[1].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[1].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[1].IsOutEnabled ? 0x01 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[1].IsInverted ? 0x02 : 0x00);
            buffer[38] |= (byte)((byte)config.AxisConfig[1].Function << 2);
            buffer[38] |= (byte)(config.AxisConfig[1].FilterLevel << 5);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 39] = (byte)config.AxisConfig[1].CurveShape[i].Y;
            }
            buffer[50] = (byte)(config.AxisConfig[1].Resolution - 1);
            buffer[50] |= (byte)(config.AxisConfig[1].Channel << 4);
            buffer[51] = (byte)(config.AxisConfig[1].Deadband);
            buffer[51] |= (byte)(config.AxisConfig[1].IsDynamicDeadband ? 0x80 : 0x00);
            buffer[52] = (byte)(config.AxisConfig[1].SourceMain);
            buffer[53] = (byte)(config.AxisConfig[1].SourceSecondary);
            buffer[53] |= (byte)((config.AxisConfig[1].OffsetAngle / 15) << 3);
            buffer[54] = (byte)(config.AxisConfig[1].Button1 - 1);
            buffer[55] = (byte)(config.AxisConfig[1].Button2 - 1);
            buffer[56] = (byte)(config.AxisConfig[1].Button3 - 1);
            buffer[57] = (byte)(config.AxisConfig[1].Divider);
            buffer[58] = (byte)(config.AxisConfig[1].I2cAddress);
            buffer[59] = (byte)(config.AxisConfig[1].Button1_Type);
            buffer[59] |= (byte)((byte)config.AxisConfig[1].Button2_Type << 3);
            buffer[59] |= (byte)((byte)config.AxisConfig[1].Button3_Type << 5);
            buffer[60] = (byte)(config.AxisConfig[1].Prescaler);
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
            buffer[8] |= (byte)(config.AxisConfig[2].IsInverted ? 0x02 : 0x00);
            buffer[8] |= (byte)((byte)config.AxisConfig[2].Function << 2);
            buffer[8] |= (byte)(config.AxisConfig[2].FilterLevel << 5);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 9] = (byte)config.AxisConfig[2].CurveShape[i].Y;
            }
            buffer[20] = (byte)(config.AxisConfig[2].Resolution - 1);
            buffer[20] |= (byte)(config.AxisConfig[2].Channel << 4);
            buffer[21] = (byte)(config.AxisConfig[2].Deadband);
            buffer[21] |= (byte)(config.AxisConfig[2].IsDynamicDeadband ? 0x80 : 0x00);
            buffer[22] = (byte)(config.AxisConfig[2].SourceMain);
            buffer[23] = (byte)(config.AxisConfig[2].SourceSecondary);
            buffer[23] |= (byte)((config.AxisConfig[2].OffsetAngle / 15) << 3);
            buffer[24] = (byte)(config.AxisConfig[2].Button1 - 1);
            buffer[25] = (byte)(config.AxisConfig[2].Button2 - 1);
            buffer[26] = (byte)(config.AxisConfig[2].Button3 - 1);
            buffer[27] = (byte)(config.AxisConfig[2].Divider);
            buffer[28] = (byte)(config.AxisConfig[2].I2cAddress);
            buffer[29] = (byte)(config.AxisConfig[2].Button1_Type);
            buffer[29] |= (byte)((byte)config.AxisConfig[2].Button2_Type << 3);
            buffer[29] |= (byte)((byte)config.AxisConfig[2].Button3_Type << 5);
            buffer[30] = (byte)(config.AxisConfig[2].Prescaler);

            buffer[32] = (byte)(config.AxisConfig[3].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[3].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[3].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[3].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[3].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[3].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[3].IsOutEnabled ? 0x01 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[3].IsInverted ? 0x02 : 0x00);
            buffer[38] |= (byte)((byte)config.AxisConfig[3].Function << 2);
            buffer[38] |= (byte)(config.AxisConfig[3].FilterLevel << 5);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 39] = (byte)config.AxisConfig[3].CurveShape[i].Y;
            }
            buffer[50] = (byte)(config.AxisConfig[3].Resolution - 1);
            buffer[50] |= (byte)(config.AxisConfig[3].Channel << 4);
            buffer[51] = (byte)(config.AxisConfig[3].Deadband);
            buffer[51] |= (byte)(config.AxisConfig[3].IsDynamicDeadband ? 0x80 : 0x00);
            buffer[52] = (byte)(config.AxisConfig[3].SourceMain);
            buffer[53] = (byte)(config.AxisConfig[3].SourceSecondary);
            buffer[53] |= (byte)((config.AxisConfig[3].OffsetAngle / 15) << 3);
            buffer[54] = (byte)(config.AxisConfig[3].Button1 - 1);
            buffer[55] = (byte)(config.AxisConfig[3].Button2 - 1);
            buffer[56] = (byte)(config.AxisConfig[3].Button3 - 1);
            buffer[57] = (byte)(config.AxisConfig[3].Divider);
            buffer[58] = (byte)(config.AxisConfig[3].I2cAddress);
            buffer[59] = (byte)(config.AxisConfig[3].Button1_Type);
            buffer[59] |= (byte)((byte)config.AxisConfig[3].Button2_Type << 3);
            buffer[59] |= (byte)((byte)config.AxisConfig[3].Button3_Type << 5);
            buffer[60] = (byte)(config.AxisConfig[3].Prescaler);
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
            buffer[8] |= (byte)(config.AxisConfig[4].IsInverted ? 0x02 : 0x00);
            buffer[8] |= (byte)((byte)config.AxisConfig[4].Function << 2);
            buffer[8] |= (byte)(config.AxisConfig[4].FilterLevel << 5);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 9] = (byte)config.AxisConfig[4].CurveShape[i].Y;
            }
            buffer[20] = (byte)(config.AxisConfig[4].Resolution - 1);
            buffer[20] |= (byte)(config.AxisConfig[4].Channel << 4);
            buffer[21] = (byte)(config.AxisConfig[4].Deadband);
            buffer[21] |= (byte)(config.AxisConfig[4].IsDynamicDeadband ? 0x80 : 0x00);
            buffer[22] = (byte)(config.AxisConfig[4].SourceMain);
            buffer[23] = (byte)(config.AxisConfig[4].SourceSecondary);
            buffer[23] |= (byte)((config.AxisConfig[4].OffsetAngle / 15) << 3);
            buffer[24] = (byte)(config.AxisConfig[4].Button1 - 1);
            buffer[25] = (byte)(config.AxisConfig[4].Button2 - 1);
            buffer[26] = (byte)(config.AxisConfig[4].Button3 - 1);
            buffer[27] = (byte)(config.AxisConfig[4].Divider);
            buffer[28] = (byte)(config.AxisConfig[4].I2cAddress);
            buffer[29] = (byte)(config.AxisConfig[4].Button1_Type);
            buffer[29] |= (byte)((byte)config.AxisConfig[4].Button2_Type << 3);
            buffer[29] |= (byte)((byte)config.AxisConfig[4].Button3_Type << 5);
            buffer[30] = (byte)(config.AxisConfig[4].Prescaler);

            buffer[32] = (byte)(config.AxisConfig[5].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[5].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[5].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[5].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[5].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[5].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[5].IsOutEnabled ? 0x01 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[5].IsInverted ? 0x02 : 0x00);
            buffer[38] |= (byte)((byte)config.AxisConfig[5].Function << 2);
            buffer[38] |= (byte)(config.AxisConfig[5].FilterLevel << 5);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 39] = (byte)config.AxisConfig[5].CurveShape[i].Y;
            }
            buffer[50] = (byte)(config.AxisConfig[5].Resolution - 1);
            buffer[50] |= (byte)(config.AxisConfig[5].Channel << 4);
            buffer[51] = (byte)(config.AxisConfig[5].Deadband);
            buffer[51] |= (byte)(config.AxisConfig[5].IsDynamicDeadband ? 0x80 : 0x00);
            buffer[52] = (byte)(config.AxisConfig[5].SourceMain);
            buffer[53] = (byte)(config.AxisConfig[5].SourceSecondary);
            buffer[53] |= (byte)((config.AxisConfig[5].OffsetAngle / 15) << 3);
            buffer[54] = (byte)(config.AxisConfig[5].Button1 - 1);
            buffer[55] = (byte)(config.AxisConfig[5].Button2 - 1);
            buffer[56] = (byte)(config.AxisConfig[5].Button3 - 1);
            buffer[57] = (byte)(config.AxisConfig[5].Divider);
            buffer[58] = (byte)(config.AxisConfig[5].I2cAddress);
            buffer[59] = (byte)(config.AxisConfig[5].Button1_Type);
            buffer[59] |= (byte)((byte)config.AxisConfig[5].Button2_Type << 3);
            buffer[59] |= (byte)((byte)config.AxisConfig[5].Button3_Type << 5);
            buffer[60] = (byte)(config.AxisConfig[5].Prescaler);
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
            buffer[8] |= (byte)(config.AxisConfig[6].IsInverted ? 0x02 : 0x00);
            buffer[8] |= (byte)((byte)config.AxisConfig[6].Function << 2);
            buffer[8] |= (byte)(config.AxisConfig[6].FilterLevel << 5);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 9] = (byte)config.AxisConfig[6].CurveShape[i].Y;
            }
            buffer[20] = (byte)(config.AxisConfig[6].Resolution - 1);
            buffer[20] |= (byte)(config.AxisConfig[6].Channel << 4);
            buffer[21] = (byte)(config.AxisConfig[6].Deadband);
            buffer[21] |= (byte)(config.AxisConfig[6].IsDynamicDeadband ? 0x80 : 0x00);
            buffer[22] = (byte)(config.AxisConfig[6].SourceMain);
            buffer[23] = (byte)(config.AxisConfig[6].SourceSecondary);
            buffer[23] |= (byte)((config.AxisConfig[6].OffsetAngle / 15) << 3);
            buffer[24] = (byte)(config.AxisConfig[6].Button1 - 1);
            buffer[25] = (byte)(config.AxisConfig[6].Button2 - 1);
            buffer[26] = (byte)(config.AxisConfig[6].Button3 - 1);
            buffer[27] = (byte)(config.AxisConfig[6].Divider);
            buffer[28] = (byte)(config.AxisConfig[6].I2cAddress);
            buffer[29] = (byte)(config.AxisConfig[6].Button1_Type);
            buffer[29] |= (byte)((byte)config.AxisConfig[6].Button2_Type << 3);
            buffer[29] |= (byte)((byte)config.AxisConfig[6].Button3_Type << 5);
            buffer[30] = (byte)(config.AxisConfig[6].Prescaler);

            buffer[32] = (byte)(config.AxisConfig[7].CalibMin & 0xFF);
            buffer[33] = (byte)(config.AxisConfig[7].CalibMin >> 8);
            buffer[34] = (byte)(config.AxisConfig[7].CalibCenter & 0xFF);
            buffer[35] = (byte)(config.AxisConfig[7].CalibCenter >> 8);
            buffer[36] = (byte)(config.AxisConfig[7].CalibMax & 0xFF);
            buffer[37] = (byte)(config.AxisConfig[7].CalibMax >> 8);
            buffer[38] = (byte)(config.AxisConfig[7].IsOutEnabled ? 0x01 : 0x00);
            buffer[38] |= (byte)(config.AxisConfig[7].IsInverted ? 0x02 : 0x00);
            buffer[38] |= (byte)((byte)config.AxisConfig[7].Function << 2);
            buffer[38] |= (byte)(config.AxisConfig[7].FilterLevel << 5);
            for (int i = 0; i < 11; i++)
            {
                buffer[i + 39] = (byte)config.AxisConfig[7].CurveShape[i].Y;
            }
            buffer[50] = (byte)(config.AxisConfig[7].Resolution - 1);
            buffer[50] |= (byte)(config.AxisConfig[7].Channel << 4);
            buffer[51] = (byte)(config.AxisConfig[7].Deadband);
            buffer[51] |= (byte)(config.AxisConfig[7].IsDynamicDeadband ? 0x80 : 0x00);
            buffer[52] = (byte)(config.AxisConfig[7].SourceMain);
            buffer[53] = (byte)(config.AxisConfig[7].SourceSecondary);
            buffer[53] |= (byte)((config.AxisConfig[7].OffsetAngle / 15) << 3);
            buffer[54] = (byte)(config.AxisConfig[7].Button1 - 1);
            buffer[55] = (byte)(config.AxisConfig[7].Button2 - 1);
            buffer[56] = (byte)(config.AxisConfig[7].Button3 - 1);
            buffer[57] = (byte)(config.AxisConfig[7].Divider);
            buffer[58] = (byte)(config.AxisConfig[7].I2cAddress);
            buffer[59] = (byte)(config.AxisConfig[7].Button1_Type);
            buffer[59] |= (byte)((byte)config.AxisConfig[7].Button2_Type << 3);
            buffer[59] |= (byte)((byte)config.AxisConfig[7].Button3_Type << 5);
            buffer[60] = (byte)(config.AxisConfig[7].Prescaler);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 6
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x06;
            for (int i = 0; i < 20; i++)
            {
                buffer[3 * i + 2] = (byte)(config.ButtonConfig[i].PhysicalNumber - 1);
                buffer[3 * i + 3] = (byte)config.ButtonConfig[i].Type;
                buffer[3 * i + 3] |= (byte)((byte)config.ButtonConfig[i].ShiftModificator << 5);
                //buffer[3 * i + 4] = (byte)(config.ButtonConfig[i].ButtonTimerNumber);
                buffer[3 * i + 4] = (byte)(config.ButtonConfig[i].IsInverted ? 0x01 : 0x00);
                buffer[3 * i + 4] |= (byte)(config.ButtonConfig[i].IsDisabled ? 0x02 : 0x00);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i].ButtonDelayNumber << 2);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i].ButtonToggleNumber << 5);
            }
            buffer[62] = (byte)(config.ButtonTimer1Ms & 0xFF);
            buffer[63] = (byte)(config.ButtonTimer1Ms >> 8);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 7
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x07;
            for (int i = 0; i < 20; i++)
            {
                buffer[3 * i + 2] = (byte)(config.ButtonConfig[i + 20].PhysicalNumber - 1);
                buffer[3 * i + 3] = (byte)config.ButtonConfig[i + 20].Type;
                buffer[3 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 20].ShiftModificator << 5);

                buffer[3 * i + 4] = (byte)(config.ButtonConfig[i + 20].IsInverted ? 0x01 : 0x00);
                buffer[3 * i + 4] |= (byte)(config.ButtonConfig[i + 20].IsDisabled ? 0x02 : 0x00);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 20].ButtonDelayNumber << 2);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 20].ButtonToggleNumber << 5);
            }
            buffer[62] = (byte)(config.ButtonTimer2Ms & 0xFF);
            buffer[63] = (byte)(config.ButtonTimer2Ms >> 8);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 8
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x08;
            for (int i = 0; i < 20; i++)
            {
                buffer[3 * i + 2] = (byte)(config.ButtonConfig[i + 40].PhysicalNumber - 1);
                buffer[3 * i + 3] = (byte)config.ButtonConfig[i + 40].Type;
                buffer[3 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 40].ShiftModificator << 5);

                buffer[3 * i + 4] = (byte)(config.ButtonConfig[i + 40].IsInverted ? 0x01 : 0x00);
                buffer[3 * i + 4] |= (byte)(config.ButtonConfig[i + 40].IsDisabled ? 0x02 : 0x00);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 40].ButtonDelayNumber << 2);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 40].ButtonToggleNumber << 5);
            }
            buffer[62] = (byte)(config.ButtonTimer3Ms & 0xFF);
            buffer[63] = (byte)(config.ButtonTimer3Ms >> 8);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 9
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x09;
            for (int i = 0; i < 20; i++)
            {
                buffer[3 * i + 2] = (byte)(config.ButtonConfig[i + 60].PhysicalNumber - 1);
                buffer[3 * i + 3] = (byte)config.ButtonConfig[i + 60].Type;
                buffer[3 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 60].ShiftModificator << 5);

                buffer[3 * i + 4] = (byte)(config.ButtonConfig[i + 60].IsInverted ? 0x01 : 0x00);
                buffer[3 * i + 4] |= (byte)(config.ButtonConfig[i + 60].IsDisabled ? 0x02 : 0x00);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 60].ButtonDelayNumber << 2);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 60].ButtonToggleNumber << 5);
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 10
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0A;
            for (int i = 0; i < 20; i++)
            {
                buffer[3 * i + 2] = (byte)(config.ButtonConfig[i + 80].PhysicalNumber - 1);
                buffer[3 * i + 3] = (byte)config.ButtonConfig[i + 80].Type;
                buffer[3 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 80].ShiftModificator << 5);

                buffer[3 * i + 4] = (byte)(config.ButtonConfig[i + 80].IsInverted ? 0x01 : 0x00);
                buffer[3 * i + 4] |= (byte)(config.ButtonConfig[i + 80].IsDisabled ? 0x02 : 0x00);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 80].ButtonDelayNumber << 2);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 80].ButtonToggleNumber << 5);
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 11
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0B;
            for (int i = 0; i < 20; i++)
            {
                buffer[3 * i + 2] = (byte)(config.ButtonConfig[i + 100].PhysicalNumber - 1);
                buffer[3 * i + 3] = (byte)config.ButtonConfig[i + 100].Type;
                buffer[3 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 100].ShiftModificator << 5);

                buffer[3 * i + 4] = (byte)(config.ButtonConfig[i + 100].IsInverted ? 0x01 : 0x00);
                buffer[3 * i + 4] |= (byte)(config.ButtonConfig[i + 100].IsDisabled ? 0x02 : 0x00);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 100].ButtonDelayNumber << 2);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 100].ButtonToggleNumber << 5);
            }
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 12
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0C;
            for (int i = 0; i < 8; i++)
            {
                buffer[3 * i + 2] = (byte)(config.ButtonConfig[i + 120].PhysicalNumber - 1);
                buffer[3 * i + 3] = (byte)config.ButtonConfig[i + 120].Type;
                buffer[3 * i + 3] |= (byte)((byte)config.ButtonConfig[i + 120].ShiftModificator << 5);

                buffer[3 * i + 4] = (byte)(config.ButtonConfig[i + 120].IsInverted ? 0x01 : 0x00);
                buffer[3 * i + 4] |= (byte)(config.ButtonConfig[i + 120].IsDisabled ? 0x02 : 0x00);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 120].ButtonDelayNumber << 2);
                buffer[3 * i + 4] |= (byte)((byte)config.ButtonConfig[i + 120].ButtonToggleNumber << 5);
            }
            // axes to buttons 1
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 26] = (byte)config.AxisToButtonsConfig[0].Points[i];
            }
            buffer[39] = (byte)config.AxisToButtonsConfig[0].ButtonsCnt;
            buffer[40] = (byte)(config.AxisToButtonsConfig[0].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 41] = (byte)config.AxisToButtonsConfig[1].Points[i];
            }
            buffer[54] = (byte)config.AxisToButtonsConfig[1].ButtonsCnt;
            buffer[55] = (byte)(config.AxisToButtonsConfig[1].IsEnabled ? 0x01 : 0x00);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 13
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0D;

            // axes to buttons
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 2] = (byte)config.AxisToButtonsConfig[2].Points[i];
            }
            buffer[15] = (byte)config.AxisToButtonsConfig[2].ButtonsCnt;
            buffer[16] = (byte)(config.AxisToButtonsConfig[2].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 17] = (byte)config.AxisToButtonsConfig[3].Points[i];
            }
            buffer[30] = (byte)config.AxisToButtonsConfig[3].ButtonsCnt;
            buffer[31] = (byte)(config.AxisToButtonsConfig[3].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 32] = (byte)config.AxisToButtonsConfig[4].Points[i];
            }
            buffer[45] = (byte)config.AxisToButtonsConfig[4].ButtonsCnt;
            buffer[46] = (byte)(config.AxisToButtonsConfig[4].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 47] = (byte)config.AxisToButtonsConfig[5].Points[i];
            }
            buffer[60] = (byte)config.AxisToButtonsConfig[5].ButtonsCnt;
            buffer[61] = (byte)(config.AxisToButtonsConfig[5].IsEnabled ? 0x01 : 0x00);
            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 14
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0E;

            for (int i = 0; i < 13; i++)
            {
                buffer[i + 2] = (byte)config.AxisToButtonsConfig[6].Points[i];
            }
            buffer[15] = (byte)config.AxisToButtonsConfig[6].ButtonsCnt;
            buffer[16] = (byte)(config.AxisToButtonsConfig[6].IsEnabled ? 0x01 : 0x00);
            for (int i = 0; i < 13; i++)
            {
                buffer[i + 17] = (byte)config.AxisToButtonsConfig[7].Points[i];
            }
            buffer[30] = (byte)config.AxisToButtonsConfig[7].ButtonsCnt;
            buffer[31] = (byte)(config.AxisToButtonsConfig[7].IsEnabled ? 0x01 : 0x00);


            for (int i = 0; i < 4; i++)
            {
                buffer[i * 4 + 32] = (byte)config.ShiftRegistersConfig[i].Type;
                buffer[i * 4 + 33] = (byte)config.ShiftRegistersConfig[i].ButtonCnt;
                buffer[i * 4 + 34] = 0;
                buffer[i * 4 + 35] = 0;
            }

            for (int i = 0; i < 5; i++)
            {
                buffer[i + 48] = (byte)(config.ShiftModificatorConfig[i].Button - 1);
                //buffer[2 * i + 34] = (byte)config.ShiftModificatorConfig[i].Mode;
            }

            buffer[53] = (byte)(config.Vid & 0xFF);
            buffer[54] = (byte)(config.Vid >> 8);
            buffer[55] = (byte)(config.Pid & 0xFF);
            buffer[56] = (byte)(config.Pid >> 8);
            buffer[57] = (byte)(config.IsDynamicConfig ? 0x01 : 0x00);

            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));
      
            // Report 15
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x0F;

            for (int i = 0; i < 3; i++)
            {
                buffer[2 + i] = config.LedPwmConfig.DutyCycle[i];
            }

            for (int i = 0; i < 24; i++)
            {
                buffer[2 * i + 12] = (byte)(config.LedConfig[i].InputNumber - 1);
                buffer[2 * i + 13] = (byte)config.LedConfig[i].Type;
            }

            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            // Report 16
            buffer.Initialize();
            buffer[0] = (byte)ReportID.CONFIG_OUT_REPORT;
            buffer[1] = 0x10;

            for (int i = 0; i < 16; i++)
            {
                buffer[2 + i] = (byte)config.EncodersConfig[i].Type;
            }

            hidReports.Add(new HidReport(64, new HidDeviceData(buffer, HidDeviceData.ReadStatus.Success)));

            return hidReports;
        }
    }
}
