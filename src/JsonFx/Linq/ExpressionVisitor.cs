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

namespace JsonFx.Linq
{
	internal abstract class ExpressionVisitor
	{
		#region Constants

		private const string ErrorUnexpectedExpression = "Unexpected expression ({0})";
		private const string ErrorUnexpectedMemberBinding = "Unexpected member binding ({0})";

		#endregion Constants

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		protected ExpressionVisitor()
		{
		}

		#endregion Init

		#region Visit Methods

		protected virtual Expression Visit(Expression expression)
		{
			if (expression == null)
			{
				return null;
			}

			//if (expression.CanReduce)
			//{
			//    return this.Visit(expression.ReduceAndCheck());
			//}

			switch (expression.NodeType)
			{
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.ArrayLength:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
				case ExpressionType.UnaryPlus:
#if NET40
				case ExpressionType.Decrement:
				case ExpressionType.Increment:
				case ExpressionType.Throw:
				case ExpressionType.Unbox:
				case ExpressionType.PreIncrementAssign:
				case ExpressionType.PreDecrementAssign:
				case ExpressionType.PostIncrementAssign:
				case ExpressionType.PostDecrementAssign:
				case ExpressionType.OnesComplement:
				case ExpressionType.IsTrue:
				case ExpressionType.IsFalse:
#endif
				{
					return this.VisitUnary((UnaryExpression)expression);
				}
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.Divide:
				case ExpressionType.Modulo:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.Equal:
				case ExpressionType.NotEqual:
				case ExpressionType.Coalesce:
				case ExpressionType.ArrayIndex:
				case ExpressionType.RightShift:
				case ExpressionType.LeftShift:
				case ExpressionType.ExclusiveOr:
				case ExpressionType.Power:
#if NET40
				case ExpressionType.Assign:
				case ExpressionType.AddAssign:
				case ExpressionType.AndAssign:
				case ExpressionType.DivideAssign:
				case ExpressionType.ExclusiveOrAssign:
				case ExpressionType.LeftShiftAssign:
				case ExpressionType.ModuloAssign:
				case ExpressionType.MultiplyAssign:
				case ExpressionType.OrAssign:
				case ExpressionType.PowerAssign:
				case ExpressionType.RightShiftAssign:
				case ExpressionType.SubtractAssign:
				case ExpressionType.AddAssignChecked:
				case ExpressionType.MultiplyAssignChecked:
				case ExpressionType.SubtractAssignChecked:
#endif
				{
					return this.VisitBinary((BinaryExpression)expression);
				}
				case ExpressionType.TypeIs:
				{
					return this.VisitTypeIs((TypeBinaryExpression)expression);
				}
				case ExpressionType.Conditional:
				{
					return this.VisitConditional((ConditionalExpression)expression);
				}
				case ExpressionType.Constant:
				{
					return this.VisitConstant((ConstantExpression)expression);
				}
				case ExpressionType.Parameter:
				{
					return this.VisitParameter((ParameterExpression)expression);
				}
				case ExpressionType.MemberAccess:
				{
					return this.VisitMemberAccess((MemberExpression)expression);
				}
				case ExpressionType.Call:
				{
					return this.VisitMethodCall((MethodCallExpression)expression);
				}
				case ExpressionType.Lambda:
				{
					return this.VisitLambda((LambdaExpression)expression);
				}
				case ExpressionType.New:
				{
					return this.VisitNew((NewExpression)expression);
				}
				case ExpressionType.NewArrayInit:
				case ExpressionType.NewArrayBounds:
				{
					return this.VisitNewArray((NewArrayExpression)expression);
				}
				case ExpressionType.Invoke:
				{
					return this.VisitInvocation((InvocationExpression)expression);
				}
				case ExpressionType.MemberInit:
				{
					return this.VisitMemberInit((MemberInitExpression)expression);
				}
				case ExpressionType.ListInit:
				{
					return this.VisitListInit((ListInitExpression)expression);
				}
#if NET40
				case ExpressionType.Block:
				{
					return this.VisitBlock((BlockExpression)expression);
				}
				case ExpressionType.Dynamic:
				{
					return this.VisitDynamic((DynamicExpression)expression);
				}
				case ExpressionType.Default:
				{
					return this.VisitDefault((DefaultExpression)expression);
				}
				case ExpressionType.Goto:
				case ExpressionType.Label:
				case ExpressionType.RuntimeVariables:
				case ExpressionType.Loop:
				case ExpressionType.Switch:
				case ExpressionType.Try:
				case ExpressionType.TypeEqual:
				case ExpressionType.DebugInfo:
				case ExpressionType.Index:
				case ExpressionType.Extension:
				{
					// TODO: implement visitor method
					return expression;
				}
#endif
				default:
				{
					return this.VisitUnknown(expression);
				}
			}
		}

		protected virtual Expression VisitUnknown(Expression exp)
		{
			throw new NotSupportedException(String.Format(
				ExpressionVisitor.ErrorUnexpectedExpression,
				exp.NodeType));
		}

		protected virtual MemberBinding VisitBinding(MemberBinding binding)
		{
			switch (binding.BindingType)
			{
				case MemberBindingType.Assignment:
				{
					return this.VisitMemberAssignment((MemberAssignment)binding);
				}
				case MemberBindingType.MemberBinding:
				{
					return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
				}
				case MemberBindingType.ListBinding:
				{
					return this.VisitMemberListBinding((MemberListBinding)binding);
				}
				default:
				{
					throw new NotSupportedException(String.Format(
						ExpressionVisitor.ErrorUnexpectedMemberBinding,
						binding.BindingType));
				}
			}
		}

		protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
		{
			IEnumerable<Expression> arguments = this.VisitExpressionList(initializer.Arguments);
			if (arguments == initializer.Arguments)
			{
				// no change
				return initializer;
			}

			return Expression.ElementInit(initializer.AddMethod, arguments);
		}

		protected virtual Expression VisitUnary(UnaryExpression unary)
		{
			Expression operand = this.Visit(unary.Operand);
			if (operand == unary.Operand)
			{
				// no change
				return unary;
			}

			return Expression.MakeUnary(unary.NodeType, operand, unary.Type, unary.Method);
		}

		protected virtual Expression VisitBinary(BinaryExpression binary)
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

		protected virtual Expression VisitTypeIs(TypeBinaryExpression binary)
		{
			Expression expr = this.Visit(binary.Expression);
			if (expr == binary.Expression)
			{
				// no change
				return binary;
			}

			return Expression.TypeIs(expr, binary.TypeOperand);
		}

		protected virtual Expression VisitConstant(ConstantExpression constant)
		{
			// no change
			return constant;
		}

		protected virtual Expression VisitConditional(ConditionalExpression conditional)
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

		protected virtual Expression VisitParameter(ParameterExpression p)
		{
			// no change
			return p;
		}

		protected virtual IEnumerable<Expression> VisitParameterList(IList<ParameterExpression> original)
		{
			List<Expression> list = null;

			for (int i=0, length=original.Count; i<length; i++)
			{
				Expression exp = this.VisitParameter(original[i]);
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

		protected virtual Expression VisitMemberAccess(MemberExpression m)
		{
			Expression exp = this.Visit(m.Expression);

			if (exp == m.Expression)
			{
				// no change
				return m;
			}

			return Expression.MakeMemberAccess(exp, m.Member);
		}

		protected virtual Expression VisitMethodCall(MethodCallExpression m)
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

		protected virtual IEnumerable<Expression> VisitExpressionList(IList<Expression> original)
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

		protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
		{
			Expression exp = this.Visit(assignment.Expression);

			if (exp == assignment.Expression)
			{
				// no change
				return assignment;
			}

			return Expression.Bind(assignment.Member, exp);
		}

		protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
		{
			IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);

			if (bindings == binding.Bindings)
			{
				// no change
				return binding;
			}

			return Expression.MemberBind(binding.Member, bindings);
		}

		protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
		{
			IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);

			if (initializers == binding.Initializers)
			{
				// no change
				return binding;
			}

			return Expression.ListBind(binding.Member, initializers);
		}

		protected virtual IEnumerable<MemberBinding> VisitBindingList(IList<MemberBinding> original)
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

		protected virtual IEnumerable<ElementInit> VisitElementInitializerList(IList<ElementInit> original)
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

		protected virtual Expression VisitLambda(LambdaExpression lambda)
		{
			Expression body = this.Visit(lambda.Body);
			if (body == lambda.Body)
			{
				// no change
				return lambda;
			}

			return Expression.Lambda(lambda.Type, body, lambda.Parameters);
		}

		protected virtual NewExpression VisitNew(NewExpression ctor)
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

		protected virtual Expression VisitMemberInit(MemberInitExpression init)
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

		protected virtual Expression VisitListInit(ListInitExpression init)
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

		protected virtual Expression VisitNewArray(NewArrayExpression ctor)
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

		protected virtual Expression VisitInvocation(InvocationExpression invoke)
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

#if NET40
		protected virtual Expression VisitBlock(BlockExpression block)
		{
			// TODO: implement visitor method
			return block;
		}

		protected virtual Expression VisitDynamic(DynamicExpression dyn)
		{
			// TODO: implement visitor method
			return dyn;
		}

		protected virtual Expression VisitDefault(DefaultExpression exp)
		{
			// TODO: implement visitor method
			return exp;
		}
#endif

		#endregion Visit Methods

		#region Utility Methods

		/// <summary>
		/// Unwraps quote expressions
		/// </summary>
		/// <param name="exp"></param>
		/// <returns></returns>
		protected static Expression StripQuotes(Expression exp)
		{
			while (exp.NodeType == ExpressionType.Quote)
			{
				exp = ((UnaryExpression)exp).Operand;
			}

			return exp;
		}

		#endregion Utility Methods
	}
}
