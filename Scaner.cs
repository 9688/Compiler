﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Compiler
{
	class Token
	{
		public enum Type { OPERATOR, IDENTIFICATOR, SEPARATOR, EOF, INTEGER, DOUBLE, CHAR, STRING, NONE };

		public int pos, line;
		public string value;
		public Type type;

		public Token(int pos, int line, Type type, string val)
		{
			this.line = line;
			this.pos = pos - val.Count() + 1;
			this.type = type;
			this.value = val;
		}

		public string ToString()
		{
			return this.type + " " + this.line + " " + this.pos + " " + this.value;
		}
	}

   class Scaner
   {
		class Buffer: StreamReader
		{
			public const int EOF = -1;
			private int pos = 0, line = 1;

			public Buffer(Stream stream) : base(stream) {}

			public int Read()
			{
				int ch = base.Read();

				if (ch == '\n')
				{
					line++; pos = 0;
				}
				else
					pos++;

				return ch;
			}

			public int GetLine()
			{
				return line;
			}

			public int getPos()
			{
				return pos;
			}
		}
		
		Buffer buf;
		string val;
		Token.Type type;

      public Scaner(System.IO.Stream istream) { buf = new Buffer(istream); }

		private int ReadInValue()
		{
			int ch = buf.Peek();
			val += (char)buf.Read();
			return ch;
		}

		private void throw_exception(string s)
		{
			throw new Exception(s + " Строка " + buf.GetLine() + " позиция " + buf.getPos());
		}

#region helper functions

		private bool IsDecimal(int ch)
		{
			return ch >= '0' && ch <= '9';
		}

		private bool IsOctal(int ch)
		{
			return ch >= '0' && ch <= '7';
		}

		private bool IsHex(int ch)
		{
			return IsDecimal(ch) || ch >= 'A' && ch <= 'F';
		}

		private bool IsWhite(int ch)
		{
			return char.IsWhiteSpace((char)ch);
		}

		private bool IsAlpha(int ch)
		{
			return ch == '_' || ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z';
		}

		private bool IsDigit(int ch, int platform)
		{
			switch (platform)
			{
				case 8:
					return IsOctal(ch);
				case 10:
					return IsDecimal(ch);
				case 16:
					return IsHex(ch);
				default:
					return false;
			}
		}

		private bool IsEnter(string s, int ch)
		{
			return s.IndexOf((char)ch) > -1;
		}

		private bool IsSeparator(int ch)
		{
			return "{};,()".IndexOf((char)ch) > -1;
		}

		private bool IsOperator(int ch)
		{
			return "+-=/%*&|^~?!.:<>[]".IndexOf((char)ch) > -1;
		}

#endregion

#region convert functions
		private int StringToInt(string s, int platform)
		{
			try
			{
				return Convert.ToInt32(s, platform);
			}
			catch (OverflowException)
			{
				throw_exception("Невозможно преобразовать.");
				return 0;
			}
		}


#endregion

#region parsing functions

		private int GetInteger(int pl = 0, bool part_float = false)
		{
			int platform = 10;
			int first_ch = buf.Peek();

			if (first_ch == '0')
			{
				
				ReadInValue();

				if ((buf.Peek() == 'x' || buf.Peek() == 'X'))
				{
					ReadInValue();
					platform = 16;
				}
				else if(IsOctal(buf.Peek()) && !part_float)
				{
					platform = 8;
				}
			}

			if (pl != 0 && pl != platform)
			{
				throw_exception("Ожидалось " + pl + "ричное число.");// неверная система счисления
			}

			if (first_ch != '0' || platform != 10)
			{
				if (!IsDigit(buf.Peek(), platform))
				{
					throw_exception("Ожидалось число.");//не обходимо число
				}
			}

			while (IsDigit(buf.Peek(), platform)) { ReadInValue(); }

			return platform;
		}

		private int GetFloat(bool is_exist_integer = false, int pl = 0){
			int platform = 10;

			if (buf.Peek() == '.')
			{
				ReadInValue();

				if (IsDecimal(buf.Peek()))
					platform = GetInteger(pl, true);
				else if(!is_exist_integer)
				{
					type = Token.Type.OPERATOR;

					if (buf.Peek() == '.')
					{
						ReadInValue();
						if (buf.Peek() != '.')
						{
							throw_exception("Ожидалась \".\".");
						}
						ReadInValue();			
					} 
		//			else if (!is_exist_integer)
		//				throw_exception("Ожидалось число.");//необходимо число после . тк нет числа перед точкой
					return 0;
				}
			}
			else if (!is_exist_integer)
			{
				throw_exception("Ожидалась \".\".");//необходима .
			}

			if (IsEnter("EepP", buf.Peek()))
			{
				if ((platform == 8 || platform == 16) && (!IsEnter("Pp", buf.Peek())))
				{
					throw_exception("Ожидалось \"P\", обнаружено \"E\"");// ожидалось p 
				}
				else if (platform == 10 && (!IsEnter("Ee", buf.Peek())))
				{
					throw_exception("Ожидалось \"E\", обнаружено \"P\"");// ожидалось E
				}

				ReadInValue();

				if (IsEnter("+-", buf.Peek()))
				{
					ReadInValue();
				}

				if (!IsDecimal(buf.Peek()))
				{
					throw_exception("Ожидалось число.");//полсе Е должна быть цифра
				}

				GetInteger(platform, true);
			}

			type = Token.Type.INTEGER;

			if (IsAlpha(buf.Peek()))
				throw new Exception();

			return platform;
		}

		private void GetScalar()
		{
			int platform = GetInteger();

			if (IsEnter(".eEpP", buf.Peek()))
			{
				GetFloat(true, platform);
			}
			else
				type = Token.Type.INTEGER;

			if (IsAlpha(buf.Peek()))
			{
				throw new Exception();
			}
		}

		private void GetIdentificator()
		{
			if (!IsAlpha(buf.Peek()))
			{
				throw_exception("Ожидался идентификатор.");
			}

			while (IsAlpha(buf.Peek()) || IsDecimal(buf.Peek())) { ReadInValue(); }

			type = Token.Type.IDENTIFICATOR;
		}

		private void PassComment(bool multiline = false)
		{
			int ch;
			if (multiline)
			{
				do
				{
					ch = buf.Read();
				} while (!(ch == '*' && buf.Peek() == '/' || ch == Buffer.EOF));
			}
			else
			{
				do
				{
					ch = buf.Read();
				} while (!(ch == '\n' || ch == Buffer.EOF));
			}

		}

		private void GetOperator()
		{
			if(!IsOperator(buf.Peek()))
				throw_exception("Ожидался оператор.");

			int ch = ReadInValue();

			if (":?[]".IndexOf((char)ch) > -1)
			{
				return;
			}
			else if (ch == '/' && (buf.Peek() == '/' || buf.Peek() == '*'))
			{
				buf.Read();
				PassComment(buf.Peek() == '*');
				Read();
			}
			else if (IsOperator(buf.Peek()))
			{
				if ((buf.Peek() == '=' && "+-/%*<>|&^".IndexOf((char)ch) > -1)  // += -= /= и тд
					|| (ch == '-' && buf.Peek() == '>') // ->
					|| (buf.Peek() == ch && "+-<>=|&.".IndexOf((char)buf.Peek()) > -1)) // ++ -- >> и тд
				{
					ch = ReadInValue();

					if (ch == '.' && ch != buf.Peek())
					{
						throw_exception("Ожидалась \".\".");
					}


					if (((ch == '<' || ch == '>') && buf.Peek() == '=') || (ch == '.' && buf.Peek() == '.'))
					{
						ReadInValue();
					}
				}
			}

		}

		private int ReadChar()
		{
			string result = "";

			if (buf.Peek() == '\\')
			{
				buf.Read();
				switch (buf.Peek())
				{
					case '\'': return '\'';
					case '\"': return '\"';
					case '\\': return '\\';
					case 'a': return '\a';
					case 'b': return '\b';
					case 'f': return '\f';
					case 'n': return '\n';
					case 'r': return '\r';
					case 't': return '\t';
					case 'v': return '\v';
					case 'x':
						result += '0' + (char)buf.Read();

						if (!IsHex(buf.Peek()))
						{
							throw_exception("Ожидaлось 16 ричное число.");
						}

						while (IsHex(buf.Peek())) { result += (char)buf.Read(); };

						return StringToInt(result, 16);
					default:
						if (buf.Peek() != '0')
						{
							throw_exception("Ожидалось 8рично число");
						}

						while (IsOctal(buf.Peek())) { result += (char)buf.Read(); };

						return StringToInt(result, 8);
				}
			}
			else
				return buf.Read();
		}

		private void GetChar()
		{
			buf.Read(); //пропускаем первую ковычку
			val += (char)ReadChar();
			if (buf.Peek() != '\'')
			{
				throw_exception("Ожидалась \" \' \"");
			}

			buf.Read();//пропускаем закрывающуюся ковычку
		}

		private void GetString()
		{
			buf.Read();

			while (buf.Peek() != '\n' && buf.Peek() != '\"')
			{
				val += (char)ReadChar();
			}

			if (buf.Peek() == '\n')
			{
				throw_exception("Ожидалась \".");
			}

			buf.Read();
		}

#endregion

		public Token Read()
		{
			type = Token.Type.NONE;
			val = "";

			while (IsWhite(buf.Peek())) { buf.Read(); }

			int ch = buf.Peek();

			if (ch == Buffer.EOF)
			{
				type = Token.Type.EOF;
			}
			else if (IsDecimal(ch))
			{
				GetScalar();
			}
			else if (IsAlpha(ch))
			{
				GetIdentificator();
			}
			else if (ch == '.')
			{
				GetFloat();
			}
			else if (IsSeparator(ch))
			{
				type = Token.Type.SEPARATOR;
				ReadInValue();
			}
			else if (IsOperator(ch))
			{
				type = Token.Type.OPERATOR;
				GetOperator();
			}
			else if (ch == '\'')
			{
				type = Token.Type.CHAR;
				GetChar();
			}
			else if (ch == '\"')
			{
				type = Token.Type.STRING;
				GetString();
			}


			if (type == Token.Type.NONE)
				throw_exception("Undifined TOKEN");

			return new Token(buf.getPos(), buf.GetLine(), type, val);
		}
   }
}