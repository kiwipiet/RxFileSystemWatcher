using System;
using System.Reflection;
using Fixie;
using JetBrains.Annotations;

namespace RxFileSystemWatcher.Tests
{
    public class CustomConvention : Convention
    {
        public CustomConvention()
        {
            Classes
                .NameEndsWith("Tests");
            Methods
                .Where(DoesNotHaveSetupTearDownAttributes);

            CaseExecution
                .Wrap<SetUpTearDown>();
        }

        private static bool DoesNotHaveSetupTearDownAttributes(MethodInfo methodInfo)
        {
            return !methodInfo.HasOrInherits<SetUpAttribute>() && !methodInfo.HasOrInherits<TearDownAttribute>();
        }

        [UsedImplicitly]
        private sealed class SetUpTearDown : CaseBehavior
        {
            public void Execute(Case @case, Action next)
            {
                @case.Class.InvokeAll<SetUpAttribute>(@case.Fixture.Instance);
                next();
                @case.Class.InvokeAll<TearDownAttribute>(@case.Fixture.Instance);
            }
        }
    }
}