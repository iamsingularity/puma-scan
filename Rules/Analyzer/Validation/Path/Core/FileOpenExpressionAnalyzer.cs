/* 
 * Copyright(c) 2016 - 2018 Puma Security, LLC (https://www.pumascan.com)
 * 
 * Project Leader: Eric Johnson (eric.johnson@pumascan.com)
 * Lead Developer: Eric Mead (eric.mead@pumascan.com)
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 */

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Puma.Security.Rules.Analyzer.Core;
using Puma.Security.Rules.Common.Extensions;

namespace Puma.Security.Rules.Analyzer.Validation.Path.Core
{
    internal class FileOpenExpressionAnalyzer : IFileOpenExpressionAnalyzer
    {
        public bool IsVulnerable(SemanticModel model, InvocationExpressionSyntax syntax)
        {
            if (!ContainsFileOpenCommands(syntax)) return false;

            var symbol = model.GetSymbolInfo(syntax).Symbol as IMethodSymbol;

            if (!IsFileOpenCommand(symbol)) return false;

            if (syntax.ArgumentList.Arguments.Count > 0)
            {
                var argSyntax = syntax.ArgumentList.Arguments[0].Expression;
                var expressionAnalyzer = SyntaxNodeAnalyzerFactory.Create(argSyntax);
                if (expressionAnalyzer.CanIgnore(model, argSyntax))
                    return false;
                if (expressionAnalyzer.CanSuppress(model, argSyntax))
                    return false;
            }

            return true;
        }

        private static bool ContainsFileOpenCommands(InvocationExpressionSyntax syntax)
        {
            return syntax.ToString().Contains("File.Open") ||
                   syntax.ToString().Contains("File.OpenRead") ||
                   syntax.ToString().Contains("File.OpenWrite") ||
                   syntax.ToString().Contains("File.OpenText");
        }

        private bool IsFileOpenCommand(IMethodSymbol symbol)
        {
            return symbol.IsMethod("System.IO.File", "Open") ||
                   symbol.IsMethod("System.IO.File", "OpenRead") ||
                   symbol.IsMethod("System.IO.File", "OpenWrite") ||
                   symbol.IsMethod("System.IO.File", "OpenText");
        }
    }
}