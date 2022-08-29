using CK.Core;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.StObj.TypeScript.Tests
{
    [TestFixture]
    public class PocoTypeScriptTests
    {
        /// <summary>
        /// IPoco are not automatically exported.
        /// Using the [TypeScript] attribute declares the type.
        /// </summary>
        [TypeScript]
        public interface IWithUnions : IPoco
        {
            /// <summary>
            /// Gets or sets a nullable int or string.
            /// </summary>
            [UnionType]
            object? NullableIntOrString { get; set; }

            /// <summary>
            /// Gets or sets a complex algebraic type.
            /// </summary>
            [UnionType]
            object NonNullableListOrDictionaryOrDouble { get; set; }

            [DefaultValue(3712)]
            int WithDefaultValue { get; set; }

            struct UnionTypes
            {
                public (int,string)? NullableIntOrString { get; }
                public (List<string?>,Dictionary<IPoco,ISet<int?>>[],double) NonNullableListOrDictionaryOrDouble { get; }
            }
        }

        [Test]
        public void with_union_types()
        {
            var output = LocalTestHelper.GenerateTSCode( nameof( with_union_types ), typeof( IWithUnions ) );
        }

        /// <summary>
        /// Demonstrates the read only properties support.
        /// </summary>
        [TypeScript]
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
            List<string> List { get; }

            /// <summary>
            /// Gets the mutable map from name to numeric values.
            /// </summary>
            Dictionary<string, double?> Map { get; }

            /// <summary>
            /// Gets the mutable set of unique string.
            /// </summary>
            HashSet<string> Set { get; }

            /// <summary>
            /// Gets the algebraic types demonstrations.
            /// </summary>
            IWithUnions Poco { get; }
        }

        [Test]
        public void array_set_maps_and_IPoco_can_be_readonly_and_default_values_are_applied_when_possible()
        {
            var output = LocalTestHelper.GenerateTSCode( nameof( array_set_maps_and_IPoco_can_be_readonly_and_default_values_are_applied_when_possible ),
                                                         typeof( IWithReadOnly ),
                                                         typeof( IWithUnions ) );
        }

        [TypeScript]
        public interface IBasicGeneric<T> : IPoco
        {
            T Value { get; set; }
        }

        [Test]
        public void open_generics_are_not_supported()
        {
            FluentActions.Invoking( () =>
            {
                var output = LocalTestHelper.GenerateTSCode( nameof( open_generics_are_not_supported ),
                                                             typeof( IBasicGeneric<> ) );
            } ).Should().Throw<ArgumentException>();
        }

    }

}
