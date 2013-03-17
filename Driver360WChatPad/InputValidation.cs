using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Driver360WChatPad
{
    public static class InputValidation
    {
        public static byte[] PlayerSearch()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 8;
            bytearray[3] = 65;
            return bytearray;
        }
        public static byte[] Player(int playerNumber)
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 8;
            bytearray[3] = (byte)(65+playerNumber);
            return bytearray;
        }
        public static byte[] GreenSquareOn()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 9;
            return bytearray;
        }
        public static byte[] GreenSquareOff()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 1;
            return bytearray;
        }
        public static byte[] CapsLockOn()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 8;
            return bytearray;
        }
        public static byte[] CapsLockOff()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 0;
            return bytearray;
        }
        public static byte[] EnableBacklight()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 27;
            return bytearray;
        }
        public static byte[] BacklightOn()
        {
            byte[] temparray = new byte[4];
            temparray[0] = 0;
            temparray[1] = 0;
            temparray[2] = 12;
            temparray[3] = 9;
            return temparray;
        }
        public static byte[] PeopleOn()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 11;
            return bytearray;
        }
        public static byte[] PeopleOff()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 3;
            return bytearray;
        }
        public static byte[] OrangeCircleOn()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 10;
            return bytearray;
        }
        public static byte[] OrangeCircleOff()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 2;
            return bytearray;
        }
        public static byte[] TurnOffController()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 8;
            bytearray[3] = 192;
            return bytearray;
        }
        public static byte[] KeepAliveInitial()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 30;
            return bytearray;
        }
        public static byte[] KeepAliveAlternate()
        {
            byte[] bytearray = new byte[4];
            bytearray[0] = 0;
            bytearray[1] = 0;
            bytearray[2] = 12;
            bytearray[3] = 31;
            return bytearray;
        }
        public static byte[] InitializationOne()
        {
            byte[] bytearray = new byte[8];
            bytearray[0] = (byte)int.Parse("40", System.Globalization.NumberStyles.HexNumber);
            bytearray[1] = (byte)int.Parse("A9", System.Globalization.NumberStyles.HexNumber);
            bytearray[2] = (byte)int.Parse("0C", System.Globalization.NumberStyles.HexNumber);
            bytearray[3] = (byte)int.Parse("A3", System.Globalization.NumberStyles.HexNumber);
            bytearray[4] = (byte)int.Parse("23", System.Globalization.NumberStyles.HexNumber);
            bytearray[5] = (byte)int.Parse("44", System.Globalization.NumberStyles.HexNumber);
            bytearray[6] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[7] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            return bytearray;
        }
        public static byte[] InitializationTwo()
        {
            byte[] bytearray = new byte[8];
            bytearray[0] = (byte)int.Parse("40", System.Globalization.NumberStyles.HexNumber);
            bytearray[1] = (byte)int.Parse("A9", System.Globalization.NumberStyles.HexNumber);
            bytearray[2] = (byte)int.Parse("44", System.Globalization.NumberStyles.HexNumber);
            bytearray[3] = (byte)int.Parse("23", System.Globalization.NumberStyles.HexNumber);
            bytearray[4] = (byte)int.Parse("03", System.Globalization.NumberStyles.HexNumber);
            bytearray[5] = (byte)int.Parse("7F", System.Globalization.NumberStyles.HexNumber);
            bytearray[6] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[7] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            return bytearray;
        }
        public static byte[] InitializationThree()
        {
            byte[] bytearray = new byte[8];
            bytearray[0] = (byte)int.Parse("40", System.Globalization.NumberStyles.HexNumber);
            bytearray[1] = (byte)int.Parse("A9", System.Globalization.NumberStyles.HexNumber);
            bytearray[2] = (byte)int.Parse("39", System.Globalization.NumberStyles.HexNumber);
            bytearray[3] = (byte)int.Parse("58", System.Globalization.NumberStyles.HexNumber);
            bytearray[4] = (byte)int.Parse("32", System.Globalization.NumberStyles.HexNumber);
            bytearray[5] = (byte)int.Parse("08", System.Globalization.NumberStyles.HexNumber);
            bytearray[6] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[7] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            return bytearray;
        }
        public static byte[] InitializationFour()
        {
            byte[] bytearray = new byte[8];
            bytearray[0] = (byte)int.Parse("C0", System.Globalization.NumberStyles.HexNumber);
            bytearray[1] = (byte)int.Parse("A1", System.Globalization.NumberStyles.HexNumber);
            bytearray[2] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[3] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[4] = (byte)int.Parse("16", System.Globalization.NumberStyles.HexNumber);
            bytearray[5] = (byte)int.Parse("E4", System.Globalization.NumberStyles.HexNumber);
            bytearray[6] = (byte)int.Parse("02", System.Globalization.NumberStyles.HexNumber);
            bytearray[7] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            return bytearray;
        }
        public static byte[] InitializationFive()
        {
            byte[] bytearray = new byte[8];
            bytearray[0] = (byte)int.Parse("40", System.Globalization.NumberStyles.HexNumber);
            bytearray[1] = (byte)int.Parse("A1", System.Globalization.NumberStyles.HexNumber);
            bytearray[2] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[3] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[4] = (byte)int.Parse("16", System.Globalization.NumberStyles.HexNumber);
            bytearray[5] = (byte)int.Parse("E4", System.Globalization.NumberStyles.HexNumber);
            bytearray[6] = (byte)int.Parse("02", System.Globalization.NumberStyles.HexNumber);
            bytearray[7] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            return bytearray;
        }
        public static byte[] InitializationSix()
        {
            byte[] bytearray = new byte[8];
            bytearray[0] = (byte)int.Parse("C0", System.Globalization.NumberStyles.HexNumber);
            bytearray[1] = (byte)int.Parse("A1", System.Globalization.NumberStyles.HexNumber);
            bytearray[2] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[3] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[4] = (byte)int.Parse("16", System.Globalization.NumberStyles.HexNumber);
            bytearray[5] = (byte)int.Parse("E4", System.Globalization.NumberStyles.HexNumber);
            bytearray[6] = (byte)int.Parse("02", System.Globalization.NumberStyles.HexNumber);
            bytearray[7] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            return bytearray;
        }
        public static byte[] InitializationSeven()
        {
            byte[] bytearray = new byte[8];
            bytearray[0] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[1] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[2] = (byte)int.Parse("0C", System.Globalization.NumberStyles.HexNumber);
            bytearray[3] = (byte)int.Parse("1B", System.Globalization.NumberStyles.HexNumber);
            bytearray[4] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[5] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[6] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            bytearray[7] = (byte)int.Parse("00", System.Globalization.NumberStyles.HexNumber);
            return bytearray;
        }
    }
}
