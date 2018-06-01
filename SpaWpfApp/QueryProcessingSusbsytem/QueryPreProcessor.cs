using SpaWpfApp.Exceptions;
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
        public List<Relation> relationsList { get; set; }

        private Dictionary<string, string[]> entityAttributeValue;
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
            relationsList = new List<Relation>();
            relationsReference = new Dictionary<string, Action> {
                { Relation.Modifies, CheckModifies},
                { Relation.ModifiesX, CheckModifies},
                { Relation.Uses, CheckModifies},
                { Relation.UsesX, CheckModifies},
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

            entityAttributeValue = new Dictionary<string, string[]>
            {
                {"procedure", new string[] { "procName", "string" } },
                {"stmt", new string[] { "stmt#", "int" } },
                {"assign", new string[] { "stmt#", "int" } },
                {"call", new string[] { "procName", "string" } },
                {"variable", new string[] { "varName", "string" } },
                {"constatnt", new string[] { "value", "int" } },
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
            relationsList = new List<Relation>();
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


            Trace.WriteLine("Declarations:");
            foreach (var v in declarationsList)
            {
                Trace.WriteLine(v.Value + " " + v.Key);
            }
            Trace.WriteLine("Return List:");
            foreach (var v in returnList)
            {
                Trace.WriteLine(v.Value + " " + v.Key);
            }
            Trace.WriteLine("Relations List:");
            foreach (var r in relationsList)
            {
                Trace.WriteLine(r.ToString());
            }

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
            string[] separators = new string[] { " ", Environment.NewLine };
            return query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
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
                            if (designEntity != "assign" && designEntity != "stmt")
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

            //Trace.WriteLine("Relationship: " + relationship);
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
                    IsSynonym(arg1.Trim('"'));
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
                    case Relation.ModifiesX:
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
                    case Relation.UsesX:
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

                relationsList.Add(new Relation(relRef, arg1, arg1type, arg2, arg2type));
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
            Trace.WriteLine("With: " + with);
            CheckWith(with);

            parsedQuery += " " + with;
        }

        private void CheckWith(string with)
        {
            if (Regex.IsMatch(with, @"^[^= ]+[=][^= ]+$"))
            {
                string leftRef = with.Substring(0, with.IndexOf('='));
                CheckTuple(leftRef);

                with = with.Substring(with.IndexOf('=') + 1);
                string rightRef = with.Substring(0);
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
            Trace.WriteLine("Pattern: " + pattern);

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
                    throw new QueryException("Synonym used in pattern is not declared: " + synonym + "\nCheck pattern: " + pattern);
                }
                string assignWhileIf = declarationsList[synonym];
                string args = pattern.Substring(pattern.IndexOf('(') + 1);
                switch (assignWhileIf)
                {
                    case "assign":
                        if (Regex.IsMatch(args, @"^[^(,) ]+[,][^(,) ]+[)]$"))
                            CheckAssignPattern(args);
                        else
                            throw new QueryException("Wrong pattern for assign: " + pattern);
                        break;
                    case "while":
                        if (Regex.IsMatch(args, @"^[^(,) ]+[,][_][)]$"))
                            CheckWhilePattern(args);
                        else
                            throw new QueryException("Wrong pattern for while: " + pattern);
                        break;
                    case "if":
                        if (Regex.IsMatch(args, @"^[^(,) ]+[,][_][,][_][)]$"))
                            CheckIfPattern(args);
                        else
                            throw new QueryException("Wrong pattern for if: " + pattern);
                        break;
                }

            }
            else
            {
                throw new QueryException("With wrong format: " + pattern);
            }
        }

        private void CheckAssignPattern(string pattern)
        {
            string firstArg = pattern.Substring(0, pattern.IndexOf(','));
            pattern = pattern.Substring(pattern.IndexOf(',') + 1);
            string secArg = pattern.Substring(0, pattern.IndexOf(')'));
            //check arguments

        }
        private void CheckWhilePattern(string pattern)
        {
            string firstArg = pattern.Substring(0, pattern.IndexOf(','));
            pattern = pattern.Substring(pattern.IndexOf(',') + 1);
            string secArg = pattern.Substring(0, pattern.IndexOf(')'));
            //check arguments

        }
        private void CheckIfPattern(string pattern)
        {
            string firstArg = pattern.Substring(0, pattern.IndexOf(','));
            pattern = pattern.Substring(pattern.IndexOf(',') + 1);
            string secArg = pattern.Substring(0, pattern.IndexOf(')'));
            //check arguments

        }
    }
}
