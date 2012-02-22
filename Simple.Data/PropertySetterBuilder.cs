namespace Simple.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    class PropertySetterBuilder
    {
        private static readonly MethodInfo DictionaryContainsKeyMethod = typeof(IDictionary<string, object>).GetMethod("ContainsKey", new[] { typeof(string) });
        private static readonly PropertyInfo DictionaryIndexerProperty = typeof(IDictionary<string, object>).GetProperty("Item");

        private static readonly MethodInfo ToArrayMethod = typeof(Enumerable).GetMethod("ToArray",
                                                                                        BindingFlags.Public |
                                                                                        BindingFlags.Static).MakeGenericMethod(typeof(IDictionary<string, object>));

        private static readonly PropertyInfo ArrayLengthProperty =
            typeof(IDictionary<string, object>[]).GetProperty("Length");

        private readonly ParameterExpression _param;
        private readonly ParameterExpression _obj;
        private readonly PropertyInfo _property;
        private MemberExpression _nameProperty;
        private IndexExpression _itemProperty;
        private MethodCallExpression _containsKey;
        private static readonly MethodInfo CreatorCreateMethod = typeof(ConcreteTypeCreator).GetMethod("Create");

        public PropertySetterBuilder(ParameterExpression param, ParameterExpression obj, PropertyInfo property)
        {
            _param = param;
            _obj = obj;
            _property = property;
        }

        public ConditionalExpression CreatePropertySetter()
        {
            CreatePropertyExpressions();

            if (PropertyIsPrimitive())
            {
                return Expression.IfThen(_containsKey, CreateTrySimpleAssign());
            }

            if (_property.PropertyType.IsGenericCollection())
            {
                var collectionCreator = BuildCollectionCreator();
                if (collectionCreator != null)
                {
                    return Expression.IfThen(_containsKey, collectionCreator);
                }
            }

            var isDictionary = Expression.TypeIs(_itemProperty, typeof(IDictionary<string, object>));

            var tryComplexAssign = Expression.TryCatch(CreateComplexAssign(),
                                                       CreateCatchBlock());

            var ifThen = Expression.IfThen(_containsKey, // if (dict.ContainsKey(propertyName)) {
                                           Expression.IfThenElse(isDictionary, tryComplexAssign, CreateTrySimpleAssign()));

            return ifThen;
        }

        private Expression BuildCollectionCreator()
        {
            var genericType = _property.PropertyType.GetGenericArguments().Single();
            var creatorInstance = ConcreteTypeCreator.Get(genericType);
            var collection = Expression.Variable(_property.PropertyType);
            BinaryExpression createCollection = null;
            if (_property.CanWrite)
            {
                if (_property.PropertyType.IsInterface)
                {
                    createCollection = Expression.Assign(collection,
                                                         Expression.Call(
                                                             typeof (PropertySetterBuilder).GetMethod("CreateList",
                                                                                                      BindingFlags.
                                                                                                          NonPublic |
                                                                                                      BindingFlags.
                                                                                                          Static).
                                                                 MakeGenericMethod(genericType)));
                }
                else
                {
                    var defaultConstructor = _property.PropertyType.GetConstructor(Type.EmptyTypes);
                    if (defaultConstructor != null)
                    {
                        createCollection = Expression.Assign(collection, Expression.New(defaultConstructor));
                    }
                }
            }
            else
            {
                createCollection = Expression.Assign(collection, _nameProperty);
            }
            var addMethod = _property.PropertyType.GetMethod("Add");

            if (createCollection != null && addMethod != null)
            {
                var creator = Expression.Constant(creatorInstance);
                var array = Expression.Variable(typeof(IDictionary<string, object>[]));
                var i = Expression.Variable(typeof(int));
                var current = Expression.Variable(typeof(IDictionary<string, object>));


                var isDictionaryCollection = Expression.TypeIs(_itemProperty,
                                                               typeof(IEnumerable<IDictionary<string, object>>));

                var toArray = Expression.Assign(array, Expression.Call(ToArrayMethod, Expression.Convert(_itemProperty, typeof(IEnumerable<IDictionary<string, object>>))));
                var start = Expression.Assign(i, Expression.Constant(0));
                var label = Expression.Label();
                var loop = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(i, Expression.Property(array, ArrayLengthProperty)),
                        Expression.Block(
                            Expression.Assign(current, Expression.ArrayIndex(array, i)),
                            Expression.Call(collection, addMethod,
                                            Expression.Convert(Expression.Call(creator, CreatorCreateMethod, current), genericType)),
                            Expression.PreIncrementAssign(i)
                            ),
                        Expression.Break(label)
                        ),
                    label
                    );

                var block = Expression.Block(
                    new[] { array, i, collection, current },
                    createCollection,
                    toArray,
                    start,
                    loop,
                    _property.CanWrite ? (Expression)Expression.Assign(_nameProperty, collection) : Expression.Empty());

                return Expression.IfThen(isDictionaryCollection, block);
            }
            return null;
        }

        private bool PropertyIsPrimitive()
        {
            return _property.PropertyType.IsPrimitive || _property.PropertyType == typeof(string) ||
                   _property.PropertyType == typeof(DateTime) || _property.PropertyType == typeof(byte[]) ||
                   _property.PropertyType.IsEnum ||
                   (_property.PropertyType.IsGenericType && _property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        private void CreatePropertyExpressions()
        {
            var name = Expression.Constant(_property.Name, typeof(string));
            _containsKey = Expression.Call(_param, DictionaryContainsKeyMethod, name);
            _nameProperty = Expression.Property(_obj, _property);
            _itemProperty = Expression.Property(_param, DictionaryIndexerProperty, name);
        }

        private CatchBlock CreateCatchBlock()
        {
            return Expression.Catch(typeof(Exception), Expression.Assign(_nameProperty,
                                                                         Expression.Default(_property.PropertyType)));
        }

        private BinaryExpression CreateComplexAssign()
        {
            var creator = Expression.Constant(ConcreteTypeCreator.Get(_property.PropertyType));
            var methodCallExpression = Expression.Call(creator, CreatorCreateMethod,
                                                       Expression.Convert(_itemProperty,
                                                                          typeof(IDictionary<string, object>)));

            var complexAssign = Expression.Assign(_nameProperty,
                                                  Expression.Convert(
                                                      methodCallExpression, _property.PropertyType));
            return complexAssign;
        }

        private TryExpression CreateTrySimpleAssign()
        {
            Expression assign;
            var changeTypeMethod = typeof (PropertySetterBuilder).GetMethod("SafeConvert",
                                                                            BindingFlags.Static | BindingFlags.NonPublic);

            MethodCallExpression callConvert;
            if (_property.PropertyType.IsEnum)
            {
                callConvert = Expression.Call(changeTypeMethod, _itemProperty,
                                              Expression.Constant(_property.PropertyType.GetEnumUnderlyingType()));
            }
            else if (_property.PropertyType.IsGenericType && _property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                callConvert = Expression.Call(changeTypeMethod, _itemProperty,
                                              Expression.Constant(_property.PropertyType.GetGenericArguments().Single()));
            }
            else
            {
                callConvert = Expression.Call(changeTypeMethod, _itemProperty,
                                              Expression.Constant(_property.PropertyType));
            }

            assign = Expression.Assign(_nameProperty, Expression.Convert(callConvert, _property.PropertyType));
            if (_property.PropertyType.IsEnum)
            {
                return Expression.TryCatch( // try {
                    Expression.IfThenElse(Expression.TypeIs(_itemProperty, typeof (string)),
                                          Expression.Assign(_nameProperty,
                                                            Expression.Convert(Expression.Call(typeof (Enum).GetMethod("Parse", new[] {typeof(Type), typeof(string), typeof(bool)}),
                                                                                               Expression.Constant(_property.PropertyType),
                                                                                               Expression.Call(_itemProperty, typeof(object).GetMethod("ToString")), Expression.Constant(true)), _property.PropertyType)),
                                          assign), Expression.Catch(typeof(Exception), Expression.Empty()));
            }
            return Expression.TryCatch( // try {
                assign, 
                CreateCatchBlock());
        }

        private static object SafeConvert(object source, Type targetType)
        {
            return ReferenceEquals(source, null) ? null : Convert.ChangeType(source, targetType);
        }

        private static List<T> CreateList<T>()
        {
            return new List<T>();
        }
    }
}