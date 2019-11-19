using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWSpellCrawler
{
    class StringScanner : IDisposable, IClosable
    {
        private StringReader reader_ = null;
        private int position_ = 0;
        private int length_ = 0;

        public StringScanner(string source)
        {
            reader_ = new StringReader(source);
            position_ = 0;
            length_ = source.Length;
        }

        public void Dispose()
        {
            reader_.Dispose();
        }

        public void Close()
        {
            reader_.Close();
        }

        public int Position
        {
            get { return position_; }
            set { position_ = value; }
        }

        public int Length
        {
            get { return length_; }
        }

        public bool isEof()
        {
            return (position_ >= length_);
        }

        public bool hasNext()
        {
            return (reader_.Peek() != -1);
        }

        public bool isNull()
        {
            return (reader_.Peek() == '\0');
        }

        public char get() { return (char)reader_.Peek(); }
        public void next() { position_++; reader_.Read(); }

        public void next(int count)
        {
            for (int i = 0; i < count; i++)
            {
                position_++;
                if (reader_.Read() == -1)
                    break;
            }
        }

        public void skip() { next(); }
        public void skip(int count) { next(count); }

        public char read() { position_++; return (char)reader_.Read(); }

        public string read(int length)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                position_++;
                int ch = reader_.Read();
                if (ch != -1)
                    builder.Append((char)ch);
                else
                    break;
            }
            return builder.ToString();
        }

        public string read(int index, int length)
        {
            StringBuilder builder = new StringBuilder();
            char[] buffer = new char[length];
            int readBytes = reader_.Read(buffer, index, length);
            if (readBytes < length)
                buffer[readBytes] = '\0';
            builder.Append(buffer);
            return builder.ToString();
        }

        public bool startsWith(string words)
        {
            string prefix = read(words.Length);
            return (prefix == words);
        }

        public bool startsWith(string words, ref string prefix)
        {
            prefix = read(words.Length);
            return (prefix == words);
        }

        public void skipWhiteSpace()
        {
            do
            {
                int ch = reader_.Peek();
                if (Chars.isWhiteSpace(ch))
                {
                    reader_.Read();
                    position_++;
                }
                else
                {
                    break;
                }
            } while (true);
        }

        public void skipNewLine()
        {
            do
            {
                int ch = reader_.Peek();
                if (Chars.isNewLine(ch))
                {
                    reader_.Read();
                    position_++;
                }
                else
                {
                    break;
                }
            } while (true);
        }

        public void skipWhiteSpaces()
        {
            do
            {
                int ch = reader_.Peek();
                if (Chars.isWhiteSpaces(ch))
                {
                    reader_.Read();
                    position_++;
                }
                else
                {
                    break;
                }
            } while (true);
        }

        public string parseIdentifier()
        {
            StringBuilder identifier = new StringBuilder();
            int ch = reader_.Peek();
            if (Chars.isIdentifierFirst(ch))
            {
                identifier.Append((char)ch);
                reader_.Read();
                position_++;

                do
                {
                    ch = reader_.Peek();
                    if (Chars.isIdentifierBody(ch))
                    {
                        identifier.Append((char)ch);
                        reader_.Read();
                        position_++;
                    }
                    else
                    {
                        break;
                    }
                } while (true);
            }
            return identifier.ToString();
        }

        public string parseTo(char delimiter = ';')
        {
            StringBuilder result = new StringBuilder();
            while (hasNext())
            {
                int ch = reader_.Peek();
                if (ch != delimiter)
                {
                    result.Append((char)ch);
                    reader_.Read();
                    position_++;
                }
                else
                {
                    break;
                }
            }
            return result.ToString();
        }

        public string parseToDelimiter(string delimiters = ";")
        {
            StringBuilder result = new StringBuilder();
            while (hasNext())
            {
                int ch = reader_.Peek();
                if (delimiters.IndexOf((char)ch) == -1)
                {
                    result.Append((char)ch);
                    reader_.Read();
                    position_++;
                }
                else
                {
                    break;
                }
            }
            return result.ToString();
        }
    }
}
