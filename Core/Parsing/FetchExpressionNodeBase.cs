// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Utilities;

namespace Remotion.Linq.EagerFetching.Parsing
{
  /// <summary>
  /// Provides common functionality used by all expression nodes representing fetch operations.
  /// </summary>
  public abstract class FetchExpressionNodeBase : ResultOperatorExpressionNodeBase
  {
    protected FetchExpressionNodeBase (MethodCallExpressionParseInfo parseInfo, LambdaExpression relatedObjectSelector)
        : base (parseInfo, null, null)
    {
      ArgumentUtility.CheckNotNull ("relatedObjectSelector", relatedObjectSelector);

      var memberExpression = relatedObjectSelector.Body as MemberExpression;
      if (memberExpression == null)
      {
        var message = string.Format (
            "A fetch request must be a simple member access expression; '{0}' is a {1} instead.",
            relatedObjectSelector.Body,
            relatedObjectSelector.Body.GetType ().Name);
        throw new ArgumentException (message, "relatedObjectSelector");
      }

      var owner = StripConverts (memberExpression.Expression);
      if (owner.NodeType != ExpressionType.Parameter)
      {
        var message = string.Format (
            "A fetch request must be a simple member access expression of the kind o => o.Related; '{0}' is too complex.",
            relatedObjectSelector.Body);
        throw new ArgumentException (message, "relatedObjectSelector");
      }

      RelationMember = memberExpression.Member;
    }

    private Expression StripConverts (Expression expression)
    {
      while (expression.NodeType == ExpressionType.Convert)
        expression = ((UnaryExpression) expression).Operand;
      return expression;
    }

    public MemberInfo RelationMember { get; private set; }

    public override Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      ArgumentUtility.CheckNotNull ("inputParameter", inputParameter);
      ArgumentUtility.CheckNotNull ("expressionToBeResolved", expressionToBeResolved);

      return Source.Resolve (inputParameter, expressionToBeResolved, clauseGenerationContext);
    }
  }
}
