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

            for(int i = 0; i < sourceCodeLines.Length -1; i++)
            {
                //if (sourceCodeLines[i] == "") { break; }
                lineWords = sourceCodeLines[i].Split(' ');
                switch (lineWords[0])
                {
                    case "procedure":
                        {
                            if (actualCfgStructure != null)
                            {
                                CfgList.Add(actualCfgStructure);
                            }
                            actualCfgStructure = new Cfg();
                            actualNode = null;
                            currentPreviousNode = null;
                        }
                        break;

                    case "while":
                        {
                            if(actualNode != null)
                            {
                                currentPreviousNode = actualNode;
                            }

                            actualNode = new GNode(GNodeTypeEnum.While);
                            actualNode.programLineList.Add(++programLineNumber);

                            if(currentPreviousNode != null)
                            {
                                currentPreviousNode.nextGNodeList.Add(actualNode);
                                actualNode.previousGNodeList.Add(currentPreviousNode);

                                HandleIfHaveToLinkStmtThenNode(ref actualNode, ref currentPreviousNode);
                            }

                            //dodaj do struktury
                            actualCfgStructure.GNodeList.Add(actualNode);

                            currentPreviousNode = actualNode;

                            actualNode = null;

                            //to do
                            //jesli po if else, to wroc tez do if stmt
                        }
                        break;

                    case "if":
                        {
                            if (actualNode != null)
                            {
                                currentPreviousNode = actualNode;
                            }

                            actualNode = new GNode(GNodeTypeEnum.If);
                            actualNode.programLineList.Add(++programLineNumber);

                            if (currentPreviousNode != null)
                            {
                                currentPreviousNode.nextGNodeList.Add(actualNode);
                                actualNode.previousGNodeList.Add(currentPreviousNode);

                                HandleIfHaveToLinkStmtThenNode(ref actualNode, ref currentPreviousNode);
                            }

                            actualCfgStructure.GNodeList.Add(actualNode);

                            currentPreviousNode = actualNode;

                            // tworzymy node dla then stmt
                            actualNode = new GNode(GNodeTypeEnum.StmtLstThen);

                            currentPreviousNode.nextGNodeList.Add(actualNode);
                            actualNode.previousGNodeList.Add(currentPreviousNode);
                            actualCfgStructure.GNodeList.Add(actualNode);

                            //to do
                            //jesli po if else, to wroc tez do if stmt
                        }
                        break;

                    case "else":
                        {
                            actualNode = new GNode(GNodeTypeEnum.StmtLstElse);

                            currentPreviousNode.nextGNodeList.Add(actualNode);
                            actualNode.previousGNodeList.Add(currentPreviousNode);

                            actualCfgStructure.GNodeList.Add(actualNode);
                        }
                        break;

                    // assign, call
                    default:
                        {
                            if (actualNode == null)
                            {
                                actualNode = new GNode(GNodeTypeEnum.StmtLst);

                                if(currentPreviousNode != null)
                                {
                                    currentPreviousNode.nextGNodeList.Add(actualNode);
                                    actualNode.previousGNodeList.Add(currentPreviousNode);

                                    HandleIfHaveToLinkStmtThenNode(ref actualNode, ref currentPreviousNode);       
                                }

                                actualCfgStructure.GNodeList.Add(actualNode);
                            }

                            actualNode.programLineList.Add(++programLineNumber);


                            //what if end of statement???
                            HandleEndOfStatement(ref actualNode, ref currentPreviousNode, lineWords, (i==sourceCodeLines.Length-2 || sourceCodeLines[i+1].Contains("procedure")) ? true : false);
                        }
                        break;
                }
            }

            this.CfgList.Add(actualCfgStructure);
        }

        private void HandleIfHaveToLinkStmtThenNode(ref GNode actualNode, ref GNode currentPreviousNode)
        {
            GNode tmp;
            if (currentPreviousNode.type == GNodeTypeEnum.StmtLstElse)
            {
                tmp = currentPreviousNode.previousGNodeList[0];
                tmp = tmp.nextGNodeList[0];
                tmp.nextGNodeList.Add(actualNode);
                actualNode.previousGNodeList.Add(tmp);
            }
        }

        private void HandleEndOfStatement(ref GNode actualNode, ref GNode currentPreviousNode, string[] lineWords, bool lastLineWord)
        {
            int howManyStatementsEnd = determineHowManyStatementsEnd(lineWords);
            if(!lastLineWord)
            {
                while (howManyStatementsEnd-- > 0)
                {
                    if (actualNode.type == GNodeTypeEnum.StmtLst)
                    {                      
                        actualNode.nextGNodeList.Add(currentPreviousNode);
                        currentPreviousNode.previousGNodeList.Add(actualNode);

                        if (howManyStatementsEnd == 0)
                        {
                            actualNode = null;
                        }
                        else
                        {
                            actualNode = currentPreviousNode;
                        }
                    }

                    else if (actualNode.type == GNodeTypeEnum.While && !lastLineWord)
                    {
                        GNode tmp = actualNode;
                        do
                        {
                            tmp = tmp.previousGNodeList[0];
                        }
                        while (tmp.type != GNodeTypeEnum.If && tmp.type != GNodeTypeEnum.While);

                        actualNode.nextGNodeList.Add(tmp);
                        tmp.previousGNodeList.Add(actualNode);

                        actualNode = tmp;
                    }

                    else if (actualNode.type == GNodeTypeEnum.If) // kiedy z ifa trzeba wrocic do while lub innego ifa
                    {
                        GNode tmp = actualNode;
                        do
                        {
                            tmp = tmp.previousGNodeList[0];
                        }
                        while (tmp.type != GNodeTypeEnum.If && tmp.type != GNodeTypeEnum.While);

                        actualNode.nextGNodeList[0].nextGNodeList.Add(tmp);
                        actualNode.nextGNodeList[1].nextGNodeList.Add(tmp);

                        tmp.previousGNodeList.Add(actualNode.nextGNodeList[0]);
                        tmp.previousGNodeList.Add(actualNode.nextGNodeList[1]);

                        actualNode = tmp;
                    }

                    else if (actualNode.type == GNodeTypeEnum.StmtLstThen)
                    {
                    }

                    else if (actualNode.type == GNodeTypeEnum.StmtLstElse && !lastLineWord)
                    {
                        if (howManyStatementsEnd == 0)
                        {
                            currentPreviousNode = actualNode;
                            actualNode = null;
                        }
                        else
                        {
                            actualNode = currentPreviousNode;
                        }
                    }
                }
            }
        }

        private int determineHowManyStatementsEnd(string[] lineWords)
        {
            int i = lineWords.Length - 1;
            int closeBracketCounter = 0;

            while (lineWords[i--][0] == 125) // '}'
            { closeBracketCounter++; }

            return closeBracketCounter;
        }
    }
}
