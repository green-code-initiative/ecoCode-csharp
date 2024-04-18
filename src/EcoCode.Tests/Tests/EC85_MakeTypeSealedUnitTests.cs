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
    public async Task DontMakePublicClassSealedAsync() => await VerifyAsync("""
        public static class Test1;
        public abstract class Test2;
        public class Test3 : Test2;
        public class Test4 : Test3;
        public sealed class Test5 : Test4;
        
        public static class Container1
        {
            public static class Test1;
            public abstract class Test2;
            public class Test3 : Test2;
            public class Test4 : Test3;
            public sealed class Test5 : Test4;
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
    public async Task MakeRecordSealedAsync() => await VerifyAsync(
        "public record [|Test|];",
        fixedSource: "public sealed record Test;")
        .ConfigureAwait(false);
}
