using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Reusable.Quickey
{
    public class PlainSelectorFormatterTest
    {
        // [Fact]
        // public void Throws_when_attributes_missing()
        // {
        //     var ex = Assert.Throws<InvalidOperationException>(() => From<NoAttributes>.Select(x => x.P1));
        //     //Assert.Equal("'x => x.Text' does not specify any selectors.", ex.Message);
        // }

        [Fact]
        public void Can_format_member_plain()
        {
            var selector = From<MemberPlain>.Select(x => x.P1).ToString();
            Assert.Equal("P1", selector);
        }

        [Fact]
        public void Can_format_type_member_plain()
        {
            var selector = From<TypeMemberPlain>.Select(x => x.P1).ToString();
            Assert.Equal("TypeMemberPlain.P1", selector);
        }

        [Fact]
        public void Member_can_override_type_attributes()
        {
            var selector = From<TypeMemberPlain>.Select(x => x.P2).ToString();
            Assert.Equal("P2", selector);
        }

        [Fact]
        public void Can_format_namespace_type_member_plain()
        {
            var selector = From<NamespaceTypeMemberPlain>.Select(x => x.P1).ToString();
            Assert.Equal("Reusable.ReMember+NamespaceTypeMemberPlain.P1", selector);
        }

        [Fact]
        public void Can_rename_type_or_member_plain()
        {
            var selector = From<TypeMemberPlainRename>.Select(x => x.P1).ToString();
            Assert.Equal("TypeMemberPlain.P2", selector);
        }

        [Fact]
        public void Can_format_scheme_type_member_plain()
        {
            var selector = From<SchemeTypeMemberPlain>.Select(x => x.P1).ToString();
            Assert.Equal("test:SchemeTypeMemberPlain.P1", selector);
        }

        [Fact]
        public void Can_format_member_index_plain()
        {
            var selector = From<MemberPlain>.Select(x => x.P1, new UseIndexAttribute.Parameter { Index = "test" }).ToString();
            Assert.Equal("P1[test]", selector);
        }

        [Fact]
        public void Can_format_type_member_by_base()
        {
            var selectorP1 = From<DerivedTypeMemberPlain>.Select(x => x.P1);
            Assert.Equal("DerivedTypeMemberPlain.P1", selectorP1.ToString());

            var selectorP2 = From<DerivedTypeMemberPlain>.Select(x => x.P2);
            Assert.Equal("DerivedTypeMemberPlain.P2", selectorP2.ToString());
        }

        [Fact]
        public void Can_override_base_format()
        {
            var selectorP1 = From<DerivedMemberPlain>.Select(x => x.P1);
            Assert.Equal("P1", selectorP1.ToString());

            var selectorP2 = From<DerivedMemberPlain>.Select(x => x.P2);
            Assert.Equal("P2", selectorP2.ToString());
        }
        
        [Fact]
        public void Can_ignore_member()
        {
            var selectorP1 = From<Parent.Child>.Select(x => x.P1);
            Assert.Equal("Parent.P1", selectorP1.ToString());
        }

        // todo - add tests with interfaces

        private class NoAttributes
        {
            public string P1 { get; set; }
        }

        [UseMember, UseIndex]
        [JoinSelectorTokens]
        private class MemberPlain
        {
            public string P1 { get; set; }
        }

        [UseType, UseMember]
        [JoinSelectorTokens]
        private class TypeMemberPlain
        {
            public string P1 { get; set; }

            [UseMember]
            public string P2 { get; set; }
        }

        [UseType("TypeMemberPlain"), UseMember]
        [JoinSelectorTokens]
        private class TypeMemberPlainRename
        {
            [Replace("1", "2")]
            public string P1 { get; set; }
        }

        [UseNamespace, UseType, UseMember]
        [JoinSelectorTokens]
        private class NamespaceTypeMemberPlain
        {
            public string P1 { get; set; }
        }

        [UseScheme("test"), UseType, UseMember]
        [JoinSelectorTokens]
        private class SchemeTypeMemberPlain
        {
            public string P1 { get; set; }
        }

        [UseType, UseMember]
        [JoinSelectorTokens]
        private class BaseTypeMemberPlain
        {
            public string P1 { get; set; }
        }

        private class DerivedTypeMemberPlain : BaseTypeMemberPlain
        {
            public string P2 { get; set; }
        }

        [UseMember]
        private class DerivedMemberPlain : BaseTypeMemberPlain
        {
            public string P2 { get; set; }
        }

        [UseType, UseMember]
        private class Parent
        {
            [Selector(Ignore = true)]
            public class Child
            {
                public string P1 { get; set; }
            }
        }
    }
}