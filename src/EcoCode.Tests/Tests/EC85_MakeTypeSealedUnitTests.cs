namespace EcoCode.Tests;

[TestClass]
public sealed class MakeTypeSealed
{
    private static readonly VerifyDlg VerifyAsync = CodeFixVerifier.VerifyAsync<
        MakeTypeSealedAnalyzer,
        MakeTypeSealedCodeFixProvider>;

    [TestMethod]
    public async Task EmptyCodeAsync() => await VerifyAsync("").ConfigureAwait(false);

    [TestMethod]
    public async Task SealableClassesAsync() => await VerifyAsync("""
        public class [|Test|];
        public static class Test0
        {
            public class [|Test01|];
            internal class [|Test02|];
            private class [|Test03|];
        }
        internal static class Test1
        {
            public class [|Test11|];
            internal class [|Test12|];
            private class [|Test13|];
        }
        """,
        fixedSource: """
        public sealed class Test;
        public static class Test0
        {
            public sealed class Test01;
            internal sealed class Test02;
            private sealed class Test03;
        }
        internal static class Test1
        {
            public sealed class Test11;
            internal sealed class Test12;
            private sealed class Test13;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task StaticClassesAsync() => await VerifyAsync("""
        public static class Test0
        {
            public static class Test01;
            internal static class Test02;
            private static class Test03;
        }
        internal static class Test1
        {
            public static class Test11;
            internal static class Test12;
            private static class Test13;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task AbstractClassesAsync() => await VerifyAsync("""
        public abstract class Test;
        public static class Test0
        {
            public abstract class Test01;
            internal abstract class Test02;
            private abstract class Test03;
        }
        internal static class Test1
        {
            public abstract class Test11;
            internal abstract class Test12;
            private abstract class Test13;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task SealedClassesAsync() => await VerifyAsync("""
        public sealed class Test;
        public static class Test0
        {
            public sealed class Test01;
            internal sealed class Test02;
            private sealed class Test03;
        }
        internal static class Test1
        {
            public sealed class Test11;
            internal sealed class Test12;
            private sealed class Test13;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task SealableClassesWithInterfaceAsync() => await VerifyAsync("""
        public interface ITest { void Method(); }
        public class [|Test|] : ITest { public void Method() { } }
        public static class Test0
        {
            public class [|Test01|] : ITest { public void Method() { } }
            internal class [|Test02|] : ITest { public void Method() { } }
            private class [|Test03|] : ITest { public void Method() { } }
        }
        internal static class Test1
        {
            public class [|Test11|] : ITest { public void Method() { } }
            internal class [|Test12|] : ITest { public void Method() { } }
            private class [|Test13|] : ITest { public void Method() { } }
        }
        """,
        fixedSource: """
        public interface ITest { void Method(); }
        public sealed class Test : ITest { public void Method() { } }
        public static class Test0
        {
            public sealed class Test01 : ITest { public void Method() { } }
            internal sealed class Test02 : ITest { public void Method() { } }
            private sealed class Test03 : ITest { public void Method() { } }
        }
        internal static class Test1
        {
            public sealed class Test11 : ITest { public void Method() { } }
            internal sealed class Test12 : ITest { public void Method() { } }
            private sealed class Test13 : ITest { public void Method() { } }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task SealableClassesWithOverridableAsync() => await VerifyAsync("""
        public class Test { public virtual void Method() { } };
        public static class Test0
        {
            public class Test01 { public virtual void Method() { } };
            internal class [|Test02|] { public virtual void Method() { } };
            private class [|Test03|] { public virtual void Method() { } };
        }
        internal static class Test1
        {
            public class [|Test11|] { public virtual void Method() { } };
            internal class [|Test12|] { public virtual void Method() { } };
            private class [|Test13|] { public virtual void Method() { } };
        }
        """,
        fixedSource: """
        public class Test { public virtual void Method() { } };
        public static class Test0
        {
            public class Test01 { public virtual void Method() { } };
            internal sealed class Test02 { public void Method() { } };
            private sealed class Test03 { public void Method() { } };
        }
        internal static class Test1
        {
            public sealed class Test11 { public void Method() { } };
            internal sealed class Test12 { public void Method() { } };
            private sealed class Test13 { public void Method() { } };
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task InheritanceAsync() => await VerifyAsync("""
        public abstract class Test2 { public virtual void Overridable() { } }
        public class Test3 : Test2;
        public sealed class Test4 : Test3;
        public class Test5 : Test3;
        public class Test6 : Test3 { public override void Overridable() { } }
        public class [|Test7|] : Test3 { public sealed override void Overridable() { } }
        public class Test8 : Test3 { public sealed override void Overridable() { } }
        public class [|Test9|] : Test8
        {
            public sealed override int GetHashCode() => 0;
            public override string ToString() => string.Empty;
        }
        """, fixedSource: """
        public abstract class Test2 { public virtual void Overridable() { } }
        public class Test3 : Test2;
        public sealed class Test4 : Test3;
        public class Test5 : Test3;
        public class Test6 : Test3 { public override void Overridable() { } }
        public sealed class Test7 : Test3 { public override void Overridable() { } }
        public class Test8 : Test3 { public sealed override void Overridable() { } }
        public sealed class Test9 : Test8
        {
            public override int GetHashCode() => 0;
            public override string ToString() => string.Empty;
        }
        """).ConfigureAwait(false);
}
