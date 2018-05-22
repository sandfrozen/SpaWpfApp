using SpaWpfApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Parser
{
    class ParserByTombs
    {
        private int currentIndex;
        private int currentLine;
        private string[] wordsInCode;
        private ParserByTombs()
        {
            currentIndex = 0;
            currentLine = 1;
            wordsInCode = null;
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
            wordsInCode = null;

            wordsInCode = GetWordsInCode(code);
            while(currentIndex < wordsInCode.Length)
            {
                ParseProcedure();
            }
            return "";
        }

        private void ParseProcedure()
        {
            if( wordsInCode[currentIndex] != "procedure")
            {
                throw new WrongCodeException("'procedure' keyword not found (line: " + currentLine + ")");
            }
            string procName = wordsInCode[++currentIndex];
            int openBracket = 0;
            if (wordsInCode[++currentIndex] != "{")
            {
                throw new WrongCodeException("'{' not found after 'procedure': (line: " + currentLine + ")");
            }
            //openBracket++;
            //while(openBracket > 0 && wordsInCode[currentIndex])
            //{
            //    //if ( )
            //    //{

            //    //}
            //}
            return;
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
