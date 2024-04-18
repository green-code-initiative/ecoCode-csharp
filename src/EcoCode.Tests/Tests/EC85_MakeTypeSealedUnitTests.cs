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
    public async Task PublicClassesAsync() => await VerifyAsync("""
        public static class Test1;
        public abstract class Test2 { public virtual void Overridable() { } }
        public class Test3 : Test2;
        public class Test4 : Test3;
        public sealed class Test5 : Test4;
        public class Test6 : Test4;
        public class Test7 : Test4 { public override void Overridable() { } }
        public class [|Test8|] : Test4 { public sealed override void Overridable() { } }
        
        public static class Container1
        {
            public static class Test1;
            public abstract class Test2;
            public class Test3 : Test2;
            public class Test4 : Test3;
            public sealed class Test5 : Test4;
            public class Test6 { public virtual void Overridable() { } }

            public static class PublicContainer
            {
                public class Test { public virtual void Overridable() { } }
            }

            internal static class ProtectedContainer
            {
                public class [|Test|] { public virtual void Overridable() { } }
            }

            private static class PrivateContainer
            {
                public class [|Test|] { public virtual void Overridable() { } }
            }
        }
        
        internal static class Container2
        {
            public static class Test1;
            public abstract class Test2;
            public class Test3 : Test2;
            public class Test4 : Test3;
            public sealed class Test5 : Test4;
            public class [|Test6|] { public virtual void Overridable() { } }

            public static class PublicContainer
            {
                public class [|Test|] { public virtual void Overridable() { } }
            }
        
            internal static class ProtectedContainer
            {
                public class [|Test|] { public virtual void Overridable() { } }
            }
        
            private static class PrivateContainer
            {
                public class [|Test|] { public virtual void Overridable() { } }
            }
        }
        """,
        fixedSource: """
        public static class Test1;
        public abstract class Test2 { public virtual void Overridable() { } }
        public class Test3 : Test2;
        public class Test4 : Test3;
        public sealed class Test5 : Test4;
        public class Test6 : Test4;
        public class Test7 : Test4 { public override void Overridable() { } }
        public sealed class Test8 : Test4 { public sealed override void Overridable() { } }

        public static class Container1
        {
            public static class Test1;
            public abstract class Test2;
            public class Test3 : Test2;
            public class Test4 : Test3;
            public sealed class Test5 : Test4;
            public class Test6 { public virtual void Overridable() { } }
        
            public static class PublicContainer
            {
                public class Test { public virtual void Overridable() { } }
            }
        
            internal static class ProtectedContainer
            {
                public sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            private static class PrivateContainer
            {
                public sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        }
        
        internal static class Container2
        {
            public static class Test1;
            public abstract class Test2;
            public class Test3 : Test2;
            public class Test4 : Test3;
            public sealed class Test5 : Test4;
            public sealed class Test6 { public virtual void {|CS0549:Overridable|}() { } }
        
            public static class PublicContainer
            {
                public sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            internal static class ProtectedContainer
            {
                public sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            private static class PrivateContainer
            {
                public sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task InternalClassesAsync() => await VerifyAsync("""
        internal static class Test1;
        internal abstract class Test2;
        internal class Test3 : Test2;
        internal class Test4 : Test3;
        internal sealed class Test5 : Test4;
        internal class [|Test6|] { public virtual void Overridable() { } }

        public static class Container1
        {
            internal static class Test1;
            internal abstract class Test2;
            internal class Test3 : Test2;
            internal class Test4 : Test3;
            internal sealed class Test5 : Test4;
            internal class [|Test6|] { public virtual void Overridable() { } }

            public static class PublicContainer
            {
                internal class [|Test|] { public virtual void Overridable() { } }
            }

            internal static class ProtectedContainer
            {
                internal class [|Test|] { public virtual void Overridable() { } }
            }

            private static class PrivateContainer
            {
                internal class [|Test|] { public virtual void Overridable() { } }
            }
        }
        
        internal static class Container2
        {
            internal static class Test1;
            internal abstract class Test2;
            internal class Test3 : Test2;
            internal class Test4 : Test3;
            internal sealed class Test5 : Test4;
            internal class [|Test6|] { public virtual void Overridable() { } }

            public static class PublicContainer
            {
                internal class [|Test|] { public virtual void Overridable() { } }
            }
        
            internal static class ProtectedContainer
            {
                internal class [|Test|] { public virtual void Overridable() { } }
            }
        
            private static class PrivateContainer
            {
                internal class [|Test|] { public virtual void Overridable() { } }
            }
        }
        """,
        fixedSource: """
        internal static class Test1;
        internal abstract class Test2;
        internal class Test3 : Test2;
        internal class Test4 : Test3;
        internal sealed class Test5 : Test4;
        internal sealed class Test6 { public virtual void {|CS0549:Overridable|}() { } }
        
        public static class Container1
        {
            internal static class Test1;
            internal abstract class Test2;
            internal class Test3 : Test2;
            internal class Test4 : Test3;
            internal sealed class Test5 : Test4;
            internal sealed class Test6 { public virtual void {|CS0549:Overridable|}() { } }
        
            public static class PublicContainer
            {
                internal sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            internal static class ProtectedContainer
            {
                internal sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            private static class PrivateContainer
            {
                internal sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        }
        
        internal static class Container2
        {
            internal static class Test1;
            internal abstract class Test2;
            internal class Test3 : Test2;
            internal class Test4 : Test3;
            internal sealed class Test5 : Test4;
            internal sealed class Test6 { public virtual void {|CS0549:Overridable|}() { } }
        
            public static class PublicContainer
            {
                internal sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            internal static class ProtectedContainer
            {
                internal sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            private static class PrivateContainer
            {
                internal sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task PrivateClassesAsync() => await VerifyAsync("""
        public static class Container1
        {
            private static class Test1;
            private abstract class Test2;
            private class Test3 : Test2;
            private class Test4 : Test3;
            private sealed class Test5 : Test4;
            private class [|Test6|] { public virtual void Overridable() { } }

            public static class PublicContainer
            {
                private class [|Test|] { public virtual void Overridable() { } }
            }

            internal static class ProtectedContainer
            {
                private class [|Test|] { public virtual void Overridable() { } }
            }

            private static class PrivateContainer
            {
                private class [|Test|] { public virtual void Overridable() { } }
            }
        }
        
        internal static class Container2
        {
            private static class Test1;
            private abstract class Test2;
            private class Test3 : Test2;
            private class Test4 : Test3;
            private sealed class Test5 : Test4;
            private class [|Test6|] { public virtual void Overridable() { } }

            public static class PublicContainer
            {
                private class [|Test|] { public virtual void Overridable() { } }
            }
        
            internal static class ProtectedContainer
            {
                private class [|Test|] { public virtual void Overridable() { } }
            }
        
            private static class PrivateContainer
            {
                private class [|Test|] { public virtual void Overridable() { } }
            }
        }
        """,
        fixedSource: """
        public static class Container1
        {
            private static class Test1;
            private abstract class Test2;
            private class Test3 : Test2;
            private class Test4 : Test3;
            private sealed class Test5 : Test4;
            private sealed class Test6 { public virtual void {|CS0549:Overridable|}() { } }
        
            public static class PublicContainer
            {
                private sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            internal static class ProtectedContainer
            {
                private sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            private static class PrivateContainer
            {
                private sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        }
        
        internal static class Container2
        {
            private static class Test1;
            private abstract class Test2;
            private class Test3 : Test2;
            private class Test4 : Test3;
            private sealed class Test5 : Test4;
            private sealed class Test6 { public virtual void {|CS0549:Overridable|}() { } }
        
            public static class PublicContainer
            {
                private sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            internal static class ProtectedContainer
            {
                private sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        
            private static class PrivateContainer
            {
                private sealed class Test { public virtual void {|CS0549:Overridable|}() { } }
            }
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontMakePublicClassSealedAsync() => await VerifyAsync("""
        public static class Test1;
        public abstract class Test2;
        public class Test3 : Test2;
        public class Test4 : Test3;
        public sealed class Test5 : Test4;
        public class Test6 { public virtual void Overridable() { } }
        
        public static class Container1
        {
            public static class Test1;
            public abstract class Test2;
            public class Test3 : Test2;
            public class Test4 : Test3;
            public sealed class Test5 : Test4;
            public class Test6 { public virtual void Overridable() { } }
        }
        
        internal static class Container2
        {
            public static class Test1;
            public abstract class Test2;
            public class Test3 : Test2;
            public class Test4 : Test3;
            public sealed class Test5 : Test4;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontMakePublicRecordSealedAsync() => await VerifyAsync("""
        public abstract record Record1;
        public record Record2 : Record1;
        public record Record3 : Record2;
        public sealed record Record4 : Record3;
        
        public static class Container1
        {
            public abstract record Record1;
            public record Record2 : Record1;
            public record Record3 : Record2;
            public sealed record Record4 : Record3;
        }
        
        internal static class Container2
        {
            public abstract record Record1;
            public record Record2 : Record1;
            public record Record3 : Record2;
            public sealed record Record4 : Record3;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontMakeInternalClassSealedAsync() => await VerifyAsync("""
        internal static class Test1;
        internal abstract class Test2;
        internal class Test3 : Test2;
        internal class Test4 : Test3;
        internal sealed class Test5 : Test4;

        public static class Container1
        {
            internal static class Test1;
            internal abstract class Test2;
            internal class Test3 : Test2;
            internal class Test4 : Test3;
            internal sealed class Test5 : Test4;
        }

        internal static class Container2
        {
            internal static class Test1;
            internal abstract class Test2;
            internal class Test3 : Test2;
            internal class Test4 : Test3;
            internal sealed class Test5 : Test4;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontMakeInternalRecordSealedAsync() => await VerifyAsync("""
        internal abstract record Record1;
        internal record Record2 : Record1;
        internal record Record3 : Record2;
        internal sealed record Record4 : Record3;
        
        public static class Container1
        {
            internal abstract record Record1;
            internal record Record2 : Record1;
            internal record Record3 : Record2;
            internal sealed record Record4 : Record3;
        }
        
        internal static class Container2
        {
            internal abstract record Record1;
            internal record Record2 : Record1;
            internal record Record3 : Record2;
            internal sealed record Record4 : Record3;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontMakePrivateClassSealedAsync() => await VerifyAsync("""
        public static class Container1
        {
            private static class Test1;
            private abstract class Test2;
            private class Test3 : Test2;
            private class Test4 : Test3;
            private sealed class Test5 : Test4;
        }

        internal static class Container2
        {
            private static class Test1;
            private abstract class Test2;
            private class Test3 : Test2;
            private class Test4 : Test3;
            private sealed class Test5 : Test4;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task DontMakePrivateRecordSealedAsync() => await VerifyAsync("""
        public static class Container1
        {
            private abstract record Record1;
            private record Record2 : Record1;
            private record Record3 : Record2;
            private sealed record Record4 : Record3;
        }
        
        internal static class Container2
        {
            private abstract record Record1;
            private record Record2 : Record1;
            private record Record3 : Record2;
            private sealed record Record4 : Record3;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task MakePublicClassSealedAsync() => await VerifyAsync("""
        public class [|Test|];

        public static class Container1
        {
            public class [|Test|];
        }

        internal static class Container2
        {
            public class [|Test|];
        }
        """,
        fixedSource: """
        public sealed class Test;
        
        public static class Container1
        {
            public sealed class Test;
        }
        
        internal static class Container2
        {
            public sealed class Test;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task MakePublicRecordSealedAsync() => await VerifyAsync("""
        public record [|Test|];

        public static class Container1
        {
            public record [|Test|];
        }

        internal static class Container2
        {
            public record [|Test|];
        }
        """,
        fixedSource: """
        public sealed record Test;
        
        public static class Container1
        {
            public sealed record Test;
        }
        
        internal static class Container2
        {
            public sealed record Test;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task MakeInternalClassSealedAsync() => await VerifyAsync("""
        internal class [|Test|];

        public static class Container1
        {
            internal class [|Test|];
        }

        internal static class Container2
        {
            internal class [|Test|];
        }
        """,
        fixedSource: """
        internal sealed class Test;
        
        public static class Container1
        {
            internal sealed class Test;
        }
        
        internal static class Container2
        {
            internal sealed class Test;
        }
        """).ConfigureAwait(false);

    [TestMethod]
    public async Task MakeRecordSealedAsync() => await VerifyAsync("public record [|Test|];", fixedSource: "public sealed record Test;").ConfigureAwait(false);
}
