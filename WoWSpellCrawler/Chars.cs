using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWSpellCrawler
{
    public class Chars
    {
        public static bool isNull(int ch)
        {
            return (ch == '\0');
        }

        public static bool isWhiteSpace(int ch)
        {
            return (ch == ' ' || ch == '\t' ||
                    ch == '\v' || ch == '\f');
        }

        public static bool isNewLine(int ch)
        {
            return (ch == '\n' || ch == '\r');
        }

        public static bool isWhiteSpaces(int ch)
        {
            return (isWhiteSpace(ch) || isNewLine(ch));
        }

        public static bool isDigital(int ch)
        {
            return ((ch >= '0' && ch <= '9'));
        }

        public static bool isLetter(int ch)
        {
            return ((ch >= 'a' && ch <= 'z') ||
                    (ch >= 'A' && ch <= 'Z'));
        }

        public static bool isLowerLetter(int ch)
        {
            return ((ch >= 'a' && ch <= 'z'));
        }

        public static bool isUpperLetter(int ch)
        {
            return ((ch >= 'A' && ch <= 'Z'));
        }

        public static bool isIdentifierFirst(int ch)
        {
            return ((ch >= 'a' && ch <= 'z') ||
                    (ch >= 'A' && ch <= 'Z') ||
                    (ch == '_'));
        }

        public static bool isIdentifierBody(int ch)
        {
            return ((ch >= 'a' && ch <= 'z') ||
                    (ch >= 'A' && ch <= 'Z') ||
                    (ch >= '0' && ch <= '9') ||
                    (ch == '_'));
        }

        public static bool parseBoolean(string value, ref bool bvalue)
        {
            bool isValid = false;
            value = value.ToLower();
            if (value == "true" || value == "!0")
            {
                bvalue = true;
                isValid = true;
            }
            else if (value == "false" || value == "!1")
            {
                bvalue = false;
                isValid = true;
            }
            else
            {
                bvalue = false;
                isValid = false;
            }
            return isValid;
        }

        public static bool parseBooleanInt(string value, ref bool bvalue)
        {
            bool isValid = false;
            value = value.ToLower();
            if (value == "0")
            {
                bvalue = false;
                isValid = true;
            }
            else if (value == "1")
            {
                bvalue = true;
                isValid = true;
            }
            else
            {
                bvalue = false;
                isValid = false;
            }
            return isValid;
        }

        public static bool parseInteger(string value, ref int ivalue)
        {
            int len = 0;
            foreach (var ch in value)
            {
                if (!Chars.isDigital(ch))
                {
                    break;
                }
                len++;
            }
            if (len > 0)
            {
                value = value.Substring(0, len);
                ivalue = int.Parse(value);
            }
            else
            {
                ivalue = 0;
            }
            return (len == value.Length);
        }

        public static bool parseInteger(string value, ref long lvalue)
        {
            int len = 0;
            foreach (var ch in value)
            {
                if (!Chars.isDigital(ch))
                {
                    break;
                }
                len++;
            }
            if (len > 0)
            {
                value = value.Substring(0, len);
                lvalue = long.Parse(value);
            }
            else
            {
                lvalue = 0;
            }
            return (len == value.Length);
        }

        public static bool parseString(string value, ref string svalue)
        {
            int pos = 0;
            int start = -1;
            int end = -1;
            foreach (var ch in value)
            {
                if (Chars.isNull(ch))
                {
                    break;
                }
                if (ch == '"')
                {
                    if (start == -1)
                    {
                        start = pos + 1;
                    }
                    else if (end == -1)
                    {
                        end = pos + 1;
                        pos++;
                        break;
                    }
                }
                pos++;
            }

            if (end >= start)
                svalue = value.Substring(start, end - start);
            else
                svalue = value;

            return (start == 1 && end == value.Length);
        }
    }
}
