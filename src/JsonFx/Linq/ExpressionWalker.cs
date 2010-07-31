#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

using JsonFx.Common;
using JsonFx.Serialization;

namespace JsonFx.Linq
{
	internal class ExpressionWalker :
		ExpressionVisitor,
		IObjectWalker<CommonTokenType>,
		IQueryTextProvider
	{
		#region Fields

		private readonly ITextFormatter<CommonTokenType> Formatter;
		private IList<Token<CommonTokenType>> tokens;

		#endregion Fields

		#region Init

		/// <summary>
		/// 
		/// </summary>
		/// <param name="formatter"></param>
		public ExpressionWalker(ITextFormatter<CommonTokenType> formatter)
		{
			if (formatter == null)
			{
				throw new ArgumentNullException("formatter");
			}

			this.Formatter = formatter;
		}

		#endregion Init

		#region Visit Methods

		protected override Expression Visit(Expression expression)
		{
			if (expression == null)
			{
				this.tokens.Add(CommonGrammar.TokenNull);
			}

			return base.Visit(expression);
		}

		protected override Expression VisitUnknown(Expression expression)
		{
			this.tokens.Add(CommonGrammar.TokenPrimitive(expression.NodeType));

			return expression;
		}

		protected override ElementInit VisitElementInitializer(ElementInit init)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty("ElementInit"));
			this.tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);

			try
			{
				this.VisitExpressionList(init.Arguments);

				// no change
				return init;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenArrayEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitUnary(UnaryExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Type"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(this.GetTypeName(expression.Type)));

				this.tokens.Add(CommonGrammar.TokenProperty("Method"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(expression.Method != null ? expression.Method.Name : null));

				this.tokens.Add(CommonGrammar.TokenProperty("Operand"));
				this.Visit(expression.Operand);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitBinary(BinaryExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Left"));
				this.Visit(expression.Left);

				this.tokens.Add(CommonGrammar.TokenProperty("Right"));
				this.Visit(expression.Right);

				this.tokens.Add(CommonGrammar.TokenProperty("Conversion"));
				this.Visit(expression.Conversion);

				this.tokens.Add(CommonGrammar.TokenProperty("IsLiftedToNull"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(expression.IsLiftedToNull));

				this.tokens.Add(CommonGrammar.TokenProperty("Method"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(expression.Method != null ? expression.Method.Name : null));

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitTypeIs(TypeBinaryExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("TypeOperand"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(expression.TypeOperand));

				this.tokens.Add(CommonGrammar.TokenProperty("Expression"));
				this.Visit(expression.Expression);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitConstant(ConstantExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Type"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(this.GetTypeName(expression.Type)));

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitConditional(ConditionalExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Test"));
				this.Visit(expression.Test);

				this.tokens.Add(CommonGrammar.TokenProperty("IfTrue"));
				this.Visit(expression.IfTrue);

				this.tokens.Add(CommonGrammar.TokenProperty("IfFalse"));
				this.Visit(expression.IfFalse);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override ParameterExpression VisitParameter(ParameterExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Type"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(this.GetTypeName(expression.Type)));

				this.tokens.Add(CommonGrammar.TokenProperty("Name"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(expression.Name));

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override IEnumerable<ParameterExpression> VisitParameterList(IList<ParameterExpression> list)
		{
			this.tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);
			try
			{
				foreach (var item in list)
				{
					this.VisitParameter(item);
				}

				// no change
				return list;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenArrayEnd);
			}
		}

		protected override Expression VisitMemberAccess(MemberExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Member"));
				this.Visit(expression.Expression);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Object"));
				this.Visit(expression.Object);

				this.tokens.Add(CommonGrammar.TokenProperty("Method"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(expression.Method != null ? expression.Method.Name : null));

				this.tokens.Add(CommonGrammar.TokenProperty("Arguments"));

				this.VisitExpressionList(expression.Arguments);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override IEnumerable<Expression> VisitExpressionList(IList<Expression> list)
		{
			this.tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);

			try
			{
				foreach (Expression item in list)
				{
					this.Visit(item);
				}

				// no change
				return list;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenArrayEnd);
			}
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty("MemberAssignment"));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Member"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(assignment.Member.Name));

				this.tokens.Add(CommonGrammar.TokenProperty("Expression"));
				this.Visit(assignment.Expression);

				// no change
				return assignment;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty("MemberMemberBinding"));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Member"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(binding.Member.Name));

				this.tokens.Add(CommonGrammar.TokenProperty("Bindings"));
				this.VisitBindingList(binding.Bindings);

				// no change
				return binding;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty("MemberListBinding"));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Member"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(binding.Member.Name));

				this.tokens.Add(CommonGrammar.TokenProperty("Initializers"));
				this.VisitElementInitializerList(binding.Initializers);

				// no change
				return binding;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override IEnumerable<MemberBinding> VisitBindingList(IList<MemberBinding> list)
		{
			this.tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);

			try
			{
				foreach (var item in list)
				{
					this.VisitBinding(item);
				}

				// no change
				return list;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenArrayEnd);
			}
		}

		protected override IEnumerable<ElementInit> VisitElementInitializerList(IList<ElementInit> list)
		{
			this.tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);

			try
			{
				foreach (var item in list)
				{
					this.VisitElementInitializer(item);
				}

				// no change
				return list;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenArrayEnd);
			}
		}

		protected override Expression VisitLambda(LambdaExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Type"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(this.GetTypeName(expression.Type)));

				this.tokens.Add(CommonGrammar.TokenProperty("Body"));
				Expression body = this.Visit(expression.Body);

				this.tokens.Add(CommonGrammar.TokenProperty("Parameters"));
				this.tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);
				foreach (ParameterExpression param in expression.Parameters)
				{
					this.VisitParameter(param);
				}
				this.tokens.Add(CommonGrammar.TokenArrayEnd);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override NewExpression VisitNew(NewExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Arguments"));
				this.VisitExpressionList(expression.Arguments);

				this.tokens.Add(CommonGrammar.TokenProperty("Members"));
				if (expression.Members == null)
				{
					this.tokens.Add(CommonGrammar.TokenNull);
				}
				else
				{
					this.tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);
					foreach (var member in expression.Members)
					{
						this.tokens.Add(CommonGrammar.TokenPrimitive(member.Name));
					}
					this.tokens.Add(CommonGrammar.TokenArrayEnd);
				}

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitMemberInit(MemberInitExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("NewExpression"));
				this.VisitNew(expression.NewExpression);

				this.tokens.Add(CommonGrammar.TokenProperty("Bindings"));
				this.VisitBindingList(expression.Bindings);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitListInit(ListInitExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("NewExpression"));
				NewExpression ctor = this.VisitNew(expression.NewExpression);

				this.tokens.Add(CommonGrammar.TokenProperty("Initializers"));
				this.VisitElementInitializerList(expression.Initializers);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitNewArray(NewArrayExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("ElementType"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(expression.Type.GetElementType()));

				this.tokens.Add(CommonGrammar.TokenProperty("Expressions"));
				this.VisitExpressionList(expression.Expressions);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitInvocation(InvocationExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Arguments"));
				this.VisitExpressionList(expression.Arguments);

				this.tokens.Add(CommonGrammar.TokenProperty("Expression"));
				Expression expr = this.Visit(expression.Expression);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

#if NET40
		protected override Expression VisitBlock(BlockExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Variables"));
				this.VisitParameterList(expression.Variables);

				this.tokens.Add(CommonGrammar.TokenProperty("Expressions"));
				this.VisitExpressionList(expression.Expressions);

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitDynamic(DynamicExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Arguments"));
				this.VisitExpressionList(expression.Arguments);

				this.tokens.Add(CommonGrammar.TokenProperty("Binder"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(expression.Binder != null ? expression.Binder.GetType().Name : null));

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitDefault(DefaultExpression expression)
		{
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			this.tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			this.tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				this.tokens.Add(CommonGrammar.TokenProperty("Type"));
				this.tokens.Add(CommonGrammar.TokenPrimitive(this.GetTypeName(expression.Type)));

				// no change
				return expression;
			}
			finally
			{
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
				this.tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}
#endif

		#endregion Visit Methods

		#region IQueryTextProvider Members

		public string GetQueryText(Expression expression)
		{
			var tokens = this.GetTokens(expression);

			return this.Formatter.Format(tokens);
		}

		#endregion IQueryTextProvider Members

		#region IObjectWalker<Token<CommonTokenType>> Members

		IEnumerable<Token<CommonTokenType>> IObjectWalker<CommonTokenType>.GetTokens(object value)
		{
			return this.GetTokens(value as Expression);
		}

		public IEnumerable<Token<CommonTokenType>> GetTokens(Expression expression)
		{
			if (expression == null)
			{
				throw new InvalidOperationException("ExpressionWalker only walks expressions.");
			}

			this.tokens = new List<Token<CommonTokenType>>();

			this.Visit(expression);

			return this.tokens;
		}

		#endregion IObjectWalker<Token<CommonTokenType>> Members

		#region Utility Methods

		private string GetTypeName(Type type)
		{
			if (!type.IsGenericType)
			{
				return type.Name;
			}

			StringBuilder builder = new StringBuilder(type.GetGenericTypeDefinition().Name.Split('`')[0]);
			Type[] args = type.GetGenericArguments();

			builder.Append('<');
			for (int i=0, length=args.Length; i<length; i++)
			{
				if (i > 0)
				{
					builder.Append(", ");
				}
				builder.Append(this.GetTypeName(args[0]));
			}
			builder.Append('>');

			return builder.ToString();
		}

		#endregion Utility Methods
	}
}
