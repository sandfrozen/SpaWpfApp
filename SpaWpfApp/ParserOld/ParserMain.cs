using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq;
using SpaWpfApp.PkbOld;

namespace SpaWpfApp.ParserOld
{
    public class ParserMain
    {
        private static ParserMain instance;
        private string sourceCode;
        //   private string[] wordsInCode;
        private string parsedSourceCode;
        public int numberOfLines = 0;
        public int numberOfVariables = 0;
        public int numberOfProcedures = 0;

        public string CurrentProcedure;

        public List<string> ProcedureNames = new List<string>();
        public List<(string, int)> VariableNames = new List<(string, int)>();
        public List<(string, string, int)> Calls = new List<(string, string, int)>();
        public List<(string, int)> Modifies = new List<(string, int)>();

        public Pkb pkb;

        private ParserMain()
        {

        }

        public static ParserMain Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ParserMain();
                }
                return instance;
            }
        }

        public string Parse(string sourceCode)
        {
            numberOfLines = 0;
            numberOfVariables = 0;
            numberOfProcedures = 0;
            this.sourceCode = sourceCode;
            parsedSourceCode = null;
            ProcedureNames = new List<string>();
            VariableNames = new List<(string, int)>();
            Calls = new List<(string, string, int)>();
            Modifies = new List<(string, int)>();

            string[] wordsInCode = GetWordsInCode();

            for (int i = 0; i < wordsInCode.Length; i++)
            {
                if (wordsInCode[i] == "procedure")
                {
                    numberOfProcedures++;
                    CurrentProcedure = wordsInCode[i + 1];
                    ProcedureNames.Add(CurrentProcedure);
                    parsedSourceCode += wordsInCode[i] + ParserHelpers.space + wordsInCode[++i] + ParserHelpers.space + wordsInCode[++i] + Environment.NewLine;
                    parsedSourceCode += new ParserInside(wordsInCode.Skip((++i)).ToArray()).Parse();
                    //  new InstructionProcedure().ParseInstruction(wordsInCode);
                }
            }


            CreatePKB();


            return parsedSourceCode;
        }

        private void CreatePKB()
        {
            CorrectVariables();
            var varList = GetAllVarNames();

            pkb = new Pkb(numberOfLines, ProcedureNames.Count, varList.Count);
            foreach (var item in ProcedureNames)
            {
                pkb.InsertProc(item);
            }
            foreach (var item in varList)
            {
                pkb.InsertVar(item);
            }
            foreach (var item in Calls)
            {
                pkb.SetCalls(item.Item1, item.Item2, item.Item3);
            }
            foreach (var item in Modifies)
            {
                pkb.SetModifies(item.Item1, item.Item2-1);
            }
            foreach (var item in VariableNames)
            {
                pkb.SetUses(item.Item1, item.Item2-1);
            }

        }
        private void CorrectVariables()
        {
            for (int i = 0; i < VariableNames.Count; i++)
            {
                if (Char.IsDigit(VariableNames[i].Item1[0]))
                {
                    VariableNames.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < Modifies.Count; i++)
            {
                if (Char.IsDigit(Modifies[i].Item1[0]))
                {
                    Modifies.RemoveAt(i);
                    i--;
                }
            }
        }

        private List<string> GetAllVarNames()
        {
            List<string> list = new List<string>();

            for (int i = 0; i < VariableNames.Count; i++)
            {
                if (!list.Contains(VariableNames[i].Item1))
                {
                    list.Add(VariableNames[i].Item1);
                }
            }
            for (int i = 0; i < Modifies.Count; i++)
            {
                if (!list.Contains(Modifies[i].Item1))
                {
                    list.Add(Modifies[i].Item1);
                }
            }

            return list;
        }
        private string[] GetWordsInCode()
        {
            string[] separators = new string[] { " ", Environment.NewLine };
            return sourceCode.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
