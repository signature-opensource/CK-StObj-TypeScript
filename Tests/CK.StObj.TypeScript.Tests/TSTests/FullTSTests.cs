using CK.Core;
using CK.CrisLike;
using CK.Setup;
using CK.StObj.TypeScript.Tests.CrisLike;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.StObjEngineTestHelper;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.StObj.TypeScript.Tests.TSTests
{
    [TestFixture]
    public class FullTSTests
    {
        public interface IWithUnions : IPoco
        {
            /// <summary>
            /// Gets or sets a nullable int or string.
            /// </summary>
            [UnionType]
            object? NullableIntOrString { get; set; }

            /// <summary>
            /// Gets or sets a non nullable int or string.
            /// </summary>
            [UnionType]
            object IntOrStringNoDefault { get; set; }

            /// <summary>
            /// Gets or sets a non nullable int or string with default.
            /// </summary>
            [DefaultValue( 3712 )]
            [UnionType]
            object IntOrStringWithDefault { get; set; }

            /// <summary>
            /// Gets or sets a non nullable int or string with default.
            /// </summary>
            [DefaultValue( 42 )]
            [UnionType]
            object? NullableIntOrStringWithDefault { get; set; }

            readonly struct UnionTypes
            {
                public (int, string) NullableIntOrString { get; }
                public (int, string) IntOrStringNoDefault { get; }
                public (int, string) IntOrStringWithDefault { get; }
                public (int, string) NullableIntOrStringWithDefault { get; }
            }
        }

        /// <summary>
        /// Demonstrates the read only properties support.
        /// </summary>
        public interface IWithReadOnly : IPoco
        {
            /// <summary>
            /// Gets or sets the required target path.
            /// </summary>
            [DefaultValue( "The/Default/Path" )]
            string TargetPath { get; set; }

            /// <summary>
            /// Gets or sets the power.
            /// </summary>
            int? Power { get; set; }

            /// <summary>
            /// Gets the mutable list of string values.
            /// </summary>
            IList<string> List { get; }

            /// <summary>
            /// Gets the mutable map from name to numeric values.
            /// </summary>
            IDictionary<string, double?> Map { get; }

            /// <summary>
            /// Gets the mutable set of unique string.
            /// </summary>
            ISet<string> Set { get; }

            /// <summary>
            /// Gets the union types poco.
            /// </summary>
            IWithUnions Poco { get; }
        }

        public record struct RecordData( DateTime Time, List<string> Names );

        public interface IWithTyped : IPoco
        {
            IList<RecordData> TypedList { get; }
            IDictionary<string, IWithUnions> TypedDic1 { get; }
            IDictionary<IWithReadOnly, string> TypedDic2 { get; }
        }

        [Test]
        public async Task TypeScriptRunner_with_environment_variables_Async()
        {
            var targetProjectPath = TestHelper.GetTypeScriptWithTestsSupportTargetProjectPath();
            TestHelper.GenerateTypeScript( targetProjectPath,
                                           typeof( IWithReadOnly ),
                                           typeof( IWithUnions ),
                                           typeof( IWithTyped ) );
            await using var runner = TestHelper.CreateTypeScriptRunner( targetProjectPath,
                                                                        new Dictionary<string, string>()
                                                                        {
                                                                            { "SET_BY_THE_UNIT_TEST", "YES!" }
                                                                        } );
            await TestHelper.SuspendAsync( resume => resume );
            runner.Run();
        }



        public interface ITestSerializationCommand : ICommand
        {
            string String { get; set; }
            int Int32 { get; set; }
            float Single { get; set; }
            double Double { get; set; }
            Guid Guid { get; set; }
            DateTime DateTime { get; set; }
            TimeSpan TimeSpan { get; set; }
        }

        [Test]
        public async Task CrisLike_commands_and_results_Async()
        {
            var targetProjectPath = TestHelper.GetTypeScriptWithTestsSupportTargetProjectPath();
            var tsTypes = new[]
            {
                // SomeCommand
                typeof( ISomeCommand ),
                typeof( ISomeCommandIsCriticalAndReturnsInt ),
                // WithObject
                typeof( IWithObjectCommand ),
                typeof( IWithObjectSpecializedAsPocoCommand ),
                typeof( IResult ),
                typeof( IWithObjectSpecializedAsSuperPocoCommand ),
                typeof( ISuperResult ),
                // AbstractCommands
                typeof( ICommandAbs ),
                typeof( IIntCommand ),
                typeof( IStringCommand ),
                typeof( NamedRecord ),
                typeof( INamedRecordCommand ),
                typeof( IAnonymousRecordCommand ),
                typeof( ICommandAbsWithNullableKey ),
                typeof( ICommandCommand ),
                typeof( ICommandAbsWithResult ),
                typeof( INamedRecordCommandWithResult ),
                // Basic types.
                typeof( ITestSerializationCommand )
            };
            TestHelper.GenerateTypeScript( targetProjectPath,
                                           // Registers IAspNetResults only as Poco type: it is the FakeTypeScriptCrisCommandGeneratorImpl
                                           // that ensures that they benlong to the TypeScriptSet.
                                           registeredTypes: tsTypes.Concat( new[] { typeof( IAspNetCrisResult ),
                                                                                    typeof( IAspNetCrisResultError ),
                                                                                    typeof( CommonPocoJsonSupport ),
                                                                                    typeof( FakeTypeScriptCrisCommandGenerator ) } ),
                                           tsTypes );
            await using var runner = TestHelper.CreateTypeScriptRunner( targetProjectPath );
            await TestHelper.SuspendAsync( resume => resume );
            runner.Run();
        }

    }
}
