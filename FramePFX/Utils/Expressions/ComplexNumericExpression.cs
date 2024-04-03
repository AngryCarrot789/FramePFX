//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Utils.Expressions
{
    /// <summary>
    /// A recursive descent parser for numeric expressions, with support for variables and functions that take a single parameter
    /// </summary>
    public class ComplexNumericExpression
    {
        /// <summary>
        /// The default expression parser
        /// </summary>
        public static readonly ComplexNumericExpression DefaultParser;

        private readonly Stack<ExpressionState> states;
        private ExpressionState state;
        private readonly bool isReadOnly;

        public ComplexNumericExpression(bool isReadOnly = false)
        {
            this.states = new Stack<ExpressionState>();
            ExpressionState defState = new ExpressionState(this, -1);

            // standard functions
            defState.SetFunction("sin", Math.Sin);
            defState.SetFunction("cos", Math.Cos);
            defState.SetFunction("sqrt", Math.Sqrt);

            defState.SetFunction("sum", list => list.Sum());
            defState.SetFunction("min", list => list.Min());
            defState.SetFunction("max", list => list.Max());

            Func<IReadOnlyList<double>, double> avg = list => list.Sum() / list.Count;
            defState.SetFunction("mean", avg);
            defState.SetFunction("avg", avg);

            defState.SetFunction("range", list =>
            {
                if (list.Count == 1)
                    return list[0];
                double min = list[0], max = min;
                for (int i = 1; i < list.Count; i++)
                {
                    double val = list[i];
                    if (val < min)
                        min = list[i];
                    if (val > max)
                        max = list[i];
                }

                return max - min;
            });

            defState.SetFunction("mode", (list) =>
            {
                switch (list.Count)
                {
                    case 1:
                    case 2:
                        return list[0];
                    default: return list.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
                }
            });

            this.state = defState;
            this.isReadOnly = isReadOnly;
        }

        static ComplexNumericExpression()
        {
            DefaultParser = new ComplexNumericExpression(true);
        }

        public double Parse(string input)
        {
            int index = 0;
            return this.ParseExpression(input, ref index);
        }

        private double ParseExpression(string input, ref int index)
        {
            double left = this.ParseTerm(input, ref index);
            while (index < input.Length)
            {
                char op = input[index];
                if (op != '+' && op != '-')
                {
                    break;
                }

                index++;
                double right = this.ParseTerm(input, ref index);
                if (op == '+')
                {
                    left += right;
                }
                else
                {
                    left -= right;
                }
            }

            return left;
        }

        private double ParseTerm(string input, ref int index)
        {
            double left = this.ParseNextValue(input, ref index);
            while (index < input.Length)
            {
                char op = input[index];
                if (op != '*' && op != '/')
                {
                    break;
                }

                index++;
                double right = this.ParseNextValue(input, ref index);
                if (op == '*')
                {
                    left *= right;
                }
                else
                {
                    left /= right;
                }
            }

            return left;
        }

        private double ParseNextValue(string input, ref int index)
        {
            if (index >= input.Length)
            {
                throw new Exception("End of expression string before it could be fully parsed");
            }

            char ch = input[index];
            if (char.IsDigit(ch) || ch == '.')
            {
                return ParseNumber(input, ref index);
            }
            else if (ch == '(')
            {
                index++;
                double result = this.ParseExpression(input, ref index);
                if (index < input.Length && input[index] == ')')
                {
                    index++;
                    return result;
                }
                else
                {
                    throw new Exception("Mismatched parentheses");
                }
            }
            else if (char.IsLetter(ch))
            {
                string symbolName = ParseSymbolName(input, ref index);
                if (index >= input.Length)
                {
                    throw new Exception("End of expression string before it could be fully parsed");
                }
                else if (input[index] == '(')
                {
                    if (!this.state.ContainsFunction(symbolName))
                    {
                        throw new Exception($"Unknown function: {symbolName}");
                    }

                    index++;
                    double argument = this.ParseExpression(input, ref index);
                    if (index >= input.Length)
                    {
                        throw new Exception("End of expression string before it could be fully parsed");
                    }

                    if (input[index] == ')')
                    {
                        index++;
                        return this.state.Invoke(symbolName, argument);
                    }
                    else if (input[index] == ',')
                    {
                        if (!this.state.ContainsMultiParamFunction(symbolName))
                        {
                            throw new Exception($"Unknown multi-parameter function: {symbolName}");
                        }

                        index++;
                        List<double> parameters = new List<double>() { argument };
                        while (true)
                        {
                            argument = this.ParseExpression(input, ref index);
                            parameters.Add(argument);
                            if (index >= input.Length)
                            {
                                throw new Exception("End of expression string before it could be fully parsed");
                            }
                            else if (input[index] == ')')
                            {
                                index++;
                                break;
                            }
                            else if (input[index] != ',')
                            {
                                throw new Exception("Missing a comma when invoking a function");
                            }

                            index++;
                        }

                        return this.state.Invoke(symbolName, parameters);
                    }
                    else
                    {
                        throw new Exception("Mismatched parentheses");
                    }
                }
                else if (this.state.TryGetVariable(symbolName, out double value))
                {
                    return value;
                }
                else
                {
                    throw new Exception($"Undefined or invalid field variable: {symbolName}");
                }
            }
            else
            {
                throw new Exception("Invalid expression");
            }
        }

        private static string ParseSymbolName(string input, ref int index)
        {
            int startIndex = index;
            while (index < input.Length && IsLexerChar(input[index]))
            {
                index++;
            }

            return input.Substring(startIndex, index - startIndex);
        }

        private static double ParseNumber(string input, ref int index)
        {
            int startIndex = index;
            while (index < input.Length && (char.IsDigit(input[index]) || input[index] == '.'))
            {
                index++;
            }

            string numberStr = input.Substring(startIndex, index - startIndex);
            if (double.TryParse(numberStr, out double result))
                return result;
            else
                throw new Exception("Invalid number format");
        }

        private static bool IsLexerChar(char ch)
        {
            return ch == '_' || char.IsLetter(ch) || char.IsNumber(ch);
        }

        public ExpressionState PushState()
        {
            ExpressionState newState = new ExpressionState(this, this.states.Count);
            this.states.Push(this.state);
            this.state = newState;
            return newState;
        }

        public class ExpressionState : IDisposable
        {
            private Dictionary<string, Func<double, double>> spfunctions;
            private Dictionary<string, Func<IReadOnlyList<double>, double>> mpfunctions;
            private Dictionary<string, Func<double>> variables;
            private readonly ComplexNumericExpression expression;
            private readonly int index;
            private readonly ExpressionState parent;

            public bool IsReadOnly => this.index == -1 && this.expression.isReadOnly;

            public ComplexNumericExpression Expression => this.expression;

            internal ExpressionState(ComplexNumericExpression expression, int index)
            {
                this.expression = expression;
                this.index = index;
                this.parent = expression.state;
            }

            public bool ContainsFunction(string name)
            {
                if (this.spfunctions != null && this.spfunctions.ContainsKey(name) || this.mpfunctions != null && this.mpfunctions.ContainsKey(name))
                    return true;
                return this.parent != null && this.parent.ContainsFunction(name);
            }

            public bool ContainsMultiParamFunction(string name)
            {
                if (this.mpfunctions != null && this.mpfunctions.ContainsKey(name))
                    return true;
                return this.parent != null && this.parent.ContainsMultiParamFunction(name);
            }

            public bool ContainsVariable(string name)
            {
                return this.variables != null && this.variables.ContainsKey(name) || this.parent.ContainsVariable(name);
            }

            public double Invoke(string name, double parameter)
            {
                if (this.spfunctions != null && this.spfunctions.TryGetValue(name, out Func<double, double> a))
                {
                    return a(parameter);
                }
                else if (this.mpfunctions != null && this.mpfunctions.TryGetValue(name, out Func<IReadOnlyList<double>, double> b))
                {
                    return b(new SingletonList<double>(parameter));
                }
                else if (this.parent != null)
                {
                    return this.parent.Invoke(name, parameter);
                }
                else
                {
                    throw new Exception("No such method: " + name);
                }
            }

            public double Invoke(string name, IReadOnlyList<double> parameters)
            {
                if (this.mpfunctions != null && this.mpfunctions.TryGetValue(name, out Func<IReadOnlyList<double>, double> a))
                {
                    return a(parameters);
                }
                else if (parameters.Count == 1)
                {
                    if (this.spfunctions != null && this.spfunctions.TryGetValue(name, out Func<double, double> b))
                    {
                        return b(parameters[0]);
                    }
                    else if (this.parent != null)
                    {
                        return this.parent.Invoke(name, parameters);
                    }
                    else
                    {
                        throw new Exception("No such single or multi parameter method: " + name);
                    }
                }
                else if (this.parent != null)
                {
                    return this.parent.Invoke(name, parameters);
                }
                else
                {
                    throw new Exception("No such method: " + name);
                }
            }

            public bool TryGetVariable(string name, out double value)
            {
                if (this.variables != null && this.variables.TryGetValue(name, out Func<double> provider))
                {
                    value = provider();
                    return true;
                }
                else if (this.parent != null)
                {
                    return this.parent.TryGetVariable(name, out value);
                }
                else
                {
                    value = default;
                    return false;
                }
            }

            /// <summary>
            /// Sets a variable provider with the given name, which an expression can access
            /// </summary>
            /// <param name="name">Variable name</param>
            /// <param name="provider">A func which gets the variable's name</param>
            /// <exception cref="InvalidOperationException">The current instance is read only</exception>
            /// <exception cref="ArgumentNullException">The variable provider is null</exception>
            /// <exception cref="ArgumentException">The name is null, empty or whitespaces</exception>
            public void SetVariable(string name, Func<double> provider)
            {
                this.ValidateNotReadOnly();
                if (provider == null)
                    throw new ArgumentNullException(nameof(provider));
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be null, empty or whitespaces");
                (this.variables ?? (this.variables = new Dictionary<string, Func<double>>()))[name] = provider;
            }

            /// <summary>
            /// Sets a variable with the given name, which an expression can access
            /// </summary>
            /// <param name="name">Variable name</param>
            /// <param name="provider">A func which gets the variable's name</param>
            /// <exception cref="InvalidOperationException">The current instance is read only</exception>
            /// <exception cref="ArgumentException">The name is null, empty or whitespaces</exception>
            public void SetVariable(string name, double value)
            {
                this.ValidateNotReadOnly();
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be null, empty or whitespaces");
                (this.variables ?? (this.variables = new Dictionary<string, Func<double>>()))[name] = () => value;
            }

            /// <summary>
            /// Sets the given single-parameter function with the given name, which an expression can call
            /// </summary>
            /// <param name="name">Function name</param>
            /// <param name="provider">The function implementation</param>
            /// <exception cref="InvalidOperationException">The current instance is read only</exception>
            /// <exception cref="ArgumentNullException">The function (provider) is null</exception>
            /// <exception cref="ArgumentException">The name is null, empty or whitespaces</exception>
            public void SetFunction(string name, Func<double, double> provider)
            {
                this.ValidateNotReadOnly();
                if (provider == null)
                    throw new ArgumentNullException(nameof(provider));
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be null, empty or whitespaces");
                (this.spfunctions ?? (this.spfunctions = new Dictionary<string, Func<double, double>>()))[name] = provider;
            }

            /// <summary>
            /// Sets the given multi-parameter function with the given name, which an expression can call
            /// </summary>
            /// <param name="name">Function name</param>
            /// <param name="provider">The function implementation</param>
            /// <exception cref="InvalidOperationException">The current instance is read only</exception>
            /// <exception cref="ArgumentNullException">The function (provider) is null</exception>
            /// <exception cref="ArgumentException">The name is null, empty or whitespaces</exception>
            /// <exception cref="Exception">The function name is reserved</exception>
            public void SetFunction(string name, Func<IReadOnlyList<double>, double> provider)
            {
                this.ValidateNotReadOnly();
                if (provider == null)
                    throw new ArgumentNullException(nameof(provider));
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be null, empty or whitespaces");
                (this.mpfunctions ?? (this.mpfunctions = new Dictionary<string, Func<IReadOnlyList<double>, double>>()))[name] = provider;
            }

            private void ValidateNotReadOnly()
            {
                if (this.IsReadOnly)
                {
                    throw new InvalidOperationException("The default state of the expression parser is read-only");
                }
            }

            public void Dispose()
            {
                if (this.expression.states.Count - 1 != this.index)
                    throw new Exception("States popped in an invalid order");
                if (this.expression.states.Count < 1)
                    throw new Exception("Cannot pop the default state");
                this.expression.state = this.expression.states.Pop();
            }
        }
    }
}