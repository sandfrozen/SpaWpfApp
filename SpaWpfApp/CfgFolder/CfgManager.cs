using SpaWpfApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.CfgFolder
{
    public class CfgManager
    {
        List<Cfg> CfgList;

        public CfgManager(string sourceCode)
        {
            this.CfgList = new List<Cfg>();
            this.BuildCfgList(sourceCode);
        }

        private void BuildCfgList(string sourceCode)
        {
            string[] sourceCodeLines = sourceCode.Split('\n');
            string[] lineWords;
            int programLineNumber = 0;
            GNode currentPreviousNode = null;
            GNode actualNode = null;
            GNode tmp;
            Cfg actualCfgStructure = null;

            foreach (var sourceCodeLine in sourceCodeLines)
            {
                if (sourceCodeLine == "") { break; }
                lineWords = sourceCodeLine.Split(' ');
                switch (lineWords[0])
                {
                    case "procedure":
                        {
                            if(actualCfgStructure != null)
                            {
                                CfgList.Add(actualCfgStructure);
                            }
                            actualCfgStructure = new Cfg();
                        }
                        break;

                    case "while":
                        {
                            tmp = new GNode(GNodeTypeEnum.While);
                            tmp.programLineList.Add(++programLineNumber);

                            currentPreviousNode.nextGNodeList.Add(tmp);
                            tmp.previousGNodeList.Add(currentPreviousNode);

                            //to do
                            //jesli po if else, to wroc tez do if stmt
                        }
                        break;

                    case "if":
                        {
                            actualNode = new GNode(GNodeTypeEnum.If);
                            actualNode.programLineList.Add(++programLineNumber);

                            currentPreviousNode.nextGNodeList.Add(actualNode);
                            actualNode.previousGNodeList.Add(currentPreviousNode);

                            currentPreviousNode = actualNode;

                            // tworymy node dla then stmt
                            actualNode = new GNode(GNodeTypeEnum.StmtLstThen);

                            currentPreviousNode.nextGNodeList.Add(actualNode);
                            actualNode.previousGNodeList.Add(currentPreviousNode);
                            
                            //to do
                            //jesli po if else, to wroc tez do if stmt
                        }
                        break;

                    case "else":
                        {
                            actualNode = new GNode(GNodeTypeEnum.StmtLstElse);

                            currentPreviousNode.nextGNodeList.Add(actualNode);
                            actualNode.previousGNodeList.Add(currentPreviousNode);
                        }
                        break;

                    // assign, call
                    default:
                        {
                            if(actualNode == null)
                            {
                                actualNode = new GNode(GNodeTypeEnum.StmtLst);                               

                                currentPreviousNode.nextGNodeList.Add(actualNode);
                                actualNode.previousGNodeList.Add(currentPreviousNode);
                            }

                            actualNode.programLineList.Add(++programLineNumber);


                            //what if end of statement???
                            HandleGoingBackOnTheTree(ref currentPreviousNode, lineWords);
                        }
                        break;
                }
            }
        }
    }
}
