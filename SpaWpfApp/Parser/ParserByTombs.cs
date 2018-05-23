using SpaWpfApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaWpfApp.Parser
{
    class ParserByTombs
    {
        private int currentIndex = 0;
        private int currentLine = 0;
        private int currentLevel = 0;
        private string[] wordsInCode;
        private string lastParent;
        private string currentProcedure;
        private List<string> procCalls;

        private PkbAPI pkb = new Pkb();

        string[] arythmetics = { "+", "-", "*", "(", ")" };
        string[] keywords = { "if", "while", "call", "else", "procedure" };

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
            procCalls = new List<string>();

            lastParent = "";
            currentProcedure = "";
            currentIndex = 0;
            currentLine = 0;
            currentLevel = 0;
            wordsInCode = null;

            wordsInCode = GetWordsInCode(code);
            while (currentIndex < wordsInCode.Length)
            {
                ParseProcedure();
            }

            Trace.WriteLine("PROC TABLE:");
            for(int i=0; i<pkb.GetNumberOfProcs(); i++)
            {
                Trace.WriteLine(pkb.GetProcName(i));
            }
            Trace.WriteLine("VAR TABLE:");
            for (int i = 0; i < pkb.GetNumberOfVars(); i++)
            {
                Trace.WriteLine(pkb.GetVarName(i));
            }
            //Trace.WriteLine("procTable:");
            //foreach (String s in procTable)
            //{
            //    Trace.WriteLine(s);
            //}
            //Trace.WriteLine("procCalls:");
            //foreach (String s in procCalls)
            //{
            //    if(!procTable.Contains(s))
            //    {
            //        throw new WrongCodeException("Procedure '" + s + "' is used, but not delacred !");
            //    }
            //    Trace.WriteLine(s);
            //}

            //Trace.WriteLine("CallsTable:");
            //for (int i=0; i< callsTable.Count; i++)
            //{
            //    for(int j=0; j<callsTable.ElementAt(i).Count; j++)
            //    {
            //        Trace.Write(callsTable.ElementAt(i).ElementAt(j) + " ");
            //    }
            //    Trace.WriteLine("");
            //}

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
            pkb.InsertProc(procName);
            //addProc(procName);
            currentProcedure = procName;
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
                    string unknown = wordsInCode[currentIndex];
                    string message = "Unknown string '" + unknown + "' in line: " + currentLine;
                    if (unknown == "procedure" || unknown == "else")
                    {
                        message += ". You probably forgot about '}' in line: " + (currentLine - 1);
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
            pkb.InsertVar(varName);
            //addVar(varName);
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
            pkb.InsertVar(varName);
            //addVar(varName);
            // add to uses

            ParseBody();
            return;
        }

        private void ParseCall()
        {
            currentIndex++;
            string procName = wordsInCode[currentIndex];
            addProcCall(procName);
            currentIndex++;
            if (wordsInCode[currentIndex] != ";")
            {
                throw new WrongCodeException("; expected after 'call " + procName + "' in line: " + currentLine);
            }
            //SetCalls(currentProcedure, procName, currentLine);
            pkb.SetCalls(currentProcedure, procName, currentLine);
            currentIndex++;
        }

        private void ParseAssign()
        {
            int i = 0;
            string varModified = wordsInCode[currentIndex];
            IsSynonym(varModified);

            // add to vars
            pkb.InsertVar(varModified);
            //addVar(varModified);
            // add to modifies

            for (i = 0; wordsInCode[currentIndex + i] != ";"; i++)
            {
                if (i > 1 && i % 2 == 0)
                {
                    if (!int.TryParse(wordsInCode[currentIndex + i], out int r))
                    {
                        string varUsed = wordsInCode[currentIndex + i];
                        IsSynonym(varUsed);

                        // add to vars
                        pkb.InsertVar(varUsed);
                        //addVar(varUsed);
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
            if (i < 3)
            {
                throw new WrongCodeException("Assign '" + varModified + " = ...' is too short in line: " + currentLine);
            }
            else if (i % 2 == 0)
            {
                throw new WrongCodeException("Assign '" + varModified + " = ...' is ending with wrong char in line: " + currentLine);
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
            foreach (string a in arythmetics)
            {
                if (a == toCheck)
                {
                    return;
                }
            }

            throw new WrongCodeException("; expected after assign in line: " + currentLine);
            //if (keywords.Contains(toCheck))
            //{
            //    throw new WrongCodeException("; expected after assign in line: " + currentLine);
            //}
            //else
            //{
            //    throw new WrongCodeException("Invalid assign in line: " + currentLine + ". '" + toCheck + "' should be one of: +, -, *, (, )");
            //}

        }

        private void IsSynonym(string synonym)
        {
            if (!Regex.IsMatch(synonym, @"^([a-zA-Z]){1}([a-zA-Z]|[0-9]|[#])*$"))
            {
                throw new WrongCodeException("Wrong synonym format: '" + synonym + "' in line: " + currentLine);
            }
        }

        private void addProcCall(string proc)
        {
            if (!procCalls.Contains(proc)) procCalls.Add(proc);
        }
    }
}
