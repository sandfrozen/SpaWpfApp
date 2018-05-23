using SpaWpfApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaWpfApp.Parser
{
    class ParserByTombs
    {
        private int currentIndex = 0;
        private int currentLine = 1;
        private int currentLevel = 0;
        private string[] wordsInCode = null;

        private string lastParent = "";


        private ParserByTombs() { }

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
            lastParent = "";

            currentIndex = 0;
            currentLine = 0;
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
                throw new WrongCodeException("'procedure' keyword not found");
            }
            currentIndex++;
            string procName = wordsInCode[currentIndex];
            lastParent = "procedure " + procName;
            ParseBody();
            return;
        }

        private void ParseBody()
        {
            currentIndex++;
            if (wordsInCode[currentIndex] != "{")
            {
                throw new WrongCodeException("'{' not found after '" + lastParent + "' in line: " + currentLine);
            }

            int localLevel = currentLevel;
            currentLevel++;
            currentIndex++;
            while (currentLevel > localLevel && wordsInCode[currentIndex] != "}")
            {
                currentLine++;
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
                else
                {
                    string message = "Unknown string '" + wordsInCode[currentIndex] + "' in line: " + currentLine;
                    if (wordsInCode[currentIndex] == "procedure")
                    {
                        message += ". Propaby You forgot about '}' in line: " + (currentLine - 1);
                    }
                    throw new WrongCodeException(message);
                }
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
            lastParent = "if " + varName;
 
            // add to vars
            // add to uses

            ParseBody();
            if (wordsInCode[currentIndex] != "else")
            {
                throw new WrongCodeException("'else' not found after 'if " + varName + " then { ... }' in line: " + currentLine);
            }
            lastParent = "if " + varName + " { ... } else";
            ParseBody();
            return;
        }

        private void ParseWhile()
        {
            currentIndex++;
            string varName = wordsInCode[currentIndex];
            lastParent = "while " + varName;

            // add to vars
            // add to uses

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
            // add to calls
            currentIndex++;
        }

        private void ParseAssign()
        {
            int i = 0;
            IsSynonym(wordsInCode[currentIndex]);

            // add to vars
            // add to modifies

            for (i = 0; wordsInCode[currentIndex + i] != ";"; i++)
            {
                if (i > 1 && i % 2 == 0)
                {
                    if (!int.TryParse(wordsInCode[currentIndex + i], out int r))
                    {
                        IsSynonym(wordsInCode[currentIndex + i]);
                        // add to vars
                        // add to uses
                    }
                    else
                    {

                    }
                }
                else if (i > 2 && i % 2 == 1)
                {
                    IsAssignArythmetic(wordsInCode[currentIndex + i]);
                }
            }
            if (i < 3 || i % 2 == 0)
            {
                throw new WrongCodeException("Invalid factors in assign in line: " + currentLine);
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
            char[] chars = { '{', '}', '=', '+', '-', '*', '(', ')', ';' };
            foreach (char c in chars)
            {
                if (c == toCheck) return true;
            }
            return false;
        }

        private void IsAssignArythmetic(string toCheck)
        {
            string[] arythmetics = { "+", "-", "*", "(", ")" };
            foreach (string a in arythmetics)
            {
                if (a == toCheck)
                {
                    return;
                }
            }
            throw new WrongCodeException("Invalid assign in line: " + currentLine);
        }

        private void IsSynonym(string synonym)
        {
            if (!Regex.IsMatch(synonym, @"^([a-zA-Z]){1}([a-zA-Z]|[0-9]|[#])*$"))
            {
                throw new WrongCodeException("Invalid synonym: " + synonym + " in line: " + currentLine);
            }
        }
    }
}
