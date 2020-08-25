using Moq;
using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace tests
{
    public class BaseTest
    {
        protected static void ConstructorMustThrowArgumentNullException(Type type)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));

            foreach (var constructor in type.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                var mocks = parameters.Select(p =>
                {
                    var mockType = typeof(Mock<>).MakeGenericType(p.ParameterType);
                    return (Mock)Activator.CreateInstance(mockType);
                }).ToArray();

                for (var index = 0; index < parameters.Length; index++)
                {
                    var mocksCopy = mocks.Select(m => m.Object).ToArray();
                    mocksCopy[index] = null;

                    var message = parameters[index].Name;
                    try
                    {
                        Assert.Throws<ArgumentNullException>(() =>
                        {
                            constructor.Invoke(mocksCopy);
                        });
                    }
                    catch (TargetInvocationException targetInvocationException)
                    {
                        targetInvocationException.InnerException.Should().BeOfType<ArgumentNullException>();
                        targetInvocationException.InnerException.Message.Should().Contain(message);
                    }
                }
            }
        }
    }
}
