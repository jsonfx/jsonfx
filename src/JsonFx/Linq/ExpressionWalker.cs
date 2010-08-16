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

using JsonFx.Model;
using JsonFx.Serialization;

namespace JsonFx.Linq
{
	internal class ExpressionWalker :
		ExpressionVisitor<List<Token<ModelTokenType>>>,
		IObjectWalker<ModelTokenType>,
		IQueryTextProvider
	{
		#region Fields

		private readonly ITextFormatter<ModelTokenType> Formatter;

		#endregion Fields

		#region Init

		/// <summary>
		/// 
		/// </summary>
		/// <param name="formatter"></param>
		public ExpressionWalker(ITextFormatter<ModelTokenType> formatter)
		{
			if (formatter == null)
			{
				throw new ArgumentNullException("formatter");
			}

			this.Formatter = formatter;
		}

		#endregion Init

		#region Visit Methods

		protected override Expression Visit(Expression expression, List<Token<ModelTokenType>> tokens)
		{
			if (expression == null)
			{
				tokens.Add(ModelGrammar.TokenNull);
			}

			return base.Visit(expression, tokens);
		}

		protected override Expression VisitUnknown(Expression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenPrimitive(expression.NodeType));

			return expression;
		}

		protected override ElementInit VisitElementInitializer(ElementInit init, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty("ElementInit"));
			tokens.Add(ModelGrammar.TokenArrayBeginUnnamed);

			try
			{
				this.VisitExpressionList(init.Arguments, tokens);

				// no change
				return init;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenArrayEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitUnary(UnaryExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Type"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				tokens.Add(ModelGrammar.TokenProperty("Method"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(expression.Method)));

				tokens.Add(ModelGrammar.TokenProperty("Operand"));
				this.Visit(expression.Operand, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitBinary(BinaryExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Left"));
				this.Visit(expression.Left, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Right"));
				this.Visit(expression.Right, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Conversion"));
				this.Visit(expression.Conversion, tokens);

				tokens.Add(ModelGrammar.TokenProperty("IsLiftedToNull"));
				tokens.Add(ModelGrammar.TokenPrimitive(expression.IsLiftedToNull));

				tokens.Add(ModelGrammar.TokenProperty("Method"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(expression.Method)));

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitTypeIs(TypeBinaryExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("TypeOperand"));
				tokens.Add(ModelGrammar.TokenPrimitive(expression.TypeOperand));

				tokens.Add(ModelGrammar.TokenProperty("Expression"));
				this.Visit(expression.Expression, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitConstant(ConstantExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Type"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				tokens.Add(ModelGrammar.TokenProperty("Value"));
				if (expression.Type == null || expression.Value == null)
				{
					tokens.Add(ModelGrammar.TokenNull);
				}
				else if (typeof(IQueryable).IsAssignableFrom(expression.Type))
				{
					// prevent recursively walking Query<T>
					tokens.Add(ModelGrammar.TokenPrimitive("[ ... ]"));
				}
				else
				{
					//var value = new ModelWalker(new DataWriterSettings()).GetTokens(expression.Value);
					//tokens.AddRange(value);
					tokens.Add(ModelGrammar.TokenPrimitive(Token<ModelTokenType>.ToString(expression.Value)));
				}

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitConditional(ConditionalExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Test"));
				this.Visit(expression.Test, tokens);

				tokens.Add(ModelGrammar.TokenProperty("IfTrue"));
				this.Visit(expression.IfTrue, tokens);

				tokens.Add(ModelGrammar.TokenProperty("IfFalse"));
				this.Visit(expression.IfFalse, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override ParameterExpression VisitParameter(ParameterExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Type"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				tokens.Add(ModelGrammar.TokenProperty("Name"));
				tokens.Add(ModelGrammar.TokenPrimitive(expression.Name));

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override IEnumerable<ParameterExpression> VisitParameterList(IList<ParameterExpression> list, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenArrayBeginUnnamed);
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
				tokens.Add(ModelGrammar.TokenArrayEnd);
			}
		}

		protected override Expression VisitMemberAccess(MemberExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Member"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(expression.Member)));

				tokens.Add(ModelGrammar.TokenProperty("Expression"));
				this.Visit(expression.Expression, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Object"));
				this.Visit(expression.Object, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Method"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(expression.Method)));

				tokens.Add(ModelGrammar.TokenProperty("Arguments"));

				this.VisitExpressionList(expression.Arguments, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override IEnumerable<Expression> VisitExpressionList(IList<Expression> list, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenArrayBeginUnnamed);

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
				tokens.Add(ModelGrammar.TokenArrayEnd);
			}
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty("MemberAssignment"));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Member"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(assignment.Member)));

				tokens.Add(ModelGrammar.TokenProperty("Expression"));
				this.Visit(assignment.Expression, tokens);

				// no change
				return assignment;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty("MemberMemberBinding"));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Member"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(binding.Member)));

				tokens.Add(ModelGrammar.TokenProperty("Bindings"));
				this.VisitBindingList(binding.Bindings, tokens);

				// no change
				return binding;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty("MemberListBinding"));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Member"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(binding.Member)));

				tokens.Add(ModelGrammar.TokenProperty("Initializers"));
				this.VisitElementInitializerList(binding.Initializers, tokens);

				// no change
				return binding;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override IEnumerable<MemberBinding> VisitBindingList(IList<MemberBinding> list, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenArrayBeginUnnamed);

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
				tokens.Add(ModelGrammar.TokenArrayEnd);
			}
		}

		protected override IEnumerable<ElementInit> VisitElementInitializerList(IList<ElementInit> list, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenArrayBeginUnnamed);

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
				tokens.Add(ModelGrammar.TokenArrayEnd);
			}
		}

		protected override Expression VisitLambda(LambdaExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Type"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				tokens.Add(ModelGrammar.TokenProperty("Body"));
				Expression body = this.Visit(expression.Body, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Parameters"));
				tokens.Add(ModelGrammar.TokenArrayBeginUnnamed);
				foreach (ParameterExpression param in expression.Parameters)
				{
					this.VisitParameter(param, tokens);
				}
				tokens.Add(ModelGrammar.TokenArrayEnd);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override NewExpression VisitNew(NewExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Arguments"));
				this.VisitExpressionList(expression.Arguments, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Members"));
				if (expression.Members == null)
				{
					tokens.Add(ModelGrammar.TokenNull);
				}
				else
				{
					tokens.Add(ModelGrammar.TokenArrayBeginUnnamed);
					foreach (var member in expression.Members)
					{
						tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetMemberName(member)));
					}
					tokens.Add(ModelGrammar.TokenArrayEnd);
				}

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitMemberInit(MemberInitExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("NewExpression"));
				this.VisitNew(expression.NewExpression, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Bindings"));
				this.VisitBindingList(expression.Bindings, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitListInit(ListInitExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("NewExpression"));
				NewExpression ctor = this.VisitNew(expression.NewExpression, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Initializers"));
				this.VisitElementInitializerList(expression.Initializers, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitNewArray(NewArrayExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("ElementType"));
				tokens.Add(ModelGrammar.TokenPrimitive(expression.Type.GetElementType()));

				tokens.Add(ModelGrammar.TokenProperty("Expressions"));
				this.VisitExpressionList(expression.Expressions, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitInvocation(InvocationExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Arguments"));
				this.VisitExpressionList(expression.Arguments, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Expression"));
				Expression expr = this.Visit(expression.Expression, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

#if NET40
		protected override Expression VisitBlock(BlockExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Variables"));
				this.VisitParameterList(expression.Variables, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Expressions"));
				this.VisitExpressionList(expression.Expressions, tokens);

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitDynamic(DynamicExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Arguments"));
				this.VisitExpressionList(expression.Arguments, tokens);

				tokens.Add(ModelGrammar.TokenProperty("Binder"));
				tokens.Add(ModelGrammar.TokenPrimitive(expression.Binder != null ? expression.Binder.GetType().Name : null));

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
			}
		}

		protected override Expression VisitDefault(DefaultExpression expression, List<Token<ModelTokenType>> tokens)
		{
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);
			tokens.Add(ModelGrammar.TokenProperty(expression.NodeType));
			tokens.Add(ModelGrammar.TokenObjectBeginUnnamed);

			try
			{
				tokens.Add(ModelGrammar.TokenProperty("Type"));
				tokens.Add(ModelGrammar.TokenPrimitive(ExpressionWalker.GetTypeName(expression.Type)));

				// no change
				return expression;
			}
			finally
			{
				tokens.Add(ModelGrammar.TokenObjectEnd);
				tokens.Add(ModelGrammar.TokenObjectEnd);
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

		#region IObjectWalker<Token<ModelTokenType>> Members

		IEnumerable<Token<ModelTokenType>> IObjectWalker<ModelTokenType>.GetTokens(object value)
		{
			return this.GetTokens(value as Expression);
		}

		public IEnumerable<Token<ModelTokenType>> GetTokens(Expression expression)
		{
			if (expression == null)
			{
				throw new InvalidOperationException("ExpressionWalker only walks expressions.");
			}

			var tokens = new List<Token<ModelTokenType>>();

			this.Visit(expression, tokens);

			return tokens;
		}

		#endregion IObjectWalker<Token<ModelTokenType>> Members

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
