using System;
using Reusable.Data;
using Xunit;

namespace Reusable.Quickey
{
    public class PlainSelectorFormatterTest
    {
        [Fact]
        public void Throws_when_attributes_missing()
        {
            var ex = Assert.Throws<ArgumentException>(() => From<NoAttributes>.Select(x => x.P1));
            //Assert.Equal("'x => x.Text' does not specify any selectors.", ex.Message);
        }

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
            Assert.Equal("Reusable.Quickey+NamespaceTypeMemberPlain.P1", selector);
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
            var selector = From<MemberPlain>.Select(x => x.P1).Index("test").ToString();
            Assert.Equal("P1[test]", selector);
        }

        private class NoAttributes
        {
            public string P1 { get; set; }
        }

        [UseMember]
        [PlainSelectorFormatter]
        private class MemberPlain
        {
            public string P1 { get; set; }
        }

        [UseType, UseMember]
        [PlainSelectorFormatter]
        private class TypeMemberPlain
        {
            public string P1 { get; set; }

            [UseMember]
            public string P2 { get; set; }
        }

        [UseType, UseMember]
        [Rename("TypeMemberPlain")]
        [PlainSelectorFormatter]
        private class TypeMemberPlainRename
        {
            [Rename("P2")]
            public string P1 { get; set; }
        }

        [UseNamespace, UseType, UseMember]
        [PlainSelectorFormatter]
        private class NamespaceTypeMemberPlain
        {
            public string P1 { get; set; }
        }

        [UseScheme("test"), UseType, UseMember]
        [PlainSelectorFormatter]
        private class SchemeTypeMemberPlain
        {
            public string P1 { get; set; }
        }
    }
}