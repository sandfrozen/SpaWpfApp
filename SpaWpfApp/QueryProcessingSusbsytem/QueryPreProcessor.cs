﻿using SpaWpfApp.Exceptions;
using SpaWpfApp.PkbNew;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    public class QueryPreProcessor
    {
        private string parsedQuery;

        private string[] wordsInQuery;
        private int currentIndex;

        private List<string> selectClauses;
        private Dictionary<string, Action> relationsReference;
        public Dictionary<string, string> declarationsList { get; set; }
        public Dictionary<string, string> returnList { get; set; }
        public List<Condition> conditionsList { get; set; }

        //private Dictionary<string, string[]> entityAttributeValue;
        private Dictionary<string, string> entityAttributeType;
        private Dictionary<string, Action> declarationActions;

        private static QueryPreProcessor instance;

        public static QueryPreProcessor GetInstance()
        {
            if (instance == null)
            {
                instance = new QueryPreProcessor();
            }
            return instance;
        }

        public QueryPreProcessor()
        {
            declarationsList = new Dictionary<string, string>();
            returnList = new Dictionary<string, string>();
            conditionsList = new List<Condition>();
            relationsReference = new Dictionary<string, Action> {
                { Relation.Modifies, CheckModifies},
                { Relation.Uses, CheckModifies},
                { Relation.Calls, CheckModifies},
                { Relation.CallsX, CheckModifies},
                { Relation.Parent, CheckModifies},
                { Relation.ParentX, CheckModifies},
                { Relation.Follows, CheckModifies},
                { Relation.FollowsX, CheckModifies},
                { Relation.Next, CheckModifies},
                { Relation.NextX, CheckModifies},
                { Relation.Affects, CheckModifies},
                { Relation.AffectsX, CheckModifies },
            };
            selectClauses = new List<string>
            {
                "such", "pattern", "with", "and"
            };

            //entityAttributeValue = new Dictionary<string, string[]>
            //{
            //    {"procedure", new string[] { "procName", "string" } },
            //    {"stmt", new string[] { "stmt#", "int" } },
            //    {"assign", new string[] { "stmt#", "int" } },
            //    {"call", new string[] { "procName", "string" } },
            //    {"variable", new string[] { "varName", "string" } },
            //    {"constatnt", new string[] { "value", "int" } },
            //};

            entityAttributeType = new Dictionary<string, string>
            {
                {"string", "string" },
                {"int", "int" },
                {"procedure", "string" },
                {"procedure.procName", "string" },
                {"stmtLst", "int" },
                {"stmt", "int" },
                {"stmt.stmt#", "int" },
                {"assign", "int" },
                {"assign.stmt#", "int" },
                {"call", "int" },
                {"call.procName", "string" },
                {"variable", "string" },
                {"variable.varName", "string"},
                {"constant", "int" },
                {"constant.value", "int" },
                {"if", "int" },
                {"if.stmt#", "int" },
                {"while", "int" },
                {"while.stmt#", "int" },
                {"prog_line", "int" },
                {"prog_line.stmt#", "int" },
            };

            declarationActions = new Dictionary<string, Action>{
              { Entity.procedure, ParseDeclaration},
              { Entity.stmtLst, ParseDeclaration},
              { Entity.stmt, ParseDeclaration},
              { Entity.assign, ParseDeclaration},
              { Entity.call, ParseDeclaration},
              { Entity._while, ParseDeclaration},
              { Entity._if, ParseDeclaration},
              { Entity.variable, ParseDeclaration},
              { Entity.constant, ParseDeclaration},
              { Entity.prog_line, ParseDeclaration},
            };
        }

        private void CheckModifies()
        {

        }

        public string Parse(string query)
        {
            declarationsList = new Dictionary<string, string>();
            returnList = new Dictionary<string, string>();
            conditionsList = new List<Condition>();
            parsedQuery = "";
            if (!query.Contains("Select"))
            {
                throw new QueryException("Select is missing.");
            }
            wordsInQuery = GetWordsInCode(query);
            currentIndex = 0;
            if (wordsInQuery[0] != "Select")
            {
                for (currentIndex = 0; currentIndex < wordsInQuery.Length; currentIndex++)
                {
                    if (declarationActions.Keys.Any(k => k == wordsInQuery[currentIndex]))
                    {
                        declarationActions[wordsInQuery[currentIndex]]();

                    }
                    else if (wordsInQuery[currentIndex] == "Select")
                    {
                        break;
                    }
                    else
                    {
                        throw new QueryException("Invalid word or character in declaration: " + wordsInQuery[currentIndex]);
                    }
                }
            }
            if (wordsInQuery[currentIndex] != "Select")
            {
                throw new QueryException("Select not found: " + wordsInQuery[currentIndex]);
            }
            parsedQuery += wordsInQuery[currentIndex++];
            ParseTouple();
            while (currentIndex < wordsInQuery.Length)
            {
                if (wordsInQuery[currentIndex] == "such")
                {
                    parsedQuery += " " + wordsInQuery[currentIndex++];
                    if (wordsInQuery[currentIndex] == "that")
                    {
                        parsedQuery += " " + wordsInQuery[currentIndex];
                    }
                    else
                    {
                        throw new QueryException("\"that\" after \"such\" not found: " + wordsInQuery[currentIndex]);
                    }
                    do
                    {
                        ++currentIndex;
                        ParseSuchThat();
                        if (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and")
                        {
                            parsedQuery += " and";
                        }
                    } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and");
                }

                else if (wordsInQuery[currentIndex] == "with")
                {
                    parsedQuery += " " + wordsInQuery[currentIndex];
                    do
                    {
                        ++currentIndex;
                        ParseWith();
                        if (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and")
                        {
                            parsedQuery += " and";
                        }
                    } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and");
                }

                else if (wordsInQuery[currentIndex] == "pattern")
                {
                    parsedQuery += " " + wordsInQuery[currentIndex];
                    do
                    {
                        ++currentIndex;
                        ParsePattern();
                        if (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and")
                        {
                            parsedQuery += " and";
                        }
                    } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] == "and");
                }

                else
                {
                    throw new QueryException("(such that|with|pattern) not found: " + wordsInQuery[currentIndex]);
                }
            }


            //Trace.WriteLine("Declarations:");
            //foreach (var v in declarationsList)
            //{
            //    Trace.WriteLine(v.Value + " " + v.Key);
            //}
            //Trace.WriteLine("Return List:");
            //foreach (var v in returnList)
            //{
            //    Trace.WriteLine(v.Value + " " + v.Key);
            //}
            //Trace.WriteLine("Relations List:");
            //foreach (var r in conditionsList)
            //{
            //    Trace.WriteLine(r.ToString());
            //}

            return parsedQuery;
        }

        private string[] GetWordsInCode(string query)
        {
            for (int i = 0; i < query.Length; i++)
            {
                if (i < query.Length - 1 && IsSeparatorChar(query[i]))
                {
                    query = query.Insert(i, " ");
                    query = query.Insert(i + 2, " ");
                    i = i + 2;
                }
            }
            string[] separators = new string[] { " " };
            return Regex.Replace(query, @"\s+", " ").Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        private bool IsSeparatorChar(char toCheck)
        {
            char[] chars = { '(', ')', ',', ';', '<', '>' };
            foreach (char c in chars)
            {
                if (c == toCheck) return true;
            }
            return false;
        }

        private void CkeckIsDeclared(string synonym)
        {
            if (!declarationsList.Keys.Contains(synonym))
            {
                throw new QueryException("Synonym used in Select is not declared: " + synonym);
            }
        }

        private void CheckTuple(string tuple)
        {
            if (tuple.First() == '<' && tuple.Last() == '>')
            {
                ExtractMultipleTuple(tuple);
            }
            else
            {
                if (tuple.Contains("."))
                {
                    string synonym = tuple.Substring(0, tuple.IndexOf('.'));
                    CkeckIsDeclared(synonym);
                    string attrName = tuple.Substring(tuple.IndexOf('.') + 1);
                    string designEntity = declarationsList[synonym];

                    switch (attrName)
                    {
                        case "stmt#":
                            if (designEntity != "assign" && designEntity != "stmt" && designEntity != "while" && designEntity != "if")
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            break;
                        case "procName":
                            if (designEntity != "procedure" && designEntity != "call")
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            break;
                        case "varName":
                            if (designEntity != "variable")
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            break;
                        case "value":
                            if (designEntity != "constant")
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            break;
                        default:
                            throw new QueryException("Attrybute name: " + attrName + " is unknown");
                    }
                    returnList.Add(tuple, designEntity);
                }
                else
                {
                    CkeckIsDeclared(tuple);
                    returnList.Add(tuple, declarationsList[tuple]);
                }
            }
        }

        private void ExtractMultipleTuple(string tuple)
        {
            for (int elemIndex = 1; elemIndex < tuple.Length - 1; elemIndex++)
            {
                string elem = "";
                do
                {
                    elem += tuple[elemIndex++];
                } while (elemIndex < tuple.Length && tuple[elemIndex] != ',' && tuple[elemIndex] != '>');
                CheckTuple(elem);
            }
        }

        private void IsSynonym(string synonym)
        {
            if (!Regex.IsMatch(synonym, @"^([a-zA-Z]){1}([a-zA-Z]|[0-9]|[#])*$"))
            {
                throw new QueryException("Invalid synonym: " + synonym);
            }
        }

        private void CheckSynonyms(string declaration)
        {
            try
            {
                IsSynonym(declaration);
            }
            catch
            {
                for (int declarationIndex = 0; declarationIndex < declaration.Length - 1; declarationIndex++)
                {
                    string synonym = "";
                    do
                    {
                        synonym += declaration[declarationIndex++];
                    } while (declarationIndex < declaration.Length && declaration[declarationIndex] != ',' && declaration[declarationIndex] != ';');

                    IsSynonym(synonym);
                }
            }
        }

        private void ParseDeclaration()
        {
            List<String> synonymsList = new List<string>();
            string declaration = wordsInQuery[currentIndex] + " ";
            while (wordsInQuery[currentIndex] != ";")
            {
                declaration += wordsInQuery[++currentIndex];
                if (wordsInQuery[currentIndex] != "," && wordsInQuery[currentIndex] != ";")
                {
                    synonymsList.Add(wordsInQuery[currentIndex]);
                }
                if (wordsInQuery[currentIndex] != "," && wordsInQuery[currentIndex] != ";" && wordsInQuery[currentIndex + 1] != "," && wordsInQuery[currentIndex + 1] != ";")
                {
                    declaration += " ";
                }
            }
            string designEntity = declaration.Substring(0, declaration.IndexOf(' '));
            string synonyms = declaration.Substring(declaration.IndexOf(' ') + 1);
            CheckSynonyms(synonyms);

            try
            {
                foreach (string synonym in synonymsList)
                {
                    declarationsList.Add(synonym, designEntity);
                }
            }
            catch
            {
                throw new QueryException("Synonyms can't have same names.\nCheck declaration:  " + declaration);
            }


            parsedQuery += declaration;
            if (wordsInQuery[currentIndex + 1] == "Select")
            {
                parsedQuery += Environment.NewLine;
            }
            else
            {
                parsedQuery += " ";
            }
        }

        private void ParseTouple()
        {
            string tuple = "";
            do
            {
                tuple += wordsInQuery[currentIndex++];
            } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] != "such" && wordsInQuery[currentIndex] != "with" && wordsInQuery[currentIndex] != "pattern");
            if (tuple == "BOOLEAN")
            {
                returnList.Add("BOOLEAN", "BOOLEAN");
            }
            else
            {
                CheckTuple(tuple);
            }
            parsedQuery += " " + tuple;
        }

        private void ParseSuchThat()
        {
            string relation = "";
            relation += wordsInQuery[currentIndex++];
            if (!relationsReference.Keys.Contains(relation))
            {
                throw new QueryException("Unknown Relationship Reference: " + relation);
            }
            while (currentIndex < wordsInQuery.Length && !selectClauses.Contains(wordsInQuery[currentIndex]))
            {
                relation += wordsInQuery[currentIndex++];
            }

            CheckRelation(relation);
            parsedQuery += " " + relation;
        }

        private void CheckRelation(string relation)
        {
            if (Regex.IsMatch(relation, @"^[^(,) ]+[(][^(,) ]+[,][^(,) ]+[)]$"))
            {
                string relRef = relation.Substring(0, relation.IndexOf('('));
                relation = relation.Substring(relation.IndexOf('(') + 1);

                string arg1 = relation.Substring(0, relation.IndexOf(','));
                string arg1type = "";
                if (int.TryParse(arg1, out int result1))
                {
                    arg1type = Entity._int;
                }
                else if (arg1 == Entity._)
                {
                    arg1type = Entity._;
                }
                else if (arg1.First() == '"' && arg1.Last() == '"')
                {
                    IsSynonym(arg1.Trim('"'));
                    arg1type = Entity._string;
                }
                else if (declarationsList.ContainsKey(arg1))
                {
                    arg1type = declarationsList[arg1];
                }
                else
                {
                    throw new QueryException(relRef + " - argument 1 is invalid: " + arg1);
                }

                relation = relation.Substring(relation.IndexOf(',') + 1);
                string arg2 = relation.Substring(0, relation.IndexOf(')'));
                string arg2type = "";
                if (int.TryParse(arg2, out int result2))
                {
                    arg2type = Entity._int;
                }
                else if (arg2 == Entity._)
                {
                    arg2type = Entity._;
                }
                else if (arg2.First() == '"' && arg2.Last() == '"')
                {
                    IsSynonym(arg2.Trim('"'));
                    arg2type = Entity._string;
                }
                else if (declarationsList.ContainsKey(arg2))
                {
                    arg2type = declarationsList[arg2];
                }
                else
                {
                    throw new QueryException(relRef + " - argument 2 is invalid: " + arg2);
                }

                switch (relRef)
                {
                    case Relation.Modifies:
                        if (!Relation.ModifiesArgs1.Contains(arg1type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg1type + " as first argument");
                        }
                        if (!Relation.ModifiesArgs2.Contains(arg2type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg2type + " as second argument");
                        }
                        break;
                    case Relation.Uses:
                        if (!Relation.UsesArgs1.Contains(arg1type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg1type + " as first argument");
                        }
                        if (!Relation.UsesArgs2.Contains(arg2type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg2type + " as second argument");
                        }
                        break;
                    case Relation.Calls:
                    case Relation.CallsX:
                        if (!Relation.CallsArgs1.Contains(arg1type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg1type + " as first argument");
                        }
                        if (!Relation.CallsArgs2.Contains(arg2type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg2type + " as second argument");
                        }
                        break;
                    case Relation.Parent:
                    case Relation.ParentX:
                        if (!Relation.ParentArgs1.Contains(arg1type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg1type + " as first argument");
                        }
                        if (!Relation.ParentArgs2.Contains(arg2type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg2type + " as second argument");
                        }
                        break;
                    case Relation.Follows:
                    case Relation.FollowsX:
                        if (!Relation.FollowsArgs1.Contains(arg1type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg1type + " as first argument");
                        }
                        if (!Relation.FollowsArgs2.Contains(arg2type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg2type + " as second argument");
                        }
                        break;
                    case Relation.Next:
                    case Relation.NextX:
                        if (!Relation.NextArgs1.Contains(arg1type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg1type + " as first argument");
                        }
                        if (!Relation.NextArgs2.Contains(arg2type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg2type + " as second argument");
                        }
                        break;
                    case Relation.Affects:
                    case Relation.AffectsX:
                        if (!Relation.AffectsArgs1.Contains(arg1type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg1type + " as first argument");
                        }
                        if (!Relation.AffectsArgs2.Contains(arg2type))
                        {
                            throw new QueryException("In " + relRef + " you cannot use: " + arg2type + " as second argument");
                        }
                        break;
                }

                conditionsList.Add(new Relation(relRef, arg1, arg1type, arg2, arg2type));
            }
            else
            {
                throw new QueryException("Relationship wrong format: " + relation);
            }
        }

        internal bool ReturnTypeIsBoolean()
        {
            return this.returnList.First().Key == "BOOLEAN" ? true : false;
        }

        private void ParseWith()
        {
            string with = "";
            do
            {
                with += wordsInQuery[currentIndex++];
            } while (currentIndex < wordsInQuery.Length && !selectClauses.Contains(wordsInQuery[currentIndex]));
            CheckWith(with);
            parsedQuery += " " + with;
        }

        private void CheckWith(string with)
        {
            // With
            if (Regex.IsMatch(with, @"^[^= ]+[=][^= ]+$"))
            {
                string left = with.Substring(0, with.IndexOf('='));
                string leftType = "";

                if (int.TryParse(left, out int result1))
                {
                    leftType = Entity._int;
                }
                else if (left.First() == '"' && left.Last() == '"')
                {
                    IsSynonym(left.Trim('"'));
                    leftType = Entity._string;
                }
                else if (declarationsList.ContainsKey(left))
                {
                    leftType = declarationsList[left];
                }
                else if (left.Contains("."))
                {
                    string synonym = left.Substring(0, left.IndexOf('.'));
                    string attrRef = left.Substring(left.IndexOf('.'));
                    if (declarationsList.ContainsKey(synonym))
                    {
                        string synonymType = declarationsList[synonym];
                        if (entityAttributeType.Keys.Contains(synonymType + attrRef))
                        {
                            leftType = synonymType + attrRef;
                        }
                        else
                        {
                            throw new QueryException("Left argument in with have wrong attribute name after dot: " + left);
                        }
                    }
                    else
                    {
                        throw new QueryException("Synonym on the left side of with is not declared: " + synonym);
                    }

                }
                else
                {
                    throw new QueryException("Left argument in with is invalid: " + left);
                }

                with = with.Substring(with.IndexOf('=') + 1);
                string right = with.Substring(0);
                string rightType = "";

                if (int.TryParse(right, out int result2))
                {
                    if (int.TryParse(left, out int result22))
                    {
                        throw new QueryException("Left and right argument in with cannot be <int>: " + left + " = " + right);
                    }
                    rightType = Entity._int;
                }
                else if (right.First() == '"' && right.Last() == '"')
                {
                    if (left.First() == '"' && left.Last() == '"')
                    {
                        throw new QueryException("Left and right argument in with cannot be <string>: " + left + " = " + right);
                    }
                    IsSynonym(right.Trim('"'));
                    rightType = Entity._string;
                }
                else if (declarationsList.ContainsKey(right))
                {
                    rightType = declarationsList[right];
                }
                else if (right.Contains("."))
                {
                    string synonym = right.Substring(0, right.IndexOf('.'));
                    string attrRef = right.Substring(right.IndexOf('.'));
                    if (declarationsList.ContainsKey(synonym))
                    {
                        string synonymType = declarationsList[synonym];
                        if (entityAttributeType.ContainsKey(synonymType + attrRef))
                        {
                            rightType = synonymType + attrRef;
                        }
                        else
                        {
                            throw new QueryException("Right argument in with have wrong attribute name after dot: " + left);
                        }
                    }
                    else
                    {
                        throw new QueryException("Synonym on the right side of with is not declared: " + synonym);
                    }

                }
                else
                {
                    throw new QueryException("Right argument in with is invalid: " + right);
                }


                //checking left and right argument types
                string leftTypeTest = entityAttributeType[leftType];
                string rightTypeTest = entityAttributeType[rightType];
                if (!leftTypeTest.Equals(rightTypeTest))
                {
                    throw new QueryException("Arguments in with have different types: " + left + " (" + leftTypeTest + ") = " + right + " (" + rightTypeTest + ")");
                }
                conditionsList.Add(new With(left, leftType, right, rightType));
            }
            else
            {
                throw new QueryException("With wrong format: " + with);
            }
        }

        private void ParsePattern()
        {
            string pattern = "";
            do
            {
                pattern += wordsInQuery[currentIndex++];
            } while (currentIndex < wordsInQuery.Length && !selectClauses.Contains(wordsInQuery[currentIndex]));
            CheckPattern(pattern);
            parsedQuery += " " + pattern;
        }

        private void CheckPattern(string pattern)
        {
            if (Regex.IsMatch(pattern, @"^[^(,) ]+[(][^(,) ]+(,[^, ]+){0,2}[)]$"))
            {
                string synonym = pattern.Substring(0, pattern.IndexOf('('));
                if (!declarationsList.Keys.Contains(synonym))
                {
                    throw new QueryException("Synonym used in pattern is not declared: " + synonym + ". Check pattern: " + pattern);
                }
                string synonymType = declarationsList[synonym];

                string args = pattern.Substring(pattern.IndexOf('(') + 1);
                args = args.Remove(args.Length - 1);
                switch (synonymType)
                {
                    case "assign":
                        if (Regex.IsMatch(args, @"^[^(,) ]+[,][^(,) ]+$"))
                        {
                            string arg1 = args.Substring(0, args.IndexOf(','));
                            string arg1type = "";
                            if (arg1 == Entity._)
                            {
                                arg1type = Entity._;
                            }
                            else if (arg1.First() == '"' && arg1.Last() == '"')
                            {
                                string arg1string = arg1.Trim('"');
                                IsSynonym(arg1string);
                                arg1type = Entity._string;
                            }
                            else if (declarationsList.ContainsKey(arg1))
                            {
                                arg1type = declarationsList[arg1];
                            }
                            else
                            {
                                throw new QueryException("Pattern is invalid: " + pattern);
                            }
                            string arg2 = args.Substring(args.IndexOf(',') + 1);
                            string arg2type = "";
                            if (arg2 == Entity._)
                            {
                                arg2type = Entity._;
                            }
                            else if (arg2.First().Equals('_') && arg2.Last().Equals('_'))
                            {
                                string arg2string = arg2.Remove(0, 1);
                                arg2string = arg2string.Remove(arg2string.Length - 1);
                                if (arg2string.Contains("_"))
                                {
                                    throw new QueryException("Argument 2 in pattern <assign> cannot be: " + arg2string);
                                }
                                arg2type = Entity._string;
                            }
                            else if (arg2.First() == '"' && arg2.Last() == '"')
                            {
                                arg2type = Entity._string;
                            }
                            else
                            {
                                throw new QueryException("Argument 2 in pattern <assign> cannot be: " + arg2);

                            }
                            conditionsList.Add(new Pattern(synonym, synonymType, arg1, arg1type, arg2, arg2type));
                        }
                        else
                        {
                            throw new QueryException("Wrong pattern for assign: " + pattern);
                        }
                        break;
                    case "while":
                        if (Regex.IsMatch(args, @"^[^(,) ]+[,][_]$"))
                        {
                            string arg1 = args.Substring(0, args.IndexOf(','));
                            string arg1type = "";
                            if (arg1 == Entity._)
                            {
                                arg1type = Entity._;
                            }
                            else if (arg1.First() == '"' && arg1.Last() == '"')
                            {
                                string arg1string = arg1.Trim('"');
                                IsSynonym(arg1string);
                                arg1type = Entity._string;
                            }
                            else if (declarationsList.ContainsKey(arg1))
                            {
                                arg1type = declarationsList[arg1];
                            }
                            else
                            {
                                throw new QueryException("Pattern is invalid: " + pattern);
                            }
                            string arg2 = args.Substring(args.IndexOf(',') + 1);
                            string arg2type = "";
                            if (arg2 == Entity._)
                            {
                                arg2type = Entity._;
                            }
                            else
                            {
                                throw new QueryException("Argument 2 in pattern <while> must be: '_', but is: " + arg2);
                            }
                            conditionsList.Add(new Pattern(synonym, synonymType, arg1, arg1type, arg2, arg2type));
                        }
                        else
                        {
                            throw new QueryException("Wrong pattern for while: " + pattern);
                        }
                        break;
                    case "if":
                        if (Regex.IsMatch(args, @"^[^(,) ]+[,][_][,][_]$"))
                        {
                            string arg1 = args.Substring(0, args.IndexOf(','));
                            string arg1type = "";
                            if (arg1 == Entity._)
                            {
                                arg1type = Entity._;
                            }
                            else if (arg1.First() == '"' && arg1.Last() == '"')
                            {
                                string arg1string = arg1.Trim('"');
                                IsSynonym(arg1string);
                                arg1type = Entity._string;
                            }
                            else if (declarationsList.ContainsKey(arg1))
                            {
                                arg1type = declarationsList[arg1];
                            }
                            else
                            {
                                throw new QueryException("Pattern is invalid: " + pattern);
                            }
                            args = args.Substring(args.IndexOf(',') + 1);

                            string arg2 = args.Substring(0, args.IndexOf(','));
                            string arg2type = "";
                            if (arg2 == Entity._)
                            {
                                arg2type = Entity._;
                            }
                            else
                            {
                                throw new QueryException("Wrong pattern: " + pattern + ". 'if' as second paremeter must have: '_', but founded " + arg2);
                            }
                            string arg3 = args.Substring(args.IndexOf(',') + 1);
                            if (arg3 != Entity._)
                            {
                                throw new QueryException("Wrong pattern: " + pattern + ". 'if' as third paremeter must have: '_', but founded " + arg3);
                            }
                            conditionsList.Add(new Pattern(synonym, synonymType, arg1, arg1type, arg2, arg2type));
                        }
                        else
                        {
                            throw new QueryException("Wrong pattern for if: " + pattern);
                        }
                        break;
                }

            }
            else
            {
                throw new QueryException("With is in wrong format: " + pattern);
            }
        }
    }
}