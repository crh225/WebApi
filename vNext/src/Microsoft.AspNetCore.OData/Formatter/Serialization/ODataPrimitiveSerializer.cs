// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Reflection;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
	/// <summary>
	/// Represents an <see cref="ODataSerializer"/> for serializing <see cref="IEdmPrimitiveType" />'s.
	/// </summary>
	public class ODataPrimitiveSerializer : ODataEdmTypeSerializer
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ODataPrimitiveSerializer"/>.
		/// </summary>
		public ODataPrimitiveSerializer()
			: base(ODataPayloadKind.Property)
		{
		}

		/// <inheritdoc/>
		public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
		{
			if (messageWriter == null)
			{
				throw Error.ArgumentNull("messageWriter");
			}
			if (writeContext == null)
			{
				throw Error.ArgumentNull("writeContext");
			}
			if (writeContext.RootElementName == null)
			{
				throw Error.Argument("writeContext", SRResources.RootElementNameMissing, typeof(ODataSerializerContext).Name);
			}

			IEdmTypeReference edmType = writeContext.GetEdmType(graph, type);
			Contract.Assert(edmType != null);

			messageWriter.WriteProperty(await CreateProperty(graph, edmType, writeContext.RootElementName, writeContext));
		}

		/// <inheritdoc/>
		public sealed override async Task<ODataValue> CreateODataValueAsync(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
		{
			if (!expectedType.IsPrimitive())
			{
				throw Error.InvalidOperation(SRResources.CannotWriteType, typeof(ODataPrimitiveSerializer), expectedType.FullName());
			}

			ODataPrimitiveValue value = await CreateODataPrimitiveValueAsync(graph, expectedType.AsPrimitive(), writeContext);
			if (value == null)
			{
				return new ODataNullValue();
			}

			return value;
		}

		/// <summary>
		/// Creates an <see cref="ODataPrimitiveValue"/> for the object represented by <paramref name="graph"/>.
		/// </summary>
		/// <param name="graph">The primitive value.</param>
		/// <param name="primitiveType">The EDM primitive type of the value.</param>
		/// <param name="writeContext">The serializer write context.</param>
		/// <returns>The created <see cref="ODataPrimitiveValue"/>.</returns>
		public virtual Task<ODataPrimitiveValue> CreateODataPrimitiveValueAsync(object graph, IEdmPrimitiveTypeReference primitiveType,
			ODataSerializerContext writeContext)
		{
			// TODO: Bug 467598: validate the type of the object being passed in here with the underlying primitive type. 
			return Task.FromResult(CreatePrimitive(graph, primitiveType, writeContext));
		}

		private static void AddTypeNameAnnotationAsNeeded(ODataPrimitiveValue primitive, IEdmPrimitiveTypeReference primitiveType,
			ODataMetadataLevel metadataLevel)
		{
			// ODataLib normally has the caller decide whether or not to serialize properties by leaving properties
			// null when values should not be serialized. The TypeName property is different and should always be
			// provided to ODataLib to enable model validation. A separate annotation is used to decide whether or not
			// to serialize the type name (a null value prevents serialization).

			Contract.Assert(primitive != null);

			object value = primitive.Value;
			string typeName = null; // Set null to force the type name not to serialize.

			// Provide the type name to serialize.
			if (!ShouldSuppressTypeNameSerialization(value, metadataLevel))
			{
				typeName = primitiveType.FullName();
			}

			primitive.SetAnnotation<SerializationTypeNameAnnotation>(new SerializationTypeNameAnnotation
			{
				TypeName = typeName
			});
		}

		private static ODataPrimitiveValue CreatePrimitive(object value, IEdmPrimitiveTypeReference primitveType,
			ODataSerializerContext writeContext)
		{
			if (value == null)
			{
				return null;
			}

			object supportedValue = ConvertPrimitiveValue(value, primitveType);
			ODataPrimitiveValue primitive = new ODataPrimitiveValue(supportedValue);

			if (writeContext != null)
			{
				AddTypeNameAnnotationAsNeeded(primitive, primitveType, writeContext.MetadataLevel);
			}

			return primitive;
		}

        internal static object ConvertPrimitiveValue(object value, IEdmPrimitiveTypeReference primitiveType)
        {
            if (value == null)
            {
                return null;
            }

            Type type = value.GetType();
            if (primitiveType != null && primitiveType.IsDate() && TypeHelper.IsDateTime(type))
            {
                Date dt = (DateTime)value;
                return dt;
            }

            if (primitiveType != null && primitiveType.IsTimeOfDay() && TypeHelper.IsTimeSpan(type))
            {
                TimeOfDay tod = (TimeSpan)value;
                return tod;
            }

            return ConvertUnsupportedPrimitives(value);
        }

        internal static object ConvertUnsupportedPrimitives(object value)
		{
			if (value != null)
			{
				Type type = value.GetType();

				// Note that type cannot be a nullable type as value is not null and it is boxed.
				switch (type.GetTypeCode())
				{
					case TypeCodeInternal.Char:
						return new String((char)value, 1);

					case TypeCodeInternal.UInt16:
						return (int)(ushort)value;

					case TypeCodeInternal.UInt32:
						return (long)(uint)value;

					case TypeCodeInternal.UInt64:
						return checked((long)(ulong)value);

                    case TypeCodeInternal.DateTime:
                        DateTime dateTime = (DateTime)value;

                        TimeZoneInfo timeZone = TimeZoneInfoHelper.TimeZone;
                        TimeSpan utcOffset = timeZone.GetUtcOffset(dateTime);
                        if (utcOffset >= TimeSpan.Zero)
                        {
                            if (dateTime <= DateTime.MinValue + utcOffset)
                            {
                                return DateTimeOffset.MinValue;
                            }
                        }
                        else
                        {
                            if (dateTime >= DateTime.MaxValue + utcOffset)
                            {
                                return DateTimeOffset.MaxValue;
                            }
                        }

                        if (dateTime.Kind == DateTimeKind.Local)
                        {
                            TimeZoneInfo localTimeZoneInfo = TimeZoneInfo.Local;
                            TimeSpan localTimeSpan = localTimeZoneInfo.GetUtcOffset(dateTime);
                            if (localTimeSpan < TimeSpan.Zero)
                            {
                                if (dateTime >= DateTime.MaxValue + localTimeSpan)
                                {
                                    return DateTimeOffset.MaxValue;
                                }
                            }
                            else
                            {
                                if (dateTime <= DateTime.MinValue + localTimeSpan)
                                {
                                    return DateTimeOffset.MinValue;
                                }
                            }

                            return TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), timeZone);
                        }

                        if (dateTime.Kind == DateTimeKind.Utc)
                        {
                            return TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), timeZone);
                        }

                        return new DateTimeOffset(dateTime, timeZone.GetUtcOffset(dateTime));


                    default:
						if (type == typeof(char[]))
						{
							return new String(value as char[]);
						}
						else if (type == typeof(XElement))
						{
							return ((XElement)value).ToString();
						}
						// TODO: Binary not supported.
						//else if (type == typeof(Binary))
						//{
						//    return ((Binary)value).ToArray();
						//}
						break;
				}
			}

			return value;
		}

		private static bool CanTypeBeInferredInJson(object value)
		{
			Contract.Assert(value != null);

			TypeCodeInternal typeCode = value.GetType().GetTypeCode();

			switch (typeCode)
			{
				// The type for a Boolean, Int32 or String can always be inferred in JSON.
				case TypeCodeInternal.Boolean:
				case TypeCodeInternal.Int32:
				case TypeCodeInternal.String:
					return true;
				// The type for a Double can be inferred in JSON ...
				case TypeCodeInternal.Double:
					double doubleValue = (double)value;
					// ... except for NaN or Infinity (positive or negative).
					if (Double.IsNaN(doubleValue) || Double.IsInfinity(doubleValue))
					{
						return false;
					}
					else
					{
						return true;
					}
				default:
					return false;
			}
		}

		private static bool ShouldSuppressTypeNameSerialization(object value, ODataMetadataLevel metadataLevel)
		{
			// For dynamic properties in minimal metadata level, the type name always appears as declared property.
			if (metadataLevel != ODataMetadataLevel.FullMetadata)
			{
				return true;
			}

			return CanTypeBeInferredInJson(value);
		}
	}
}
