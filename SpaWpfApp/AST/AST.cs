using SpaWpfApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.AST
{
    public class AST
    {
        public List<TNode> NodeList { get; set; }

        public AST(string sourceCode)
        {
            string[] sourceCodeLines = sourceCode.Split('\n');
            string[] lineWords;
            int braceCounter = 0;

            foreach (var sourceCodeLine in sourceCodeLines)
            {
                lineWords = sourceCodeLine.Split(' ');
                foreach (var lineWord in lineWords)
                {
                    if (lineWord.Equals("procedure"))
                    {

                    }
                }
            }
        }



        private TNode CreateTNode(TNodeTypeEnum type, )
        {
            return new TNode();
        }
    }
}
