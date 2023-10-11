using System;

namespace FramePFX.Utils.Expressions {
    public class BasicNumericExpression {
        private string input;
        private int index;

        public BasicNumericExpression(string input) {
            this.input = input;
            this.index = 0;
        }

        public double Parse() {
            return this.ParseExpression();
        }

        private double ParseExpression() {
            double left = this.ParseTerm();
            while (this.index < this.input.Length) {
                char op = this.input[this.index];
                if (op != '+' && op != '-')
                    break;

                this.index++;

                double right = this.ParseTerm();

                if (op == '+')
                    left += right;
                else
                    left -= right;
            }

            return left;
        }

        private double ParseTerm() {
            double left = this.ParseFactor();
            while (this.index < this.input.Length) {
                char op = this.input[this.index];
                if (op != '*' && op != '/')
                    break;

                this.index++;

                double right = this.ParseFactor();

                if (op == '*')
                    left *= right;
                else
                    left /= right;
            }

            return left;
        }

        private double ParseFactor() {
            if (this.index < this.input.Length) {
                char currentChar = this.input[this.index];
                if (char.IsDigit(currentChar) || currentChar == '.')
                    return this.ParseNumber();
                else if (currentChar == '(') {
                    this.index++;
                    double result = this.ParseExpression();
                    if (this.index < this.input.Length && this.input[this.index] == ')') {
                        this.index++;
                        return result;
                    }
                    else
                        throw new InvalidOperationException("Mismatched parentheses");
                }
            }

            throw new InvalidOperationException("Invalid expression");
        }

        private double ParseNumber() {
            int startIndex = this.index;
            while (this.index < this.input.Length && (char.IsDigit(this.input[this.index]) || this.input[this.index] == '.')) {
                this.index++;
            }

            string numberStr = this.input.Substring(startIndex, this.index - startIndex);
            if (double.TryParse(numberStr, out double result))
                return result;
            else
                throw new InvalidOperationException("Invalid number format");
        }
    }
}