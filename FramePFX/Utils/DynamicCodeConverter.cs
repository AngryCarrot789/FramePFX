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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;
using Microsoft.CSharp;

namespace FramePFX.Utils
{
    /// <summary>
    /// A converter that uses a C# code generator to evaluate the converter output
    /// </summary>
    public class DynamicCodeConverter : MarkupExtension, IValueConverter
    {
        private static readonly HashSet<string> UsedClassNames = new HashSet<string>();

        private string lastScript;
        private MethodInfo lastCompiledMethod;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string script))
            {
                throw new Exception("Parameter must be a C# script");
            }

            if (this.lastScript == script && this.lastCompiledMethod != null)
            {
                return this.lastCompiledMethod.Invoke(null, new[] { value });
            }
            else
            {
                this.lastCompiledMethod = null;
                this.lastScript = null;
            }

            string klass = RandomUtils.RandomPrefixedLettersWhere("Script_", 16, x => !UsedClassNames.Contains(x));
            UsedClassNames.Add(klass);
            string[] code =
            {
                "using System;" +
                "namespace FramePFX.DynamicScript" +
                "{" +
                "   public class " + klass +
                "   {" +
                "       static public object Execute(object x)" +
                "       {" +
                "           return " + script + ";" +
                "       }" +
                "   }" +
                "}"
            };

            CompilerParameters CompilerParams = new CompilerParameters
            {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false,
                GenerateExecutable = false,
                CompilerOptions = "/optimize"
            };

            CompilerParams.ReferencedAssemblies.AddRange(new[] { "System.dll" });

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults compile = provider.CompileAssemblyFromSource(CompilerParams, code);

            if (compile.Errors.HasErrors)
            {
                string text = "Compile error: ";
                foreach (CompilerError ce in compile.Errors)
                    text += "\n" + ce;

                throw new Exception(text);
            }

            Module module = compile.CompiledAssembly.GetModules()[0];
            if (module == null)
                throw new Exception("Error during compilation; could not find 0th module");

            Type mt = module.GetType("FramePFX.DynamicScript." + klass);
            if (mt == null)
                throw new Exception("Error during compilation; could not find compiled class");

            MethodInfo methInfo = mt.GetMethod("Execute");
            if (methInfo == null)
                throw new Exception("Error during compilation; could not find execute function in compiled class");

            this.lastCompiledMethod = methInfo;
            this.lastScript = script;
            return methInfo.Invoke(null, new[] { value });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}