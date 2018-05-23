//using SpaWpfApp.Exceptions;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;

//namespace SpaWpfApp.Parser
//{
//    class ParserInside
//    {
//        string[] words;
//        Dictionary<string, Action> instructionActions;
//        private int curentIndex;
//        private string parsedCode;


//        public ParserInside(string[] words)
//        {
//            this.words = words;
//            parsedCode = null;
//            instructionActions = new Dictionary<string, Action>{
//              {"call", ParseCall},
//              {"while", ParseWhile},
//              {"if", ParseIf},
//              {"else", ParseElse},
//              {"else{", ParseElseWithBracket},
//            };



//        }

//        public string Parse()
//        {

//            // Console.WriteLine("sssssssssssss");
//            Trace.WriteLine("START================");

//            for (curentIndex = 0; curentIndex < words.Length; curentIndex++)
//            {
//                Console.WriteLine(words[curentIndex]);
//                if (words[curentIndex] == "}")
//                {
//                    if (parsedCode != null)
//                    {

//                        parsedCode = parsedCode.Substring(0, parsedCode.Length - 2);
//                        parsedCode += ParserHelpers.space + "}" + Environment.NewLine;
//                        //   curentIndex++;
//                        break;
//                    }
//                    else
//                    {
//                        throw new WrongCodeException();
//                    }
//                }
//                if (instructionActions.Keys.Any(k => k == words[curentIndex]))
//                {
//                    instructionActions[words[curentIndex]]();

//                }
//                else
//                {
//                    ParseAssigment();
//                }

//            }
//            Trace.WriteLine(parsedCode);
//            Trace.WriteLine("END");
//            return parsedCode;
//        }

//        public int GetLastWordIndex()
//        {
//            return curentIndex;
//        }
//        private void ParseCall()
//        {
//            ParserMain.Instance.numberOfLines++;
//            ParserMain.Instance.Calls.Add((ParserMain.Instance.CurrentProcedure, words[curentIndex+1], ParserMain.Instance.numberOfLines));
//            parsedCode += words[GetIndex()] + ParserHelpers.space + EndOfLine(words[GetIndex(true)]) + Environment.NewLine;
//        }
//        private void ParseWhile()
//        {
//            ParserMain.Instance.numberOfLines++;
//            parsedCode += words[GetIndex()] + ParserHelpers.space + StartBracket(words[GetIndex(true)]) + Environment.NewLine;
//            CallInnerParser();
//        }
//        private void ParseIf()
//        {
//            ParserMain.Instance.numberOfLines++;
//            parsedCode += words[GetIndex()] + ParserHelpers.space + words[GetIndex(true)] + ParserHelpers.space + StartBracket(words[GetIndex(true)], "then") + Environment.NewLine;
//            CallInnerParser();
//        }
//        private void ParseElse()
//        {
//            parsedCode += words[GetIndex()] + ParserHelpers.space + StartBracket(words[GetIndex(true)]) + Environment.NewLine;
//            CallInnerParser();
//        }
//        private void ParseElseWithBracket()
//        {
//            var firstWords = words[GetIndex()];
//            parsedCode += firstWords[firstWords.Length - 1] + ParserHelpers.space + "{" + Environment.NewLine;
//            CallInnerParser();

//        }
//        private void ParseAssigment()
//        {
//            ParserMain.Instance.numberOfLines++;
//            string instriction = null;
//            bool specialCharacterFound = false;
//            if (words[GetIndex()].Contains("="))
//            {
//                for (int i = 0; i < words[GetIndex()].Length; i++)
//                {

//                    if (words[GetIndex()][i] != '=')
//                    {
//                        instriction += words[GetIndex()][i];
//                    }
//                    else
//                    {
//                        if (!specialCharacterFound)
//                        {
//                            specialCharacterFound = true;
//                            ParserMain.Instance.Modifies.Add((instriction, ParserMain.Instance.numberOfLines));
//                            instriction += ParserHelpers.space + words[GetIndex()][i] + (i == (words[GetIndex()].Length - 1) ? "" : ParserHelpers.space);
//                        }
//                        else
//                        {
//                            throw new WrongCodeException();
//                        }
//                    }
//                }
//            }
//            else
//            {
//                instriction += words[GetIndex()];
//                ParserMain.Instance.Modifies.Add((instriction, ParserMain.Instance.numberOfLines));
//                if (words[GetIndex(true)] != "=")
//                {
//                    throw new WrongCodeException();
//                }
//                else
//                {
//                    instriction += ParserHelpers.space + words[GetIndex()];
//                }
//            }
//            specialCharacterFound = false;
//            bool shouldBeSign = false;
//            for (int i = curentIndex + 1; i < words.Length; i++)
//            {
//                curentIndex = i;
//                string word = words[i];
//                if (word.Contains("+") || word.Contains("-") || word.Contains("*"))
//                {
//                    instriction += ParserHelpers.space + word;//TODO
//                    shouldBeSign = false;
//                }
//                else
//                {
//                    if (shouldBeSign)
//                    {
//                        if (word == "+" || word == "*" || word == "-" || word == ";")
//                        {
//                            instriction += ParserHelpers.space + word;
//                            shouldBeSign = false;
//                        }
//                        else
//                        {
//                            throw new WrongCodeException();
//                        }
//                        if (word == ";")
//                        {
//                            break;//TODO
//                        }

//                    }
//                    else
//                    {
//                        shouldBeSign = true;
//                        ParserMain.Instance.VariableNames.Add((word, ParserMain.Instance.numberOfLines));
//                        instriction += ParserHelpers.space + word;
//                    }
//                }
//                specialCharacterFound = false;
//            }
//            parsedCode += instriction + Environment.NewLine;


//        }
//        private void CallInnerParser()
//        {
//            ParserInside parser = new ParserInside(words.Skip(GetIndex(true)).ToArray());
//            parsedCode += parser.Parse();
//            curentIndex += parser.GetLastWordIndex();
//        }
//        private string EndOfLine(string lastWord)
//        {
//            string partOfInstruction = lastWord;
//            if (partOfInstruction[partOfInstruction.Length - 1] == ';')
//            {
//                partOfInstruction = lastWord.Substring(0, partOfInstruction.Length - 1) + ParserHelpers.space + ";";
//            }
//            else
//            {
//                var nextWord = words[GetIndex(true)];
//                if (nextWord != ";")
//                {
//                    throw new WrongCodeException();
//                }
//                partOfInstruction += ParserHelpers.space + ";";
//            }
//            return partOfInstruction;
//        }
//        private string StartBracket(string lastWord, string mustBeWord = null)
//        {
//            string partOfInstruction = lastWord;
//            if (partOfInstruction[partOfInstruction.Length - 1] == '{')
//            {
//                partOfInstruction = lastWord.Substring(0, partOfInstruction.Length - 1) + ParserHelpers.space + "{";
//            }
//            else
//            {
//                var nextWord = words[GetIndex(true)];
//                if (nextWord != "{")
//                {
//                    throw new WrongCodeException();
//                }
//                partOfInstruction += ParserHelpers.space + "{";
//            }
//            return partOfInstruction;
//        }
//        private string CorrectWord(string word)
//        {

//            return word;
//        }
//        private int GetIndex(bool ifAddOne = false)
//        {
//            if (ifAddOne)
//            {
//                curentIndex++;
//            }
//            if (curentIndex < 0 || curentIndex >= words.Length)
//            {
//                throw new WrongCodeException();
//            }
//            return curentIndex;
//        }

//    }
//}
