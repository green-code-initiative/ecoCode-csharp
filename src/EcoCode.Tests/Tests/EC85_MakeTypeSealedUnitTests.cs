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
        public class [|TestA|];
        internal class [|TestB|];
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
        """, """
        public sealed class TestA;
        internal sealed class TestB;
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
    public async Task SealableRecordsAsync() => await VerifyAsync("""
        public record [|TestA|];
        internal record [|TestB|];
        public static class Test0
        {
            public record [|Test01|];
            internal record [|Test02|];
            private record [|Test03|];
        }
        internal static class Test1
        {
            public record [|Test11|];
            internal record [|Test12|];
            private record [|Test13|];
        }
        """, """
        public sealed record TestA;
        internal sealed record TestB;
        public static class Test0
        {
            public sealed record Test01;
            internal sealed record Test02;
            private sealed record Test03;
        }
        internal static class Test1
        {
            public sealed record Test11;
            internal sealed record Test12;
            private sealed record Test13;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task NonSealableStructsAsync() => await VerifyAsync("""
        public struct TestA;
        internal struct TestB;
        public static class Test0
        {
            public struct Test01;
            internal struct Test02;
            private struct Test03;
        }
        internal static class Test1
        {
            public struct Test11;
            internal struct Test12;
            private struct Test13;
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
        public abstract class TestA;
        internal abstract class TestB;
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
        public sealed class TestA;
        internal sealed class TestB;
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
        public class [|TestA|] : ITest { public void Method() { } }
        internal class [|TestB|] : ITest { public void Method() { } }
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
        """, """
        public interface ITest { void Method(); }
        public sealed class TestA : ITest { public void Method() { } }
        internal sealed class TestB : ITest { public void Method() { } }
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
        public class TestA1 { public virtual void Method() { } };
        public class [|TestA2|] { internal virtual void Method() { } };
        public class TestA3 { protected virtual void Method() { } };
        public class [|TestA4|] { protected internal virtual void Method() { } };
        public class [|TestA5|] { private protected virtual void Method() { } };

        internal class [|TestB1|] { public virtual void Method() { } };
        internal class [|TestB2|] { internal virtual void Method() { } };
        internal class [|TestB3|] { protected virtual void Method() { } };
        internal class [|TestB4|] { protected internal virtual void Method() { } };
        internal class [|TestB5|] { private protected virtual void Method() { } };

        public static class Test0
        {
            public class TestA1 { public virtual void Method() { } };
            public class [|TestA2|] { internal virtual void Method() { } };
            public class TestA3 { protected virtual void Method() { } };
            public class [|TestA4|] { protected internal virtual void Method() { } };
            public class [|TestA5|] { private protected virtual void Method() { } };
        
            internal class [|TestB1|] { public virtual void Method() { } };
            internal class [|TestB2|] { internal virtual void Method() { } };
            internal class [|TestB3|] { protected virtual void Method() { } };
            internal class [|TestB4|] { protected internal virtual void Method() { } };
            internal class [|TestB5|] { private protected virtual void Method() { } };

            private class [|TestC1|] { public virtual void Method() { } };
            private class [|TestC2|] { internal virtual void Method() { } };
            private class [|TestC3|] { protected virtual void Method() { } };
            private class [|TestC4|] { protected internal virtual void Method() { } };
            private class [|TestC5|] { private protected virtual void Method() { } };
        }

        internal static class Test1
        {
            public class [|TestA1|] { public virtual void Method() { } };
            public class [|TestA2|] { internal virtual void Method() { } };
            public class [|TestA3|] { protected virtual void Method() { } };
            public class [|TestA4|] { protected internal virtual void Method() { } };
            public class [|TestA5|] { private protected virtual void Method() { } };
        
            internal class [|TestB1|] { public virtual void Method() { } };
            internal class [|TestB2|] { internal virtual void Method() { } };
            internal class [|TestB3|] { protected virtual void Method() { } };
            internal class [|TestB4|] { protected internal virtual void Method() { } };
            internal class [|TestB5|] { private protected virtual void Method() { } };
        
            private class [|TestC1|] { public virtual void Method() { } };
            private class [|TestC2|] { internal virtual void Method() { } };
            private class [|TestC3|] { protected virtual void Method() { } };
            private class [|TestC4|] { protected internal virtual void Method() { } };
            private class [|TestC5|] { private protected virtual void Method() { } };
        }
        """, """
        public class TestA1 { public virtual void Method() { } };
        public sealed class TestA2 { internal void Method() { } };
        public class TestA3 { protected virtual void Method() { } };
        public sealed class TestA4 { internal void Method() { } };
        public sealed class TestA5 { private void Method() { } };

        internal sealed class TestB1 { public void Method() { } };
        internal sealed class TestB2 { internal void Method() { } };
        internal sealed class TestB3 { private void Method() { } };
        internal sealed class TestB4 { internal void Method() { } };
        internal sealed class TestB5 { private void Method() { } };

        public static class Test0
        {
            public class TestA1 { public virtual void Method() { } };
            public sealed class TestA2 { internal void Method() { } };
            public class TestA3 { protected virtual void Method() { } };
            public sealed class TestA4 { internal void Method() { } };
            public sealed class TestA5 { private void Method() { } };
        
            internal sealed class TestB1 { public void Method() { } };
            internal sealed class TestB2 { internal void Method() { } };
            internal sealed class TestB3 { private void Method() { } };
            internal sealed class TestB4 { internal void Method() { } };
            internal sealed class TestB5 { private void Method() { } };

            private sealed class TestC1 { public void Method() { } };
            private sealed class TestC2 { internal void Method() { } };
            private sealed class TestC3 { private void Method() { } };
            private sealed class TestC4 { internal void Method() { } };
            private sealed class TestC5 { private void Method() { } };
        }

        internal static class Test1
        {
            public sealed class TestA1 { public void Method() { } };
            public sealed class TestA2 { internal void Method() { } };
            public sealed class TestA3 { private void Method() { } };
            public sealed class TestA4 { internal void Method() { } };
            public sealed class TestA5 { private void Method() { } };
        
            internal sealed class TestB1 { public void Method() { } };
            internal sealed class TestB2 { internal void Method() { } };
            internal sealed class TestB3 { private void Method() { } };
            internal sealed class TestB4 { internal void Method() { } };
            internal sealed class TestB5 { private void Method() { } };
        
            private sealed class TestC1 { public void Method() { } };
            private sealed class TestC2 { internal void Method() { } };
            private sealed class TestC3 { private void Method() { } };
            private sealed class TestC4 { internal void Method() { } };
            private sealed class TestC5 { private void Method() { } };
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
        public class [|Test9|] : Test8;
        """, """
        public abstract class Test2 { public virtual void Overridable() { } }
        public class Test3 : Test2;
        public sealed class Test4 : Test3;
        public class Test5 : Test3;
        public class Test6 : Test3 { public override void Overridable() { } }
        public sealed class Test7 : Test3 { public override void Overridable() { } }
        public class Test8 : Test3 { public sealed override void Overridable() { } }
        public sealed class Test9 : Test8;
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task PartialAsync() => await VerifyAsync("""
        public partial class [|Test1|];
        partial class Test1 { public void Method() { } }

        partial class Test2 { public void Method() { } }
        public partial class [|Test2|];

        public partial class Test3;
        partial class Test3 { public virtual void Method() { } }

        public partial class Test4;
        sealed partial class Test4 { public void Method() { } }
        """, """
        public sealed partial class Test1;
        partial class Test1 { public void Method() { } }

        partial class Test2 { public void Method() { } }
        public sealed partial class Test2;

        public partial class Test3;
        partial class Test3 { public virtual void Method() { } }

        public partial class Test4;
        sealed partial class Test4 { public void Method() { } }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task Partial2Async() => await VerifyAsync("""
        partial class Test1 { public void Method() { } }
        public partial class [|Test1|];
        """, """
        partial class Test1 { public void Method() { } }
        public sealed partial class Test1;
        """).ConfigureAwait(false);
}
