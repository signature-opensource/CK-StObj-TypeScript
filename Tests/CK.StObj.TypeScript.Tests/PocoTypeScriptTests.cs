using CK.Core;
using CK.Setup;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.StObjEngineTestHelper;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.StObj.TypeScript.Tests
{
    [TestFixture]
    public class PocoTypeScriptTests
    {
        [ExternalName( "NotGeneratedByDefault" )]
        public interface INotGeneratedByDefault : IPoco
        {
        }

        /// <summary>
        /// IPoco are not automatically exported.
        /// Using the [TypeScript] attribute declares the type.
        /// </summary>
        [TypeScript]
        public interface IGeneratedByDefault : IPoco
        {
            INotGeneratedByDefault Some { get; set; }
        }

        [Test]
        public void no_TypeScript_attribute_provide_no_generation()
        {
            // NotGeneratedByDefault is not generated.
            var targetProjectPath = TestHelper.GetTypeScriptGeneratedOnlyTargetProjectPath();
            TestHelper.GenerateTypeScript( targetProjectPath, new[] { typeof( INotGeneratedByDefault ) }, Type.EmptyTypes );
            File.Exists( targetProjectPath.Combine( "ck-gen/src/CK/StObj/TypeScript/Tests/NotGeneratedByDefault.ts" ) ).Should().BeFalse();
        }

        [Test]
        public void no_TypeScript_attribute_is_generated_when_referenced()
        {
            // NotGeneratedByDefault is generated because it is referenced by IGeneratedByDefault.
            var targetProjectPath = TestHelper.GetTypeScriptGeneratedOnlyTargetProjectPath();
            TestHelper.GenerateTypeScript( targetProjectPath, new[] { typeof( IGeneratedByDefault ), typeof( INotGeneratedByDefault ) }, Type.EmptyTypes );
            File.Exists( targetProjectPath.Combine( "ck-gen/src/CK/StObj/TypeScript/Tests/NotGeneratedByDefault.ts" ) ).Should().BeTrue();
        }

        [Test]
        public void no_TypeScript_attribute_is_generated_when_Type_appears_in_Aspect()
        {
            // NotGeneratedByDefault is generated because it is configured.
            var targetProjectPath = TestHelper.GetTypeScriptGeneratedOnlyTargetProjectPath();
            TestHelper.GenerateTypeScript( targetProjectPath, new[] { typeof( INotGeneratedByDefault ) }, new[] { typeof( INotGeneratedByDefault ) } );
            File.Exists( targetProjectPath.Combine( "ck-gen/src/CK/StObj/TypeScript/Tests/NotGeneratedByDefault.ts" ) ).Should().BeTrue();
        }
    }
}
