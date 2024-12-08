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

namespace FramePFX.Avalonia.AvControls.Dragger.Expressions;

public class BasicNumericExpression
{
    private string input;
    private int index;

    public BasicNumericExpression(string input)
    {
        this.input = input;
        this.index = 0;
    }

    public double Parse()
    {
        return this.ParseExpression();
    }

    private double ParseExpression()
    {
        double left = this.ParseTerm();
        while (this.index < this.input.Length)
        {
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

    private double ParseTerm()
    {
        double left = this.ParseFactor();
        while (this.index < this.input.Length)
        {
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

    private double ParseFactor()
    {
        if (this.index < this.input.Length)
        {
            char currentChar = this.input[this.index];
            if (char.IsDigit(currentChar) || currentChar == '.')
                return this.ParseNumber();
            else if (currentChar == '(')
            {
                this.index++;
                double result = this.ParseExpression();
                if (this.index < this.input.Length && this.input[this.index] == ')')
                {
                    this.index++;
                    return result;
                }
                else
                    throw new InvalidOperationException("Mismatched parentheses");
            }
        }

        throw new InvalidOperationException("Invalid expression");
    }

    private double ParseNumber()
    {
        int startIndex = this.index;
        while (this.index < this.input.Length && (char.IsDigit(this.input[this.index]) || this.input[this.index] == '.'))
        {
            this.index++;
        }

        string numberStr = this.input.Substring(startIndex, this.index - startIndex);
        if (double.TryParse(numberStr, out double result))
            return result;
        else
            throw new InvalidOperationException("Invalid number format");
    }
}