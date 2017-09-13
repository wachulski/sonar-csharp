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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class PublicConstantFieldBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2339";
        protected const string MessageFormat = "Change this constant to a {0} property.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class PublicConstantFieldBase<TLanguageKindEnum, TFieldDeclarationSyntax, TFieldName>
        : PublicConstantFieldBase
        where TLanguageKindEnum : struct
        where TFieldDeclarationSyntax : SyntaxNode
        where TFieldName : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var field = (TFieldDeclarationSyntax)c.Node;
                    var variables = GetVariables(field).ToList();

                    if (!variables.Any())
                    {
                        return;
                    }

                    var anyVariable = variables.First();
                    var symbol = c.SemanticModel.GetDeclaredSymbol(anyVariable) as IFieldSymbol;
                    if (symbol == null ||
                        !symbol.IsConst ||
                        symbol.GetEffectiveAccessibility() != Accessibility.Public)
                    {
                        return;
                    }

                    foreach (var variable in variables)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, GetReportLocation(variable),
                            MessageArgument));
                    }
                },
                FieldDeclarationKind);
        }

        protected abstract IEnumerable<TFieldName> GetVariables(TFieldDeclarationSyntax node);
        public abstract TLanguageKindEnum FieldDeclarationKind { get; }
        public abstract string MessageArgument { get; }

        protected abstract Location GetReportLocation(TFieldName node);

        protected abstract DiagnosticDescriptor Rule { get; }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }
}
