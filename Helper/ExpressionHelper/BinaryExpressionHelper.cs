using System;
using System.Linq.Expressions;

namespace ExpressionHelper
{
    public static class BinaryExpressionHelper
    {
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expression1,
            Expression<Func<T, bool>> expression2)
        {
            var invokedExpression = Expression.Invoke(expression2, expression1.Parameters);

            return Expression.Lambda<Func<T, bool>>(Expression.Or(expression1.Body, invokedExpression),
                expression1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expression1,
            Expression<Func<T, bool>> expression2)
        {
            var invokedExpression = Expression.Invoke(expression2, expression1.Parameters);

            return Expression.Lambda<Func<T, bool>>(Expression.And(expression1.Body,
                invokedExpression), expression1.Parameters);
        }
    }
}
