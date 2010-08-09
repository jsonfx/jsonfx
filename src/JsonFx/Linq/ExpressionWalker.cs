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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using JsonFx.Common;
using JsonFx.Serialization;

namespace JsonFx.Linq
{
	internal class ExpressionWalker :
		ExpressionVisitor<List<Token<CommonTokenType>>>,
		IObjectWalker<CommonTokenType>,
		IQueryTextProvider
	{
		#region Fields

		private readonly ITextFormatter<CommonTokenType> Formatter;

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

		protected override Expression Visit(Expression expression, List<Token<CommonTokenType>> tokens)
		{
			if (expression == null)
			{
				tokens.Add(CommonGrammar.TokenNull);
			}

			return base.Visit(expression, tokens);
		}

		protected override Expression VisitUnknown(Expression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenPrimitive(expression.NodeType));

			return expression;
		}

		protected override ElementInit VisitElementInitializer(ElementInit init, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty("ElementInit"));
			tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);

			try
			{
				this.VisitExpressionList(init.Arguments, tokens);

				// no change
				return init;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenArrayEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitUnary(UnaryExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Type"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				tokens.Add(CommonGrammar.TokenProperty("Method"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(expression.Method)));

				tokens.Add(CommonGrammar.TokenProperty("Operand"));
				this.Visit(expression.Operand, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitBinary(BinaryExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Left"));
				this.Visit(expression.Left, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Right"));
				this.Visit(expression.Right, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Conversion"));
				this.Visit(expression.Conversion, tokens);

				tokens.Add(CommonGrammar.TokenProperty("IsLiftedToNull"));
				tokens.Add(CommonGrammar.TokenPrimitive(expression.IsLiftedToNull));

				tokens.Add(CommonGrammar.TokenProperty("Method"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(expression.Method)));

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitTypeIs(TypeBinaryExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("TypeOperand"));
				tokens.Add(CommonGrammar.TokenPrimitive(expression.TypeOperand));

				tokens.Add(CommonGrammar.TokenProperty("Expression"));
				this.Visit(expression.Expression, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitConstant(ConstantExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Type"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				tokens.Add(CommonGrammar.TokenProperty("Value"));
				if (expression.Type == null || expression.Value == null)
				{
					tokens.Add(CommonGrammar.TokenNull);
				}
				else if (typeof(IQueryable).IsAssignableFrom(expression.Type))
				{
					// prevent recursively walking Query<T>
					tokens.Add(CommonGrammar.TokenPrimitive("[ ... ]"));
				}
				else
				{
					//var value = new CommonWalker(new DataWriterSettings()).GetTokens(expression.Value);
					//tokens.AddRange(value);
					tokens.Add(CommonGrammar.TokenPrimitive(Token<CommonTokenType>.ToString(expression.Value)));
				}

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitConditional(ConditionalExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Test"));
				this.Visit(expression.Test, tokens);

				tokens.Add(CommonGrammar.TokenProperty("IfTrue"));
				this.Visit(expression.IfTrue, tokens);

				tokens.Add(CommonGrammar.TokenProperty("IfFalse"));
				this.Visit(expression.IfFalse, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override ParameterExpression VisitParameter(ParameterExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Type"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				tokens.Add(CommonGrammar.TokenProperty("Name"));
				tokens.Add(CommonGrammar.TokenPrimitive(expression.Name));

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override IEnumerable<ParameterExpression> VisitParameterList(IList<ParameterExpression> list, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);
			try
			{
				foreach (var item in list)
				{
					this.VisitParameter(item, tokens);
				}

				// no change
				return list;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenArrayEnd);
			}
		}

		protected override Expression VisitMemberAccess(MemberExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Member"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(expression.Member)));

				tokens.Add(CommonGrammar.TokenProperty("Expression"));
				this.Visit(expression.Expression, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Object"));
				this.Visit(expression.Object, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Method"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(expression.Method)));

				tokens.Add(CommonGrammar.TokenProperty("Arguments"));

				this.VisitExpressionList(expression.Arguments, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override IEnumerable<Expression> VisitExpressionList(IList<Expression> list, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);

			try
			{
				foreach (Expression item in list)
				{
					this.Visit(item, tokens);
				}

				// no change
				return list;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenArrayEnd);
			}
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty("MemberAssignment"));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Member"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(assignment.Member)));

				tokens.Add(CommonGrammar.TokenProperty("Expression"));
				this.Visit(assignment.Expression, tokens);

				// no change
				return assignment;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty("MemberMemberBinding"));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Member"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(binding.Member)));

				tokens.Add(CommonGrammar.TokenProperty("Bindings"));
				this.VisitBindingList(binding.Bindings, tokens);

				// no change
				return binding;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty("MemberListBinding"));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Member"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(binding.Member)));

				tokens.Add(CommonGrammar.TokenProperty("Initializers"));
				this.VisitElementInitializerList(binding.Initializers, tokens);

				// no change
				return binding;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override IEnumerable<MemberBinding> VisitBindingList(IList<MemberBinding> list, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);

			try
			{
				foreach (var item in list)
				{
					this.VisitBinding(item, tokens);
				}

				// no change
				return list;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenArrayEnd);
			}
		}

		protected override IEnumerable<ElementInit> VisitElementInitializerList(IList<ElementInit> list, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);

			try
			{
				foreach (var item in list)
				{
					this.VisitElementInitializer(item, tokens);
				}

				// no change
				return list;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenArrayEnd);
			}
		}

		protected override Expression VisitLambda(LambdaExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Type"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				tokens.Add(CommonGrammar.TokenProperty("Body"));
				Expression body = this.Visit(expression.Body, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Parameters"));
				tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);
				foreach (ParameterExpression param in expression.Parameters)
				{
					this.VisitParameter(param, tokens);
				}
				tokens.Add(CommonGrammar.TokenArrayEnd);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override NewExpression VisitNew(NewExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Arguments"));
				this.VisitExpressionList(expression.Arguments, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Members"));
				if (expression.Members == null)
				{
					tokens.Add(CommonGrammar.TokenNull);
				}
				else
				{
					tokens.Add(CommonGrammar.TokenArrayBeginUnnamed);
					foreach (var member in expression.Members)
					{
						tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(member)));
					}
					tokens.Add(CommonGrammar.TokenArrayEnd);
				}

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitMemberInit(MemberInitExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("NewExpression"));
				this.VisitNew(expression.NewExpression, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Bindings"));
				this.VisitBindingList(expression.Bindings, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitListInit(ListInitExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("NewExpression"));
				NewExpression ctor = this.VisitNew(expression.NewExpression, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Initializers"));
				this.VisitElementInitializerList(expression.Initializers, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitNewArray(NewArrayExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("ElementType"));
				tokens.Add(CommonGrammar.TokenPrimitive(expression.Type.GetElementType()));

				tokens.Add(CommonGrammar.TokenProperty("Expressions"));
				this.VisitExpressionList(expression.Expressions, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitInvocation(InvocationExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Arguments"));
				this.VisitExpressionList(expression.Arguments, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Expression"));
				Expression expr = this.Visit(expression.Expression, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

#if NET40
		protected override Expression VisitBlock(BlockExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Variables"));
				this.VisitParameterList(expression.Variables, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Expressions"));
				this.VisitExpressionList(expression.Expressions, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitDynamic(DynamicExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Arguments"));
				this.VisitExpressionList(expression.Arguments, tokens);

				tokens.Add(CommonGrammar.TokenProperty("Binder"));
				tokens.Add(CommonGrammar.TokenPrimitive(expression.Binder != null ? expression.Binder.GetType().Name : null));

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitDefault(DefaultExpression expression, List<Token<CommonTokenType>> tokens)
		{
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);
			tokens.Add(CommonGrammar.TokenProperty(expression.NodeType));
			tokens.Add(CommonGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(CommonGrammar.TokenProperty("Type"));
				tokens.Add(CommonGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(CommonGrammar.TokenObjectEnd);
				tokens.Add(CommonGrammar.TokenObjectEnd);
			}
		}
#endif

		#endregion Visit Methods

		#region IQueryTextProvider Members

		public string GetQueryText(Expression expression)
		{
			try
			{
				var tokens = this.GetTokens(expression);

				return this.Formatter.Format(tokens);
			}
			catch (Exception ex)
			{
				return String.Concat(
					"Error: [ ",
					ex.Message,
					" ]");
			}
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

			var tokens = new List<Token<CommonTokenType>>();

			this.Visit(expression, tokens);

			return tokens;
		}

		#endregion IObjectWalker<Token<CommonTokenType>> Members

		#region Utility Methods

		private static string GetMemberName(MemberInfo member)
		{
			if (member == null)
			{
				return null;
			}

			MethodInfo method = member as MethodInfo;
			if (method != null &&
				method.IsGenericMethod)
			{
				Type[] typeArgs = method.GetGenericArguments();
				int length = typeArgs.Length;
				string[] types = new string[length];
				for (int i=0; i<length; i++)
				{
					types[i] = ExpressionWalker.GetTypeName(typeArgs[i]);
				}

				return String.Concat(
					GetTypeName(method.ReturnType),
					" ",
					GetTypeName(method.DeclaringType),
					'.',
					method.Name,
					'<',
					String.Join(", ", types),
					'>');
			}

			return String.Concat(
				GetTypeName(member.DeclaringType),
				'.',
				member.Name);
		}

		private static string GetTypeName(Type type)
		{
			if (type == null)
			{
				return null;
			}

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
				builder.Append(ExpressionWalker.GetTypeName(args[i]));
			}
			builder.Append('>');

			return builder.ToString();
		}

		#endregion Utility Methods
	}
}
