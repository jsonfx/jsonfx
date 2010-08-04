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
	internal abstract class ExpressionVisitor<T>
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

		protected virtual Expression Visit(Expression expression, T context)
		{
			if (expression == null)
			{
				return null;
			}

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
					return this.VisitUnary((UnaryExpression)expression, context);
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
					return this.VisitBinary((BinaryExpression)expression, context);
				}
				case ExpressionType.TypeIs:
				{
					return this.VisitTypeIs((TypeBinaryExpression)expression, context);
				}
				case ExpressionType.Conditional:
				{
					return this.VisitConditional((ConditionalExpression)expression, context);
				}
				case ExpressionType.Constant:
				{
					return this.VisitConstant((ConstantExpression)expression, context);
				}
				case ExpressionType.Parameter:
				{
					return this.VisitParameter((ParameterExpression)expression, context);
				}
				case ExpressionType.MemberAccess:
				{
					return this.VisitMemberAccess((MemberExpression)expression, context);
				}
				case ExpressionType.Call:
				{
					return this.VisitMethodCall((MethodCallExpression)expression, context);
				}
				case ExpressionType.Lambda:
				{
					return this.VisitLambda((LambdaExpression)expression, context);
				}
				case ExpressionType.New:
				{
					return this.VisitNew((NewExpression)expression, context);
				}
				case ExpressionType.NewArrayInit:
				case ExpressionType.NewArrayBounds:
				{
					return this.VisitNewArray((NewArrayExpression)expression, context);
				}
				case ExpressionType.Invoke:
				{
					return this.VisitInvocation((InvocationExpression)expression, context);
				}
				case ExpressionType.MemberInit:
				{
					return this.VisitMemberInit((MemberInitExpression)expression, context);
				}
				case ExpressionType.ListInit:
				{
					return this.VisitListInit((ListInitExpression)expression, context);
				}
#if NET40
				case ExpressionType.Block:
				{
					return this.VisitBlock((BlockExpression)expression, context);
				}
				case ExpressionType.Dynamic:
				{
					return this.VisitDynamic((DynamicExpression)expression, context);
				}
				case ExpressionType.Default:
				{
					return this.VisitDefault((DefaultExpression)expression, context);
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
					return this.VisitUnknown(expression, context);
				}
			}
		}

		protected virtual Expression VisitUnknown(Expression exp, T context)
		{
			throw new NotSupportedException(String.Format(
				ExpressionVisitor<T>.ErrorUnexpectedExpression,
				exp.NodeType));
		}

		protected virtual MemberBinding VisitBinding(MemberBinding binding, T context)
		{
			switch (binding.BindingType)
			{
				case MemberBindingType.Assignment:
				{
					return this.VisitMemberAssignment((MemberAssignment)binding, context);
				}
				case MemberBindingType.MemberBinding:
				{
					return this.VisitMemberMemberBinding((MemberMemberBinding)binding, context);
				}
				case MemberBindingType.ListBinding:
				{
					return this.VisitMemberListBinding((MemberListBinding)binding, context);
				}
				default:
				{
					throw new NotSupportedException(String.Format(
						ExpressionVisitor<T>.ErrorUnexpectedMemberBinding,
						binding.BindingType));
				}
			}
		}

		protected virtual ElementInit VisitElementInitializer(ElementInit initializer, T context)
		{
			IEnumerable<Expression> arguments = this.VisitExpressionList(initializer.Arguments, context);
			if (arguments == initializer.Arguments)
			{
				// no change
				return initializer;
			}

			return Expression.ElementInit(initializer.AddMethod, arguments);
		}

		protected virtual Expression VisitUnary(UnaryExpression unary, T context)
		{
			Expression operand = this.Visit(unary.Operand, context);
			if (operand == unary.Operand)
			{
				// no change
				return unary;
			}

			return Expression.MakeUnary(unary.NodeType, operand, operand.Type, unary.Method);
		}

		protected virtual Expression VisitBinary(BinaryExpression binary, T context)
		{
			Expression left = this.Visit(binary.Left, context);
			Expression right = this.Visit(binary.Right, context);
			Expression conversion = this.Visit(binary.Conversion, context);

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

		protected virtual Expression VisitTypeIs(TypeBinaryExpression binary, T context)
		{
			Expression expr = this.Visit(binary.Expression, context);
			if (expr == binary.Expression)
			{
				// no change
				return binary;
			}

			return Expression.TypeIs(expr, binary.TypeOperand);
		}

		protected virtual Expression VisitConstant(ConstantExpression constant, T context)
		{
			// no change
			return constant;
		}

		protected virtual Expression VisitConditional(ConditionalExpression conditional, T context)
		{
			Expression test = this.Visit(conditional.Test, context);
			Expression ifTrue = this.Visit(conditional.IfTrue, context);
			Expression ifFalse = this.Visit(conditional.IfFalse, context);

			if (test == conditional.Test &&
				ifTrue == conditional.IfTrue &&
				ifFalse == conditional.IfFalse)
			{
				// no change
				return conditional;
			}

			return Expression.Condition(test, ifTrue, ifFalse);
		}

		protected virtual ParameterExpression VisitParameter(ParameterExpression p, T context)
		{
			// no change
			return p;
		}

		protected virtual IEnumerable<ParameterExpression> VisitParameterList(IList<ParameterExpression> original, T context)
		{
			List<ParameterExpression> list = null;

			for (int i=0, length=original.Count; i<length; i++)
			{
				ParameterExpression exp = this.VisitParameter(original[i], context);
				if (list != null)
				{
					list.Add(exp);
				}
				else if (exp != original[i])
				{
					list = new List<ParameterExpression>(length);
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

		protected virtual Expression VisitMemberAccess(MemberExpression m, T context)
		{
			Expression exp = this.Visit(m.Expression, context);

			if (exp == m.Expression)
			{
				// no change
				return m;
			}

			return Expression.MakeMemberAccess(exp, m.Member);
		}

		protected virtual Expression VisitMethodCall(MethodCallExpression m, T context)
		{
			Expression obj = this.Visit(m.Object, context);
			IEnumerable<Expression> args = this.VisitExpressionList(m.Arguments, context);

			if (obj == m.Object && args == m.Arguments)
			{
				// no change
				return m;
			}

			return Expression.Call(obj, m.Method, args);
		}

		protected virtual IEnumerable<Expression> VisitExpressionList(IList<Expression> original, T context)
		{
			List<Expression> list = null;

			for (int i=0, length=original.Count; i<length; i++)
			{
				Expression exp = this.Visit(original[i], context);
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

		protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment, T context)
		{
			Expression exp = this.Visit(assignment.Expression, context);

			if (exp == assignment.Expression)
			{
				// no change
				return assignment;
			}

			return Expression.Bind(assignment.Member, exp);
		}

		protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding, T context)
		{
			IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings, context);

			if (bindings == binding.Bindings)
			{
				// no change
				return binding;
			}

			return Expression.MemberBind(binding.Member, bindings);
		}

		protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding, T context)
		{
			IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers, context);

			if (initializers == binding.Initializers)
			{
				// no change
				return binding;
			}

			return Expression.ListBind(binding.Member, initializers);
		}

		protected virtual IEnumerable<MemberBinding> VisitBindingList(IList<MemberBinding> original, T context)
		{
			List<MemberBinding> list = null;

			for (int i=0, length=original.Count; i<length; i++)
			{
				MemberBinding b = this.VisitBinding(original[i], context);
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

		protected virtual IEnumerable<ElementInit> VisitElementInitializerList(IList<ElementInit> original, T context)
		{
			List<ElementInit> list = null;

			for (int i=0, length=original.Count; i<length; i++)
			{
				ElementInit init = this.VisitElementInitializer(original[i], context);
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

		protected virtual Expression VisitLambda(LambdaExpression lambda, T context)
		{
			Expression body = this.Visit(lambda.Body, context);
			if (body == lambda.Body)
			{
				// no change
				return lambda;
			}

			return Expression.Lambda(lambda.Type, body, lambda.Parameters);
		}

		protected virtual NewExpression VisitNew(NewExpression ctor, T context)
		{
			IEnumerable<Expression> args = this.VisitExpressionList(ctor.Arguments, context);
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

		protected virtual Expression VisitMemberInit(MemberInitExpression init, T context)
		{
			NewExpression ctor = this.VisitNew(init.NewExpression, context);
			IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings, context);

			if (ctor == init.NewExpression && bindings == init.Bindings)
			{
				// no change
				return init;
			}

			return Expression.MemberInit(ctor, bindings);
		}

		protected virtual Expression VisitListInit(ListInitExpression init, T context)
		{
			NewExpression ctor = this.VisitNew(init.NewExpression, context);
			IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers, context);

			if (ctor == init.NewExpression && initializers == init.Initializers)
			{
				// no change
				return init;
			}

			return Expression.ListInit(ctor, initializers);
		}

		protected virtual Expression VisitNewArray(NewArrayExpression ctor, T context)
		{
			IEnumerable<Expression> exprs = this.VisitExpressionList(ctor.Expressions, context);
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

		protected virtual Expression VisitInvocation(InvocationExpression invoke, T context)
		{
			IEnumerable<Expression> args = this.VisitExpressionList(invoke.Arguments, context);
			Expression expr = this.Visit(invoke.Expression, context);

			if (args == invoke.Arguments && expr == invoke.Expression)
			{
				// no change
				return invoke;
			}

			return Expression.Invoke(expr, args);
		}

#if NET40
		protected virtual Expression VisitBlock(BlockExpression block, T context)
		{
			// TODO: implement visitor method
			return block;
		}

		protected virtual Expression VisitDynamic(DynamicExpression dyn, T context)
		{
			// TODO: implement visitor method
			return dyn;
		}

		protected virtual Expression VisitDefault(DefaultExpression exp, T context)
		{
			// TODO: implement visitor method
			return exp;
		}
#endif

		#endregion Visit Methods

		#region Utility Methods

		protected static LambdaExpression GetLambda(Expression expression)
		{
			while (expression.NodeType == ExpressionType.Quote)
			{
				expression = ((UnaryExpression)expression).Operand;
			}

			if (expression.NodeType == ExpressionType.Constant)
			{
				return ((ConstantExpression)expression).Value as LambdaExpression;
			}

			return expression as LambdaExpression;
		}

		#endregion Utility Methods
	}
}
