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

        private GNode currentPreviousNode = null;
        private GNode actualNode = null;
        private Cfg actualCfgStructure = null;
        private string[] sourceCodeLines;
        private int programLineNumber = 0;
        private int howManyStatementsEnd = 0;


        public CfgManager(string sourceCode)
        {
            this.CfgList = new List<Cfg>();
            sourceCodeLines = sourceCode.Split('\n');

            this.BuildCfgList();
        }



        private void BuildCfgList()
        {
            string[] lineWords;

            for (int i = 0; i < sourceCodeLines.Length - 1; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');
                switch (lineWords[0])
                {
                    case "if":
                        {
                            BuildIf(ref i);
                        }
                        break;

                    case "while":
                        {
                            BuildWhile(ref i);
                        }
                        break;

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

                    default:
                        {
                            BuildAssignCall();
                        }
                        break;
                }
            }

            this.CfgList.Add(actualCfgStructure);
        }



        private void BuildAssignCall()
        {
            if (actualNode == null) // jesli pierwsza instrukcja w procedurze
            {
                actualNode = new GNode(GNodeTypeEnum.StmtLst, ++programLineNumber);
                actualCfgStructure.GNodeList.Add(actualNode);
            }
            else if (actualNode.type != GNodeTypeEnum.StmtLst) // jesli pierwsza instrukcja w while/if
            {
                currentPreviousNode = actualNode;
                actualNode = new GNode(GNodeTypeEnum.StmtLst, ++programLineNumber);
                actualCfgStructure.GNodeList.Add(actualNode);

                currentPreviousNode.nextGNodeList.Add(actualNode);
                actualNode.previousGNodeList.Add(currentPreviousNode);
            }
            else
            {
                actualNode.programLineList.Add(++programLineNumber);
            }
        }

        private void BuildIf(ref int i)
        {
            string[] lineWords;
            GNode ifNodeMain, endNodeThenSection = null, ghostNode;

            #region create node for if and remember this node and endNodeThenSection
            currentPreviousNode = actualNode;

            actualNode = new GNode(GNodeTypeEnum.If, ++programLineNumber);
            actualCfgStructure.GNodeList.Add(actualNode);

            // zapamietaj
            ifNodeMain = actualNode;

            if (currentPreviousNode != null)
            {
                currentPreviousNode.nextGNodeList.Add(actualNode);
                actualNode.previousGNodeList.Add(currentPreviousNode);
            }
            #endregion

            for (++i; i < sourceCodeLines.Length - 1; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "else":
                        {
                            BuildElse(ref i);

                            ghostNode = new GNode(GNodeTypeEnum.Ghost);
                            actualCfgStructure.GNodeList.Add(ghostNode);

                            actualNode.nextGNodeList.Add(ghostNode);
                            endNodeThenSection.nextGNodeList.Add(ghostNode);

                            ghostNode.previousGNodeList.Add(actualNode);
                            ghostNode.previousGNodeList.Add(endNodeThenSection);

                            currentPreviousNode = null;
                            actualNode = ghostNode;

                            return;
                        }
                        break;

                    case "while":
                        {
                            BuildWhile(ref i);
                        }
                        break;

                    case "if":
                        {
                            BuildIf(ref i);
                        }
                        break;

                    default:
                        {
                            BuildAssignCall();

                            if (EndOfStatement(lineWords)) // end of then section
                            {
                                endNodeThenSection = actualNode;
                                actualNode = ifNodeMain;
                                --howManyStatementsEnd;
                            }
                        }
                        break;
                }
            }
        }

        private void BuildElse(ref int i)
        {
            string[] lineWords;

            for (++i; i < sourceCodeLines.Length - 1; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "while":
                        {
                            BuildWhile(ref i);
                            if (howManyStatementsEnd > 0)
                            {
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;

                    case "if":
                        {
                            BuildIf(ref i);

                            if (howManyStatementsEnd > 0)
                            {
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;

                    default:
                        {
                            BuildAssignCall();

                            if (EndOfStatement(lineWords))
                            {
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;
                }
            }
        }

        private void BuildWhile(ref int i)
        {
            string[] lineWords;
            GNode whileNodeMain;

            #region create node for while and remember it
            currentPreviousNode = actualNode;

            actualNode = new GNode(GNodeTypeEnum.While, ++programLineNumber);
            actualCfgStructure.GNodeList.Add(actualNode);
           
            //zapamietaj
            whileNodeMain = actualNode;

            if (currentPreviousNode != null)
            {
                currentPreviousNode.nextGNodeList.Add(actualNode);
                actualNode.previousGNodeList.Add(currentPreviousNode);
            }
            currentPreviousNode = actualNode;
            #endregion

            for (++i; i < sourceCodeLines.Length - 1; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "while":
                        {
                            BuildWhile(ref i);
                            if(howManyStatementsEnd > 0)
                            {
                                CloseWhileLoop(ref whileNodeMain);
                                return;
                            }
                        }
                        break;

                    case "if":
                        {
                            BuildIf(ref i);
                            if (howManyStatementsEnd > 0)
                            {
                                CloseWhileLoop(ref whileNodeMain);
                                return;
                            }
                        }
                        break;

                    default:
                        {
                            BuildAssignCall();

                            if (EndOfStatement(lineWords))
                            {
                                CloseWhileLoop(ref whileNodeMain);
                                return;
                            }
                        }
                        break;
                }
            }
        }

        private void CloseWhileLoop(ref GNode whileNodeMain)
        {
            actualNode.nextGNodeList.Add(whileNodeMain);
            whileNodeMain.previousGNodeList.Add(actualNode);

            currentPreviousNode = null;
            actualNode = whileNodeMain;
            --howManyStatementsEnd;
        }

        private bool EndOfStatement(string[] lineWords)
        {
            howManyStatementsEnd = DetermineHowManyStatementsEnd(lineWords);

            return howManyStatementsEnd > 0 ? true : false;
        }

        private int DetermineHowManyStatementsEnd(string[] lineWords)
        {
            int i = lineWords.Length - 1;
            int closeBracketCounter = 0;

            while (lineWords[i--][0] == 125) // '}'
            { closeBracketCounter++; }

            return closeBracketCounter;
        }
    }
}
