using System;
using System.Collections;

namespace Hide_Seek.Librarys
{
    class TextManipulations
    {
        //This i used to convert the text characters
        const string hex = "0123456789ABCDEF";


        public char Cypher(char msgLetter, char keywordLetter)
        {
            int convertedNumber = msgLetter;
            int keywordNumber = keywordLetter;
            bool[] array = new bool[8];
            //taking the bits from the message character
            for (int i = 0; i < 8; i++)
            {
                array[i] = Convert.ToBoolean((convertedNumber >> i) & 1);
            }
            BitArray bits = keywordNumber.ToBinary();
            //moving the bits from the message character
            for (int i = 0; i < 8; i++)
            {
                if (!array[i]) continue; //if the bit is 0 we don`t take action
                bool check = Convert.ToBoolean(bits[0]);
                bits = BinaryConverter.ToBinary(BinaryConverter.ToNumeral(bits) >> 1);
                bits[7] = check;
            }
            keywordNumber = BinaryConverter.ToNumeral(bits);
            char result = (char)keywordNumber;
            return result;
        }

        public char DeCypher(char msgLetter, char keywordLetter)
        {
            int convertedNumber = msgLetter;
            int keywordNumber = keywordLetter;
            bool[] array = new bool[8];
            //taking the bits from the message character
            for (int i = 0; i < 8; i++)
            {
                array[i] = Convert.ToBoolean((convertedNumber >> i) & 1);
            }
            BitArray bits = keywordNumber.ToBinary();
            //recovering the message character
            for (int i = 0; i < 8; i++)
            {
                if (!array[i]) continue; //if the bit is 0 we don`t take action
                bool check = Convert.ToBoolean(bits[7]);
                bits = BinaryConverter.ToBinary(BinaryConverter.ToNumeral(bits) << 1);
                bits[0] = check;
                bits[8] = false; //this is needed as with the BinaryConverter.ToBinary() function the array is converted to 32 bits from 8 !!
            }
            keywordNumber = BinaryConverter.ToNumeral(bits);
            char result = (char)keywordNumber;
            return result;
        }


        public string ConvertTo16Bit(string msg)
        {
            string result = "";
            int numFor16Bit = 16;
            int msgNumber;
            int residueNumber;
            int divisionNumber;
            //converting string characters into 16bit hex
            for (int i = 0; i < msg.Length; i++)
            {
                msgNumber = msg[i];
                //mark to signal us if the integer was a negative number before the the convert
                //as we are not using unsigned integer
                if (msgNumber < 0) result += "1";
                else result += "0";
                msgNumber = Math.Abs(msgNumber);
                divisionNumber = msgNumber / numFor16Bit;
                residueNumber = msgNumber % numFor16Bit;
                result += hex[divisionNumber];
                result += hex[residueNumber];
            }

            return result;
        }

        public string ConvertFrom16Bit(string msg)
        {
            string result = "";
            int numFor16Bit = 16;
            int checkNumber;
            //changing back from 16bit hex to regular string
            for (int i = 0; i < msg.Length; i += 3)
            {
                //in case we end up with a invalid string
                try
                {
                    checkNumber = hex.IndexOf(msg[i + 1]) * numFor16Bit + hex.IndexOf(msg[i + 2]);
                }
                catch(IndexOutOfRangeException)
                {
                    return null;
                }
                //changing integer to negative
                if (msg[i] == '1') checkNumber = -Math.Abs(checkNumber);
                result += (char)checkNumber;
            }

            return result;
        }
    }
}
