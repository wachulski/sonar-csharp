﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class ParameterNamesShouldNotDuplicateMethodNames : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3872";
        private const string MessageFormat = "Rename the parameter '{0}' so that it does not duplicate the method name.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var method = (MethodDeclarationSyntax)c.Node;

                var methodName = method.Identifier.ToString();

                foreach (var parameter in method.ParameterList.Parameters.Select(p => p.Identifier))
                {
                    var parameterName = parameter.ToString();
                    if (string.Equals(parameterName, methodName, StringComparison.OrdinalIgnoreCase))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, parameter.GetLocation(),
                            new[] { method.Identifier.GetLocation() }, parameterName));
                    }
                }
            },
            SyntaxKind.MethodDeclaration);
        }
    }
}
