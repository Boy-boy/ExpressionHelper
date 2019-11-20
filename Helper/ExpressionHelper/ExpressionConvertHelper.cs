using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace ExpressionHelper
{
    /// <summary>
    /// 简单类型转换帮助类
    /// </summary>
    public class ExpressionConvertHelper
    {
        private static readonly Dictionary<string, object> Dictionary = new Dictionary<string, object>();
        private static readonly TimerCallback TimerCallback = s => ((ExpressionConvertHelper)s).Timer_Tick();
        private readonly object _lock = new object();
        private Timer _timer;

        public ExpressionConvertHelper(TimeSpan lifetime)
        {
            if (Lifetime == Timeout.InfiniteTimeSpan)
                Lifetime = TimeSpan.FromDays(1);
            Lifetime = lifetime;
        }
        public TimeSpan Lifetime { get; }

        public TOut ExpressionConvert<TIn, TOut>(TIn tIn)
        {
            var key = $@"funcKey_{typeof(TIn).FullName}_{typeof(TOut).FullName}";
            var memberBindingList = new List<MemberBinding>();
            if (Dictionary.ContainsKey(key)) return ((Func<TIn, TOut>)Dictionary[key]).Invoke(tIn);

            var parameterExpression = Expression.Parameter(typeof(TIn), "p");
            foreach (var item in typeof(TOut).GetProperties())
            {
                var propertyInfo = typeof(TIn).GetProperty(item.Name);
                if (propertyInfo == null) break;
                if (propertyInfo.PropertyType != item.PropertyType)
                {
                    throw new Exception($"{typeof(TIn).FullName} convert To {typeof(TOut).FullName} fail,property is {item.Name}");
                }
                var property =
                    Expression.Property(parameterExpression, propertyInfo);
                var menBinding = Expression.Bind(item, property);
                memberBindingList.Add(menBinding);
            }
            foreach (var item in typeof(TOut).GetFields())
            {
                var fieldInfo = typeof(TIn).GetField(item.Name);
                if (fieldInfo == null) break;
                if (fieldInfo.FieldType != item.FieldType)
                {
                    throw new Exception($"{typeof(TIn).FullName} convert To {typeof(TOut).FullName} fail,field is {item.Name}");
                }
                var field =
                    Expression.Field(parameterExpression, fieldInfo);
                var menBinding = Expression.Bind(item, field);
                memberBindingList.Add(menBinding);
            }
            var memberInitExpression =
                Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList);
            var lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, parameterExpression);
            var func = lambda.Compile();
            Dictionary[key] = func;
            _timer = new Timer(TimerCallback, this, Lifetime, Timeout.InfiniteTimeSpan);
            return ((Func<TIn, TOut>)Dictionary[key]).Invoke(tIn);
        }
        private void Timer_Tick()
        {
            lock (_lock)
            {
                if (_timer == null) return;
                _timer.Dispose();
                _timer = null;
                Dictionary.Clear();
            }
        }
    }
}
