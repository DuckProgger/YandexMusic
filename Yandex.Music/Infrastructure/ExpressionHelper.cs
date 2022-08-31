using System;
using System.Linq.Expressions;

namespace Yandex.Music.Infrastructure;

internal class ExpressionHelper
{
    /// <summary>
    /// Получение имени свойства с помощью <see cref="Expression"/>.
    /// </summary>
    /// <typeparam name="TOwner">Тип, в котором содержится свойство.</typeparam>
    /// <typeparam name="TProperty">Тип свойства.</typeparam>
    /// <param name="expression"></param>
    /// <returns>Имя свойства.</returns>
    public static string GetPropertyName<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> expression) {
        var expressionBody = expression.Body;
        // Получить имя свойства
        return expressionBody switch {
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression unaryExpression => (unaryExpression.Operand as MemberExpression)?.Member.Name ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Получение имени свойства с помощью <see cref="Expression"/>.
    /// </summary>
    /// <typeparam name="TOwner">Тип, в котором содержится свойство.</typeparam>
    /// <param name="expression"></param>
    /// <returns>Имя свойства.</returns>
    public static string GetPropertyName<TOwner>(Expression<Func<TOwner, object>> expression) {
        return GetPropertyName<TOwner, object>(expression);
    }
}