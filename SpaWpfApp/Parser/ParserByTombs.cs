using SpaWpfApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Parser
{
    class ParserByTombs
    {
        private int currentIndex = 0;
        private int currentLine = 1;
        private int currentLevel = 0;
        private string[] wordsInCode = null;
        private ParserByTombs()
        {
        }

        private static ParserByTombs instance;
        public static ParserByTombs Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ParserByTombs();
                }
                return instance;
            }
        }

        public string Parse(string code)
        {
            currentIndex = 0;
            currentLine = 1;
            currentLevel = 0;
            wordsInCode = null;

            wordsInCode = GetWordsInCode(code);
            while (currentIndex < wordsInCode.Length)
            {
                ParseProcedure();
            }
            return "";
        }

        private void ParseProcedure()
        {
            if (wordsInCode[currentIndex] != "procedure")
            {
                throw new WrongCodeException("'procedure' keyword not found in line: " + currentLine);
            }
            currentIndex++;
            string procName = wordsInCode[currentIndex];
            currentIndex++;
            if (wordsInCode[currentIndex] != "{")
            {
                throw new WrongCodeException("'{' not found after 'procedure " + procName + "' in line: " + currentLine);
            }
            ParseBody();
            return;
        }

        private void ParseBody()
        {
            int localLevel = currentLevel;
            currentLevel++;
            currentIndex++;
            while (currentLevel > localLevel && wordsInCode[currentIndex] != "}")
            {
                if (wordsInCode[currentIndex] == "if")
                {
                    ParseIf();
                }
                else if (wordsInCode[currentIndex] == "while")
                {
                    ParseWhile();
                }
                else if (wordsInCode[currentIndex] == "call")
                {
                    ParseCall();
                }
                else if (wordsInCode[currentIndex + 1] == "=")
                {
                    ParseAssign();
                }
                currentLine++;
            }
            currentIndex++;
            currentLevel--;
        }

        private void ParseIf()
        {
            currentIndex++;
            string varName = wordsInCode[currentIndex];
            currentIndex++;
            if (wordsInCode[currentIndex] != "then")
            {
                throw new WrongCodeException("'then' not found after 'if " + varName + "' in line: " + currentLine);
            }
            currentIndex++;
            if (wordsInCode[currentIndex] != "{")
            {
                throw new WrongCodeException("'{' not found after 'if " + varName + " then' in line: " + currentLine);
            }
            ParseBody();
            if (wordsInCode[currentIndex] != "else")
            {
                throw new WrongCodeException("'else' not found after 'if " + varName + " then { ... }' in line: " + currentLine);
            }
            currentIndex++;
            currentLine++;
            ParseBody();
            currentLine--;
            return;
        }

        private void ParseWhile()
        {
            currentIndex++;
            string varName = wordsInCode[currentIndex];
            currentIndex++;
            if (wordsInCode[currentIndex] != "{")
            {
                throw new WrongCodeException("'{' not found after 'while " + varName + "': (line: " + currentLine + ")");
            }
            ParseBody();
            return;
        }

        private void ParseCall()
        {
            currentIndex++;
            string procName = wordsInCode[currentIndex];
            currentIndex++;
            if (wordsInCode[currentIndex] != ";")
            {
                throw new WrongCodeException("Missing ';' after 'call " + procName + "' in line: " + currentLine);
            }
            currentIndex++;
        }

        private void ParseAssign()
        {
            int i = 0;
            for (i = 0; wordsInCode[currentIndex + i] != ";"; i++)
            {

            }
            currentIndex = currentIndex + i + 1;
        }

        private string[] GetWordsInCode(string code)
        {
            for (int i = 0; i < code.Length; i++)
            {
                if (i < code.Length - 1 && IsSeparatorChar(code[i]))
                {
                    code = code.Insert(i, " ");
                    code = code.Insert(i + 2, " ");
                    i = i + 2;
                }
            }
            string[] separators = new string[] { " ", Environment.NewLine };
            return code.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        private bool IsSeparatorChar(char toCheck)
        {
            char[] chars = { '{', '}', ';', '=', '*', '+', '-' };
            foreach (char c in chars)
            {
                if (c == toCheck) return true;
            }
            return false;
        }
    }
}
