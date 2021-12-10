﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider
{
    public class ExpressionToFtsRequestTranslator : ExpressionVisitor
    {
        readonly StringBuilder _resultStringBuilder;

        public ExpressionToFtsRequestTranslator()
        {
            _resultStringBuilder = new StringBuilder();
        }

        public string Translate(Expression exp)
        {
            Visit(exp);

            return _resultStringBuilder.ToString();
        }

        #region protected methods

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    Expression member;
                    Expression constant;

                    if(node.Left.NodeType == ExpressionType.Constant && node.Right.NodeType == ExpressionType.MemberAccess)
                    {
                        member = node.Right;
                        constant = node.Left;
                    }
                    else if(node.Right.NodeType == ExpressionType.Constant && node.Left.NodeType == ExpressionType.MemberAccess)
                    {
                        member = node.Left;
                        constant = node.Right;
                    }
                    else
                    {
                        throw new NotSupportedException($"The expression should consist of a Constant and a Property/Field: {node.NodeType}");
                    }
                    
                    Visit(member);
                    _resultStringBuilder.Append("(");
                    Visit(constant);
                    _resultStringBuilder.Append(")");
                    break;

                default:
                    throw new NotSupportedException($"Operation '{node.NodeType}' is not supported");
            };

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _resultStringBuilder.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _resultStringBuilder.Append(node.Value);

            return node;
        }

        #endregion
    }
}
