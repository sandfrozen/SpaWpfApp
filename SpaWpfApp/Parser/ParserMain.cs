using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq;
namespace SpaWpfApp.Parser
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
            string[] wordsInCode = GetWordsInCode();

            for (int i = 0; i < wordsInCode.Length; i++)
            {
                if (wordsInCode[i] == "procedure")
                {
                    numberOfProcedures++;
                    parsedSourceCode += wordsInCode[i] + ParserHelpers.space + wordsInCode[++i] + ParserHelpers.space + wordsInCode[++i] + Environment.NewLine;
                    parsedSourceCode += new ParserInside(wordsInCode.Skip((++i)).ToArray()).Parse();
                    //  new InstructionProcedure().ParseInstruction(wordsInCode);
                }
            }
            return parsedSourceCode;
        }
        private string[] GetWordsInCode()
        {
            string[] separators = new string[] { " ", Environment.NewLine };
            return sourceCode.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
