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

namespace JsonFx.Linq
{
	internal class ExpressionWalker :
		ExpressionVisitor,
		IQueryTextProvider
	{
		#region Fields

		private readonly StringBuilder Builder = new StringBuilder();

		#endregion Fields

		#region Visit Methods

		protected override Expression VisitUnknown(Expression exp)
		{
			WriteOpenLabel(exp.NodeType.ToString());
			try
			{
				return exp;
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override ElementInit VisitElementInitializer(ElementInit initializer)
		{
			WriteOpenLabel("ElementInitializer");
			try
			{
				IEnumerable<Expression> arguments = this.VisitExpressionList(initializer.Arguments);
				if (arguments == initializer.Arguments)
				{
					// no change
					return initializer;
				}

				return Expression.ElementInit(initializer.AddMethod, arguments);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitUnary(UnaryExpression unary)
		{
			WriteOpenLabel("Unary");
			try
			{
				Expression operand = this.Visit(unary.Operand);

				WriteItem(unary.NodeType);

				if (operand == unary.Operand)
				{
					// no change
					return unary;
				}

				return Expression.MakeUnary(unary.NodeType, operand, unary.Type, unary.Method);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitBinary(BinaryExpression binary)
		{
			WriteOpenLabel("Binary");
			try
			{
				Expression left = this.Visit(binary.Left);
				Expression right = this.Visit(binary.Right);
				Expression conversion = this.Visit(binary.Conversion);

				if (left == binary.Left &&
				right == binary.Right &&
				conversion == binary.Conversion)
				{
					// no change
					return binary;
				}

				if (binary.NodeType == ExpressionType.Coalesce && binary.Conversion != null)
				{
					return Expression.Coalesce(left, right, conversion as LambdaExpression);
				}

				return Expression.MakeBinary(binary.NodeType, left, right, binary.IsLiftedToNull, binary.Method);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitTypeIs(TypeBinaryExpression binary)
		{
			WriteOpenLabel("TypeIs");
			try
			{
				Expression expr = this.Visit(binary.Expression);
				if (expr == binary.Expression)
				{
					// no change
					return binary;
				}

				return Expression.TypeIs(expr, binary.TypeOperand);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitConstant(ConstantExpression constant)
		{
			WriteOpenLabel("Constant");
			try
			{
				WriteItem(constant.Type.Name);

				// no change
				return constant;
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitConditional(ConditionalExpression conditional)
		{
			WriteOpenLabel("Conditional");
			try
			{
				Expression test = this.Visit(conditional.Test);
				Expression ifTrue = this.Visit(conditional.IfTrue);
				Expression ifFalse = this.Visit(conditional.IfFalse);

				if (test == conditional.Test &&
				ifTrue == conditional.IfTrue &&
				ifFalse == conditional.IfFalse)
				{
					// no change
					return conditional;
				}

				return Expression.Condition(test, ifTrue, ifFalse);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitParameter(ParameterExpression p)
		{
			WriteOpenLabel("Parameter");
			try
			{
				WriteItem(p.Name);

				// no change
				return p;
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitMemberAccess(MemberExpression m)
		{
			WriteOpenLabel("MemberAccess");
			try
			{
				Expression exp = this.Visit(m.Expression);

				if (exp == m.Expression)
				{
					// no change
					return m;
				}

				return Expression.MakeMemberAccess(exp, m.Member);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			WriteOpenLabel("MethodCall");
			try
			{
				Expression obj = this.Visit(m.Object);
				IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments);

				if (obj == m.Object && args == m.Arguments)
				{
					// no change
					return m;
				}

				return Expression.Call(obj, m.Method, args);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override IEnumerable<Expression> VisitExpressionList(IList<Expression> original)
		{
			WriteOpenLabel("ExpressionList");
			try
			{
				List<Expression> list = null;

				for (int i=0, length=original.Count; i<length; i++)
				{
					Expression exp = this.Visit(original[i]);
					if (list != null)
					{
						list.Add(exp);
					}
					else if (exp != original[i])
					{
						list = new List<Expression>(length);
						for (int j=0; j<i; j++)
						{
							// copy preceding values
							list.Add(original[j]);
						}
						list.Add(exp);
					}
				}

				if (list == null)
				{
					// no change
					return original;
				}

				return list.AsReadOnly();
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			WriteOpenLabel("MemberAssignment");
			try
			{
				Expression exp = this.Visit(assignment.Expression);

				if (exp == assignment.Expression)
				{
					// no change
					return assignment;
				}

				return Expression.Bind(assignment.Member, exp);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			WriteOpenLabel("MemberMemberBinding");
			try
			{
				IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);

				if (bindings == binding.Bindings)
				{
					// no change
					return binding;
				}

				return Expression.MemberBind(binding.Member, bindings);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			WriteOpenLabel("MemberListBinding");
			try
			{
				IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);

				if (initializers == binding.Initializers)
				{
					// no change
					return binding;
				}

				return Expression.ListBind(binding.Member, initializers);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override IEnumerable<MemberBinding> VisitBindingList(IList<MemberBinding> original)
		{
			WriteOpenLabel("BindingList");
			try
			{
				List<MemberBinding> list = null;

				for (int i=0, length=original.Count; i<length; i++)
				{
					MemberBinding b = this.VisitBinding(original[i]);
					if (list != null)
					{
						list.Add(b);
					}
					else if (b != original[i])
					{
						list = new List<MemberBinding>(length);
						for (int j = 0; j < i; j++)
						{
							// copy preceding values
							list.Add(original[j]);
						}
						list.Add(b);
					}
				}

				if (list == null)
				{
					// no change
					return original;
				}

				return list;
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override IEnumerable<ElementInit> VisitElementInitializerList(IList<ElementInit> original)
		{
			WriteOpenLabel("ElementInitializerList");
			try
			{
				List<ElementInit> list = null;

				for (int i=0, length=original.Count; i<length; i++)
				{
					ElementInit init = this.VisitElementInitializer(original[i]);
					if (list != null)
					{
						list.Add(init);
					}
					else if (init != original[i])
					{
						list = new List<ElementInit>(length);
						for (int j = 0; j < i; j++)
						{
							// copy preceding values
							list.Add(original[j]);
						}
						list.Add(init);
					}
				}

				if (list == null)
				{
					// no change
					return original;
				}

				return list;
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitLambda(LambdaExpression lambda)
		{
			WriteOpenLabel("Lambda");
			try
			{
				Expression body = this.Visit(lambda.Body);
				if (body == lambda.Body)
				{
					// no change
					return lambda;
				}

				return Expression.Lambda(lambda.Type, body, lambda.Parameters);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override NewExpression VisitNew(NewExpression ctor)
		{
			WriteOpenLabel("New");
			try
			{
				IEnumerable<Expression> args = this.VisitExpressionList(ctor.Arguments);
				if (args == ctor.Arguments)
				{
					// no change
					return ctor;
				}

				if (ctor.Members == null)
				{
					return Expression.New(ctor.Constructor, args);
				}

				return Expression.New(ctor.Constructor, args, ctor.Members);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitMemberInit(MemberInitExpression init)
		{
			WriteOpenLabel("MemberInit");
			try
			{
				NewExpression ctor = this.VisitNew(init.NewExpression);
				IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings);

				if (ctor == init.NewExpression && bindings == init.Bindings)
				{
					// no change
					return init;
				}

				return Expression.MemberInit(ctor, bindings);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitListInit(ListInitExpression init)
		{
			WriteOpenLabel("ListInit");
			try
			{
				NewExpression ctor = this.VisitNew(init.NewExpression);
				IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers);

				if (ctor == init.NewExpression && initializers == init.Initializers)
				{
					// no change
					return init;
				}

				return Expression.ListInit(ctor, initializers);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitNewArray(NewArrayExpression ctor)
		{
			WriteOpenLabel("NewArray");
			try
			{
				IEnumerable<Expression> exprs = this.VisitExpressionList(ctor.Expressions);
				if (exprs == ctor.Expressions)
				{
					// no change
					return ctor;
				}

				if (ctor.NodeType == ExpressionType.NewArrayInit)
				{
					return Expression.NewArrayInit(ctor.Type.GetElementType(), exprs);
				}

				return Expression.NewArrayBounds(ctor.Type.GetElementType(), exprs);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitInvocation(InvocationExpression invoke)
		{
			WriteOpenLabel("Invocation");
			try
			{
				IEnumerable<Expression> args = this.VisitExpressionList(invoke.Arguments);
				Expression expr = this.Visit(invoke.Expression);

				if (args == invoke.Arguments && expr == invoke.Expression)
				{
					// no change
					return invoke;
				}

				return Expression.Invoke(expr, args);
			}
			finally
			{
				WriteCloseLabel();
			}
		}

#if NET40
		protected override Expression VisitBlock(BlockExpression exp)
		{
			WriteOpenLabel("Block");
			try
			{
				// TODO: implement visitor method
				return exp;
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitDynamic(DynamicExpression exp)
		{
			WriteOpenLabel("Dynamic");
			try
			{
				// TODO: implement visitor method
				return exp;
			}
			finally
			{
				WriteCloseLabel();
			}
		}

		protected override Expression VisitDefault(DefaultExpression exp)
		{
			WriteOpenLabel("Default");
			try
			{
				// TODO: implement visitor method
				return exp;
			}
			finally
			{
				WriteCloseLabel();
			}
		}
#endif

		#endregion Visit Methods

		#region IQueryTextProvider Members

		public string GetQueryText(Expression expression)
		{
			this.Builder.Length = 0;
			this.Builder.Append('[');
			this.depth++;
			pendingCRLF = true;
			pendingDelim = false;

			this.Visit(expression);

			this.depth--;
			this.WriteLine();
			this.Builder.Append(']');
			return this.Builder.ToString();
		}

		int depth;
		bool pendingCRLF, pendingDelim;
		private void WriteOpenLabel(string label)
		{
			if (pendingDelim)
			{
				this.Builder.Append(",");
				pendingDelim = false;
				pendingCRLF = true;
			}
			else if (this.Builder.Length > 0)
			{
				char last = this.Builder[this.Builder.Length-1];

				if (last == '[')
				{
					pendingCRLF = true;
				}
			}
			if (pendingCRLF)
			{
				pendingCRLF = false;
				this.WriteLine();
			}
			this.Builder.Append(label);
			this.Builder.Append(": [");

			this.depth++;
		}

		private void WriteItem(object value)
		{
			if (pendingDelim)
			{
				this.Builder.Append(',');
				this.WriteLine();
			}
			else if (pendingCRLF)
			{
				this.WriteLine();
				pendingCRLF = true;
			}
			else
			{
				this.Builder.Append(' ');
			}
			this.Builder.Append(value);
			pendingDelim = true;
		}

		private void WriteCloseLabel()
		{
			this.depth--;
			if (pendingCRLF)
			{
				this.WriteLine();
			}
			else if (pendingDelim)
			{
				this.Builder.Append(' ');
			}
			this.Builder.Append(']');
			pendingCRLF = pendingDelim = true;
		}

		private void WriteLine()
		{
			// emit CRLF
			this.Builder.AppendLine();

			// indent next line accordingly
			this.Builder.Append(new String('\t', this.depth));
		}

		#endregion IQueryTextProvider Members
	}
}
