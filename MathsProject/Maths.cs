using MathNet.Numerics;
using StuffProject.Listonary;
using System.Data;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace MathsProject
{

    /*
A calculator in C#
Copyright (C) 2024 Jordan Ferrazza

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
    public class Maths
    {

        public static readonly Dictionary<string, Func<double, double, double>> NumOps = new Dictionary<string, Func<double, double, double>>
        {
            ["/"] = (a, b) => a / b,
            ["^"] = (a, b) => Math.Pow(a, b),
            ["*"] = (a, b) => a * b,
            ["%"] = (a, b) => a % b,
            ["+"] = (a, b) => a + b,
            ["-"] = (a, b) => a - b,
            ["@"] = (a, b) => a.Round((int)b)
        };
        public static readonly Dictionary<string, Func<double, double, bool>> NumBoolOps = new Dictionary<string, Func<double, double, bool>>
        {
            [">"] = (a, b) => a > b,
            ["<"] = (a, b) => a < b,
            [">="] = (a, b) => a >= b,
            ["<="] = (a, b) => a <= b,
            ["!="] = (a, b) => a != b,
            ["="] = (a, b) => a == b,
        };
        public static readonly Dictionary<string, Func<bool, bool, bool>> BoolOps = new Dictionary<string, Func<bool, bool, bool>>
        {
            ["^"] = (a, b) => a ^ b,
            ["&"] = (a, b) => a && b,
            ["|"] = (a, b) => a || b,
            ["!="] = (a, b) => a != b,
            ["="] = (a, b) => a == b,
        };
        public static readonly Dictionary<string, Func<string, string, bool>> MiscBoolOps = new Dictionary<string, Func<string, string, bool>>
        {
            ["!="] = (a, b) => a != b,
            ["="] = (a, b) => a == b,

        };
        public static readonly Dictionary<string, Func<double, double>> NumMods = new Dictionary<string, Func<double, double>>
        {
            ["!"] = (x) => SpecialFunctions.Gamma(x + 1).Round(5)
        };
        public static readonly Dictionary<string, Func<bool, bool>> BoolMods = new Dictionary<string, Func<bool, bool>>
        {
            ["!"] = (x) => !x
        };
        public Dictionary<string, Func<string, bool>> FuncMods;
        public Dictionary<string, Func<string, string, bool>> FuncOps;
        public Dictionary<string, Func<string, string, string>> FuncOps2 = new Dictionary<string, Func<string, string, string>> { };
        public static readonly string[] Reserved = new string[] { "exists", "delete", "true", "false", "let", "set" };
        public static readonly string VariableSchema = "^[a-zA-Z][a-zA-Z0-9_]*";
        public static readonly string DoubleSchema = "-?(\\d+(\\.\\d*)?(E(\\+|-)\\d+)?)";
        public static readonly string BoolSchema = "(true|false)";
        public static readonly string BracketSchema = "\\(([^()]|[^()]*\\([^()]*\\)[^()]*)+\\)(?!=\\()";
        public static readonly string FuncSchema = VariableSchema + BracketSchema;
        public static readonly string ArgsSchema = "(([^(,)]|[^()]*\\([^()]*\\)[^(,)]*)+)(?=,|\\)$)";
        public static readonly string BodySchema = "\\{([^()]|[^()]*\\([^()]*\\)[^()]*)+\\}(?!=\\{)";
        public static readonly string LinesSchema = "(?<=\\{)(([^(,)]|[^()]*\\([^()]*\\)[^(,)]*)+)(?=;|\\}$)";

        public Dictionary<string, string> Variables = new Dictionary<string, string>();
        public Listonary<string, MathsFunction> Functions = new Listonary<string, MathsFunction>(x => x.Name);

        public Maths()
        {
            //prepare non-static operators
            FuncMods = new Dictionary<string, Func<string, bool>>
            {
                //return if the variable exists
                ["exists"] = (x) => Variables.ContainsKey(x),
                //delete a variable and return if that did anything
                ["delete"] = (x) => Variables.Remove(x),
            };
            FuncOps = new Dictionary<string, Func<string, string, bool>>
            {
                [">>"] = (a, b) =>
                {
                    //check if name is illegal or taken
                    if (Variables.ContainsKey(a)) return false;
                    if (Reserved.Contains(a)) return false;
                    if (!Regex.IsMatch(a, "[a-zA-Z]\\w*")) return false;

                    //add the variable
                    Variables.Add(a, Run(b));
                    return true;
                },
                [">>>"] = (a, b) =>
                {
                    //check if name is illegal
                    if (Reserved.Contains(a)) return false;
                    if (!Regex.IsMatch(a, VariableSchema)) return false;

                    //add the variable
                    Variables[a] = Run(b);
                    return true;
                }
            };

            //do the above backwards
            FuncOps.Add("<<", (a, b) => FuncOps[">>"](b, a));
            FuncOps.Add("<<<", (a, b) => FuncOps[">>>"](b, a));

            FuncOps2.Add("=>", (a, b) =>
            {
                //add the variable; if it didn't add, die
                if (!FuncOps[">>>"](a, b)) throw new ArgumentException($"{a} threw False while returning (invalid name, pre-existence optional).");

                //return it
                return Variables[a];

            });

            //prepare variables
            Variables["ans"] = "0";
        }


        public string Run(string com)
        {
            //prepare the result
            string o = com;

            //if the input is blank or has no left operand, put ANS
            if (o.Length == 0 || Regex.IsMatch(o[0].ToString(), "[^a-zA-Z0-9(!-]"))
                o = "ans " + o;

            //trim the input
            o = o.Trim();

            //machine the values

            //punctuation, functions and variables
            o = ProcessBrackets(o);
            o = Process(FuncOps, "\\w+", o);
            o = Process(FuncOps2, "\\w+", o);
            o = Process(FuncMods, "\\w+", o, true);
            o = ProcessVariables(o);

            //one-operand operators
            o = Process(NumMods, DoubleSchema, o);
            o = Process(BoolMods, BoolSchema, o);

            //operators
            o = Process(NumOps, DoubleSchema, o);
            o = Process(NumBoolOps, DoubleSchema, o);
            o = Process(BoolOps, BoolSchema, o);

            //misc
            o = Process(MiscBoolOps, "\\w+", o);
            o = ProcessSwitch(o);

            //if the sum still isn't a value, die
            if (!Regex.IsMatch(o, $"^({DoubleSchema}|{BoolSchema})$", RegexOptions.IgnoreCase))
                throw new InvalidOperationException($"Syntax Error '{o}'");

            //save the answer
            Variables["ans"] = o;

            //return the value
            return o;
        }

        private string ProcessSwitch(string o)
        {
            // get the switches
            List<string> s = o.Split("?").Select(x => x.Trim()).ToList();
            // if the switch is entire
            if (s.Count == 3)
            {
                //calculate and return the switch
                return bool.Parse(s[0]) ? s[1] : s[2];
            }

            //else throw plain text
            return o;
        }

        public string ProcessVariables(string input)
        {
            //if variables exist in the sum and are not illegal, return them
            return Regex.Replace(input, "\\w+", x =>
            Reserved.Contains(x.Value.ToLower()) | !Regex.IsMatch(x.Value, VariableSchema) ? x.Value : Variables[x.Value]
            );
        }


        public string ProcessFunctions(string input)
        {
            string o = input;
            //for each function delcaration in string, get the name, signature and body and add the function to memory
            o = Regex.Replace(o, FuncSchema + "{" + LinesSchema + "}", x =>
            {
                string name = x.Value.Split('(')[0];
                string argsString = Regex.Match(x.Value, BracketSchema).Value;

                //if -, delete this function instead
                if (argsString == "-")
                {
                    return Functions.Remove(Functions[name]).ToString();
                }

                string[] args = argsString[1..^1].Split(',').ToArray();

                //check if arguments are valid
                if (!args.All(x => Regex.IsMatch(x, "^"+VariableSchema+"$")))
                {
                    throw new ArgumentException("Argument names not valid");
                }
                string bodyString = Regex.Match(x.Value, BodySchema).Value;
                string[] body = Regex.Matches(x.Value, LinesSchema).Select(x => x.Value).ToArray();

                Functions.Add(new MathsFunction(name, args, body));

                return "True";
            });
            //for each function reference in string, get the name, signature and arguments of it then run the function
            o = Regex.Replace(o, FuncSchema, x =>
            {
                string name = x.Value.Split('(')[0];
                string argsString = Regex.Match(x.Value, BracketSchema).Value;
                string[] args = Regex.Matches(argsString, ArgsSchema).Select(x => Run(x.Value)).ToArray();
                return Functions[name].Run(this, args);
            });
            return o;

        }
        public string ProcessBrackets(string input)
        {


            //replace all brackets (any parentheses containing either no brackets or enclosed parentheses that is not followed by brackets) with the end number and return the resulting sum
            return Regex.Replace(input, BracketSchema, x =>
             Run(x.Value[1..^1])
             );
        }
        public string Process<T1, T2>(Dictionary<string, Func<T1, T2>> ops, string operand, string input, bool space = false)
        {
            string o = input; //prepare the output
            string s = space ? " " : ""; //check if the class above is whitespace sensitive, then get the respective token
            //generate a function that generates the regex statement
            Func<string, string> expression = (x) => $"({Regex.Escape(x)}{s}{operand})|({operand}{s}{Regex.Escape(x)})";
            //for each operation in the dictionary
            foreach (KeyValuePair<string, Func<T1, T2>> item in ops)
            {
                //generate a regex statement out of it
                Regex op = new Regex(expression(item.Key), RegexOptions.IgnoreCase);
                //while the operation is in the sum
                while (op.IsMatch(o))
                {
                    //using the operation function, type and word syntax, replace every first two instances of the operation with its result
                    o = op.Replace(o, x =>
                    {
                        List<T1> args = Regex.Matches(x.Value, operand, RegexOptions.IgnoreCase).Where(x => x.Value.Trim() != item.Key).Select(x => (T1)Convert.ChangeType(x.Value, typeof(T1))).ToList();

                        return item.Value(args[0]).ToString();
                    }, 1);
                }
            }

            //return the resulting sum
            return o;
        }
        public string Process<T1, T2>(Dictionary<string, Func<T1, T1, T2>> ops, string operand, string input, bool space = false)
        {

            string o = input; //prepare the output
            string s = space ? " " : " ?"; //check if the class above is whitespace sensitive, then get the respective token
            //generate a function that generates the regex statement
            Func<string, string> expression = (x) => $"{operand}{s}{Regex.Escape(x)}{s}{operand}";
            //while the operation is in the sum
            foreach (KeyValuePair<string, Func<T1, T1, T2>> item in ops)
            {
                //for each operation in the dictionary
                Regex op = new Regex(expression(item.Key), RegexOptions.IgnoreCase);
                while (op.IsMatch(o))
                {
                    //using the operation function, type and word syntax, replace every first instance of the operation with its result
                    o = op.Replace(o, x =>
                    {
                        List<T1> args = Regex.Matches(x.Value, operand, RegexOptions.IgnoreCase).Select(x => (T1)Convert.ChangeType(x.Value, typeof(T1))).ToList();

                        return item.Value(args[0], args[1]).ToString();
                    }, 1);
                }
            }

            //return the resulting sum
            return o;
        }

    }
}