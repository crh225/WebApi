﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.OData.Formatter
{
	public class ModernOutputFormatter : OutputFormatter
	{
		/// <summary>
		/// Returns UTF8 Encoding without BOM and throws on invalid bytes.
		/// </summary>
		public static readonly Encoding UTF8EncodingWithoutBOM
			= new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

		public ModernOutputFormatter()
		{
			// TODO: JC: Restore this
			//SupportedEncodings.Add(UTF8EncodingWithoutBOM);
			SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
			SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/json"));
			SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));
			//SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));

			//foreach (var mediaType in SupportedMediaTypes)
			//{
			//    mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
			//}
		}

		public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
		{
			var response = context.HttpContext.Response;
			//var selectedEncoding = context.ContentType.Encoding == null ? Encoding.UTF8 : context.ContentType.Encoding;
			var selectedEncoding = Encoding.UTF8;

			var value = context.Object;
			if (value is IEdmModel || context.HttpContext.ODataProperties().Model == null)
			{
				using (var delegatingStream = new NonDisposableStream(response.Body))
				using (var writer = new StreamWriter(delegatingStream, selectedEncoding, 1024, leaveOpen: true))
				{
					if (value is IEdmModel)
					{
						WriteMetadata(writer, (IEdmModel)value);
					}

					using (var jsonWriter = CreateJsonWriter(writer))
					{
						var jsonSerializer = CreateJsonSerializer();
						jsonSerializer.Serialize(jsonWriter, value);
					}
				}
			}
			else
			{
				using (var delegatingStream = new NonDisposableStream(response.Body))
				{
					await WriteObjectAsync(delegatingStream, context);
				}
			}
		}

		private JsonWriter CreateJsonWriter(TextWriter writer)
		{
			var jsonWriter = new JsonTextWriter(writer);
			jsonWriter.CloseOutput = false;

			return jsonWriter;
		}

		private JsonSerializer CreateJsonSerializer()
		{
			var serializerSettings = new JsonSerializerSettings();
			serializerSettings.Converters.Add(new ODataJsonConverter(new Uri("http://localhost:58888/")));
			var jsonSerializer = JsonSerializer.Create(serializerSettings);
			return jsonSerializer;
		}

		public override void WriteResponseHeaders(OutputFormatterWriteContext context)
		{
			if (context.Object is IEdmModel)
			{
				context.ContentType = new StringSegment(SupportedMediaTypes[2]);
			}

			context.HttpContext.Response.Headers.Add("OData-Version", new[] { "4.0" });
			base.WriteResponseHeaders(context);
		}

		// In the future, should convert to ODataEntry and use ODL to write out.
		// Or use ODL to build a JObject and use Json.NET to write out.
		public virtual async Task WriteObjectAsync(Stream stream, OutputFormatterWriteContext context)
		{
			await ResolveJsonSerializer(context).WriteJson(context.Object, stream);
		}

		private static ODataJsonSerializer ResolveJsonSerializer(OutputFormatterWriteContext context)
		{
			return new ODataJsonSerializer(context);
		}

		private void WriteMetadata(TextWriter writer, IEdmModel model)
		{
			using (var xmlWriter = XmlWriter.Create(writer))
			{
				IEnumerable<EdmError> errors;
				EdmxWriter.TryWriteEdmx(model, xmlWriter, EdmxTarget.OData, out errors);
			}
		}
	}
}