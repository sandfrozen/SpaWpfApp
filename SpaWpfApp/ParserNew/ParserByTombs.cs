using SpaWpfApp.Exceptions;
using SpaWpfApp.PkbNew;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaWpfApp.ParserNew
{
    public class ParserByTombs
    {
        private int currentIndex = 0;
        private int currentLine = 0;
        private int currentLevel = 0;
        private string[] wordsInCode;
        private string lastParent;
        private string currentProcedure;
        private List<string> calledProcedures;
        private List<string> declaredProcedures;
        private List<int> firstLineOfProcedure;

        public PkbAPI pkb { get; set; }

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

        public static void SetNewInstance()
        {
            instance = new ParserByTombs();
        }

        public string Parse(string code)
        {
            calledProcedures = new List<string>();
            declaredProcedures = new List<string>();
            firstLineOfProcedure = new List<int>();
            pkb = new Pkb();
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

            foreach (string p in calledProcedures)
            {
                if (!declaredProcedures.Contains(p))
                {
                    throw new SourceCodeException("Procedure: " + p + " is called but not delcared");
                }
            }


            //if (pkb.GetNumberOfProcs() > 1)
            //{
            //    string firstProcedure = pkb.GetProcName(0);
            //    var calledProcedures = pkb.GetCalled(firstProcedure);
            //    foreach (string calledProcedure in calledProcedures)
            //    {
            //        int callLine = pkb.IsCalls(firstProcedure, calledProcedure);
            //        if (callLine > 0)
            //        {
            //            SetRecursiveModifiesAndUses(callLine, calledProcedure);
            //        }
            //    }
            //}


            Trace.WriteLine("PROC TABLE:");
            for (int i = 0; i < pkb.GetNumberOfProcs(); i++)
            {
                Trace.WriteLine(firstLineOfProcedure[i] + " " + pkb.GetProcName(i));
            }
            Trace.WriteLine("VAR TABLE:");
            for (int i = 0; i < pkb.GetNumberOfVars(); i++)
            {
                Trace.WriteLine(pkb.GetVarName(i));
            }
            Trace.WriteLine("MODIFIES TABLE:");
            pkb.PrintModifiesTable();

            Trace.WriteLine("USES TABLE:");
            pkb.PrintUsesTable();

            Trace.WriteLine("CALLS TABLE:");
            pkb.PrintCallsTable();

            return GetParsedSouceCode();
        }

        private void SetRecursiveModifiesAndUses(int rootLine, string innerProcedure)
        {
            var calledProcedures = pkb.GetCalled(innerProcedure);
            foreach (string calledProcedure in calledProcedures)
            {
                int callLine = pkb.IsCalls(innerProcedure, calledProcedure);
                if (callLine > 0)
                {
                    SetRecursiveModifiesAndUses(callLine, calledProcedure);
                }
            }
            int procIndex = pkb.GetProcIndex(innerProcedure);
            int firstLine = firstLineOfProcedure[procIndex];
            int lastLine;
            if (procIndex < pkb.GetNumberOfProcs() - 1)
            {
                lastLine = firstLineOfProcedure[procIndex + 1];
            }
            else
            {
                lastLine = currentLine; // current line is last line where parser stopped parsing
            }
            //set modifies and uses for rootLine
            for (; firstLine <= lastLine; firstLine++)
            {
                var modifiedVars = pkb.GetModified(firstLine);
                foreach(string modifiedVar in modifiedVars)
                {
                    pkb.SetModifies(modifiedVar, rootLine);
                }
                var usedVars = pkb.GetUsed(firstLine);
                foreach (string usedVar in usedVars)
                {
                    pkb.SetUses(usedVar, rootLine);
                }
            }
        }

        public (string lineNumbers, string parsedSourceCode) GetParsedFormattedSourceCode()
        {
            int level = 0;
            string parsed = "";
            int line = 0;
            string lines = Environment.NewLine;
            int length = wordsInCode.Length;

            for (int i = 0; i < length; i++)
            {
                string s = wordsInCode[i];
                if (s == "{")
                {
                    level++;
                }
                else if (s == "}")
                {
                    level--;
                }
                parsed += s;

                if (s == "{")
                {
                    parsed += Environment.NewLine + InsertSpaces(level);
                    lines += ++line + Environment.NewLine;
                }
                else if (i == length - 1)
                {
                    //done
                }
                else if (i == length - 2)
                {
                    parsed += " ";
                }
                else if (s == ";" && wordsInCode[i + 1] == "}")
                {
                    parsed += " ";
                }
                else if (i < length - 1 && s == "}" && wordsInCode[i + 1] == "procedure")
                {
                    parsed += Environment.NewLine;
                    lines += Environment.NewLine;
                }
                else if (s == "else" && wordsInCode[i + 1] == "{")
                {
                    parsed += " ";
                }
                else if (i < length - 1 && s == "}" && wordsInCode[i + 1] == "else")
                {
                    parsed += Environment.NewLine + InsertSpaces(level);
                    lines += Environment.NewLine;
                }
                else if (i < length - 2 && s == "}" && wordsInCode[i + 1] == "}")
                {
                    parsed += " ";
                }
                else if (s == "}" && i == length - 1)
                {
                    break;
                }
                else if (s == "}")
                {
                    parsed += Environment.NewLine + InsertSpaces(level);
                    lines += ++line + Environment.NewLine;
                }
                else if (s == ";")
                {
                    parsed += Environment.NewLine + InsertSpaces(level);
                    lines += ++line + Environment.NewLine;
                }
                else
                {
                    parsed += " ";
                }

            }
            return (lines, parsed);
        }

        public string GetParsedSouceCode()
        {
            string parsed = "";
            int length = wordsInCode.Length;

            for (int i = 0; i < length; i++)
            {
                string s = wordsInCode[i];

                parsed += s;

                if (s == "{")
                {
                    parsed += Environment.NewLine;
                }
                else if (i == length - 1)
                {
                    //done
                }
                else if (i==length-2)
                {
                    parsed += " ";
                }
                else if (i < length - 1 && s == ";" && wordsInCode[i + 1] == "}")
                {
                    parsed += " ";
                }
                else if (i < length - 1 && s == "}" && wordsInCode[i + 1] == "else")
                {
                    parsed += Environment.NewLine;
                }
                else if (i < length - 2 && s == "}" && wordsInCode[i + 1] == "}")
                {
                    parsed += " ";
                }
                else if ( i > 0 && i < length - 1 && wordsInCode[i - 1] == "}" && s == "}")
                {
                    parsed += Environment.NewLine;
                }
                else if (i > 0 && wordsInCode[i - 1] == ";" && s == "}")
                {
                    parsed += Environment.NewLine;
                }
                else if (i > 0 && wordsInCode[i - 1] == "else" && s == "{")
                {
                    parsed += Environment.NewLine;
                }
                else if (s == ";")
                {
                    parsed += Environment.NewLine;
                }
                else
                {
                    parsed += " ";
                }
            }

            return parsed;
        }

        public string InsertSpaces(int size)
        {
            string spaces = "";
            for (int i = 0; i < 4 * size; i++)
            {
                spaces += " ";
            }
            return spaces;
        }

        private void ParseProcedure()
        {
            if (wordsInCode[currentIndex] != "procedure")
            {
                throw new SourceCodeException("'procedure' keyword not found");
            }
            currentIndex++;
            string procName = wordsInCode[currentIndex];
            pkb.InsertProc(procName);
            addToDeclaredProcedures(procName);
            currentProcedure = procName;
            firstLineOfProcedure.Add(currentLine + 1);

            lastParent = "procedure " + procName;
            ParseBody();
            return;
        }

        private void ParseBody()
        {
            currentIndex++;
            if (wordsInCode[currentIndex] != "{")
            {
                throw new SourceCodeException("'{' not found after '" + lastParent + "' in line: " + currentLine);
            }

            int localLevel = currentLevel;
            currentLevel++;
            currentIndex++;
            while (currentLevel > localLevel && currentIndex < wordsInCode.Length && wordsInCode[currentIndex] != "}")
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
                    throw new SourceCodeException(message);
                }
            }
            if( currentIndex >= wordsInCode.Length )
            {
                throw new SourceCodeException("'}' at the end of procedure not found");
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
                throw new SourceCodeException("'then' not found after 'if " + varName + "' in line: " + currentLine);
            }
            lastParent = "if " + varName;

            // add to vars
            pkb.InsertVar(varName, currentLine);

            // add to uses
            pkb.SetUses(varName, currentLine);

            ParseBody();
            if (currentIndex < wordsInCode.Length && wordsInCode[currentIndex] != "else")
            {
                throw new SourceCodeException("'else' not found after 'if " + varName + " then { ... }' in line: " + currentLine);
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
            pkb.InsertVar(varName, currentLine);

            // add to uses
            pkb.SetUses(varName, currentLine);

            ParseBody();
            return;
        }

        private void ParseCall()
        {
            currentIndex++;
            string procName = wordsInCode[currentIndex];
            pkb.InsertProc(procName);
            addToCalledProcedures(procName);
            currentIndex++;
            if (wordsInCode[currentIndex] != ";")
            {
                throw new SourceCodeException("; expected after 'call " + procName + "' in line: " + currentLine);
            }
            pkb.SetCalls(currentProcedure, procName, currentLine);
            currentIndex++;
        }

        private void ParseAssign()
        {
            int i = 0;
            string varModified = wordsInCode[currentIndex];
            IsSynonym(varModified);

            // add to vars
            pkb.InsertVar(varModified, currentLine);

            // add to modifies
            pkb.SetModifies(varModified, currentLine);

            i++;
            if (wordsInCode[currentIndex + i] != "=")
            {
                throw new SourceCodeException("= expected after '" + varModified + "' in line: " + currentLine);
            }

            i++;
            string next = "var";
            while (wordsInCode[currentIndex + i] != ";")
            {
                if (next == "var")
                {
                    if (wordsInCode[currentIndex + i] == "(")
                    {
                        i = ParseInnerBracket(i);
                    }
                    else
                    {
                        if (!int.TryParse(wordsInCode[currentIndex + i], out int r))
                        {
                            string varUsed = wordsInCode[currentIndex + i];
                            IsSynonym(varUsed);

                            // add to vars
                            pkb.InsertVar(varUsed, currentLine);

                            // add to uses
                            pkb.SetUses(varUsed, currentLine);
                        }
                        else
                        {
                            // its number
                        }
                    }
                    next = "operator";
                }
                else
                {
                    IsAssignArythmetic(wordsInCode[currentIndex + i]);
                    next = "var";
                }
                i++;
            }
            if (i < 3)
            {
                throw new SourceCodeException("Assign '" + varModified + " = ...' is too short in line: " + currentLine);
            }
            else if (i % 2 == 0)
            {
                throw new SourceCodeException("Assign '" + varModified + " = ...' is ending with wrong char in line: " + currentLine);
            }
            currentIndex = currentIndex + i + 1;
        }

        private int ParseInnerBracket(int i)
        {
            i++;
            string next = "var";
            while (wordsInCode[currentIndex + i] != ")")
            {
                if (next == "var")
                {
                    if (wordsInCode[currentIndex + i] == "(")
                    {
                        i = ParseInnerBracket(i);
                    }
                    else
                    {
                        if (!int.TryParse(wordsInCode[currentIndex + i], out int r))
                        {
                            string varUsed = wordsInCode[currentIndex + i];
                            IsSynonym(varUsed);

                            // add to vars
                            pkb.InsertVar(varUsed, currentLine);

                            // add to uses
                            pkb.SetUses(varUsed, currentLine);
                        }
                        else
                        {
                            // its number
                        }
                    }
                    next = "operator";
                }
                else
                {
                    IsAssignArythmetic(wordsInCode[currentIndex + i]);
                    next = "var";
                }
                i++;
            }
            return i;
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
            string[] separators = new string[] { " " };
            return Regex.Replace(code, @"\s+", " ").Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //return code.Split(separators, StringSplitOptions.RemoveEmptyEntries);
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

            //throw new WrongCodeException("; expected after assign in line: " + currentLine);
            if (keywords.Contains(toCheck) || int.TryParse(toCheck, out int r) || Regex.IsMatch(toCheck, @"^([a-zA-Z]){1}([a-zA-Z]|[0-9]|[#])*$"))
            {
                throw new SourceCodeException("; expected after assign in line: " + currentLine);
            }
            else if (toCheck == ";")
            {
                throw new SourceCodeException(") expected before ; in assign in line: " + currentLine);
            }
            else
            {
                throw new SourceCodeException("Invalid assign in line: " + currentLine);
            }

        }

        private void IsSynonym(string synonym)
        {
            if (!Regex.IsMatch(synonym, @"^([a-zA-Z]){1}([a-zA-Z]|[0-9]|[#])*$"))
            {
                throw new SourceCodeException("Wrong synonym format: '" + synonym + "' in line: " + currentLine);
            }
        }

        private void addToCalledProcedures(string proc)
        {
            if (!calledProcedures.Contains(proc)) calledProcedures.Add(proc);
        }

        private void addToDeclaredProcedures(string proc)
        {
            if (!declaredProcedures.Contains(proc)) declaredProcedures.Add(proc);
        }
    }
}
