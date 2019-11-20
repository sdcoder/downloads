using System.Collections.Generic;
using FirstAgain.Common;
using FirstAgain.Common.Data;

namespace FirstAgain.Web.UI
{
    public sealed class ReApplyApplicationTypeLookup : LookupTable<ReApplyApplicationTypeLookup.ReApplyApplicationType>
    {
        #region Enumerations
        public enum ReApplyApplicationType : short
        {
            NotSelected = 32700,
            IndividualWithSSN1 = 10,
            IndividualWithSSN2 = 11,
            JointWithSSN1andSSN2 = 12//,
            //JointWithSSN1andOther = 13,
            //JointWithSSN2andOther = 14
        }
        #endregion

        #region Fields
        private static readonly ReApplyApplicationTypeLookup _instance = new ReApplyApplicationTypeLookup();
        #endregion

        #region Construction
        static ReApplyApplicationTypeLookup()
        {
        }

        private ReApplyApplicationTypeLookup()
        { } // prevent instantiation -- singleton instance only
        #endregion

        #region Iterators
        /// <summary>The collection of captions or "friendly names".  Note that lookup values flagged as "hidden" will <b>not</b> be returned.</summary>
        public new static IEnumerable<string> Captions
        {
            get { return ((LookupTable<ReApplyApplicationType>)_instance).Captions; }
        }

        /// <summary>The collection of definitions.  Note that lookup values flagged as "hidden" will <b>not</b> be returned.</summary>
        public new static IEnumerable<string> Definitions
        {
            get { return ((LookupTable<ReApplyApplicationType>)_instance).Definitions; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Assign this property to a control's <b>DataSource</b> property for databinding purposes.  Note that lookup values flagged as 
        /// "hidden" will <b>not</b> be bound.  In such a scenario, one would typically assign "Caption" to <b>DisplayMember</b> or
        /// <b>DataTextField</b> and "Enumeration" to <b>ValueMember</b> or <b>DataValueField</b>.  For WinForm and WebForm ListControl-derived
        /// controls, simply use <see cref="BindListControl"/>.
        /// </summary>
        public static ValueList BindingSource
        {
            get { return new ValueList(_instance.Values); }
        }

        public static ValueList BindingSourceWithHiddenValues
        {
            get { return new ValueList(_instance.Values, true); }
        }
        #endregion

        #region Methods



        /// <summary>Returns the enumerated type equivilent for the specified enumeration.</summary>
        /// <param name="enumeration">The lookup value enumeration.</param>
        /// <returns>The enumerated type or <b>null</b> if invalid <paramref name="enumeration"/> value.</returns>
        public new static ReApplyApplicationType GetEnumeration(string enumeration)
        {
            return ((LookupTable<ReApplyApplicationType>)_instance).GetEnumeration(enumeration);
        }

        /// <summary>Returns the caption or "friendly name" for the specified enumeration.</summary>
        /// <param name="enumeration">The lookup value enumeration.</param>
        /// <returns>The caption.</returns>
        public new static string GetCaption(ReApplyApplicationType enumeration)
        {
            return ((LookupTable<ReApplyApplicationType>)_instance).GetCaption(enumeration);
        }

        /// <summary>Returns the caption or "friendly name" for the specified enumeration.</summary>
        /// <param name="enumeration">The lookup value enumeration.</param>
        /// <returns>The caption or <b>null</b> if invalid <paramref name="enumeration"/> value.</returns>
        public new static string GetCaption(string enumeration)
        {
            return ((LookupTable<ReApplyApplicationType>)_instance).GetCaption(enumeration);
        }

        /// <summary>Returns the definition for the specified enumeration.</summary>
        /// <param name="enumeration">The lookup value enumeration.</param>
        /// <returns>The definition.</returns>
        public new static string GetDefinition(ReApplyApplicationType enumeration)
        {
            return ((LookupTable<ReApplyApplicationType>)_instance).GetDefinition(enumeration);
        }

        /// <summary>Returns the definition for the specified enumeration.</summary>
        /// <param name="enumeration">The lookup value enumeration.</param>
        /// <returns>The definition or <b>null</b> if invalid <paramref name="enumeration"/> value.</returns>
        public new static string GetDefinition(string enumeration)
        {
            return ((LookupTable<ReApplyApplicationType>)_instance).GetDefinition(enumeration);
        }

        /// <summary>Returns the enumeration for a specific caption.</summary>
        /// <param name="definition">The lookup value caption.</param>
        /// <returns>The enumeration</returns>
        public static ReApplyApplicationType GetEnumerationFromCaption(string caption)
        {
            foreach (Value value in BindingSourceWithHiddenValues)
            {
                if (string.Compare(value.Caption, caption, true) == 0)
                    return value.Enumeration;
            }
            throw new FirstAgainException("Caption " + caption + " was not found for enumeration " + typeof(ReApplyApplicationType).Name);
        }

        /// <summary>Returns the enumeration for a specific definition.</summary>
        /// <param name="definition">The lookup value definition.</param>
        /// <returns>The enumeration</returns>
        public static ReApplyApplicationType GetEnumerationFromDefinition(string definition)
        {
            foreach (Value value in BindingSourceWithHiddenValues)
            {
                if (string.Compare(value.Definition, definition, true) == 0)
                    return value.Enumeration;
            }
            throw new FirstAgainException("Definition " + definition + " was not found for enumeration " + typeof(ReApplyApplicationType).Name);
        }
        #endregion

        #region Inner Classes
        /// <summary>
        /// Represents a unique <see cref="VerificationTypeLookup"/> value comprised of the enumerated type,
        /// caption, and definition.  In databinding scenarios where <b>BindingSource</b> or <b>GetFilteredBindingSource</b> is
        /// bound to a control's <b>DataSource</b> property, the control's <b>SelectedItem</b> property will return instances of
        /// this class.
        /// </summary>
        public sealed new class Value : LookupTable<ReApplyApplicationType>.Value
        {
            internal Value(ReApplyApplicationType enumeration, string caption, string definition, bool isHidden)
                : base(enumeration, caption, definition, isHidden, 0)
            { }

            /// <summary>The enumerated type of this value.</summary>
            public new ReApplyApplicationType Enumeration
            {
                get { return (ReApplyApplicationType)base.Enumeration; }
            }

            protected override LookupTable<ReApplyApplicationType>.Value Copy()
            {
                return new Value(Enumeration, Caption, Definition, IsHidden);
            }
        }
        #endregion
    }
}
